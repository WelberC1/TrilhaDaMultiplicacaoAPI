using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using TrilhaDaMultiplicacaoAPI.Data;
using TrilhaDaMultiplicacaoAPI.Dtos;
using TrilhaDaMultiplicacaoAPI.Exceptions;
using TrilhaDaMultiplicacaoAPI.Models;

namespace TrilhaDaMultiplicacaoAPI.Services;

public interface IAuthService
{
    Task<AuthResponse> RegistrarAsync(RegistrarRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> RefreshAsync(RefreshTokenRequest request);
    Task EsqueciSenhaAsync(EsqueciSenhaRequest request);
    Task RedefinirSenhaAsync(RedefinirSenhaRequest request);
    Task LogoutAsync(int alunoId);
}

public class AuthService(AppDbContext db, ITokenService tokenService, IEmailSender emailSender) : IAuthService
{
    private const int MinutosExpiracaoCodigo = 15;
    private const int MaximoTentativasFalhas = 10;
    private const int MaximoTentativasLogin = 10;
    private const int MinutosBloqueioLogin = 15;
    private const int DiasExpiracaoRefreshToken = 30;

    public async Task<AuthResponse> RegistrarAsync(RegistrarRequest request)
    {
        var nomeUsuario = request.NomeUsuario.Trim().ToLowerInvariant();
        var email = request.Email.Trim().ToLowerInvariant();

        if (await db.Alunos.AnyAsync(a => a.NomeUsuario == nomeUsuario))
            throw new ConflitoException("Esse nome de usuário já está em uso.");

        if (await db.Alunos.AnyAsync(a => a.Email == email))
            throw new ConflitoException("Já existe uma conta com este e-mail.");

        var aluno = new Aluno
        {
            Nome = request.Nome.Trim(),
            NomeUsuario = nomeUsuario,
            Email = email,
            SenhaHash = BCrypt.Net.BCrypt.HashPassword(request.Senha)
        };

        db.Alunos.Add(aluno);
        await db.SaveChangesAsync();

        var refreshToken = await CriarRefreshTokenAsync(aluno.Id);
        await db.SaveChangesAsync();

        return new AuthResponse(tokenService.GerarToken(aluno), refreshToken, ParaResponse(aluno));
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var nomeUsuario = request.NomeUsuario.Trim().ToLowerInvariant();

        var aluno = await db.Alunos
            .Include(a => a.Progresso)
            .FirstOrDefaultAsync(a => a.NomeUsuario == nomeUsuario);

        if (aluno is not null && aluno.BloqueadoAte > DateTime.UtcNow)
            throw new NaoAutorizadoException("Conta temporariamente bloqueada por muitas tentativas. Tente novamente mais tarde.");

        if (aluno is null || !BCrypt.Net.BCrypt.Verify(request.Senha, aluno.SenhaHash))
        {
            if (aluno is not null)
            {
                aluno.TentativasLoginFalhas++;
                if (aluno.TentativasLoginFalhas >= MaximoTentativasLogin)
                    aluno.BloqueadoAte = DateTime.UtcNow.AddMinutes(MinutosBloqueioLogin);

                await db.SaveChangesAsync();
            }

            throw new NaoAutorizadoException("Usuário ou senha inválidos.");
        }

        aluno.TentativasLoginFalhas = 0;
        aluno.BloqueadoAte = null;

        var refreshToken = await CriarRefreshTokenAsync(aluno.Id);
        await db.SaveChangesAsync();

        return new AuthResponse(tokenService.GerarToken(aluno), refreshToken, ParaResponse(aluno));
    }

