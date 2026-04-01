using System.ComponentModel.DataAnnotations;

namespace IITS.Modelo;

/// <summary>Modelo para validación del formulario de Aplicaciones.</summary>
public class FormAplicacionModel
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [MaxLength(300, ErrorMessage = "Máximo 300 caracteres.")]
    [Display(Name = "Nombre")]
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
    public string ModeloLicenciamiento { get; set; } = "";

    public decimal? CostoAnualEstimado { get; set; }

    /// <summary>Representación en español (coma decimal) para el formulario. No persistir directamente.</summary>
    public string CostoAnualEstimadoDisplay { get; set; } = "";

    public DateTime? FechaAdquisicionImplementacion { get; set; }

    [MaxLength(100)]
    public string VersionActual { get; set; } = "";

    [MaxLength(200)]
    public string SLA { get; set; } = "";

    [MaxLength(200)]
    public string RPORTO { get; set; } = "";

    [MaxLength(200)]
    public string Autenticacion { get; set; } = "";

    [Required(ErrorMessage = "Seleccione un estatus.")]
    public string EstatusIdStr { get; set; } = "";

    public string AlojamientoIdStr { get; set; } = "";
}
