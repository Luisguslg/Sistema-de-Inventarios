using System.ComponentModel.DataAnnotations;

namespace IITS.Entities;

public class AuditEvent
{
    public Guid Id { get; set; }
    [Required, MaxLength(50)]
    public string EntityType { get; set; } = "";
    [Required, MaxLength(50)]
    public string EntityId { get; set; } = "";
    [Required, MaxLength(50)]
    public string Action { get; set; } = "";
    public Guid? PerformedByUserId { get; set; }
    public DateTime PerformedAt { get; set; }
    public string? BeforeJson { get; set; }
    public string? AfterJson { get; set; }
    [MaxLength(500)]
    public string? Comment { get; set; }
    [MaxLength(50)]
    public string? CorrelationId { get; set; }
}
