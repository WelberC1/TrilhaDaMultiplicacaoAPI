using System.IdentityModel.Tokens.Jwt;
using TrilhaDaMultiplicacaoAPI.Exceptions;

namespace TrilhaDaMultiplicacaoAPI.Services;

public interface ICurrentUserService
{
    /// <summary>Id do aluno autenticado, extraído da claim "sub" do token JWT da requisição atual.</summary>
    int AlunoId { get; }
}

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public int AlunoId
    {
        get
        {
            var sub = httpContextAccessor.HttpContext?.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

            if (sub is null || !int.TryParse(sub, out var alunoId))
                throw new NaoAutorizadoException("Token inválido.");

            return alunoId;
        }
    }
}
