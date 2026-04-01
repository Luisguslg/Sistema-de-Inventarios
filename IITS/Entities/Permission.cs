using System.ComponentModel.DataAnnotations;

namespace IITS.Entities;

public class Permission
{
    public Guid Id { get; set; }
    [Required, MaxLength(100)]
    public string Code { get; set; } = "";
    [MaxLength(200)]
    public string Description { get; set; } = "";

    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
