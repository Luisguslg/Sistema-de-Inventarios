using IITS.Data;
using IITS.Entities;
using Microsoft.EntityFrameworkCore;

namespace IITS.Services;

public class AuditLogService : IAuditLogService
{
    private readonly AppDbContext _db;

    public AuditLogService(AppDbContext db)
    {
        _db = db;
    }

    public async Task RegistrarAsync(string tabla, Guid entidadId, string accion, string? detalle = null, Guid? usuarioId = null)
    {
        _db.AuditLogs.Add(new AuditLog
        {
            Id = Guid.NewGuid(),
            Tabla = tabla,
            EntidadId = entidadId,
            Accion = accion,
            UsuarioId = usuarioId,
            Fecha = DateTime.UtcNow,
            Detalle = detalle
        });
        await _db.SaveChangesAsync();
    }

    public async Task<List<AuditLogEntry>> GetLogsAsync(string? tabla = null, int max = 500)
    {
        var q = _db.AuditLogs.AsNoTracking()
            .OrderByDescending(x => x.Fecha)
            .Take(max);
        if (!string.IsNullOrWhiteSpace(tabla))
            q = q.Where(x => x.Tabla == tabla);
        var logs = await q.ToListAsync();
        var userIds = logs.Where(x => x.UsuarioId.HasValue).Select(x => x.UsuarioId!.Value).Distinct().ToList();
        var userDict = new Dictionary<Guid, string>();
        if (userIds.Count > 0)
        {
            var users = await _db.Users.AsNoTracking().Where(u => userIds.Contains(u.Id)).Select(u => new { u.Id, u.Username }).ToListAsync();
            foreach (var u in users)
                userDict[u.Id] = u.Username ?? "";
        }
        return logs.Select(log => new AuditLogEntry(
            log.Id, log.Tabla, log.EntidadId, log.Accion, log.UsuarioId,
            log.UsuarioId.HasValue && userDict.TryGetValue(log.UsuarioId.Value, out var name) ? name : null,
            log.Fecha, log.Detalle)).ToList();
    }
}
