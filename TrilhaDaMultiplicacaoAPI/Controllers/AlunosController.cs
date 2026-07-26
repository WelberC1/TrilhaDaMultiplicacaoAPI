using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TrilhaDaMultiplicacaoAPI.Dtos;
using TrilhaDaMultiplicacaoAPI.Exceptions;
using TrilhaDaMultiplicacaoAPI.Services;

namespace TrilhaDaMultiplicacaoAPI.Controllers;

[ApiController]
[Authorize]
[Route("api/alunos")]
public class AlunosController(IAlunoService alunoService, ICurrentUserService currentUser) : ControllerBase
{
    [HttpGet("me")]
    public async Task<ActionResult<AlunoResponse>> ObterPerfil()
    {
        try
        {
            var perfil = await alunoService.ObterPerfilAsync(currentUser.AlunoId);
            return Ok(perfil);
        }
        catch (ApiException ex)
        {
            return StatusCode(ex.StatusCode, new { mensagem = ex.Message });
        }
    }

    [HttpPut("me")]
    public async Task<ActionResult<AlunoResponse>> AtualizarPerfil(AtualizarPerfilRequest request)
    {
        try
        {
            var perfil = await alunoService.AtualizarPerfilAsync(currentUser.AlunoId, request);
            return Ok(perfil);
        }
        catch (ApiException ex)
        {
            return StatusCode(ex.StatusCode, new { mensagem = ex.Message });
        }
    }

    [HttpPut("me/senha")]
    [EnableRateLimiting("auth")]
    public async Task<ActionResult> AlterarSenha(AlterarSenhaRequest request)
    {
        try
        {
            await alunoService.AlterarSenhaAsync(currentUser.AlunoId, request);
            return Ok(new { mensagem = "Senha alterada com sucesso!" });
        }
        catch (ApiException ex)
        {
            return StatusCode(ex.StatusCode, new { mensagem = ex.Message });
        }
    }
}
