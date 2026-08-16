namespace TrilhaDaMultiplicacaoAPI.Models;

public class RefreshToken
{
    public int Id { get; set; }

    public int AlunoId { get; set; }
    public Aluno? Aluno { get; set; }

    public required string TokenHash { get; set; }

    public DateTime ExpiraEm { get; set; }
    public DateTime? RevogadoEm { get; set; }
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
}
