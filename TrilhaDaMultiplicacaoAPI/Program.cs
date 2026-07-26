using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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
                options.UseSqlite(builder.Configuration.GetConnectionString("Default")));

            builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));

            builder.Services.AddScoped<ITokenService, TokenService>();
            builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IAlunoService, AlunoService>();
            builder.Services.AddScoped<IProgressoService, ProgressoService>();
            builder.Services.AddScoped<IRankingService, RankingService>();
            builder.Services.AddScoped<IConquistaService, ConquistaService>();

            builder.Services.AddExceptionHandler<ApiExceptionHandler>();
            builder.Services.AddProblemDetails();

            var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()!;
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
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Chave)),
                        ValidateLifetime = true
                    };
                });
            builder.Services.AddAuthorization();

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Database.Migrate();
            }

            // Configure the HTTP request pipeline.
            app.UseExceptionHandler();

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
