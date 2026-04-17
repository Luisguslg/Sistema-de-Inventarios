using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IITS.Entities;

public class Aplicacion
{
    public Guid Id { get; set; }

    [Required, MaxLength(300)]
    public string Nombre { get; set; } = "";

    [MaxLength(500)]
    public string Funcionalidad { get; set; } = "";

    [MaxLength(200)]
    public string Propietario { get; set; } = "";

    [MaxLength(200)]
    public string Responsable { get; set; } = "";

    [MaxLength(200)]
    public string TipoAlojamiento { get; set; } = "";

    [MaxLength(300)]
    public string Proveedor { get; set; } = "";

    [MaxLength(200)]
    public string ClasificacionInformacion { get; set; } = "";

    public bool Critico { get; set; }

    [MaxLength(500)]
    public string IntegracionesRelevantes { get; set; } = "";

    [MaxLength(500)]
    public string DependenciasTecnicas { get; set; } = "";

    [MaxLength(200)]
    public string? ModeloLicenciamiento { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? CostoAnualEstimado { get; set; }

    public DateTime? FechaAdquisicionImplementacion { get; set; }

    [MaxLength(100)]
    public string? VersionActual { get; set; }

    [MaxLength(200)]
    public string? SLA { get; set; }

    /// <summary>RTO: Recovery Time Objective (ISO-067-GCS).</summary>
    [MaxLength(100)]
    public string? RTO { get; set; }
    /// <summary>RPO: Recovery Point Objective (ISO-067-GCS).</summary>
    [MaxLength(100)]
    public string? RPO { get; set; }

    [MaxLength(200)]
    public string? Autenticacion { get; set; }

    public Guid EstatusId { get; set; }
    public DateTime CreatedAt { get; set; }

    public Estatus Estatus { get; set; } = null!;

    public Guid? AlojamientoId { get; set; }
    public Alojamiento? Alojamiento { get; set; }
}
