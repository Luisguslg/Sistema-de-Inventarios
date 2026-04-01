using System.ComponentModel.DataAnnotations;

namespace IITS.Entities;

public class Role
{
    public Guid Id { get; set; }
    [Required, MaxLength(50)]
    public string Nombre { get; set; } = "";

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
