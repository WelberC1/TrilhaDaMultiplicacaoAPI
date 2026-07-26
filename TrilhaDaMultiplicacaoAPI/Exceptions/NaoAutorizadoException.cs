using Microsoft.AspNetCore.Http;

namespace TrilhaDaMultiplicacaoAPI.Exceptions;

public class NaoAutorizadoException(string mensagem) : ApiException(mensagem, StatusCodes.Status401Unauthorized);
