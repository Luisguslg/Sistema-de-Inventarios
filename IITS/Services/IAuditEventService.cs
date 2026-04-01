namespace IITS.Services;

public interface IAuditEventService
{
    Task RecordAsync(string entityType, string entityId, string action, Guid? userId, string? beforeJson = null, string? afterJson = null, string? comment = null, string? correlationId = null);
}
