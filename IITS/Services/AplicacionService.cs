using IITS.Data;
using IITS.Entities;
using Microsoft.EntityFrameworkCore;

namespace IITS.Services;

public class AplicacionService : IAplicacionService
{
    private readonly AppDbContext _db;
    private readonly IAuditLogService _audit;
    private readonly IAprobacionService? _aprobacion;
    private readonly ICurrentUserService? _currentUser;
    private readonly IEmailOutboxService? _emailOutbox;
    private readonly IAprobacionPermisoService? _aprobacionPermisos;
    private readonly IConfiguration? _config;

    public AplicacionService(AppDbContext db, IAuditLogService audit, IAprobacionService? aprobacion = null,
        ICurrentUserService? currentUser = null, IEmailOutboxService? emailOutbox = null, IAprobacionPermisoService? aprobacionPermisos = null, IConfiguration? config = null)
    {
        _db = db;
        _audit = audit;
        _aprobacion = aprobacion;
        _currentUser = currentUser;
        _emailOutbox = emailOutbox;
        _aprobacionPermisos = aprobacionPermisos;
        _config = config;
    }

    public async Task<List<Aplicacion>> GetAllAsync()
    {
        var query = _db.Aplicaciones
            .Include(x => x.Estatus)
            .AsNoTracking();
        if (await _db.TableExistsAsync("Alojamientos"))
            query = query.Include(x => x.Alojamiento);
        return await query.OrderBy(x => x.Nombre).ToListAsync();
    }

    public async Task<Aplicacion?> GetByIdAsync(Guid id)
    {
        return await _db.Aplicaciones.FindAsync(id);
    }

    public async Task<List<Estatus>> GetEstatusListAsync()
    {
        return await _db.Estatus.AsNoTracking().OrderBy(e => e.Codigo).ToListAsync();
    }

    public async Task<Dictionary<Guid, Estatus>> GetEstatusDictionaryAsync()
    {
        var list = await GetEstatusListAsync();
        return list.ToDictionary(e => e.Id);
    }

    public async Task<Aplicacion> CreateAsync(Aplicacion entity)
    {
        var userId = _currentUser?.UserId;
        entity.Id = Guid.NewGuid();
        entity.CreatedAt = DateTime.UtcNow;
        _db.Aplicaciones.Add(entity);
        await _db.SaveChangesAsync();
        await _audit.RegistrarAsync("Aplicaciones", entity.Id, "Crear", entity.Nombre, userId);
        if (_aprobacion != null)
        {
            await _aprobacion.RegistrarAsync("Aplicaciones", entity.Id, "Por aprobar", "Alta de aplicación", userId);
            try { await NotificarAprobadoresAsync("Aplicaciones", entity.Id, "Alta de aplicación", entity.Nombre); } catch { /* no fallar si EmailOutbox no existe o falla */ }
        }
        return entity;
    }

