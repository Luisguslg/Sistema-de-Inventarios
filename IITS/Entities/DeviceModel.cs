using System.ComponentModel.DataAnnotations;

namespace IITS.Entities;

public class DeviceModel
{
    public Guid Id { get; set; }
    public Guid ManufacturerId { get; set; }
    [Required, MaxLength(150)]
    public string Name { get; set; } = "";

    public Vendor Manufacturer { get; set; } = null!;
}
