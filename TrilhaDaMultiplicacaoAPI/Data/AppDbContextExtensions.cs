using Microsoft.EntityFrameworkCore;

namespace TrilhaDaMultiplicacaoAPI.Data;

public static class AppDbContextExtensions
{
    /// <summary>
    /// Revoga todo refresh token ativo do aluno. Usado junto com a rotação do SecurityStamp
    /// (logout, troca de senha, redefinição de senha) — o stamp invalida o access token já
    /// emitido, isto invalida a capacidade de tirar um access token novo sem logar de novo.
    /// </summary>
    public static Task RevogarRefreshTokensAsync(this AppDbContext db, int alunoId) =>
        db.RefreshTokens
            .Where(t => t.AlunoId == alunoId && t.RevogadoEm == null)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.RevogadoEm, DateTime.UtcNow));
}
