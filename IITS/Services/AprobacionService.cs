using System.Text.Json;
using IITS.Data;
using IITS.Entities;
using Microsoft.EntityFrameworkCore;

namespace IITS.Services;

public class AprobacionService : IAprobacionService
{
    private readonly AppDbContext _db;
    private readonly IAuditLogService? _audit;
    private readonly IAprobacionPermisoService? _permisos;

    public AprobacionService(AppDbContext db, IAuditLogService? audit = null, IAprobacionPermisoService? permisos = null)
    {
        _db = db;
        _audit = audit;
        _permisos = permisos;
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

    public async Task<HashSet<Guid>> GetCrearPendientesAsync(string modulo)
    {
        var modulosAprob = ModulosParaFiltro(modulo);
        var list = await _db.Aprobaciones.AsNoTracking()
            .Where(a => a.Estado == "Por aprobar" && a.TipoAccion == "Crear" && modulosAprob.Contains(a.Modulo))
            .Select(a => a.EntidadId)
            .ToListAsync();
        return list.ToHashSet();
    }

    public async Task RegistrarAsync(string modulo, Guid entidadId, string estado, string? comentario = null, Guid? usuarioId = null,
        string? tipoAccion = null, string? datosPropuestos = null)
    {
        _db.Aprobaciones.Add(new Aprobacion
        {
            Id = Guid.NewGuid(),
            Modulo = modulo,
            EntidadId = entidadId,
            Estado = estado,
            Comentario = comentario,
            UsuarioId = usuarioId,
            Fecha = DateTime.UtcNow,
            TipoAccion = tipoAccion,
            DatosPropuestos = datosPropuestos
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
                await AplicarAprobacionAsync(a);
            }
        }
        else
        {
            a.Estado = "Aprobado";
            a.UsuarioId = usuarioId;
            a.Fecha = DateTime.UtcNow;
            if (comentario != null) a.Comentario = comentario;
            await _db.SaveChangesAsync();
            await AplicarAprobacionAsync(a);
        }

        if (_audit != null)
            await _audit.RegistrarAsync("Aprobaciones", a.Id, "Aprobar", $"{a.Modulo} {a.EntidadId:N}", usuarioId);
        return true;
    }

    private async Task AplicarAprobacionAsync(Aprobacion a)
    {
        if (string.Equals(a.TipoAccion, "Editar", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(a.DatosPropuestos))
        {
            try
            {
                var json = JsonDocument.Parse(a.DatosPropuestos).RootElement;
                if (string.Equals(a.Modulo, "Aplicaciones", StringComparison.OrdinalIgnoreCase))
                    await AplicarEdicionAplicacionAsync(a.EntidadId, json);
                else if (string.Equals(a.Modulo, "Operaciones", StringComparison.OrdinalIgnoreCase))
                    await AplicarEdicionOperacionAsync(a.EntidadId, json);
                else if (string.Equals(a.Modulo, "Cuentas", StringComparison.OrdinalIgnoreCase))
                    await AplicarEdicionCuentaAsync(a.EntidadId, json);
            }
            catch { /* No interrumpir el flujo de aprobación por un fallo de deserialización */ }
        }
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
        await AplicarRechazoAsync(a);
        if (_audit != null)
            await _audit.RegistrarAsync("Aprobaciones", a.Id, "Rechazar", $"{a.Modulo} {a.EntidadId:N}", usuarioId);
        return true;
    }

    private async Task AplicarRechazoAsync(Aprobacion a)
    {
        if (!string.Equals(a.TipoAccion, "Crear", StringComparison.OrdinalIgnoreCase)) return;
        try
        {
            if (string.Equals(a.Modulo, "Aplicaciones", StringComparison.OrdinalIgnoreCase))
            {
                var ent = await _db.Aplicaciones.FindAsync(a.EntidadId);
                if (ent != null) { _db.Aplicaciones.Remove(ent); await _db.SaveChangesAsync(); }
            }
            else if (string.Equals(a.Modulo, "Operaciones", StringComparison.OrdinalIgnoreCase))
            {
                var ent = await _db.Operaciones.FindAsync(a.EntidadId);
                if (ent != null) { _db.Operaciones.Remove(ent); await _db.SaveChangesAsync(); }
            }
            else if (string.Equals(a.Modulo, "Cuentas", StringComparison.OrdinalIgnoreCase))
            {
                var tipo = a.DatosPropuestos != null && a.DatosPropuestos.Contains("\"Tipo\":\"Privilegiada\"") ? "Privilegiada" : "Servicio";
                if (tipo == "Privilegiada")
                {
                    var ent = await _db.CuentasPrivilegiadas.FindAsync(a.EntidadId);
                    if (ent != null) { _db.CuentasPrivilegiadas.Remove(ent); await _db.SaveChangesAsync(); }
                }
                else
                {
                    var ent = await _db.CuentasServicio.FindAsync(a.EntidadId);
                    if (ent != null) { _db.CuentasServicio.Remove(ent); await _db.SaveChangesAsync(); }
                }
            }
        }
        catch { /* No interrumpir el registro de rechazo por fallo de limpieza */ }
    }

    private static string? GetStr(JsonElement root, string prop)
        => root.TryGetProperty(prop, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() : null;
    private static Guid? GetGuid(JsonElement root, string prop)
        => root.TryGetProperty(prop, out var v) && v.ValueKind == JsonValueKind.String && Guid.TryParse(v.GetString(), out var g) ? g : null;
    private static bool? GetBool(JsonElement root, string prop)
        => root.TryGetProperty(prop, out var v) && v.ValueKind != JsonValueKind.Null ? v.GetBoolean() : null;
    private static int? GetInt(JsonElement root, string prop)
        => root.TryGetProperty(prop, out var v) && v.ValueKind == JsonValueKind.Number ? v.GetInt32() : null;
    private static decimal? GetDecimal(JsonElement root, string prop)
        => root.TryGetProperty(prop, out var v) && v.ValueKind == JsonValueKind.Number ? v.GetDecimal() : null;
    private static DateTime? GetDate(JsonElement root, string prop)
        => root.TryGetProperty(prop, out var v) && v.ValueKind == JsonValueKind.String && DateTime.TryParse(v.GetString(), out var d) ? d : null;

    private async Task AplicarEdicionAplicacionAsync(Guid id, JsonElement json)
    {
        var ent = await _db.Aplicaciones.FindAsync(id);
        if (ent == null) return;
        ent.Nombre = GetStr(json, "Nombre") ?? ent.Nombre;
        ent.Funcionalidad = GetStr(json, "Funcionalidad") ?? ent.Funcionalidad;
        ent.Propietario = GetStr(json, "Propietario") ?? ent.Propietario;
        ent.Responsable = GetStr(json, "Responsable") ?? ent.Responsable;
        ent.TipoAlojamiento = GetStr(json, "TipoAlojamiento") ?? ent.TipoAlojamiento;
        ent.Proveedor = GetStr(json, "Proveedor") ?? ent.Proveedor;
        ent.ClasificacionInformacion = GetStr(json, "ClasificacionInformacion") ?? ent.ClasificacionInformacion;
        if (json.TryGetProperty("Critico", out var crit)) ent.Critico = crit.GetBoolean();
        ent.IntegracionesRelevantes = GetStr(json, "IntegracionesRelevantes") ?? ent.IntegracionesRelevantes;
        ent.DependenciasTecnicas = GetStr(json, "DependenciasTecnicas") ?? ent.DependenciasTecnicas;
        ent.ModeloLicenciamiento = GetStr(json, "ModeloLicenciamiento");
        ent.CostoAnualEstimado = GetDecimal(json, "CostoAnualEstimado");
        ent.FechaAdquisicionImplementacion = GetDate(json, "FechaAdquisicionImplementacion");
        ent.VersionActual = GetStr(json, "VersionActual");
        ent.SLA = GetStr(json, "SLA");
        ent.RTO = GetStr(json, "RTO");
        ent.RPO = GetStr(json, "RPO");
        ent.Autenticacion = GetStr(json, "Autenticacion");
        if (GetGuid(json, "EstatusId") is Guid eid) ent.EstatusId = eid;
        ent.AlojamientoId = GetGuid(json, "AlojamientoId");
        await _db.SaveChangesAsync();
    }

    private async Task AplicarEdicionOperacionAsync(Guid id, JsonElement json)
    {
        var ent = await _db.Operaciones.FindAsync(id);
        if (ent == null) return;
        ent.Hostname = GetStr(json, "Hostname") ?? ent.Hostname;
        ent.Serial = GetStr(json, "Serial") ?? ent.Serial;
        if (GetGuid(json, "EstatusId") is Guid eid) ent.EstatusId = eid;
        ent.OfficeId = GetGuid(json, "OfficeId");
        ent.AreaId = GetGuid(json, "AreaId");
        ent.AlojamientoId = GetGuid(json, "AlojamientoId");
        ent.Propietario = GetStr(json, "Propietario");
        ent.BCP = GetBool(json, "BCP");
        ent.RTO = GetStr(json, "RTO");
        ent.RPO = GetStr(json, "RPO");
        ent.ClasificacionInformacion = GetStr(json, "ClasificacionInformacion");
        ent.EnvironmentId = GetGuid(json, "EnvironmentId");
        ent.CriticalityId = GetGuid(json, "CriticalityId");
        ent.CategoryId = GetGuid(json, "CategoryId");
        ent.ManufacturerId = GetGuid(json, "ManufacturerId");
        ent.DeviceModelId = GetGuid(json, "DeviceModelId");
        ent.TipoDispositivo = GetStr(json, "TipoDispositivo");
        ent.Funcion = GetStr(json, "Funcion");
        ent.TipoInfraestructura = GetStr(json, "TipoInfraestructura");
        ent.Host = GetStr(json, "Host");
        ent.RAM = GetStr(json, "RAM");
        ent.CantidadCPU = GetInt(json, "CantidadCPU");
        ent.VelocidadCPU = GetStr(json, "VelocidadCPU");
        ent.CapacidadDAS = GetStr(json, "CapacidadDAS");
        ent.CapacidadSAN = GetStr(json, "CapacidadSAN");
        ent.SistemaOperativo = GetStr(json, "SistemaOperativo");
        ent.Firmware = GetStr(json, "Firmware");
        ent.GarantiaExpira = GetDate(json, "GarantiaExpira");
        ent.IP = GetStr(json, "IP");
        ent.MAC = GetStr(json, "MAC");
        ent.Observaciones = GetStr(json, "Observaciones");
        await _db.SaveChangesAsync();
    }

    private async Task AplicarEdicionCuentaAsync(Guid id, JsonElement json)
    {
        var tipo = GetStr(json, "Tipo") ?? "Privilegiada";
        if (tipo == "Privilegiada")
        {
            var ent = await _db.CuentasPrivilegiadas.FindAsync(id);
            if (ent == null) return;
            ent.Nombre = GetStr(json, "Nombre") ?? ent.Nombre;
            if (GetGuid(json, "EstatusId") is Guid eid) ent.EstatusId = eid;
            ent.AreaId = GetGuid(json, "AreaId");
            ent.AplicacionId = GetGuid(json, "AplicacionId");
            ent.Responsable = GetStr(json, "Responsable");
            ent.Origen = GetStr(json, "Origen");
            ent.ServicioRelacionado = GetStr(json, "ServicioRelacionado");
            ent.TipoConfiguracionCambio = GetStr(json, "TipoConfiguracionCambio");
            ent.IntervaloCambioDias = GetInt(json, "IntervaloCambioDias");
            ent.GruposSeguridad = GetStr(json, "GruposSeguridad");
            ent.Descripcion = GetStr(json, "Descripcion");
            await _db.SaveChangesAsync();
        }
        else
        {
            var ent = await _db.CuentasServicio.FindAsync(id);
            if (ent == null) return;
            ent.Nombre = GetStr(json, "Nombre") ?? ent.Nombre;
            if (GetGuid(json, "EstatusId") is Guid eid) ent.EstatusId = eid;
            ent.AreaId = GetGuid(json, "AreaId");
            ent.AplicacionId = GetGuid(json, "AplicacionId");
            ent.Responsable = GetStr(json, "Responsable");
            ent.Origen = GetStr(json, "Origen");
            ent.ServicioRelacionado = GetStr(json, "ServicioRelacionado");
            ent.TipoConfiguracionCambio = GetStr(json, "TipoConfiguracionCambio");
            ent.IntervaloCambioDias = GetInt(json, "IntervaloCambioDias");
            ent.GruposSeguridad = GetStr(json, "GruposSeguridad");
            ent.Descripcion = GetStr(json, "Descripcion");
            await _db.SaveChangesAsync();
        }
    }
}
