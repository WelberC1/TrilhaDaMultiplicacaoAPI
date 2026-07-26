namespace TrilhaDaMultiplicacaoAPI.Models;

public enum TipoCriterioConquista
{
    FasesConcluidas,
    FasesComTresEstrelas,
    PontosTotais
}

public class Conquista
{
    public int Id { get; set; }
    public required string Titulo { get; set; }
    public required string Descricao { get; set; }
    public required string Icone { get; set; }

    public required TipoCriterioConquista TipoCriterio { get; set; }

    /// <summary>Valor mínimo do critério (nº de fases, nº de fases com 3 estrelas ou pontos) para desbloquear.</summary>
    public required int ValorNecessario { get; set; }

    public List<AlunoConquista> AlunosQueDesbloquearam { get; set; } = [];
}
