using System.ComponentModel.DataAnnotations;

namespace IITS.Entities;

public class Aprobacion
{
    public Guid Id { get; set; }
    [MaxLength(50)]
    public string Modulo { get; set; } = "";
    public Guid EntidadId { get; set; }
    [MaxLength(50)]
    public string Estado { get; set; } = "";
    public string? Comentario { get; set; }
    public Guid? UsuarioId { get; set; }
    public DateTime Fecha { get; set; }
    /// <summary>"Crear" o "Editar". Null en registros legacy (se trata como "Editar").</summary>
    [MaxLength(20)]
    public string? TipoAccion { get; set; }
    /// <summary>JSON con los datos propuestos para ediciones pendientes. Null para altas.</summary>
    public string? DatosPropuestos { get; set; }
}
