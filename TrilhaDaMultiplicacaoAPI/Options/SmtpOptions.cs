namespace TrilhaDaMultiplicacaoAPI.Options;

public class SmtpOptions
{
    public const string SectionName = "Smtp";

    public string Host { get; set; } = "";
    public int Port { get; set; } = 587;
    public string Usuario { get; set; } = "";
    public string Senha { get; set; } = "";
    public string RemetenteNome { get; set; } = "Trilha da Multiplicação";
    public string RemetenteEmail { get; set; } = "";
}
