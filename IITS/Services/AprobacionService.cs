using IITS.Data;
using IITS.Entities;
using Microsoft.EntityFrameworkCore;

namespace IITS.Services;

public class AprobacionService : IAprobacionService
{
    private readonly AppDbContext _db;
    private readonly IAuditLogService? _audit;
    private readonly IAprobacionPermisoService? _permisos;
    private readonly ICurrentUserService? _currentUser;

    public AprobacionService(AppDbContext db, IAuditLogService? audit = null, IAprobacionPermisoService? permisos = null, ICurrentUserService? currentUser = null)
    {
        _db = db;
        _audit = audit;
        _permisos = permisos;
        _currentUser = currentUser;
    }

    public async Task<List<Aprobacion>> GetAllAsync(string? modulo = null, int max = 200)
    {
        var q = _db.Aprobaciones.AsNoTracking().OrderByDescending(a => a.Fecha).Take(max);
        if (!string.IsNullOrWhiteSpace(modulo))
            q = q.Where(a => a.Modulo == modulo);
        return await q.ToListAsync();
    }

    public async Task<List<PendienteConVoto>> GetPendientesConVotoAsync(string modulo, Guid? userId)
    {
        var modulosAprob = ModulosParaFiltro(modulo);
        var pendientes = await _db.Aprobaciones.AsNoTracking()
            .Where(a => a.Estado == "Por aprobar" && modulosAprob.Contains(a.Modulo))
            .OrderByDescending(a => a.Fecha)
            .Take(100)
            .ToListAsync();
        if (pendientes.Count == 0) return new List<PendienteConVoto>();
        var ids = pendientes.Select(p => p.Id).ToList();
        var votos = userId.HasValue && await _db.TableExistsAsync("AprobacionVotos")
            ? await _db.AprobacionVotos.AsNoTracking()
                .Where(v => ids.Contains(v.AprobacionId) && v.UserId == userId.Value)
                .ToDictionaryAsync(v => v.AprobacionId, v => v.Estado)
            : new Dictionary<Guid, string>();
        var entidadIds = pendientes.Select(p => p.EntidadId).Distinct().ToList();
        var nombresEntidad = await ResolverNombresEntidadAsync(modulo, entidadIds);
        return pendientes.Select(p => new PendienteConVoto
        {
            Aprobacion = p,
            MiVotoEstado = votos.TryGetValue(p.Id, out var est) ? est : null,
            NombreEntidad = nombresEntidad.TryGetValue(p.EntidadId, out var nom) ? nom : p.EntidadId.ToString("N")[..8] + "…"
        }).ToList();
    }

    private async Task<Dictionary<Guid, string>> ResolverNombresEntidadAsync(string modulo, List<Guid> entidadIds)
    {
        var dict = new Dictionary<Guid, string>();
        if (entidadIds.Count == 0) return dict;
        if (string.Equals(modulo, "Aplicaciones", StringComparison.OrdinalIgnoreCase))
        {
            var apps = await _db.Aplicaciones.AsNoTracking().Where(a => entidadIds.Contains(a.Id)).Select(a => new { a.Id, a.Nombre }).ToListAsync();
            foreach (var a in apps) dict[a.Id] = a.Nombre ?? "";
        }
        else if (string.Equals(modulo, "Operaciones", StringComparison.OrdinalIgnoreCase))
        {
            var ops = await _db.Operaciones.AsNoTracking().Where(o => entidadIds.Contains(o.Id)).Select(o => new { o.Id, o.Hostname }).ToListAsync();
            foreach (var o in ops) dict[o.Id] = o.Hostname ?? "";
        }
        else if (string.Equals(modulo, "Cuentas", StringComparison.OrdinalIgnoreCase))
        {
            var priv = await _db.CuentasPrivilegiadas.AsNoTracking().Where(c => entidadIds.Contains(c.Id)).Select(c => new { c.Id, c.Nombre }).ToListAsync();
            var serv = await _db.CuentasServicio.AsNoTracking().Where(c => entidadIds.Contains(c.Id)).Select(c => new { c.Id, c.Nombre }).ToListAsync();
            foreach (var c in priv) dict[c.Id] = c.Nombre ?? "";
            foreach (var c in serv) dict[c.Id] = c.Nombre ?? "";
        }
        return dict;
    }

    private static string[] ModulosParaFiltro(string modulo)
    {
        if (string.Equals(modulo, "Cuentas", StringComparison.OrdinalIgnoreCase))
            return new[] { "Cuentas", "CuentasPrivilegiadas", "CuentasServicio" };
        return new[] { modulo };
    }

