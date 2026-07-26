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
            .OrderBy(c => c.Id)
            .Select(c => new ConquistaResponse(c.Id, c.Titulo, c.Descricao, c.Icone, desbloqueadasIds.Contains(c.Id)))
            .ToListAsync();
    }

    public async Task DesbloquearElegiveisAsync(int alunoId)
    {
        var progresso = await db.FasesProgresso.Where(p => p.AlunoId == alunoId).ToListAsync();
        var fasesConcluidas = progresso.Count;
        var fasesComTresEstrelas = progresso.Count(p => p.Estrelas == 3);
        var pontosTotais = progresso.Sum(p => p.Pontos);

        var conquistasJaDesbloqueadas = await db.AlunoConquistas
            .Where(ac => ac.AlunoId == alunoId)
            .Select(ac => ac.ConquistaId)
            .ToListAsync();

        var todasConquistas = await db.Conquistas.ToListAsync();

        var novasConquistas = todasConquistas
            .Where(c => !conquistasJaDesbloqueadas.Contains(c.Id) &&
                        AtingiuCriterio(c, fasesConcluidas, fasesComTresEstrelas, pontosTotais))
            .ToList();

        if (novasConquistas.Count == 0) return;

        db.AlunoConquistas.AddRange(novasConquistas.Select(c => new AlunoConquista
        {
            AlunoId = alunoId,
            ConquistaId = c.Id
        }));

        await db.SaveChangesAsync();
    }

    private static bool AtingiuCriterio(Conquista conquista, int fasesConcluidas, int fasesComTresEstrelas, int pontosTotais) =>
        conquista.TipoCriterio switch
        {
            TipoCriterioConquista.FasesConcluidas => fasesConcluidas >= conquista.ValorNecessario,
            TipoCriterioConquista.FasesComTresEstrelas => fasesComTresEstrelas >= conquista.ValorNecessario,
            TipoCriterioConquista.PontosTotais => pontosTotais >= conquista.ValorNecessario,
            _ => false
        };
}
