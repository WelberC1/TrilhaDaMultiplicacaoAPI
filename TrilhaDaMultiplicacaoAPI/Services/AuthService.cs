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
}

public class AuthService(AppDbContext db, ITokenService tokenService) : IAuthService
{
    public async Task<AuthResponse> RegistrarAsync(RegistrarRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        if (await db.Alunos.AnyAsync(a => a.Email == email))
            throw new ConflitoException("Já existe uma conta com este e-mail.");

        var aluno = new Aluno
        {
            Nome = request.Nome.Trim(),
            Email = email,
            SenhaHash = BCrypt.Net.BCrypt.HashPassword(request.Senha)
        };

        db.Alunos.Add(aluno);
        await db.SaveChangesAsync();

        return new AuthResponse(tokenService.GerarToken(aluno), ParaResponse(aluno));
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var aluno = await db.Alunos
            .Include(a => a.Progresso)
            .FirstOrDefaultAsync(a => a.Email == email);

        if (aluno is null || !BCrypt.Net.BCrypt.Verify(request.Senha, aluno.SenhaHash))
            throw new NaoAutorizadoException("E-mail ou senha inválidos.");

        return new AuthResponse(tokenService.GerarToken(aluno), ParaResponse(aluno));
    }

    private static AlunoResponse ParaResponse(Aluno aluno) =>
        new(aluno.Id, aluno.Nome, aluno.Email, aluno.AvatarEmoji, aluno.PontosTotais);
}
