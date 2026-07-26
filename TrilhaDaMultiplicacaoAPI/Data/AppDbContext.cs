using Microsoft.EntityFrameworkCore;
using TrilhaDaMultiplicacaoAPI.Models;

namespace TrilhaDaMultiplicacaoAPI.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Aluno> Alunos => Set<Aluno>();
    public DbSet<FaseProgresso> FasesProgresso => Set<FaseProgresso>();
    public DbSet<Conquista> Conquistas => Set<Conquista>();
    public DbSet<AlunoConquista> AlunoConquistas => Set<AlunoConquista>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Aluno>()
            .HasIndex(a => a.Email)
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

        modelBuilder.Entity<Conquista>().HasData(
            new Conquista { Id = 1, Titulo = "Primeiros passos", Descricao = "Concluiu a primeira fase da trilha.", Icone = "🌱", FasesConcluidasNecessarias = 1 },
            new Conquista { Id = 2, Titulo = "Pegando o jeito", Descricao = "Concluiu 3 fases da trilha.", Icone = "🔥", FasesConcluidasNecessarias = 3 },
            new Conquista { Id = 3, Titulo = "Mestre da multiplicação", Descricao = "Concluiu 6 fases da trilha.", Icone = "🏆", FasesConcluidasNecessarias = 6 }
        );
    }
}
