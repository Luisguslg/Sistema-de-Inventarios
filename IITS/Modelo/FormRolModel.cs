using System.ComponentModel.DataAnnotations;

namespace IITS.Modelo;

public class FormRolModel
{
    [Required(ErrorMessage = "El nombre del rol es obligatorio.")]
    [MaxLength(50)]
    public string Nombre { get; set; } = "";
}
