using System.ComponentModel.DataAnnotations;

namespace IITS.Entities;

public class ManagedAccountSecurityGroup
{
    public Guid Id { get; set; }
    public Guid ManagedAccountId { get; set; }
    [Required, MaxLength(300)]
    public string GroupName { get; set; } = "";

    public ManagedAccount ManagedAccount { get; set; } = null!;
}