    public async Task<HashSet<Guid>> GetEntidadIdsPendientesAsync(string modulo)
    {
        var modulosAprob = ModulosParaFiltro(modulo);
        var list = await _db.Aprobaciones.AsNoTracking()
            .Where(a => a.Estado == "Por aprobar" && modulosAprob.Contains(a.Modulo))
            .Select(a => a.EntidadId)
            .ToListAsync();
        return list.ToHashSet();
    }

    public async Task RegistrarAsync(string modulo, Guid entidadId, string estado, string? comentario = null, Guid? usuarioId = null)
    {
        _db.Aprobaciones.Add(new Aprobacion
        {
            Id = Guid.NewGuid(),
            Modulo = modulo,
            EntidadId = entidadId,
            Estado = estado,
            Comentario = comentario,
            UsuarioId = usuarioId,
            Fecha = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();
    }

    public async Task<bool> MarcarAprobadoAsync(Guid aprobacionId, Guid? usuarioId, string? comentario = null)
    {
        var a = await _db.Aprobaciones.FindAsync(aprobacionId);
        if (a == null || !usuarioId.HasValue) return false;

        if (await _db.TableExistsAsync("AprobacionVotos"))
        {
            var yaVoto = await _db.AprobacionVotos.AsNoTracking().AnyAsync(v => v.AprobacionId == aprobacionId && v.UserId == usuarioId.Value);
            if (!yaVoto)
            {
                _db.AprobacionVotos.Add(new AprobacionVoto
                {
                    Id = Guid.NewGuid(),
                    AprobacionId = aprobacionId,
                    UserId = usuarioId.Value,
                    Estado = "Aprobado",
                    Fecha = DateTime.UtcNow,
                    Comentario = comentario
                });
                await _db.SaveChangesAsync();
            }

            var esAdmin = _currentUser?.IsAdmin == true;
            if (esAdmin)
            {
                a.Estado = "Aprobado";
                a.UsuarioId = usuarioId;
                a.Fecha = DateTime.UtcNow;
                if (comentario != null) a.Comentario = comentario;
                await _db.SaveChangesAsync();
            }
            else
            {
                var aprobadores = _permisos != null ? await _permisos.GetApproversForModuloAsync(a.Modulo) : new List<User>();
                var votos = await _db.AprobacionVotos.AsNoTracking().Where(v => v.AprobacionId == aprobacionId).ToListAsync();
                var rechazo = votos.Any(v => string.Equals(v.Estado, "Rechazado", StringComparison.OrdinalIgnoreCase));
                if (rechazo)
                {
                    a.Estado = "Rechazado";
                    a.UsuarioId = usuarioId;
                    a.Fecha = DateTime.UtcNow;
                    await _db.SaveChangesAsync();
                }
                else if (aprobadores.Count == 0 || (votos.Count >= aprobadores.Count && votos.All(v => string.Equals(v.Estado, "Aprobado", StringComparison.OrdinalIgnoreCase))))
                {
                    a.Estado = "Aprobado";
                    a.UsuarioId = usuarioId;
                    a.Fecha = DateTime.UtcNow;
                    if (comentario != null) a.Comentario = comentario;
                    await _db.SaveChangesAsync();
                }
            }
        }
        else
        {
            a.Estado = "Aprobado";
            a.UsuarioId = usuarioId;
            a.Fecha = DateTime.UtcNow;
            if (comentario != null) a.Comentario = comentario;
            await _db.SaveChangesAsync();
        }

        if (_audit != null)
            await _audit.RegistrarAsync("Aprobaciones", a.Id, "Aprobar", $"{a.Modulo} {a.EntidadId:N}", usuarioId);
        return true;
    }

    public async Task<bool> MarcarRechazadoAsync(Guid aprobacionId, Guid? usuarioId, string? comentario = null)
    {
        var a = await _db.Aprobaciones.FindAsync(aprobacionId);
        if (a == null || !usuarioId.HasValue) return false;

        if (await _db.TableExistsAsync("AprobacionVotos"))
        {
            var yaVoto = await _db.AprobacionVotos.AsNoTracking().AnyAsync(v => v.AprobacionId == aprobacionId && v.UserId == usuarioId.Value);
            if (!yaVoto)
            {
                _db.AprobacionVotos.Add(new AprobacionVoto
                {
                    Id = Guid.NewGuid(),
                    AprobacionId = aprobacionId,
                    UserId = usuarioId.Value,
                    Estado = "Rechazado",
                    Fecha = DateTime.UtcNow,
                    Comentario = comentario
                });
            }
        }

        a.Estado = "Rechazado";
        a.UsuarioId = usuarioId;
        a.Fecha = DateTime.UtcNow;
        if (comentario != null) a.Comentario = comentario;
        await _db.SaveChangesAsync();
        if (_audit != null)
            await _audit.RegistrarAsync("Aprobaciones", a.Id, "Rechazar", $"{a.Modulo} {a.EntidadId:N}", usuarioId);
        return true;
    }
}
