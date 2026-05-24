using IITS.Data;
using IITS.Entities;
using Microsoft.EntityFrameworkCore;

namespace IITS.Services;

public interface IEmailOutboxService
{
    Task EnqueueAsync(string to, string subject, string bodyHtml, string? cc = null);
    Task ProcessPendingAsync(int max = 10);
}

public class EmailOutboxService : IEmailOutboxService
{
    private readonly AppDbContext _db;
    private readonly IEmailSender _sender;
    private readonly ILogger<EmailOutboxService> _log;

    public EmailOutboxService(AppDbContext db, IEmailSender sender, ILogger<EmailOutboxService> log)
    {
        _db = db;
        _sender = sender;
        _log = log;
    }

    public async Task EnqueueAsync(string to, string subject, string bodyHtml, string? cc = null)
    {
        _db.EmailOutbox.Add(new EmailOutbox
        {
            Id = Guid.NewGuid(),
            To = to,
            Cc = cc,
            Subject = subject,
            BodyHtml = bodyHtml,
            CreatedAt = DateTime.UtcNow,
            Status = "Pending",
            RetryCount = 0
        });
        await _db.SaveChangesAsync();
    }

    public async Task ProcessPendingAsync(int max = 10)
    {
        var items = await _db.EmailOutbox
            .Where(e => e.Status == "Pending" && e.RetryCount < 5)
            .OrderBy(e => e.CreatedAt)
            .Take(max)
            .ToListAsync();

        foreach (var item in items)
        {
            try
            {
                await _sender.SendAsync(item.To, item.Subject, item.BodyHtml ?? "", null, item.Cc);
                item.SentAt = DateTime.UtcNow;
                item.Status = "Sent";
            }
            catch (Exception ex)
            {
                item.RetryCount++;
                item.Error = ex.Message.Length > 1000 ? ex.Message[..1000] : ex.Message;
                item.Status = item.RetryCount >= 5 ? "Failed" : "Pending";
                _log.LogWarning(ex, "EmailOutbox {Id} send failed", item.Id);
            }
            await _db.SaveChangesAsync();
        }
    }
}