    public async Task UpdateAsync(Aplicacion entity)
    {
        var userId = _currentUser?.UserId;
        var existing = await _db.Aplicaciones.AsNoTracking().FirstOrDefaultAsync(x => x.Id == entity.Id);
        var cambios = new List<string>();
        if (existing != null)
        {
            if ((existing.Nombre ?? "") != (entity.Nombre ?? "")) cambios.Add($"Nombre: {existing.Nombre ?? "-"} → {entity.Nombre ?? "-"}");
            if ((existing.Funcionalidad ?? "") != (entity.Funcionalidad ?? "")) cambios.Add($"Funcionalidad: {existing.Funcionalidad ?? "-"} → {entity.Funcionalidad ?? "-"}");
            if ((existing.Propietario ?? "") != (entity.Propietario ?? "")) cambios.Add($"Propietario: {existing.Propietario ?? "-"} → {entity.Propietario ?? "-"}");
            if ((existing.Responsable ?? "") != (entity.Responsable ?? "")) cambios.Add($"Responsable: {existing.Responsable ?? "-"} → {entity.Responsable ?? "-"}");
            if ((existing.TipoAlojamiento ?? "") != (entity.TipoAlojamiento ?? "")) cambios.Add($"Alojamiento: {existing.TipoAlojamiento ?? "-"} → {entity.TipoAlojamiento ?? "-"}");
            if ((existing.Proveedor ?? "") != (entity.Proveedor ?? "")) cambios.Add($"Proveedor: {existing.Proveedor ?? "-"} → {entity.Proveedor ?? "-"}");
            if ((existing.ClasificacionInformacion ?? "") != (entity.ClasificacionInformacion ?? "")) cambios.Add($"Clasificación: {existing.ClasificacionInformacion ?? "-"} → {entity.ClasificacionInformacion ?? "-"}");
            if (existing.Critico != entity.Critico) cambios.Add($"Crítico: {(existing.Critico ? "Sí" : "No")} → {(entity.Critico ? "Sí" : "No")}");
            if (existing.EstatusId != entity.EstatusId) cambios.Add($"Estatus: (cambió)");
            if ((existing.ModeloLicenciamiento ?? "") != (entity.ModeloLicenciamiento ?? "")) cambios.Add($"Modelo lic.: {existing.ModeloLicenciamiento ?? "-"} → {entity.ModeloLicenciamiento ?? "-"}");
            if ((existing.Autenticacion ?? "") != (entity.Autenticacion ?? "")) cambios.Add($"Autenticación: {existing.Autenticacion ?? "-"} → {entity.Autenticacion ?? "-"}");
            if (existing.CostoAnualEstimado != entity.CostoAnualEstimado) cambios.Add($"Costo anual: {existing.CostoAnualEstimado?.ToString("N2") ?? "-"} → {entity.CostoAnualEstimado?.ToString("N2") ?? "-"}");
        }
        var detalleAudit = cambios.Count > 0 ? string.Join("; ", cambios) : entity.Nombre ?? "";
        _db.Aplicaciones.Update(entity);
        await _db.SaveChangesAsync();
        await _audit.RegistrarAsync("Aplicaciones", entity.Id, "Actualizar", detalleAudit, userId);
        if (_aprobacion != null)
        {
            await _aprobacion.RegistrarAsync("Aplicaciones", entity.Id, "Por aprobar", "Modificación de aplicación", userId);
            var cambiosHtml = cambios.Count > 0 ? string.Join("<br/>", cambios.Select(c => System.Net.WebUtility.HtmlEncode(c))) : null;
            try { await NotificarAprobadoresAsync("Aplicaciones", entity.Id, "Modificación de aplicación", entity.Nombre ?? "", cambiosHtml); } catch { /* no fallar si EmailOutbox no existe o falla */ }
        }
    }

    public async Task DeleteAsync(Guid id)
    {
        var ent = await _db.Aplicaciones.FindAsync(id);
        if (ent == null) return;
        var nombre = ent.Nombre;
        var userId = _currentUser?.UserId;
        _db.Aplicaciones.Remove(ent);
        await _db.SaveChangesAsync();
        await _audit.RegistrarAsync("Aplicaciones", id, "Eliminar", nombre, userId);
    }

    private async Task NotificarAprobadoresAsync(string modulo, Guid entidadId, string asuntoAccion, string nombreEntidad, string? cambiosHtml = null)
    {
        if (_emailOutbox == null || _aprobacionPermisos == null) return;
        var aprobadores = await _aprobacionPermisos.GetApproversForModuloAsync(modulo);
        var subject = $"IITS: Solicitud por aprobar - {modulo} - {nombreEntidad}";
        var body = "<p>Se ha registrado una solicitud pendiente de aprobación.</p>";
        body += "<p><strong>Módulo:</strong> " + System.Net.WebUtility.HtmlEncode(modulo) + "<br/><strong>Acción:</strong> " + System.Net.WebUtility.HtmlEncode(asuntoAccion) + "<br/><strong>Registro:</strong> " + System.Net.WebUtility.HtmlEncode(nombreEntidad) + "</p>";
        if (!string.IsNullOrEmpty(cambiosHtml))
            body += "<p><strong>Detalle de cambios (antes → después):</strong></p><p style=\"margin-left:1em;font-family:monospace;\">" + cambiosHtml + "</p>";
        var baseUrl = _config?["App:BaseUrl"]?.TrimEnd('/');
        if (!string.IsNullOrEmpty(baseUrl))
        {
            var ruta = modulo.Equals("Aplicaciones", StringComparison.OrdinalIgnoreCase) ? "aplicaciones" : modulo.Equals("Operaciones", StringComparison.OrdinalIgnoreCase) ? "operaciones" : "cuentas";
            body += "<p><a href=\"" + System.Net.WebUtility.HtmlEncode(baseUrl) + "/auditoria/" + ruta + "\" style=\"color:#00338d;\">Ir a IITS para aprobar o rechazar</a></p>";
        }
        foreach (var u in aprobadores)
        {
            if (string.IsNullOrWhiteSpace(u.Email)) continue;
            try { await _emailOutbox.EnqueueAsync(u.Email, subject, body); } catch { /* no fallar el flujo */ }
        }
    }
}
