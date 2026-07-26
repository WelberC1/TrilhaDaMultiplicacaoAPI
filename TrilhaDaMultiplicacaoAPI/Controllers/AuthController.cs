using Microsoft.AspNetCore.Mvc;
using TrilhaDaMultiplicacaoAPI.Dtos;
using TrilhaDaMultiplicacaoAPI.Exceptions;
using TrilhaDaMultiplicacaoAPI.Services;

namespace TrilhaDaMultiplicacaoAPI.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("registrar")]
    public async Task<ActionResult<AuthResponse>> Registrar(RegistrarRequest request)
    {
        try
        {
            var resultado = await authService.RegistrarAsync(request);
            return Ok(resultado);
        }
        catch (ApiException ex)
        {
            return StatusCode(ex.StatusCode, new { mensagem = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        try
        {
            var resultado = await authService.LoginAsync(request);
            return Ok(resultado);
        }
        catch (ApiException ex)
        {
            return StatusCode(ex.StatusCode, new { mensagem = ex.Message });
        }
    }
}
