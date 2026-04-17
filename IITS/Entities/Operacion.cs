using System.ComponentModel.DataAnnotations;

namespace IITS.Entities;

/// <summary>Activos de operaciones. Campos según "Campos de Activos" (Campos.xlsx): obligatorios y solo aplicables a dispositivos físicos.</summary>
public class Operacion
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }

    public Guid EstatusId { get; set; }
    [Required, MaxLength(200)]
    public string Hostname { get; set; } = "";
    [MaxLength(100)]
    public string Serial { get; set; } = "";
    public Guid? OfficeId { get; set; }
    public Guid? AreaId { get; set; }
    public Guid? AlojamientoId { get; set; }
    public Guid? OwnerAreaId { get; set; }
    public Guid? CriticalityId { get; set; }
    public Guid? EnvironmentId { get; set; }
    public Guid? CategoryId { get; set; }
    [MaxLength(100)]
    public string? TipoDispositivo { get; set; }
    [MaxLength(200)]
    public string? Funcion { get; set; }
    [MaxLength(50)]
    public string? TipoInfraestructura { get; set; }
    [MaxLength(200)]
    public string? Host { get; set; }
    [MaxLength(100)]
    public string? RAM { get; set; }
    public int? CantidadCPU { get; set; }
    [MaxLength(50)]
    public string? VelocidadCPU { get; set; }
    [MaxLength(100)]
    public string? CapacidadDAS { get; set; }
    [MaxLength(100)]
    public string? CapacidadSAN { get; set; }
    [MaxLength(200)]
    public string? SistemaOperativo { get; set; }

    public Guid? ManufacturerId { get; set; }
    public Guid? DeviceModelId { get; set; }
    [MaxLength(50)]
    public string? IP { get; set; }
    [MaxLength(100)]
    public string? MAC { get; set; }
    [MaxLength(200)]
    public string? Firmware { get; set; }
    public DateTime? GarantiaExpira { get; set; }
    [MaxLength(500)]
    public string? Observaciones { get; set; }

    public bool? BCP { get; set; }
    /// <summary>RTO: Recovery Time Objective — tiempo máximo tolerable de interrupción (ISO-067-GCS).</summary>
    [MaxLength(100)]
    public string? RTO { get; set; }
    /// <summary>RPO: Recovery Point Objective — pérdida máxima de datos tolerable (ISO-067-GCS).</summary>
    [MaxLength(100)]
    public string? RPO { get; set; }
    [MaxLength(100)]
    public string? Propietario { get; set; }
    [MaxLength(200)]
    public string? ClasificacionInformacion { get; set; }

    public Estatus Estatus { get; set; } = null!;
    public Office? Office { get; set; }
    public Area? Area { get; set; }
    public Alojamiento? Alojamiento { get; set; }
    public Area? OwnerArea { get; set; }
    public Environment? Environment { get; set; }
    public Criticality? Criticality { get; set; }
    public Category? Category { get; set; }
    public Vendor? Manufacturer { get; set; }
    public DeviceModel? DeviceModel { get; set; }
}
