using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TrilhaDaMultiplicacaoAPI.Dtos;
using TrilhaDaMultiplicacaoAPI.Exceptions;
using TrilhaDaMultiplicacaoAPI.Services;

namespace TrilhaDaMultiplicacaoAPI.Controllers;

[ApiController]
[Route("api/auth")]
[EnableRateLimiting("auth")]
public class AuthController(IAuthService authService, ICurrentUserService currentUser) : ControllerBase
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

    [HttpPost("esqueci-senha")]
    public async Task<ActionResult> EsqueciSenha(EsqueciSenhaRequest request)
    {
        try
        {
            await authService.EsqueciSenhaAsync(request);
            return Ok(new { mensagem = "Se o e-mail existir, enviamos um código de recuperação." });
        }
        catch (ApiException ex)
        {
            return StatusCode(ex.StatusCode, new { mensagem = ex.Message });
        }
    }

    [HttpPost("redefinir-senha")]
    public async Task<ActionResult> RedefinirSenha(RedefinirSenhaRequest request)
    {
        try
        {
            await authService.RedefinirSenhaAsync(request);
            return Ok(new { mensagem = "Senha redefinida com sucesso!" });
        }
        catch (ApiException ex)
        {
            return StatusCode(ex.StatusCode, new { mensagem = ex.Message });
        }
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<ActionResult> Logout()
    {
        try
        {
            await authService.LogoutAsync(currentUser.AlunoId);
            return Ok(new { mensagem = "Sessão encerrada." });
        }
        catch (ApiException ex)
        {
            return StatusCode(ex.StatusCode, new { mensagem = ex.Message });
        }
    }
}
