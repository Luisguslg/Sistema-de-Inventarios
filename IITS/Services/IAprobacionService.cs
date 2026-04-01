using IITS.Entities;

namespace IITS.Services;

/// <summary>Pendiente de aprobación con el voto del usuario actual (null si no ha votado).</summary>
public class PendienteConVoto
{
    public Aprobacion Aprobacion { get; set; } = null!;
    /// <summary>Estado del voto del usuario actual: null, "Aprobado" o "Rechazado".</summary>
    public string? MiVotoEstado { get; set; }
    /// <summary>Nombre o identificador legible de la entidad (ej. nombre aplicación, hostname, cuenta).</summary>
    public string NombreEntidad { get; set; } = "";
}

public interface IAprobacionService
{
    Task<List<Aprobacion>> GetAllAsync(string? modulo = null, int max = 200);
    Task<List<PendienteConVoto>> GetPendientesConVotoAsync(string modulo, Guid? userId);
    Task<HashSet<Guid>> GetEntidadIdsPendientesAsync(string modulo);
    Task RegistrarAsync(string modulo, Guid entidadId, string estado, string? comentario = null, Guid? usuarioId = null);
    Task<bool> MarcarAprobadoAsync(Guid aprobacionId, Guid? usuarioId, string? comentario = null);
    Task<bool> MarcarRechazadoAsync(Guid aprobacionId, Guid? usuarioId, string? comentario = null);
}
