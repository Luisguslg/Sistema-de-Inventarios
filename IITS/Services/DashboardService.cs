using IITS.Data;
using IITS.Entities;
using Microsoft.EntityFrameworkCore;

namespace IITS.Services;

public class DashboardResumen
{
    public int Total { get; set; }
    public int Activos { get; set; }
    public int Inactivos { get; set; }
    public int Desincorporados { get; set; }
}

public class DashboardService
{
    private readonly AppDbContext _db;

    public DashboardService(AppDbContext db) => _db = db;

    public (Guid Activo, Guid Inactivo, Guid Desincorporado)? GetEstatusIds()
    {
        var activo = _db.Estatus.AsNoTracking().OrderBy(e => e.Codigo).FirstOrDefault(e => e.Codigo == 1000);
        var inactivo = _db.Estatus.AsNoTracking().OrderBy(e => e.Codigo).FirstOrDefault(e => e.Codigo == 1500);
        var desinc = _db.Estatus.AsNoTracking().OrderBy(e => e.Codigo).FirstOrDefault(e => e.Codigo == 2000);
        if (activo == null || desinc == null) return null;
        return (activo.Id, inactivo?.Id ?? Guid.Empty, desinc.Id);
    }

    public DashboardResumen GetResumenAplicaciones(Guid idActivo, Guid idInactivo, Guid idDesinc)
    {
        var list = _db.Aplicaciones.AsNoTracking().ToList();
        var act = list.Count(e => e.EstatusId == idActivo);
        var inact = idInactivo != Guid.Empty ? list.Count(e => e.EstatusId == idInactivo) : 0;
        var des = list.Count(e => e.EstatusId == idDesinc);
        return new DashboardResumen { Total = list.Count, Activos = act, Inactivos = inact, Desincorporados = des };
    }

    public DashboardResumen GetResumenOperaciones(Guid idActivo, Guid idInactivo, Guid idDesinc)
    {
        var list = _db.Operaciones.AsNoTracking().ToList();
        var act = list.Count(e => e.EstatusId == idActivo);
        var inact = idInactivo != Guid.Empty ? list.Count(e => e.EstatusId == idInactivo) : 0;
        var des = list.Count(e => e.EstatusId == idDesinc);
        return new DashboardResumen { Total = list.Count, Activos = act, Inactivos = inact, Desincorporados = des };
    }

    public DashboardResumen GetResumenCuentasPrivilegiadas(Guid idActivo, Guid idInactivo, Guid idDesinc)
    {
        var list = _db.CuentasPrivilegiadas.AsNoTracking().ToList();
        var act = list.Count(e => e.EstatusId == idActivo);
        var inact = idInactivo != Guid.Empty ? list.Count(e => e.EstatusId == idInactivo) : 0;
        var des = list.Count(e => e.EstatusId == idDesinc);
        return new DashboardResumen { Total = list.Count, Activos = act, Inactivos = inact, Desincorporados = des };
    }

    public DashboardResumen GetResumenCuentasServicio(Guid idActivo, Guid idInactivo, Guid idDesinc)
    {
        var list = _db.CuentasServicio.AsNoTracking().ToList();
        var act = list.Count(e => e.EstatusId == idActivo);
        var inact = idInactivo != Guid.Empty ? list.Count(e => e.EstatusId == idInactivo) : 0;
        var des = list.Count(e => e.EstatusId == idDesinc);
        return new DashboardResumen { Total = list.Count, Activos = act, Inactivos = inact, Desincorporados = des };
    }

    public async Task<int> GetPendingApprovalCountAsync()
    {
        if (!await _db.TableExistsAsync("ApprovalRequests")) return 0;
        return await _db.ApprovalRequests.AsNoTracking().CountAsync(r => r.Status == "Pending");
    }

    public async Task<List<ApprovalRequestDto>> GetPendingApprovalsAsync(int max = 20)
    {
        if (!await _db.TableExistsAsync("ApprovalRequests")) return new List<ApprovalRequestDto>();
        return await _db.ApprovalRequests
            .AsNoTracking()
            .Where(r => r.Status == "Pending")
            .OrderByDescending(r => r.SubmittedAt)
            .Take(max)
            .Select(r => new ApprovalRequestDto(r.Id, r.EntityType, r.EntityId, r.Summary ?? "", r.SubmittedAt))
            .ToListAsync();
    }
}

public record ApprovalRequestDto(Guid Id, string EntityType, string EntityId, string Summary, DateTime SubmittedAt);
