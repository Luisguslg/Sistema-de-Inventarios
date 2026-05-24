using System.ComponentModel.DataAnnotations;

namespace IITS.Entities;

public class User
{
    public Guid Id { get; set; }
    [Required, MaxLength(100)]
    public string Username { get; set; } = "";
    [MaxLength(150)]
    public string Nombre { get; set; } = "";
    [MaxLength(150)]
    public string Apellido { get; set; } = "";
    [MaxLength(200)]
    public string Email { get; set; } = "";
    [MaxLength(50)]
    public string CodSap { get; set; } = "";

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
