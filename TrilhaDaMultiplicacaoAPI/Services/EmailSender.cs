using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using TrilhaDaMultiplicacaoAPI.Options;

namespace TrilhaDaMultiplicacaoAPI.Services;

public interface IEmailSender
{
    Task EnviarAsync(string destinatario, string assunto, string corpoHtml);
}

/// <summary>Envia e-mail de verdade via SMTP. Usado quando <c>Smtp:Host</c> está configurado.</summary>
public class SmtpEmailSender(IOptions<SmtpOptions> options) : IEmailSender
{
    public async Task EnviarAsync(string destinatario, string assunto, string corpoHtml)
    {
        var smtp = options.Value;

        var mensagem = new MimeMessage();
        mensagem.From.Add(new MailboxAddress(smtp.RemetenteNome, smtp.RemetenteEmail));
        mensagem.To.Add(MailboxAddress.Parse(destinatario));
        mensagem.Subject = assunto;
        mensagem.Body = new TextPart("html") { Text = corpoHtml };

        using var cliente = new SmtpClient();
        await cliente.ConnectAsync(smtp.Host, smtp.Port, SecureSocketOptions.Auto);
        await cliente.AuthenticateAsync(smtp.Usuario, smtp.Senha);
        await cliente.SendAsync(mensagem);
        await cliente.DisconnectAsync(true);
    }
}

/// <summary>Loga o e-mail em vez de enviar. Usado em desenvolvimento quando <c>Smtp:Host</c> não está configurado.</summary>
public class ConsoleEmailSender(ILogger<ConsoleEmailSender> logger) : IEmailSender
{
    public Task EnviarAsync(string destinatario, string assunto, string corpoHtml)
    {
        logger.LogInformation(
            "(Smtp não configurado) E-mail para {Destinatario} — {Assunto}:\n{Corpo}",
            destinatario, assunto, corpoHtml);

        return Task.CompletedTask;
    }
}
