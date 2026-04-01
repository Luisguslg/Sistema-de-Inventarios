using System.ComponentModel.DataAnnotations;

namespace IITS.Entities;

public class AuditLog
{
    public Guid Id { get; set; }
    [MaxLength(100)]
    public string Tabla { get; set; } = "";
    public Guid EntidadId { get; set; }
    [MaxLength(50)]
    public string Accion { get; set; } = "";
    public Guid? UsuarioId { get; set; }
    public DateTime Fecha { get; set; }
    public string? Detalle { get; set; }
}
