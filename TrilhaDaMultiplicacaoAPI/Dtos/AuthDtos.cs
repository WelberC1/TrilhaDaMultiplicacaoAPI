using System.ComponentModel.DataAnnotations;

namespace TrilhaDaMultiplicacaoAPI.Dtos;

public record RegistrarRequest(
    [Required, MinLength(2)] string Nome,
    [Required, EmailAddress] string Email,
    [Required, MinLength(6)] string Senha
);

public record LoginRequest(
    [Required, EmailAddress] string Email,
    [Required] string Senha
);

public record AuthResponse(string Token, AlunoResponse Aluno);

public record EsqueciSenhaRequest([Required, EmailAddress] string Email);

public record RedefinirSenhaRequest(
    [Required, EmailAddress] string Email,
    [Required, StringLength(6, MinimumLength = 6)] string Codigo,
    [Required, MinLength(6)] string NovaSenha
);
