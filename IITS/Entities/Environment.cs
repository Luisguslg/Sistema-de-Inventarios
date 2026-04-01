using System.ComponentModel.DataAnnotations;

namespace IITS.Entities;

public class Environment
{
    public Guid Id { get; set; }
    [Required, MaxLength(100)]
    public string Name { get; set; } = "";
}
