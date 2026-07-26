using Microsoft.EntityFrameworkCore;
using TrilhaDaMultiplicacaoAPI.Data;
using TrilhaDaMultiplicacaoAPI.Dtos;
using TrilhaDaMultiplicacaoAPI.Models;

namespace TrilhaDaMultiplicacaoAPI.Services;

public interface IConquistaService
{
    Task<IReadOnlyList<ConquistaResponse>> ObterConquistasAsync(int alunoId);

    /// <summary>Desbloqueia para o aluno todas as conquistas cujo critério já foi atingido.</summary>
    Task DesbloquearElegiveisAsync(int alunoId);
}

public class ConquistaService(AppDbContext db) : IConquistaService
{
    public async Task<IReadOnlyList<ConquistaResponse>> ObterConquistasAsync(int alunoId)
    {
        var desbloqueadasIds = await db.AlunoConquistas
            .Where(ac => ac.AlunoId == alunoId)
            .Select(ac => ac.ConquistaId)
            .ToListAsync();

        return await db.Conquistas
            .OrderBy(c => c.FasesConcluidasNecessarias)
            .Select(c => new ConquistaResponse(c.Id, c.Titulo, c.Descricao, c.Icone, desbloqueadasIds.Contains(c.Id)))
            .ToListAsync();
    }

    public async Task DesbloquearElegiveisAsync(int alunoId)
    {
        var totalFasesConcluidas = await db.FasesProgresso.CountAsync(p => p.AlunoId == alunoId);

        var conquistasJaDesbloqueadas = await db.AlunoConquistas
            .Where(ac => ac.AlunoId == alunoId)
            .Select(ac => ac.ConquistaId)
            .ToListAsync();

        var novasConquistas = await db.Conquistas
            .Where(c => c.FasesConcluidasNecessarias <= totalFasesConcluidas && !conquistasJaDesbloqueadas.Contains(c.Id))
            .ToListAsync();

        if (novasConquistas.Count == 0) return;

        db.AlunoConquistas.AddRange(novasConquistas.Select(c => new AlunoConquista
        {
            AlunoId = alunoId,
            ConquistaId = c.Id
        }));

        await db.SaveChangesAsync();
    }
}
