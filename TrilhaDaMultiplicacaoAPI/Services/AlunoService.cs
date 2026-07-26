using Microsoft.EntityFrameworkCore;
using TrilhaDaMultiplicacaoAPI.Data;
using TrilhaDaMultiplicacaoAPI.Dtos;
using TrilhaDaMultiplicacaoAPI.Exceptions;
using TrilhaDaMultiplicacaoAPI.Models;

namespace TrilhaDaMultiplicacaoAPI.Services;

public interface IAlunoService
{
    Task<AlunoResponse> ObterPerfilAsync(int alunoId);
    Task<AlunoResponse> AtualizarPerfilAsync(int alunoId, AtualizarPerfilRequest request);
}

public class AlunoService(AppDbContext db) : IAlunoService
{
    public async Task<AlunoResponse> ObterPerfilAsync(int alunoId)
    {
        var aluno = await BuscarAlunoAsync(alunoId);
        return ParaResponse(aluno);
    }

    public async Task<AlunoResponse> AtualizarPerfilAsync(int alunoId, AtualizarPerfilRequest request)
    {
        var aluno = await BuscarAlunoAsync(alunoId);
        var email = request.Email.Trim().ToLowerInvariant();

        var emailEmUsoPorOutroAluno = await db.Alunos.AnyAsync(a => a.Email == email && a.Id != aluno.Id);
        if (emailEmUsoPorOutroAluno)
            throw new ConflitoException("Já existe uma conta com este e-mail.");

        aluno.Nome = request.Nome.Trim();
        aluno.Email = email;
        aluno.AvatarEmoji = request.AvatarEmoji;

        await db.SaveChangesAsync();

        return ParaResponse(aluno);
    }

    private async Task<Aluno> BuscarAlunoAsync(int alunoId) =>
        await db.Alunos
            .Include(a => a.Progresso)
            .FirstOrDefaultAsync(a => a.Id == alunoId)
        ?? throw new NaoEncontradoException("Aluno não encontrado.");

    private static AlunoResponse ParaResponse(Aluno aluno) =>
        new(aluno.Id, aluno.Nome, aluno.Email, aluno.AvatarEmoji, aluno.PontosTotais);
}
