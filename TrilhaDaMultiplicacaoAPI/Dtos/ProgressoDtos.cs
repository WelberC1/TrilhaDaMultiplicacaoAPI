using System.ComponentModel.DataAnnotations;

namespace TrilhaDaMultiplicacaoAPI.Dtos;

public record FaseProgressoResponse(int NumeroFase, int Estrelas, int Pontos, DateTime ConcluidaEm);

public record RegistrarConclusaoRequest([Required, Range(0, 3)] int Estrelas);
