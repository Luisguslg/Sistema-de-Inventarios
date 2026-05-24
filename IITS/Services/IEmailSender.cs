namespace IITS.Services;

public interface IEmailSender
{
    Task SendAsync(string to, string subject, string bodyHtml, string? bodyText = null, string? cc = null);
}
