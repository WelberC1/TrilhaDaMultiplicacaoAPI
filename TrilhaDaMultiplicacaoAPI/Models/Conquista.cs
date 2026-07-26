namespace TrilhaDaMultiplicacaoAPI.Models;

public class Conquista
{
    public int Id { get; set; }
    public required string Titulo { get; set; }
    public required string Descricao { get; set; }
    public required string Icone { get; set; }

    /// <summary>Número de fases concluídas necessário para desbloquear esta conquista automaticamente.</summary>
    public required int FasesConcluidasNecessarias { get; set; }

    public List<AlunoConquista> AlunosQueDesbloquearam { get; set; } = [];
}
