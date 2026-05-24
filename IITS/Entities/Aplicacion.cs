using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IITS.Entities;

/// <summary>
/// Catálogo de aplicaciones. Campos según maestro "Catalogo de Aplicaciones.xlsx" (Ejemplo).
/// Propietario y Responsable son texto libre (Partes fue eliminado).
/// </summary>
public class Aplicacion
{
    public Guid Id { get; set; }

    // CWE-209: mensajes de error genéricos, sin exponer nombres de propiedades internas
    [Required(ErrorMessage = "El nombre de la aplicación es requerido.")]
    [MaxLength(300, ErrorMessage = "El nombre de la aplicación es demasiado largo.")]
    public string Nombre { get; set; } = "";

    [MaxLength(500, ErrorMessage = "La descripción de funcionalidad es demasiado larga.")]
    public string Funcionalidad { get; set; } = "";

    [MaxLength(200, ErrorMessage = "El campo del propietario es demasiado largo.")]
    public string Propietario { get; set; } = "";

    [MaxLength(200, ErrorMessage = "El campo del responsable es demasiado largo.")]
    public string Responsable { get; set; } = "";

    [MaxLength(200, ErrorMessage = "El campo Tipo de alojamiento es demasiado largo.")]
    public string TipoAlojamiento { get; set; } = "";

    [MaxLength(300, ErrorMessage = "El campo del proveedor es demasiado largo.")]
    public string Proveedor { get; set; } = "";

    [MaxLength(200, ErrorMessage = "El campo Clasificación de información es demasiado largo.")]
    public string ClasificacionInformacion { get; set; } = "";

    public bool Critico { get; set; }

    [MaxLength(500, ErrorMessage = "El campo Integraciones relevantes es demasiado largo.")]
    public string IntegracionesRelevantes { get; set; } = "";

    [MaxLength(500, ErrorMessage = "El campo Dependencias técnicas es demasiado largo.")]
    public string DependenciasTecnicas { get; set; } = "";

    [MaxLength(200, ErrorMessage = "El campo Modelo de licenciamiento es demasiado largo.")]
    public string? ModeloLicenciamiento { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? CostoAnualEstimado { get; set; }

    public DateTime? FechaAdquisicionImplementacion { get; set; }

    [MaxLength(100, ErrorMessage = "El campo Versión actual es demasiado largo.")]
    public string? VersionActual { get; set; }

    [MaxLength(200, ErrorMessage = "El campo SLA es demasiado largo.")]
    public string? SLA { get; set; }

    /// <summary>RTO: Recovery Time Objective (ISO-067-GCS).</summary>
    [MaxLength(100, ErrorMessage = "El campo RTO es demasiado largo.")]
    public string? RTO { get; set; }
    /// <summary>RPO: Recovery Point Objective (ISO-067-GCS).</summary>
    [MaxLength(100, ErrorMessage = "El campo RPO es demasiado largo.")]
    public string? RPO { get; set; }

    [MaxLength(200, ErrorMessage = "El campo Autenticación es demasiado largo.")]
    public string? Autenticacion { get; set; }

    public Guid EstatusId { get; set; }
    public DateTime CreatedAt { get; set; }

    public Estatus Estatus { get; set; } = null!;

    public Guid? AlojamientoId { get; set; }
    public Alojamiento? Alojamiento { get; set; }
}
