using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TrilhaDaMultiplicacaoAPI.Models;
using TrilhaDaMultiplicacaoAPI.Options;

namespace TrilhaDaMultiplicacaoAPI.Services;

public interface ITokenService
{
    string GerarToken(Aluno aluno);
}

public class TokenService(IOptions<JwtOptions> jwtOptions) : ITokenService
{
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;

    public string GerarToken(Aluno aluno)
    {
        var chave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Chave));
        var credenciais = new SigningCredentials(chave, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, aluno.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, aluno.Email),
            new Claim(ClaimTypes.Name, aluno.Nome)
        };

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Emissor,
            audience: _jwtOptions.Audiencia,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: credenciais
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
