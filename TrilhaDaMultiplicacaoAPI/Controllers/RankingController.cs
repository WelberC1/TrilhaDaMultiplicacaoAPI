using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrilhaDaMultiplicacaoAPI.Dtos;
using TrilhaDaMultiplicacaoAPI.Exceptions;
using TrilhaDaMultiplicacaoAPI.Services;

namespace TrilhaDaMultiplicacaoAPI.Controllers;

[ApiController]
[Authorize]
[Route("api/ranking")]
public class RankingController(IRankingService rankingService, ICurrentUserService currentUser) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<RankingEntradaResponse>>> ObterRanking()
    {
        try
        {
            var ranking = await rankingService.ObterRankingAsync(currentUser.AlunoId);
            return Ok(ranking);
        }
        catch (ApiException ex)
        {
            return StatusCode(ex.StatusCode, new { mensagem = ex.Message });
        }
    }
}
