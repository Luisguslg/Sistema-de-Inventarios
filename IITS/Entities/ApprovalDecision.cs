using System.ComponentModel.DataAnnotations;

namespace IITS.Entities;

public class ApprovalDecision
{
    public Guid Id { get; set; }
    public Guid ApprovalRequestId { get; set; }
    [Required, MaxLength(20)]
    public string Decision { get; set; } = "";
    [Required]
    public string Comment { get; set; } = "";
    public Guid? DecidedByUserId { get; set; }
    public DateTime DecidedAt { get; set; }

    public ApprovalRequest ApprovalRequest { get; set; } = null!;
    public User? DecidedByUser { get; set; }
}
