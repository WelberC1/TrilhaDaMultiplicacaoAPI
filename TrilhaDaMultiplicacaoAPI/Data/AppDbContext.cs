using Microsoft.EntityFrameworkCore;
using TrilhaDaMultiplicacaoAPI.Models;

namespace TrilhaDaMultiplicacaoAPI.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Aluno> Alunos => Set<Aluno>();
    public DbSet<FaseProgresso> FasesProgresso => Set<FaseProgresso>();
    public DbSet<Conquista> Conquistas => Set<Conquista>();
    public DbSet<AlunoConquista> AlunoConquistas => Set<AlunoConquista>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Aluno>()
            .HasIndex(a => a.Email)
            .IsUnique();

        modelBuilder.Entity<Aluno>()
            .HasIndex(a => a.NomeUsuario)
            .IsUnique();

        modelBuilder.Entity<FaseProgresso>()
            .HasIndex(p => new { p.AlunoId, p.NumeroFase })
            .IsUnique();

        modelBuilder.Entity<AlunoConquista>()
            .HasKey(ac => new { ac.AlunoId, ac.ConquistaId });

        modelBuilder.Entity<AlunoConquista>()
            .HasOne(ac => ac.Aluno)
            .WithMany(a => a.Conquistas)
            .HasForeignKey(ac => ac.AlunoId);

        modelBuilder.Entity<AlunoConquista>()
            .HasOne(ac => ac.Conquista)
            .WithMany(c => c.AlunosQueDesbloquearam)
            .HasForeignKey(ac => ac.ConquistaId);

        modelBuilder.Entity<PasswordResetToken>()
            .HasOne(t => t.Aluno)
            .WithMany()
            .HasForeignKey(t => t.AlunoId);

        modelBuilder.Entity<RefreshToken>()
            .HasOne(t => t.Aluno)
            .WithMany()
            .HasForeignKey(t => t.AlunoId);

        modelBuilder.Entity<RefreshToken>()
            .HasIndex(t => t.TokenHash)
            .IsUnique();

        modelBuilder.Entity<Conquista>().HasData(
            new Conquista { Id = 1, Titulo = "Primeiro Passo", Descricao = "Complete a sua primeira fase da trilha.", Icone = "🥇", TipoCriterio = TipoCriterioConquista.FasesConcluidas, ValorNecessario = 1 },
            new Conquista { Id = 2, Titulo = "Trinca de Ouro", Descricao = "Consiga 3 estrelas em pelo menos uma fase.", Icone = "🌟", TipoCriterio = TipoCriterioConquista.FasesComTresEstrelas, ValorNecessario = 1 },
            new Conquista { Id = 3, Titulo = "Sequência de Craque", Descricao = "Complete 3 fases da trilha.", Icone = "🔥", TipoCriterio = TipoCriterioConquista.FasesConcluidas, ValorNecessario = 3 },
            new Conquista { Id = 4, Titulo = "Meio Caminho", Descricao = "Complete 6 fases da trilha.", Icone = "🏃", TipoCriterio = TipoCriterioConquista.FasesConcluidas, ValorNecessario = 6 },
            new Conquista { Id = 5, Titulo = "Trilha Completa", Descricao = "Complete as 12 fases da trilha da multiplicação.", Icone = "🏆", TipoCriterio = TipoCriterioConquista.FasesConcluidas, ValorNecessario = 12 },
            new Conquista { Id = 6, Titulo = "Estrela em Dobro", Descricao = "Consiga 3 estrelas em 5 fases diferentes.", Icone = "✨", TipoCriterio = TipoCriterioConquista.FasesComTresEstrelas, ValorNecessario = 5 },
            new Conquista { Id = 7, Titulo = "Colecionador de Pontos", Descricao = "Acumule 300 pontos.", Icone = "💰", TipoCriterio = TipoCriterioConquista.PontosTotais, ValorNecessario = 300 },
            new Conquista { Id = 8, Titulo = "Mestre da Trilha", Descricao = "Acumule 800 pontos.", Icone = "👑", TipoCriterio = TipoCriterioConquista.PontosTotais, ValorNecessario = 800 }
        );
    }
}
