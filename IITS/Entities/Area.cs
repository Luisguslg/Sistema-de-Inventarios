using System.ComponentModel.DataAnnotations;

namespace IITS.Entities;

public class Area
{
    public Guid Id { get; set; }
    [Required, MaxLength(100)]
    public string Name { get; set; } = "";
}
