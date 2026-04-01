using System.ComponentModel.DataAnnotations;

namespace IITS.Entities;

public class EmailOutbox
{
    public Guid Id { get; set; }
    [Required, MaxLength(500)]
    public string To { get; set; } = "";
    [MaxLength(500)]
    public string? Cc { get; set; }
    [Required, MaxLength(300)]
    public string Subject { get; set; } = "";
    public string? BodyHtml { get; set; }
    public string? BodyText { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? SentAt { get; set; }
    [Required, MaxLength(20)]
    public string Status { get; set; } = "Pending";
    [MaxLength(1000)]
    public string? Error { get; set; }
    public int RetryCount { get; set; }
}
