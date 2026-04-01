using System.ComponentModel.DataAnnotations;

namespace IITS.Entities;

public class Asset
{
    public Guid Id { get; set; }
    public Guid OfficeId { get; set; }
    public Guid AreaId { get; set; }
    [MaxLength(100)]
    public string? DeviceType { get; set; }
    [MaxLength(200)]
    public string? Hostname { get; set; }
    public Guid? OperationEnvironmentId { get; set; }
    public Guid? OwnerAreaId { get; set; }
    public Guid? CriticalityId { get; set; }
    public Guid? EnvironmentId { get; set; }
    public Guid? CategoryId { get; set; }
    public Guid? ManufacturerId { get; set; }
    public Guid? DeviceModelId { get; set; }
    public Guid StatusId { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    [MaxLength(20)]
    public string ApprovalStatus { get; set; } = "Draft";

    public Office Office { get; set; } = null!;
    public Area Area { get; set; } = null!;
    public Estatus Status { get; set; } = null!;
    public Environment? OperationEnvironment { get; set; }
    public Area? OwnerArea { get; set; }
    public Criticality? Criticality { get; set; }
    public Environment? Environment { get; set; }
    public Category? Category { get; set; }
    public Vendor? Manufacturer { get; set; }
    public DeviceModel? DeviceModel { get; set; }
}
