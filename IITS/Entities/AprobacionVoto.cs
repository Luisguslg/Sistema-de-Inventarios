using System.ComponentModel.DataAnnotations;

namespace IITS.Entities;

/// <summary>Voto de un aprobador sobre una solicitud. Permite multi-aprobador: todos deben aprobar.</summary>
public class AprobacionVoto
{
    public Guid Id { get; set; }
    public Guid AprobacionId { get; set; }
    public Guid UserId { get; set; }
    [MaxLength(20)]
    public string Estado { get; set; } = ""; // "Aprobado" | "Rechazado"
    public DateTime Fecha { get; set; }
    public string? Comentario { get; set; }

    public Aprobacion Aprobacion { get; set; } = null!;
    public User User { get; set; } = null!;
}
