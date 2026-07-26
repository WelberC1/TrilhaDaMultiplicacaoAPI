namespace TrilhaDaMultiplicacaoAPI.Exceptions;

/// <summary>Exceção base para erros de negócio que devem virar uma resposta HTTP com status específico.</summary>
public abstract class ApiException(string mensagem, int statusCode) : Exception(mensagem)
{
    public int StatusCode { get; } = statusCode;
}
