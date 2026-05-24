using System.ComponentModel.DataAnnotations;

namespace IITS.Entities;

public class CuentaPrivilegiada
{
    public Guid Id { get; set; }
    [Required, MaxLength(300)]
    public string Nombre { get; set; } = "";
    public Guid EstatusId { get; set; }
    public Guid? AreaId { get; set; }
    [MaxLength(200)]
    public string? Responsable { get; set; }
    [MaxLength(200)]
    public string? Origen { get; set; }
    [MaxLength(300)]
    public string? ServicioRelacionado { get; set; }
    [MaxLength(50)]
    public string? TipoConfiguracionCambio { get; set; }
    public int? IntervaloCambioDias { get; set; }
    [MaxLength(2000)]
    public string? GruposSeguridad { get; set; }
    [MaxLength(500)]
    public string? Descripcion { get; set; }
    public Guid? AplicacionId { get; set; }
    public DateTime CreatedAt { get; set; }

    public Estatus Estatus { get; set; } = null!;
    public Area? Area { get; set; }
    public Aplicacion? Aplicacion { get; set; }
}
