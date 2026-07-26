using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TrilhaDaMultiplicacaoAPI.Data;
using TrilhaDaMultiplicacaoAPI.Middleware;
using TrilhaDaMultiplicacaoAPI.Options;
using TrilhaDaMultiplicacaoAPI.Services;

namespace TrilhaDaMultiplicacaoAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddOpenApi();
            builder.Services.AddHttpContextAccessor();

            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

            builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
            builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection(SmtpOptions.SectionName));

            builder.Services.AddScoped<ITokenService, TokenService>();
            builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IAlunoService, AlunoService>();
            builder.Services.AddScoped<IProgressoService, ProgressoService>();
            builder.Services.AddScoped<IRankingService, RankingService>();
            builder.Services.AddScoped<IConquistaService, ConquistaService>();

            var smtpHost = builder.Configuration[$"{SmtpOptions.SectionName}:Host"];
            if (string.IsNullOrWhiteSpace(smtpHost))
                builder.Services.AddScoped<IEmailSender, ConsoleEmailSender>();
            else
                builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();

            builder.Services.AddExceptionHandler<ApiExceptionHandler>();
            builder.Services.AddProblemDetails();

            builder.Services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var mensagem = context.ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .FirstOrDefault() ?? "Dados inválidos.";

                    return new BadRequestObjectResult(new { mensagem });
                };
            });

            var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()!;
            if (Encoding.UTF8.GetByteCount(jwtOptions.Chave ?? "") < 32)
            {
                throw new InvalidOperationException(
                    "Jwt:Chave ausente ou fraca (precisa de pelo menos 32 bytes). " +
                    "Configure via 'dotnet user-secrets set \"Jwt:Chave\" \"<valor>\"' em desenvolvimento, " +
                    "ou pela variável de ambiente Jwt__Chave em produção.");
            }

            builder.Services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.MapInboundClaims = false;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = jwtOptions.Emissor,
                        ValidateAudience = true,
                        ValidAudience = jwtOptions.Audiencia,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Chave!)),
                        ValidateLifetime = true
                    };

                    // Permite revogar um token antes de ele expirar naturalmente: logout e troca de
                    // senha rotacionam o SecurityStamp do aluno, e qualquer token emitido com o
                    // stamp antigo passa a ser rejeitado aqui, mesmo dentro da validade original.
                    options.Events = new JwtBearerEvents
                    {
                        OnTokenValidated = async context =>
                        {
                            var sub = context.Principal?.FindFirstValue(JwtRegisteredClaimNames.Sub);
                            var stamp = context.Principal?.FindFirstValue(TokenService.SecurityStampClaimType);

                            if (sub is null || stamp is null || !int.TryParse(sub, out var alunoId))
                            {
                                context.Fail("Token inválido.");
                                return;
                            }

                            var db = context.HttpContext.RequestServices.GetRequiredService<AppDbContext>();
                            var stampAtual = await db.Alunos
                                .Where(a => a.Id == alunoId)
                                .Select(a => (Guid?)a.SecurityStamp)
                                .FirstOrDefaultAsync();

                            if (stampAtual is null || stampAtual.Value.ToString() != stamp)
                            {
                                context.Fail("Sessão expirada. Faça login novamente.");
                            }
                        }
                    };
                });
            builder.Services.AddAuthorization();

            // Nenhuma origem liberada por padrão — o app desktop não é sujeito a CORS
            // (não é um navegador), então isso só bloqueia chamadas de páginas web não autorizadas
            // caso a API seja exposta publicamente. Adicione WithOrigins(...) aqui se um cliente
            // web legítimo passar a existir.
            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy => policy.AllowAnyHeader().AllowAnyMethod());
            });

            builder.Services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                options.OnRejected = async (context, cancellationToken) =>
                {
                    context.HttpContext.Response.ContentType = "application/json";
                    await context.HttpContext.Response.WriteAsJsonAsync(
                        new { mensagem = "Muitas tentativas. Aguarde um pouco antes de tentar de novo." },
                        cancellationToken);
                };

                // Baseline global: 100 requisições/minuto por IP, em qualquer rota.
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "desconhecido",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 100,
                            Window = TimeSpan.FromMinutes(1),
                            QueueLimit = 0
                        }));

                // Política mais estrita para rotas de autenticação (login, registro, recuperação de senha).
                options.AddPolicy("auth", httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "desconhecido",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 5,
                            Window = TimeSpan.FromMinutes(1),
                            QueueLimit = 0
                        }));
            });

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Database.Migrate();
            }

            // Configure the HTTP request pipeline.
            app.UseExceptionHandler();

            if (!app.Environment.IsDevelopment())
            {
                app.UseHsts();
            }

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            app.Use(async (context, next) =>
            {
                context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
                context.Response.Headers.Append("X-Frame-Options", "DENY");
                context.Response.Headers.Append("Referrer-Policy", "no-referrer");
                await next();
            });

            app.UseCors();

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseRateLimiter();

            app.MapControllers();

            app.Run();
        }
    }
}
