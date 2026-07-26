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
    Task EsqueciSenhaAsync(EsqueciSenhaRequest request);
    Task RedefinirSenhaAsync(RedefinirSenhaRequest request);
}

public class AuthService(AppDbContext db, ITokenService tokenService, IEmailSender emailSender) : IAuthService
{
    private const int MinutosExpiracaoCodigo = 15;
    private const int MaximoTentativasFalhas = 5;

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

        return new AuthResponse(tokenService.GerarToken(aluno), ParaResponse(aluno));
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var nomeUsuario = request.NomeUsuario.Trim().ToLowerInvariant();

        var aluno = await db.Alunos
            .Include(a => a.Progresso)
            .FirstOrDefaultAsync(a => a.NomeUsuario == nomeUsuario);

        if (aluno is null || !BCrypt.Net.BCrypt.Verify(request.Senha, aluno.SenhaHash))
            throw new NaoAutorizadoException("Usuário ou senha inválidos.");

        return new AuthResponse(tokenService.GerarToken(aluno), ParaResponse(aluno));
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
            CodigoHash = HashCodigo(codigo),
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

        if (token.CodigoHash != HashCodigo(request.Codigo.Trim()))
        {
            token.TentativasFalhas++;
            await db.SaveChangesAsync();
            throw new NaoAutorizadoException("Código inválido ou expirado.");
        }

        aluno.SenhaHash = BCrypt.Net.BCrypt.HashPassword(request.NovaSenha);
        token.UsadoEm = DateTime.UtcNow;
        await db.SaveChangesAsync();
    }

    private static string GerarCodigo() => RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");

    private static string HashCodigo(string codigo) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(codigo)));

    private static AlunoResponse ParaResponse(Aluno aluno) =>
        new(aluno.Id, aluno.Nome, aluno.NomeUsuario, aluno.Email, aluno.AvatarEmoji, aluno.PontosTotais);
}
