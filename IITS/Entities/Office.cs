using System.ComponentModel.DataAnnotations;

namespace IITS.Entities;

public class Office
{
    public Guid Id { get; set; }
    [Required, MaxLength(150)]
    public string Name { get; set; } = "";
}
