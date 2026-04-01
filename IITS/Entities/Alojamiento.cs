using System.ComponentModel.DataAnnotations;

namespace IITS.Entities;

public class Alojamiento
{
    public Guid Id { get; set; }
    [Required, MaxLength(150)]
    public string Nombre { get; set; } = "";
}
