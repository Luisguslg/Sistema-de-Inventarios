namespace IITS.Services;

public class DevEmailSender : IEmailSender
{
    private readonly ILogger<DevEmailSender> _log;

    public DevEmailSender(ILogger<DevEmailSender> log) => _log = log;

    public Task SendAsync(string to, string subject, string bodyHtml, string? bodyText = null, string? cc = null)
    {
        _log.LogInformation("[Dev] Email: To={To}, Subject={Subject}", to, subject);
        return Task.CompletedTask;
    }
}
