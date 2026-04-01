using System.ComponentModel.DataAnnotations;

namespace IITS.Entities;

public class PaginaWeb
{
    public Guid Id { get; set; }
    [Required, MaxLength(300)]
    public string Nombre { get; set; } = "";
    [MaxLength(500)]
    public string Url { get; set; } = "";
    public Guid EstatusId { get; set; }
    public DateTime CreatedAt { get; set; }

    public Estatus Estatus { get; set; } = null!;
}
