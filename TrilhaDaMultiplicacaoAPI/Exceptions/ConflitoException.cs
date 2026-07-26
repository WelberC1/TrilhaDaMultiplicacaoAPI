using Microsoft.AspNetCore.Http;

namespace TrilhaDaMultiplicacaoAPI.Exceptions;

public class ConflitoException(string mensagem) : ApiException(mensagem, StatusCodes.Status409Conflict);
