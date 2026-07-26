namespace TrilhaDaMultiplicacaoAPI.Models;

public class PasswordResetToken
{
    public int Id { get; set; }

    public int AlunoId { get; set; }
    public Aluno? Aluno { get; set; }

    public required string CodigoHash { get; set; }
    public int TentativasFalhas { get; set; }
    public DateTime ExpiraEm { get; set; }
    public DateTime? UsadoEm { get; set; }
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
}
