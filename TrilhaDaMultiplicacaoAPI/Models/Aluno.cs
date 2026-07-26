namespace TrilhaDaMultiplicacaoAPI.Models;

public class Aluno
{
    public int Id { get; set; }
    public required string Nome { get; set; }
    public required string NomeUsuario { get; set; }
    public required string Email { get; set; }
    public required string SenhaHash { get; set; }
    public string AvatarEmoji { get; set; } = "🦉";
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

    public int TentativasLoginFalhas { get; set; }
    public DateTime? BloqueadoAte { get; set; }

    /// <summary>Rotacionado a cada logout ou troca de senha, pra invalidar tokens já emitidos antes de expirarem.</summary>
    public Guid SecurityStamp { get; set; } = Guid.NewGuid();

    public List<FaseProgresso> Progresso { get; set; } = [];
    public List<AlunoConquista> Conquistas { get; set; } = [];

    public int PontosTotais => Progresso.Sum(p => p.Pontos);
}
