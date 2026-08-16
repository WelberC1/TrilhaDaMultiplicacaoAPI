using Microsoft.EntityFrameworkCore;
using TrilhaDaMultiplicacaoAPI.Data;
using TrilhaDaMultiplicacaoAPI.Dtos;
using TrilhaDaMultiplicacaoAPI.Exceptions;
using TrilhaDaMultiplicacaoAPI.Models;

namespace TrilhaDaMultiplicacaoAPI.Services;

public interface IProgressoService
{
    Task<IReadOnlyList<FaseProgressoResponse>> ObterProgressoAsync(int alunoId);
    Task<FaseProgressoResponse> RegistrarConclusaoAsync(int alunoId, int numeroFase, RegistrarConclusaoRequest request);
}

public class ProgressoService(AppDbContext db, IConquistaService conquistaService) : IProgressoService
{
    public async Task<IReadOnlyList<FaseProgressoResponse>> ObterProgressoAsync(int alunoId) =>
        await db.FasesProgresso
            .Where(p => p.AlunoId == alunoId)
            .OrderBy(p => p.NumeroFase)
            .Select(p => new FaseProgressoResponse(p.NumeroFase, p.Estrelas, p.Pontos, p.ConcluidaEm))
            .ToListAsync();

    public async Task<FaseProgressoResponse> RegistrarConclusaoAsync(int alunoId, int numeroFase, RegistrarConclusaoRequest request)
    {
        if (numeroFase > 1)
        {
            var faseAnteriorConcluida = await db.FasesProgresso
                .AnyAsync(p => p.AlunoId == alunoId && p.NumeroFase == numeroFase - 1);

            if (!faseAnteriorConcluida)
                throw new ConflitoException($"Complete a fase {numeroFase - 1} antes de registrar a fase {numeroFase}.");
        }

        var existente = await db.FasesProgresso
            .FirstOrDefaultAsync(p => p.AlunoId == alunoId && p.NumeroFase == numeroFase);

        var pontos = PontosDeEstrelas(request.Estrelas);

        if (existente is null)
        {
            existente = new FaseProgresso
            {
                AlunoId = alunoId,
                NumeroFase = numeroFase,
                Estrelas = request.Estrelas,
                Pontos = pontos
            };
            db.FasesProgresso.Add(existente);
        }
        else if (request.Estrelas > existente.Estrelas)
        {
            existente.Estrelas = request.Estrelas;
            existente.Pontos = pontos;
            existente.ConcluidaEm = DateTime.UtcNow;
        }

        await db.SaveChangesAsync();
        await conquistaService.DesbloquearElegiveisAsync(alunoId);

        return new FaseProgressoResponse(existente.NumeroFase, existente.Estrelas, existente.Pontos, existente.ConcluidaEm);
    }

    private static int PontosDeEstrelas(int estrelas) => 20 + estrelas * 30;
}
