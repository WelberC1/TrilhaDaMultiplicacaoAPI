namespace TrilhaDaMultiplicacaoAPI.Options;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    public required string Emissor { get; set; }
    public required string Audiencia { get; set; }
    public required string Chave { get; set; }
}
