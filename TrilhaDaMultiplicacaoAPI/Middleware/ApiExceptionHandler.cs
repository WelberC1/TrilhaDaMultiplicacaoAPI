using Microsoft.AspNetCore.Diagnostics;
using TrilhaDaMultiplicacaoAPI.Exceptions;

namespace TrilhaDaMultiplicacaoAPI.Middleware;

/// <summary>Traduz as <see cref="ApiException"/> lançadas pelos services em respostas HTTP,
/// mantendo os controllers livres de tratamento manual de erro.</summary>
public class ApiExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not ApiException apiException) return false;

        httpContext.Response.StatusCode = apiException.StatusCode;
        await httpContext.Response.WriteAsJsonAsync(new { mensagem = apiException.Message }, cancellationToken);

        return true;
    }
}
