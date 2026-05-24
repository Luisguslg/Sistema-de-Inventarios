using System.ComponentModel.DataAnnotations;

namespace IITS.Modelo;

public class FormUsuarioModel
{
    [Required(ErrorMessage = "El usuario (username) es obligatorio.")]
    [MaxLength(100)]
    public string Username { get; set; } = "";

    [MaxLength(150)]
    public string Nombre { get; set; } = "";

    [MaxLength(150)]
    public string Apellido { get; set; } = "";

    [MaxLength(200)]
    public string Email { get; set; } = "";

    [MaxLength(50)]
    public string CodSap { get; set; } = "";

    public List<Guid> RoleIds { get; set; } = new();
}
