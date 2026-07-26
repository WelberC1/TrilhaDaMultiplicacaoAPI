namespace TrilhaDaMultiplicacaoAPI.Models;

public class FaseProgresso
{
    public int Id { get; set; }

    public int AlunoId { get; set; }
    public Aluno? Aluno { get; set; }

    public required int NumeroFase { get; set; }
    public int Estrelas { get; set; }
    public int Pontos { get; set; }
    public DateTime ConcluidaEm { get; set; } = DateTime.UtcNow;
}
