namespace IITS.Services;

public interface IAuditLogService
{
    Task RegistrarAsync(string tabla, Guid entidadId, string accion, string? detalle = null, Guid? usuarioId = null);
    Task<List<AuditLogEntry>> GetLogsAsync(string? tabla = null, int max = 500);
}

public record AuditLogEntry(Guid Id, string Tabla, Guid EntidadId, string Accion, Guid? UsuarioId, string? UsuarioNombre, DateTime Fecha, string? Detalle);
