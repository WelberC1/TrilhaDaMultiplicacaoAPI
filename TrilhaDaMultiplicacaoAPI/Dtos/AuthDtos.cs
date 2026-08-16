using System.ComponentModel.DataAnnotations;

namespace TrilhaDaMultiplicacaoAPI.Dtos;

public record RegistrarRequest(
    [Required, MinLength(2)] string Nome,
    [Required, MinLength(3), MaxLength(30), RegularExpression(@"^[a-zA-Z0-9_.]+$",
        ErrorMessage = "O usuário só pode ter letras, números, ponto e underline.")]
    string NomeUsuario,
    [Required, EmailAddress] string Email,
    [Required, MinLength(6)] string Senha
);

public record LoginRequest(
    [Required] string NomeUsuario,
    [Required] string Senha
);

public record AuthResponse(string Token, string RefreshToken, AlunoResponse Aluno);

public record RefreshTokenRequest([Required] string RefreshToken);

public record EsqueciSenhaRequest([Required, EmailAddress] string Email);

public record RedefinirSenhaRequest(
    [Required, EmailAddress] string Email,
    [Required, StringLength(6, MinimumLength = 6)] string Codigo,
    [Required, MinLength(6)] string NovaSenha
);
