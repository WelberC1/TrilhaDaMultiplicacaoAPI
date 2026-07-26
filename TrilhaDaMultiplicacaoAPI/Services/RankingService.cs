using Microsoft.EntityFrameworkCore;
using TrilhaDaMultiplicacaoAPI.Data;
using TrilhaDaMultiplicacaoAPI.Dtos;

namespace TrilhaDaMultiplicacaoAPI.Services;

public interface IRankingService
{
    Task<IReadOnlyList<RankingEntradaResponse>> ObterRankingAsync(int alunoId);
}

public class RankingService(AppDbContext db) : IRankingService
{
    public async Task<IReadOnlyList<RankingEntradaResponse>> ObterRankingAsync(int alunoId)
    {
        var alunos = await db.Alunos.Include(a => a.Progresso).ToListAsync();

        return alunos
            .Select(a => new { a.Id, a.Nome, a.AvatarEmoji, Pontos = a.PontosTotais })
            .OrderByDescending(a => a.Pontos)
            .Select((a, indice) => new RankingEntradaResponse(indice + 1, a.Nome, a.AvatarEmoji, a.Pontos, a.Id == alunoId))
            .ToList();
    }
}