    public async Task<AuthResponse> RefreshAsync(RefreshTokenRequest request)
    {
        var hash = HashToken(request.RefreshToken);
        var tokenAtual = await db.RefreshTokens
            .Include(t => t.Aluno)
            .ThenInclude(a => a!.Progresso)
            .FirstOrDefaultAsync(t => t.TokenHash == hash);

        if (tokenAtual is null || tokenAtual.Aluno is null ||
            tokenAtual.RevogadoEm is not null || tokenAtual.ExpiraEm <= DateTime.UtcNow)
            throw new NaoAutorizadoException("Sessão expirada. Faça login novamente.");

        tokenAtual.RevogadoEm = DateTime.UtcNow;

        var aluno = tokenAtual.Aluno;
        var novoRefreshToken = await CriarRefreshTokenAsync(aluno.Id);
        await db.SaveChangesAsync();

        return new AuthResponse(tokenService.GerarToken(aluno), novoRefreshToken, ParaResponse(aluno));
    }

    public async Task EsqueciSenhaAsync(EsqueciSenhaRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var aluno = await db.Alunos.FirstOrDefaultAsync(a => a.Email == email);

        if (aluno is null) return;

        var tokensAntigos = await db.PasswordResetTokens
            .Where(t => t.AlunoId == aluno.Id && t.UsadoEm == null)
            .ToListAsync();
        db.PasswordResetTokens.RemoveRange(tokensAntigos);

        var codigo = GerarCodigo();
        db.PasswordResetTokens.Add(new PasswordResetToken
        {
            AlunoId = aluno.Id,
            CodigoHash = HashToken(codigo),
            ExpiraEm = DateTime.UtcNow.AddMinutes(MinutosExpiracaoCodigo)
        });

        await db.SaveChangesAsync();

        await emailSender.EnviarAsync(
            aluno.Email,
            "Recupere sua senha - Trilha da Multiplicação",
            $"<p>Olá, {aluno.Nome}! 👋</p><p>Seu código de recuperação é:</p><h2>{codigo}</h2><p>Ele expira em {MinutosExpiracaoCodigo} minutos.</p>");
    }

    public async Task RedefinirSenhaAsync(RedefinirSenhaRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var aluno = await db.Alunos.FirstOrDefaultAsync(a => a.Email == email);

        var token = aluno is null
            ? null
            : await db.PasswordResetTokens
                .Where(t => t.AlunoId == aluno.Id && t.UsadoEm == null && t.ExpiraEm > DateTime.UtcNow)
                .OrderByDescending(t => t.CriadoEm)
                .FirstOrDefaultAsync();

        if (aluno is null || token is null || token.TentativasFalhas >= MaximoTentativasFalhas)
            throw new NaoAutorizadoException("Código inválido ou expirado.");

        if (token.CodigoHash != HashToken(request.Codigo.Trim()))
        {
            token.TentativasFalhas++;
            await db.SaveChangesAsync();
            throw new NaoAutorizadoException("Código inválido ou expirado.");
        }

        aluno.SenhaHash = BCrypt.Net.BCrypt.HashPassword(request.NovaSenha);
        aluno.SecurityStamp = Guid.NewGuid();
        token.UsadoEm = DateTime.UtcNow;
        await db.SaveChangesAsync();
        await db.RevogarRefreshTokensAsync(aluno.Id);
    }

    public async Task LogoutAsync(int alunoId)
    {
        var aluno = await db.Alunos.FirstOrDefaultAsync(a => a.Id == alunoId)
            ?? throw new NaoEncontradoException("Aluno não encontrado.");

        aluno.SecurityStamp = Guid.NewGuid();
        await db.SaveChangesAsync();
        await db.RevogarRefreshTokensAsync(alunoId);
    }

    private async Task<string> CriarRefreshTokenAsync(int alunoId)
    {
        var bruto = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

        db.RefreshTokens.Add(new RefreshToken
        {
            AlunoId = alunoId,
            TokenHash = HashToken(bruto),
            ExpiraEm = DateTime.UtcNow.AddDays(DiasExpiracaoRefreshToken)
        });

        return bruto;
    }

    private static string GerarCodigo() => RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");

    private static string HashToken(string valor) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(valor)));

    private static AlunoResponse ParaResponse(Aluno aluno) =>
        new(aluno.Id, aluno.Nome, aluno.NomeUsuario, aluno.Email, aluno.AvatarEmoji, aluno.PontosTotais);
}
