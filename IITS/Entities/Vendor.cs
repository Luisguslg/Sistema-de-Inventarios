using System.ComponentModel.DataAnnotations;

namespace IITS.Entities;

public class Vendor
{
    public Guid Id { get; set; }
    [Required, MaxLength(150)]
    public string Name { get; set; } = "";

    public ICollection<DeviceModel> DeviceModels { get; set; } = new List<DeviceModel>();
}
