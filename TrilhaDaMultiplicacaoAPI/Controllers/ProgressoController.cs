using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrilhaDaMultiplicacaoAPI.Dtos;
using TrilhaDaMultiplicacaoAPI.Exceptions;
using TrilhaDaMultiplicacaoAPI.Services;

namespace TrilhaDaMultiplicacaoAPI.Controllers;

[ApiController]
[Authorize]
[Route("api/progresso")]
public class ProgressoController(IProgressoService progressoService, ICurrentUserService currentUser) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<FaseProgressoResponse>>> ObterProgresso()
    {
        try
        {
            var progresso = await progressoService.ObterProgressoAsync(currentUser.AlunoId);
            return Ok(progresso);
        }
        catch (ApiException ex)
        {
            return StatusCode(ex.StatusCode, new { mensagem = ex.Message });
        }
    }

    [HttpPost("fases/{numeroFase:int}")]
    public async Task<ActionResult<FaseProgressoResponse>> RegistrarConclusao(int numeroFase, RegistrarConclusaoRequest request)
    {
        try
        {
            var resultado = await progressoService.RegistrarConclusaoAsync(currentUser.AlunoId, numeroFase, request);
            return Ok(resultado);
        }
        catch (ApiException ex)
        {
            return StatusCode(ex.StatusCode, new { mensagem = ex.Message });
        }
    }
}
