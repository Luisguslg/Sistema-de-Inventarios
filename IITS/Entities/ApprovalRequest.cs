using System.ComponentModel.DataAnnotations;

namespace IITS.Entities;

public class ApprovalRequest
{
    public Guid Id { get; set; }
    [Required, MaxLength(50)]
    public string EntityType { get; set; } = "";
    [Required, MaxLength(50)]
    public string EntityId { get; set; } = "";
    public Guid? AreaId { get; set; }
    [Required, MaxLength(20)]
    public string Status { get; set; } = "Pending";
    public Guid? SubmittedByUserId { get; set; }
    public DateTime SubmittedAt { get; set; }
    public int CurrentStep { get; set; } = 1;
    [MaxLength(500)]
    public string? Summary { get; set; }

    public User? SubmittedByUser { get; set; }
    public ICollection<ApprovalDecision> Decisions { get; set; } = new List<ApprovalDecision>();
}
