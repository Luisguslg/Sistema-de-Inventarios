namespace IITS.Services;

/// <summary>Genera un PDF estructurado de auditoría (pendientes, datos del módulo, aprobadores).</summary>
public interface IAuditoriaPdfService
{
    /// <param name="modulo">aplicaciones | operaciones | cuentas</param>
    Task<byte[]> GenerarPdfAsync(string modulo);
}
