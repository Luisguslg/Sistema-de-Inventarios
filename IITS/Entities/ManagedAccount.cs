using System.ComponentModel.DataAnnotations;

namespace IITS.Entities;

public class ManagedAccount
{
    public Guid Id { get; set; }
    public Guid? AreaId { get; set; }
    [MaxLength(200)]
    public string? Responsible { get; set; }
    [Required, MaxLength(200)]
    public string AccountName { get; set; } = "";
    public int AccountType { get; set; }
    [MaxLength(100)]
    public string? Origin { get; set; }
    [MaxLength(200)]
    public string? RelatedService { get; set; }
    [MaxLength(100)]
    public string? ChangeConfigType { get; set; }
    public int? ChangeIntervalDays { get; set; }
    public Guid? EstatusId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public Area? Area { get; set; }
    public Estatus? Estatus { get; set; }
    public ICollection<ManagedAccountSecurityGroup> SecurityGroups { get; set; } = new List<ManagedAccountSecurityGroup>();
}
