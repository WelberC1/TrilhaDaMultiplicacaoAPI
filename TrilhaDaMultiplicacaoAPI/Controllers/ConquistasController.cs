using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrilhaDaMultiplicacaoAPI.Dtos;
using TrilhaDaMultiplicacaoAPI.Exceptions;
using TrilhaDaMultiplicacaoAPI.Services;

namespace TrilhaDaMultiplicacaoAPI.Controllers;

[ApiController]
[Authorize]
[Route("api/conquistas")]
public class ConquistasController(IConquistaService conquistaService, ICurrentUserService currentUser) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ConquistaResponse>>> ObterConquistas()
    {
        try
        {
            var conquistas = await conquistaService.ObterConquistasAsync(currentUser.AlunoId);
            return Ok(conquistas);
        }
        catch (ApiException ex)
        {
            return StatusCode(ex.StatusCode, new { mensagem = ex.Message });
        }
    }
}
