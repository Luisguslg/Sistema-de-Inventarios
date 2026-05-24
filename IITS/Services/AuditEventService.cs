using IITS.Data;
using IITS.Entities;
using Microsoft.EntityFrameworkCore;

namespace IITS.Services;

public class AuditEventService : IAuditEventService
{
    private readonly AppDbContext _db;

    public AuditEventService(AppDbContext db) => _db = db;

    public async Task RecordAsync(string entityType, string entityId, string action, Guid? userId, string? beforeJson = null, string? afterJson = null, string? comment = null, string? correlationId = null)
    {
        _db.AuditEvents.Add(new AuditEvent
        {
            Id = Guid.NewGuid(),
            EntityType = entityType,
            EntityId = entityId,
            Action = action,
            PerformedByUserId = userId,
            PerformedAt = DateTime.UtcNow,
            BeforeJson = beforeJson,
            AfterJson = afterJson,
            Comment = comment,
            CorrelationId = correlationId
        });
        await _db.SaveChangesAsync();
    }
}
