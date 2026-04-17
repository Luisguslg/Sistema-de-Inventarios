using System.ComponentModel.DataAnnotations;

namespace IITS.Entities;

public class AprobacionPermiso
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    [Required, MaxLength(50)]
    public string Modulo { get; set; } = "";

    public User User { get; set; } = null!;
}
