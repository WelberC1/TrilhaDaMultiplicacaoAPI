using Microsoft.AspNetCore.Http;

namespace TrilhaDaMultiplicacaoAPI.Exceptions;

public class NaoEncontradoException(string mensagem) : ApiException(mensagem, StatusCodes.Status404NotFound);
