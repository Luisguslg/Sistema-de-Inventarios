using System.ComponentModel.DataAnnotations;

namespace IITS.Entities;

/// <summary>
/// Permiso para que un usuario pueda aprobar cambios en un módulo concreto.
/// Un usuario puede tener varios registros (aprobar en varios módulos).
/// </summary>
public class AprobacionPermiso
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    /// <summary>Módulo: Aplicaciones, Operaciones, Telecomunicaciones, Cuentas.</summary>
    [Required, MaxLength(50)]
    public string Modulo { get; set; } = "";

    public User User { get; set; } = null!;
}
