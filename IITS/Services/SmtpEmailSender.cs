using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;

namespace IITS.Services;

public class SmtpEmailSenderOptions
{
    public const string SectionName = "Email";
    public string From { get; set; } = "";
    public string SmtpServer { get; set; } = "";
    public int Port { get; set; } = 25;
    public bool EnableSsl { get; set; }
    public string? UserName { get; set; }
    public string? Password { get; set; }
}

public class SmtpEmailSender : IEmailSender
{
    private readonly SmtpEmailSenderOptions _options;
    private readonly ILogger<SmtpEmailSender> _log;

    public SmtpEmailSender(IOptions<SmtpEmailSenderOptions> options, ILogger<SmtpEmailSender> log)
    {
        _options = options.Value;
        _log = log;
    }

    public async Task SendAsync(string to, string subject, string bodyHtml, string? bodyText = null, string? cc = null)
    {
        if (string.IsNullOrEmpty(_options.SmtpServer))
        {
            _log.LogWarning("Email:SmtpServer no configurado; no se envía correo.");
            return;
        }

        using var client = new SmtpClient(_options.SmtpServer, _options.Port)
        {
            EnableSsl = _options.EnableSsl,
            DeliveryMethod = SmtpDeliveryMethod.Network
        };
        if (!string.IsNullOrEmpty(_options.UserName))
            client.Credentials = new NetworkCredential(_options.UserName, _options.Password);

        var from = string.IsNullOrEmpty(_options.From) ? "noreply@localhost" : _options.From;
        var body = !string.IsNullOrEmpty(bodyHtml) ? bodyHtml : (bodyText ?? "");
        using var msg = new MailMessage(from, to, subject, body)
        {
            IsBodyHtml = !string.IsNullOrEmpty(bodyHtml)
        };
        if (!string.IsNullOrEmpty(cc))
            msg.CC.Add(cc);

        await client.SendMailAsync(msg);
        _log.LogInformation("Email enviado a {To}, asunto: {Subject}", to, subject);
    }
}
