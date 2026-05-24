using System.ComponentModel.DataAnnotations;

namespace IITS.Entities;

public class Estatus
{
    public Guid Id { get; set; }
    [Required, MaxLength(100)]
    public string Nombre { get; set; } = "";
    public long Codigo { get; set; }

    public ICollection<Aplicacion> Aplicaciones { get; set; } = new List<Aplicacion>();
    public ICollection<Operacion> Operaciones { get; set; } = new List<Operacion>();
    public ICollection<CuentaServicio> CuentasServicio { get; set; } = new List<CuentaServicio>();
    public ICollection<CuentaPrivilegiada> CuentasPrivilegiadas { get; set; } = new List<CuentaPrivilegiada>();
    public ICollection<PaginaWeb> PaginasWeb { get; set; } = new List<PaginaWeb>();
}
