using IITS.Data;
using IITS.Entities;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace IITS.Services;

public class AuditoriaPdfService : IAuditoriaPdfService
{
    private readonly AppDbContext _db;
    private readonly IAprobacionService _aprobacionService;
    private readonly IAprobacionPermisoService _aprobacionPermisoService;
    private readonly IAplicacionService _aplicacionService;

    public AuditoriaPdfService(
        AppDbContext db,
        IAprobacionService aprobacionService,
        IAprobacionPermisoService aprobacionPermisoService,
        IAplicacionService aplicacionService)
    {
        _db = db;
        _aprobacionService = aprobacionService;
        _aprobacionPermisoService = aprobacionPermisoService;
        _aplicacionService = aplicacionService;
    }

    public async Task<byte[]> GenerarPdfAsync(string modulo)
    {
        var mod = ModuloAprobacion(modulo);
        var nombreModulo = NombreModulo(modulo);
        var pendientes = await _aprobacionService.GetPendientesConVotoAsync(mod, null);
        var (columnas, datos) = await CargarDatosModuloAsync(mod);
        var aprobadores = await CargarAprobadoresAsync(mod);

        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(20);
                page.Header().Column(col =>
                {
                    col.Item().Text($"Auditoría - {nombreModulo}").Bold().FontSize(14);
                    col.Item().Text($"Fecha: {DateTime.Now.ToLocalTime():yyyy-MM-dd HH:mm}").FontSize(9).FontColor(Colors.Grey.Darken1);
                });
                page.Content().Column(col =>
                {
                    col.Spacing(12);

                    col.Item().Text("Pendientes de aprobación").Bold().FontSize(10);
                    if (pendientes.Any())
                    {
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(def =>
                            {
                                def.RelativeColumn(1.2f);
                                def.RelativeColumn(2f);
                                def.RelativeColumn(2f);
                                def.RelativeColumn(1f);
                            });
                            table.Header(h =>
                            {
                                h.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("Fecha").Bold().FontSize(7);
                                h.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("Registro").Bold().FontSize(7);
                                h.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("Acción").Bold().FontSize(7);
                                h.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("Estado").Bold().FontSize(7);
                            });
                            foreach (var p in pendientes)
                            {
                                var a = p.Aprobacion;
                                table.Cell().Padding(3).Text(a.Fecha.ToLocalTime().ToString("yyyy-MM-dd HH:mm")).FontSize(7);
                                table.Cell().Padding(3).Text(p.NombreEntidad).FontSize(7);
                                table.Cell().Padding(3).Text(a.Comentario ?? "—").FontSize(7);
                                table.Cell().Padding(3).Text(a.Estado ?? "").FontSize(7);
                            }
                        });
                    }
                    else
                        col.Item().Text("No hay pendientes de aprobación.").FontSize(8).FontColor(Colors.Grey.Medium);

                    col.Item().PaddingTop(8).Text("Datos del módulo").Bold().FontSize(10);
                    if (columnas.Count > 0 && datos.Any())
                    {
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(def =>
                            {
                                foreach (var _ in columnas)
                                    def.RelativeColumn();
                            });
                            table.Header(h =>
                            {
                                foreach (var c in columnas)
                                    h.Cell().Background(Colors.Grey.Lighten2).Padding(3).Text(c).Bold().FontSize(6);
                            });
                            foreach (var row in datos)
                            {
                                foreach (var v in row)
                                    table.Cell().Padding(2).Text(v ?? "").FontSize(6);
                            }
                        });
                    }
                    else
                        col.Item().Text("No hay datos.").FontSize(8).FontColor(Colors.Grey.Medium);

                    col.Item().PaddingTop(8).Text("Aprobadores designados").Bold().FontSize(10);
                    if (aprobadores.Any())
                    {
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(def =>
                            {
                                def.RelativeColumn(2f);
                                def.RelativeColumn(1.5f);
                                def.RelativeColumn(1f);
                            });
                            table.Header(h =>
                            {
                                h.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("Revisor").Bold().FontSize(7);
                                h.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("Cargo").Bold().FontSize(7);
                                h.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("Estado").Bold().FontSize(7);
                            });
                            foreach (var ap in aprobadores)
                            {
                                table.Cell().Padding(3).Text(ap.NombreCompleto).FontSize(7);
                                table.Cell().Padding(3).Text(ap.Cargo).FontSize(7);
                                table.Cell().Padding(3).Text(ap.Estatus).FontSize(7);
                            }
                        });
                    }
                    else
                        col.Item().Text("No hay aprobadores designados para este módulo.").FontSize(8).FontColor(Colors.Grey.Medium);
                });
            });
        });
        return doc.GeneratePdf();
    }

    private static string ModuloAprobacion(string modulo) => (modulo ?? "").ToLowerInvariant() switch
    {
        "aplicaciones" => "Aplicaciones",
        "operaciones" => "Operaciones",
        "cuentas" => "Cuentas",
        _ => modulo ?? ""
    };

    private static string NombreModulo(string modulo) => (modulo ?? "").ToLowerInvariant() switch
    {
        "aplicaciones" => "Aplicaciones",
        "operaciones" => "Tecnología",
        "cuentas" => "Cuentas",
        _ => modulo ?? ""
    };

    private static string Na(string? s) => string.IsNullOrWhiteSpace(s) ? "N/A" : s;

    private async Task<(List<string> columnas, List<List<string>> datos)> CargarDatosModuloAsync(string mod)
    {
        if (string.Equals(mod, "Aplicaciones", StringComparison.OrdinalIgnoreCase))
        {
            var apps = await _aplicacionService.GetAllAsync();
            var columnas = new List<string> { "Nombre", "Funcionalidad", "Propietario", "Responsable", "Estatus" };
            var datos = apps.Select(a => new List<string>
            {
                Na(a.Nombre),
                (a.Funcionalidad ?? "").Length > 80 ? (a.Funcionalidad ?? "").Substring(0, 80) + "…" : Na(a.Funcionalidad),
                Na(a.Propietario),
                Na(a.Responsable),
                Na(a.Estatus?.Nombre)
            }).ToList();
            return (columnas, datos);
        }
        if (string.Equals(mod, "Operaciones", StringComparison.OrdinalIgnoreCase))
        {
            var list = await _db.Operaciones.AsNoTracking()
                .Include(o => o.Office).Include(o => o.Area).Include(o => o.Alojamiento)
                .Include(o => o.OwnerArea).Include(o => o.Criticality).Include(o => o.Environment)
                .Include(o => o.Manufacturer).Include(o => o.DeviceModel)
                .OrderBy(o => o.Hostname)
                .ToListAsync();
            var columnas = new List<string> { "Oficina", "Área responsable", "Dispositivo", "Hostname", "Entorno de operación", "Propietario", "Criticidad", "Ambiente", "Fabricante", "Modelo", "Función", "Tipo de infraestructura", "Serial", "Sistema Operativo", "Firmware", "Garantía" };
            var datos = list.Select(o => new List<string>
            {
                Na(o.Office?.Name),
                Na(o.Area?.Name),
                Na(o.TipoDispositivo),
                Na(o.Hostname),
                Na(o.Alojamiento?.Nombre),
                Na(o.Propietario ?? o.OwnerArea?.Name),
                Na(o.Criticality?.Name),
                Na(o.Environment?.Name),
                Na(o.Manufacturer?.Name),
                Na(o.DeviceModel?.Name),
                Na(o.Funcion),
                Na(o.TipoInfraestructura),
                Na(o.Serial),
                Na(o.SistemaOperativo),
                Na(o.Firmware),
                o.GarantiaExpira.HasValue ? o.GarantiaExpira.Value.ToString("yyyy-MM-dd") : "N/A"
            }).ToList();
            return (columnas, datos);
        }
        if (string.Equals(mod, "Cuentas", StringComparison.OrdinalIgnoreCase))
        {
            var qPriv = _db.CuentasPrivilegiadas.AsNoTracking().Include(c => c.Estatus).Include(c => c.Area).Include(c => c.Aplicacion);
            var qServ = _db.CuentasServicio.AsNoTracking().Include(c => c.Estatus).Include(c => c.Area).Include(c => c.Aplicacion);
            var priv = await qPriv.OrderBy(c => c.Nombre).ToListAsync();
            var serv = await qServ.OrderBy(c => c.Nombre).ToListAsync();
            var columnas = new List<string> { "Tipo", "Nombre", "Área", "Responsable", "Origen", "Servicio/Aplicación", "Tipo config. cambio", "Intervalo (días)", "Grupos seguridad", "Descripción", "Estatus" };
            var datos = priv.Select(p => new List<string>
            {
                "Privilegiada", Na(p.Nombre), Na(p.Area?.Name), Na(p.Responsable), Na(p.Origen),
                Na(p.Aplicacion?.Nombre) ?? Na(p.ServicioRelacionado), Na(p.TipoConfiguracionCambio),
                p.IntervaloCambioDias?.ToString() ?? "N/A", Na(p.GruposSeguridad), Na(p.Descripcion), Na(p.Estatus?.Nombre)
            }).ToList();
            datos.AddRange(serv.Select(s => new List<string>
            {
                "Servicio", Na(s.Nombre), Na(s.Area?.Name), Na(s.Responsable), Na(s.Origen),
                Na(s.Aplicacion?.Nombre) ?? Na(s.ServicioRelacionado), Na(s.TipoConfiguracionCambio),
                s.IntervaloCambioDias?.ToString() ?? "N/A", Na(s.GruposSeguridad), Na(s.Descripcion), Na(s.Estatus?.Nombre)
            }));
            return (columnas, datos);
        }
        return (new List<string>(), new List<List<string>>());
    }

    private async Task<List<(string NombreCompleto, string Cargo, string Estatus)>> CargarAprobadoresAsync(string mod)
    {
        var users = await _aprobacionPermisoService.GetApproversForModuloAsync(mod);
        var userIds = users.Select(u => u.Id).ToList();
        var rolesByUser = userIds.Any()
            ? await _db.UserRoles.AsNoTracking().Include(ur => ur.Role).Where(ur => userIds.Contains(ur.UserId)).ToListAsync()
            : new List<UserRole>();
        var modulosAprob = string.Equals(mod, "Cuentas", StringComparison.OrdinalIgnoreCase)
            ? new[] { "Cuentas", "CuentasPrivilegiadas", "CuentasServicio" }
            : new[] { mod };
        var pendientesIds = await _db.Aprobaciones.AsNoTracking()
            .Where(a => a.Estado == "Por aprobar" && modulosAprob.Contains(a.Modulo))
            .Select(a => a.Id)
            .ToListAsync();
        var votosPorUsuario = new Dictionary<Guid, HashSet<Guid>>();
        if (userIds.Any() && pendientesIds.Any() && await _db.TableExistsAsync("AprobacionVotos"))
        {
            var votos = await _db.AprobacionVotos.AsNoTracking()
                .Where(v => pendientesIds.Contains(v.AprobacionId) && userIds.Contains(v.UserId))
                .Select(v => new { v.AprobacionId, v.UserId })
                .ToListAsync();
            votosPorUsuario = votos.GroupBy(x => x.UserId).ToDictionary(g => g.Key, g => g.Select(x => x.AprobacionId).ToHashSet());
        }
        var totalPendientes = pendientesIds.Count;
        return users.Select(u =>
        {
            var cargo = rolesByUser.Where(ur => ur.UserId == u.Id).Select(ur => ur.Role?.Nombre).FirstOrDefault(r => !string.IsNullOrEmpty(r));
            var nombreCompleto = $"{u.Nombre} {u.Apellido}".Trim();
            if (string.IsNullOrEmpty(nombreCompleto)) nombreCompleto = u.Username ?? "";
            var votados = votosPorUsuario.TryGetValue(u.Id, out var set) ? set.Count : 0;
            var porAprobar = totalPendientes - votados;
            var estatus = porAprobar <= 0 ? "Al corriente" : (porAprobar == 1 ? "1 por aprobar" : $"{porAprobar} por aprobar");
            return (NombreCompleto: nombreCompleto, Cargo: string.IsNullOrEmpty(cargo) ? "—" : cargo, Estatus: estatus);
        }).ToList();
    }
}
