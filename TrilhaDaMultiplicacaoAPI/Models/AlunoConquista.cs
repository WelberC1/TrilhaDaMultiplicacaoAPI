namespace TrilhaDaMultiplicacaoAPI.Models;

public class AlunoConquista
{
    public int AlunoId { get; set; }
    public Aluno? Aluno { get; set; }

    public int ConquistaId { get; set; }
    public Conquista? Conquista { get; set; }

    public DateTime DesbloqueadaEm { get; set; } = DateTime.UtcNow;
}
