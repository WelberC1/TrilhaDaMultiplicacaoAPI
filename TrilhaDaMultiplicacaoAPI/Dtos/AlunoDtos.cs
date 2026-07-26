using System.ComponentModel.DataAnnotations;

namespace TrilhaDaMultiplicacaoAPI.Dtos;

public record AlunoResponse(int Id, string Nome, string NomeUsuario, string Email, string AvatarEmoji, int PontosTotais);

public record AtualizarPerfilRequest(
    [Required, MinLength(2)] string Nome,
    [Required, EmailAddress] string Email,
    [Required] string AvatarEmoji
);
