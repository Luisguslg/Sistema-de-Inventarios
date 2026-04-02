using System.Globalization;
using System.Threading.RateLimiting;
using IITS.Data;
using IITS.Middleware;
using IITS.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

var sessionTimeoutMinutes = builder.Configuration.GetValue("Auth:SessionTimeoutMinutes", 30);
if (sessionTimeoutMinutes < 5) sessionTimeoutMinutes = 5;
if (sessionTimeoutMinutes > 480) sessionTimeoutMinutes = 480; // máx 8 horas

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = NegotiateDefaults.AuthenticationScheme;
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.Cookie.Name = ".IITS.Session";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(sessionTimeoutMinutes);
    options.SlidingExpiration = true;
    options.LoginPath = "/";
    options.AccessDeniedPath = "/";
})
.AddNegotiate();

builder.Services.AddAuthorization(options =>
{
    foreach (var (code, _) in PermissionCodes.All)
        options.AddPolicy(code, p => p.RequireClaim("Permission", code));
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddHttpContextAccessor();

builder.Services.AddTransient<IClaimsTransformation, IITSClaimsTransformation>();

builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<ToastService>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
builder.Services.AddScoped<IAuditEventService, AuditEventService>();
builder.Services.AddScoped<IAplicacionService, AplicacionService>();
builder.Services.AddScoped<IExportService, ExportService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IRolService, RolService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IAprobacionService, AprobacionService>();
builder.Services.AddScoped<IAprobacionPermisoService, AprobacionPermisoService>();
builder.Services.AddScoped<IAuditoriaPdfService, AuditoriaPdfService>();
builder.Services.AddScoped<IMasterDataService, MasterDataService>();
builder.Services.AddScoped<IEmailOutboxService, EmailOutboxService>();
builder.Services.Configure<SmtpEmailSenderOptions>(builder.Configuration.GetSection(SmtpEmailSenderOptions.SectionName));
if (string.Equals(builder.Configuration["Email:Mode"], "Smtp", StringComparison.OrdinalIgnoreCase))
    builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();
else
    builder.Services.AddScoped<IEmailSender, DevEmailSender>();
builder.Services.AddHostedService<EmailOutboxHostedService>();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("IITS"),
        o => o.UseQuerySplittingBehavior(Microsoft.EntityFrameworkCore.QuerySplittingBehavior.SplitQuery)));

// Rate limiting para endpoints de exportación y auditoría (ISO-082-API)
builder.Services.AddRateLimiter(opts =>
{
    opts.AddFixedWindowLimiter("export", o =>
    {
        o.Window = TimeSpan.FromMinutes(1);
        o.PermitLimit = 10;
        o.QueueLimit = 0;
        o.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });
    opts.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

var app = builder.Build();

// Carpeta Logs para escritura de logs (permisos de escritura para el identity del App Pool en IIS).
var logsDir = Path.Combine(app.Environment.ContentRootPath, "Logs");
if (!Directory.Exists(logsDir)) Directory.CreateDirectory(logsDir);

// Comando de consola: leer columnas del Excel maestro "Catalogo de Aplicaciones.xlsx".
if (args.Contains("read-catalogo-aplicaciones"))
{
    var path = args.Length > 1 ? args[1] : Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Ejemplo", "Catalogo de Aplicaciones.xlsx");
    path = Path.GetFullPath(path);
    if (!File.Exists(path)) { Console.WriteLine("No encontrado: " + path); return; }
    using var book = new ClosedXML.Excel.XLWorkbook(path);
    var sheet = book.Worksheet(1);
    var row1 = sheet.FirstRowUsed();
    if (row1 == null) { Console.WriteLine("Hoja vacía"); return; }
    var cols = new List<string>();
    for (int c = 1; c <= 100; c++)
    {
        var v = row1.Cell(c).GetString();
        if (string.IsNullOrWhiteSpace(v)) break;
        cols.Add(v.Trim());
    }
    Console.WriteLine("Columnas en Catalogo de Aplicaciones.xlsx (fila 1):");
    for (int i = 0; i < cols.Count; i++) Console.WriteLine($"  {i + 1}. {cols[i]}");
    return;
}

// Comando de consola: borrar BD, recrear y aplicar migraciones (deja todo listo desde cero).
if (args.Contains("reset-migrations"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    await db.Database.EnsureDeletedAsync();
    await db.Database.MigrateAsync();
    await db.EnsureEstatusAsync();
    await db.EnsureRolesAsync();
    if (await db.TableExistsAsync("Permissions"))
        await db.EnsurePermissionsAsync();
    if (await db.TableExistsAsync("Alojamientos"))
        await db.EnsureAlojamientosAsync();
        if (await db.TableExistsAsync("Areas"))
            await db.EnsureAreasAsync();
    await db.EnsureSuperAdminUserAsync(config["Auth:SuperAdminUsername"]);
    Console.WriteLine("Listo: base de datos recreada y migraciones aplicadas.");
    return;
}

// Comando de consola: desmarcar migraciones huérfanas y aplicar de nuevo (repara BD si el historial tiene IDs pero faltan tablas).
if (args.Contains("fix-migrations"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    await ForceRepairMigrationsAsync(db, app.Services, config);
    Console.WriteLine("Listo. Si faltaban tablas, se aplicaron las migraciones. Vuelve a ejecutar la app sin argumentos.");
    return;
}

// Comando de consola: cargar catálogos de Tecnología (Oficinas, Áreas, Entorno de operación, Ambiente, Criticidad, Categoría, Fabricante, Modelo, Dispositivo, etc.) y datos de ejemplo. No se ejecuta al iniciar la app; úsalo para probar.
if (args.Contains("seed-catalogs"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (await db.TableExistsAsync("Alojamientos"))
        await db.EnsureAlojamientosAsync();
    if (await db.TableExistsAsync("Areas"))
        await db.EnsureAreasAsync();
    if (await db.TableExistsAsync("Offices"))
    {
        await db.EnsureOfficesAsync();
        if (await db.TableExistsAsync("Environments")) await db.EnsureEnvironmentsAsync();
        if (await db.TableExistsAsync("Criticalities")) await db.EnsureCriticalitiesAsync();
        if (await db.TableExistsAsync("Categories")) await db.EnsureCategoriesAsync();
        if (await db.TableExistsAsync("Vendors")) await db.EnsureVendorsAsync();
        if (await db.TableExistsAsync("DeviceModels")) await db.EnsureDeviceModelsAsync();
        if (await db.TableExistsAsync("CatalogItems"))
            {
                await db.EnsureCatalogItemsTecnologiaAsync();
                await db.EnsureCatalogItemsAplicacionesAsync();
            }
        await db.EnsureOperacionesSampleAsync();
    }
    Console.WriteLine("Listo: catálogos de Tecnología y datos de ejemplo cargados.");
    return;
}

try
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        try
        {
            await db.Database.MigrateAsync();
        }
        catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Number == 2714)
        {
            await SyncMigrationHistoryAsync(db);
            await db.Database.MigrateAsync();
        }

        await db.EnsureAplicacionesOptionalColumnsAsync();
    await db.EnsureOperacionesOptionalColumnsAsync();
    await db.EnsureCuentasOptionalColumnsAsync();
        await db.EnsureAdminTablesAsync();
        await db.EnsureAlojamientosTableAsync();
        await db.EnsureCatalogTablesIfMissingAsync();

    var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    await UnmarkMigrationsIfTablesMissingAsync(db, app.Services, config);

    try
    {
        await db.EnsureEstatusAsync();
        await db.EnsureRolesAsync();
        if (await db.TableExistsAsync("Permissions"))
            await db.EnsurePermissionsAsync();
        if (await db.TableExistsAsync("Alojamientos"))
            await db.EnsureAlojamientosAsync();
        if (await db.TableExistsAsync("Areas"))
            await db.EnsureAreasAsync();
        if (await db.TableExistsAsync("Offices"))
        {
            await db.EnsureOfficesAsync();
            if (await db.TableExistsAsync("Environments")) await db.EnsureEnvironmentsAsync();
            if (await db.TableExistsAsync("Criticalities")) await db.EnsureCriticalitiesAsync();
            if (await db.TableExistsAsync("Categories")) await db.EnsureCategoriesAsync();
            if (await db.TableExistsAsync("Vendors")) await db.EnsureVendorsAsync();
            if (await db.TableExistsAsync("DeviceModels")) await db.EnsureDeviceModelsAsync();
            if (await db.TableExistsAsync("CatalogItems"))
                {
                    await db.EnsureCatalogItemsTecnologiaAsync();
                    await db.EnsureCatalogItemsAplicacionesAsync();
                }
            await db.EnsureOperacionesSampleAsync();
        }
        await db.EnsureSuperAdminUserAsync(config["Auth:SuperAdminUsername"]);
    }
    catch (Microsoft.Data.SqlClient.SqlException)
    {
        // Si falta alguna tabla de catálogo, la app puede arrancar igual.
    }
    }
}
catch (Exception ex)
{
    var errPath = Path.Combine(app.Environment.ContentRootPath, "startup_error.txt");
    try { File.WriteAllText(errPath, $"{DateTime.UtcNow:o}\r\n\r\n{ex}\r\n\r\n---\r\n{ex.StackTrace}"); }
    catch { /* ignorar */ }
    throw;
}

static async Task ForceRepairMigrationsAsync(AppDbContext db, IServiceProvider rootServices, IConfiguration config)
{
    var connStr = db.Database.GetConnectionString() ?? config.GetConnectionString("IITS");
    if (string.IsNullOrEmpty(connStr)) return;
    var toRemove = new[] { "20260218100000_AddPermissionRolePermission", "20260218110000_AddApprovalAuditEmailOutbox", "20260218120000_AddAssetManagedAccountCatalogs", "20260218130000_ExpandOperacionFields", "20260218140000_OperacionCamposActivos", "20260218145336_AddCatalogItemsAndCuentasFields" };
    await using (var conn = new SqlConnection(connStr))
    {
        await conn.OpenAsync();
        foreach (var id in toRemove)
        {
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM [__EFMigrationsHistory] WHERE [MigrationId] = @id";
            cmd.Parameters.AddWithValue("@id", id);
            await cmd.ExecuteNonQueryAsync();
        }
    }
    using (var scope2 = rootServices.CreateScope())
    {
        var db2 = scope2.ServiceProvider.GetRequiredService<AppDbContext>();
        await db2.Database.MigrateAsync();
    }
}

static async Task UnmarkMigrationsIfTablesMissingAsync(AppDbContext db, IServiceProvider rootServices, IConfiguration config)
{
    try
    {
        if (await db.TableExistsAsync("Permissions")) return;
        await ForceRepairMigrationsAsync(db, rootServices, config);
    }
    catch { /* si falla, seguir; las migraciones se aplicarán manualmente */ }
}

/// <summary>
/// Cuando la BD tiene las tablas pero __EFMigrationsHistory está vacío (p. ej. se borró),
/// insertamos los IDs de las migraciones ya aplicadas para que MigrateAsync solo aplique las pendientes.
/// </summary>
static async Task SyncMigrationHistoryAsync(AppDbContext db)
{
    var migrationsAlreadyInDb = new[]
    {
        "20260206193156_InitialSchema",
        "20260207000001_AprobacionPermiso",
        "20260207000002_AplicacionCatalogos",
        "20260207120000_RevertAplicacionCatalogos",
        "20260207200000_CatalogosFormulario",
        "20260218100000_AddPermissionRolePermission",
        "20260218110000_AddApprovalAuditEmailOutbox",
        "20260218120000_AddAssetManagedAccountCatalogs",
        "20260218130000_ExpandOperacionFields",
        "20260218140000_OperacionCamposActivos",
        "20260218145336_AddCatalogItemsAndCuentasFields"
    };
    var version = "8.0.11";
    foreach (var id in migrationsAlreadyInDb)
    {
        await db.Database.ExecuteSqlRawAsync(
            "IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = {0}) INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES ({0}, {1})",
            id, version);
    }
}

var pathBase = app.Configuration["PathBase"]?.Trim();
if (!string.IsNullOrEmpty(pathBase))
{
    pathBase = "/" + pathBase.Trim('/');
    app.UsePathBase(pathBase);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseMiddleware<DevAuthMiddleware>();
app.UseMiddleware<SessionCookieSignInMiddleware>();
app.UseAuthorization();
app.UseRateLimiter();

// Rutas de API primero (antes de Blazor) para que no las capture el fallback y devuelva HTML
// PDF de auditoría (pendientes + datos del módulo + aprobadores)
app.MapGet("/api/auditoria/pdf", async (HttpContext ctx) =>
{
    var modulo = ctx.Request.Query["modulo"].FirstOrDefault() ?? "";
    var modulosValidos = new[] { "aplicaciones", "operaciones", "cuentas" };
    if (!modulosValidos.Contains(modulo, StringComparer.OrdinalIgnoreCase))
    {
        ctx.Response.StatusCode = 400;
        await ctx.Response.WriteAsync("Parámetro modulo requerido: aplicaciones | operaciones | cuentas");
        return;
    }
    var scope = ctx.RequestServices.CreateScope();
    var pdfService = scope.ServiceProvider.GetRequiredService<IITS.Services.IAuditoriaPdfService>();
    var bytes = await pdfService.GenerarPdfAsync(modulo);
    var fileName = $"Auditoria_{modulo}_{DateTime.Now:yyyyMMdd_HHmm}.pdf";
    ctx.Response.ContentType = "application/pdf";
    ctx.Response.Headers.Append("Content-Disposition", $"attachment; filename=\"{fileName}\"; filename*=UTF-8''{Uri.EscapeDataString(fileName)}");
    await ctx.Response.Body.WriteAsync(bytes);
}).RequireRateLimiting("export");

// Exportación: /api/export/{modulo}/xlsx|pdf|csv (Aplicaciones, Logs, etc.)
app.MapGet("/api/export/{modulo}/{formato}", async (string modulo, string formato, HttpContext ctx) =>
{
    var scope = ctx.RequestServices.CreateScope();
    var exportSvc = scope.ServiceProvider.GetRequiredService<IITS.Services.IExportService>();
    var ahora = DateTime.Now;
    var fecha = ahora.ToString("yyyyMMdd_HHmm");
    string? tablaFilter = ctx.Request.Query["tabla"].FirstOrDefault();
    string? moduloFilter = ctx.Request.Query["modulo"].FirstOrDefault();
    string? areaFilter = ctx.Request.Query["area"].FirstOrDefault();
    string? tipoFilter = ctx.Request.Query["tipo"].FirstOrDefault();
    string? estatusFilter = ctx.Request.Query["estatus"].FirstOrDefault();
    static string Na(string? s) => string.IsNullOrWhiteSpace(s) ? "N/A" : s;

    if (modulo.Equals("Aplicaciones", StringComparison.OrdinalIgnoreCase))
    {
        var aplicacionSvc = scope.ServiceProvider.GetRequiredService<IITS.Services.IAplicacionService>();
        var list = await aplicacionSvc.GetAllAsync();
        if (!string.IsNullOrWhiteSpace(estatusFilter))
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var estatusActivo = await db.Estatus.AsNoTracking().OrderBy(e => e.Codigo).FirstOrDefaultAsync(e => e.Codigo == 1000);
            var estatusDesinc = await db.Estatus.AsNoTracking().OrderBy(e => e.Codigo).FirstOrDefaultAsync(e => e.Codigo == 2000);
            if (estatusFilter.Equals("Activo", StringComparison.OrdinalIgnoreCase) && estatusActivo != null)
                list = list.Where(a => a.EstatusId == estatusActivo.Id).ToList();
            else if (estatusFilter.Equals("Desincorporado", StringComparison.OrdinalIgnoreCase) && estatusDesinc != null)
                list = list.Where(a => a.EstatusId == estatusDesinc.Id).ToList();
            else if (estatusFilter.Equals("Inactivo", StringComparison.OrdinalIgnoreCase))
            {
                var estatusInactivo = await db.Estatus.AsNoTracking().OrderBy(e => e.Codigo).FirstOrDefaultAsync(e => e.Codigo == 1500);
                if (estatusInactivo != null) list = list.Where(a => a.EstatusId == estatusInactivo.Id).ToList();
            }
        }
        var datos = list.Count > 0
            ? list.Select(a => new Dictionary<string, object?>
            {
                ["Nombre"] = Na(a.Nombre),
                ["Funcionalidad"] = Na(a.Funcionalidad),
                ["Propietario"] = Na(a.Propietario),
                ["Responsable"] = Na(a.Responsable),
                ["TipoAlojamiento"] = Na(a.TipoAlojamiento),
                ["Proveedor"] = Na(a.Proveedor),
                ["ClasificacionInformacion"] = Na(a.ClasificacionInformacion),
                ["Critico"] = a.Critico,
                ["IntegracionesRelevantes"] = Na(a.IntegracionesRelevantes),
                ["DependenciasTecnicas"] = Na(a.DependenciasTecnicas),
                ["ModeloLicenciamiento"] = Na(a.ModeloLicenciamiento),
                ["CostoAnualEstimado"] = a.CostoAnualEstimado.HasValue ? a.CostoAnualEstimado.Value.ToString("N2", CultureInfo.GetCultureInfo("es-ES")) : "N/A",
                ["FechaAdquisicionImplementacion"] = a.FechaAdquisicionImplementacion?.ToString("yyyy-MM-dd") ?? "N/A",
                ["VersionActual"] = Na(a.VersionActual),
                ["SLA"] = Na(a.SLA),
                ["RTO"] = Na(a.RTO),
                ["RPO"] = Na(a.RPO),
                ["Autenticacion"] = Na(a.Autenticacion),
                ["Estatus"] = Na(a.Estatus?.Nombre)
            }).Cast<IReadOnlyDictionary<string, object?>>().ToList()
            : new List<IReadOnlyDictionary<string, object?>> { new Dictionary<string, object?> { ["Nombre"] = null, ["Funcionalidad"] = null, ["Propietario"] = null, ["Responsable"] = null, ["TipoAlojamiento"] = null, ["Proveedor"] = null, ["ClasificacionInformacion"] = null, ["Critico"] = null, ["IntegracionesRelevantes"] = null, ["DependenciasTecnicas"] = null, ["ModeloLicenciamiento"] = null, ["CostoAnualEstimado"] = null, ["FechaAdquisicionImplementacion"] = null, ["VersionActual"] = null, ["SLA"] = null, ["RTO"] = null, ["RPO"] = null, ["Autenticacion"] = null, ["Estatus"] = null } };
        await EscribirExportacion(ctx, exportSvc, "Aplicaciones", datos, formato, fecha);
    }
    else if (modulo.Equals("Logs", StringComparison.OrdinalIgnoreCase))
    {
        var auditSvc = scope.ServiceProvider.GetRequiredService<IITS.Services.IAuditLogService>();
        var logs = await auditSvc.GetLogsAsync(string.IsNullOrWhiteSpace(tablaFilter) ? null : tablaFilter, max: 5000);
        var datos = logs.Count > 0
            ? logs.Select(l => new Dictionary<string, object?> { ["Fecha"] = l.Fecha.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"), ["Usuario"] = l.UsuarioNombre ?? "", ["Tabla"] = l.Tabla, ["Accion"] = l.Accion, ["Detalle"] = l.Detalle ?? "" }).Cast<IReadOnlyDictionary<string, object?>>().ToList()
            : new List<IReadOnlyDictionary<string, object?>> { new Dictionary<string, object?> { ["Fecha"] = null, ["Usuario"] = null, ["Tabla"] = null, ["Accion"] = null, ["Detalle"] = null } };
        await EscribirExportacion(ctx, exportSvc, "Logs", datos, formato, fecha);
    }
    else if (modulo.Equals("Operaciones", StringComparison.OrdinalIgnoreCase) || modulo.Equals("Tecnologia", StringComparison.OrdinalIgnoreCase))
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        IReadOnlyList<IReadOnlyDictionary<string, object?>> datos;
        if (await db.TableExistsAsync("Offices"))
        {
            IQueryable<IITS.Entities.Operacion> query = db.Operaciones.AsNoTracking()
                .Include(o => o.Estatus).Include(o => o.Office).Include(o => o.Area).Include(o => o.Environment)
                .Include(o => o.Criticality).Include(o => o.Category).Include(o => o.Manufacturer).Include(o => o.DeviceModel).Include(o => o.Alojamiento).Include(o => o.OwnerArea);
            if (!string.IsNullOrWhiteSpace(areaFilter))
                query = query.Where(o => o.Area != null && o.Area.Name == areaFilter);
            if (!string.IsNullOrWhiteSpace(estatusFilter))
            {
                var estatusActivo = await db.Estatus.AsNoTracking().OrderBy(e => e.Codigo).FirstOrDefaultAsync(e => e.Nombre == "Activo" || e.Codigo == 1000);
                var estatusDesinc = await db.Estatus.AsNoTracking().OrderBy(e => e.Codigo).FirstOrDefaultAsync(e => e.Nombre == "Desincorporado" || e.Codigo == 2000);
                var estatusInactivo = await db.Estatus.AsNoTracking().OrderBy(e => e.Codigo).FirstOrDefaultAsync(e => e.Nombre == "Inactivo" || e.Codigo == 1500);
                if (estatusFilter.Equals("Activo", StringComparison.OrdinalIgnoreCase) && estatusActivo != null)
                    query = query.Where(o => o.EstatusId == estatusActivo.Id);
                else if (estatusFilter.Equals("Desincorporado", StringComparison.OrdinalIgnoreCase) && estatusDesinc != null)
                    query = query.Where(o => o.EstatusId == estatusDesinc.Id);
                else if (estatusFilter.Equals("Inactivo", StringComparison.OrdinalIgnoreCase) && estatusInactivo != null)
                    query = query.Where(o => o.EstatusId == estatusInactivo.Id);
            }
            var list = await query.OrderBy(o => o.Hostname).ToListAsync();
            var emptyRow = new Dictionary<string, object?> { ["Oficina"] = null, ["Área responsable"] = null, ["Dispositivo"] = null, ["Hostname"] = null, ["Entorno de operación"] = null, ["Propietario"] = null, ["Criticidad"] = null, ["Ambiente"] = null, ["Fabricante"] = null, ["Modelo"] = null, ["Función/Uso"] = null, ["Tipo de infraestructura"] = null, ["Serial"] = null, ["Sistema Operativo"] = null, ["Firmware"] = null, ["Garantía"] = null, ["BCP"] = null, ["RTO"] = null, ["RPO"] = null, ["Descripción"] = null, ["Clasificación de la información"] = null };
            datos = list.Count > 0
                ? list.Select(o => new Dictionary<string, object?>
                {
                    ["Oficina"] = Na(o.Office?.Name),
                    ["Área responsable"] = Na(o.Area?.Name),
                    ["Dispositivo"] = Na(o.TipoDispositivo),
                    ["Hostname"] = Na(o.Hostname),
                    ["Entorno de operación"] = Na(o.Alojamiento?.Nombre),
                    ["Propietario"] = Na(o.Propietario ?? o.OwnerArea?.Name),
                    ["Criticidad"] = Na(o.Criticality?.Name),
                    ["Ambiente"] = Na(o.Environment?.Name),
                    ["Fabricante"] = Na(o.Manufacturer?.Name),
                    ["Modelo"] = Na(o.DeviceModel?.Name),
                    ["Función/Uso"] = Na(o.Funcion),
                    ["Tipo de infraestructura"] = Na(o.TipoInfraestructura),
                    ["Serial"] = Na(o.Serial),
                    ["Sistema Operativo"] = Na(o.SistemaOperativo),
                    ["Firmware"] = Na(o.Firmware),
                    ["Garantía"] = o.GarantiaExpira.HasValue ? o.GarantiaExpira.Value.ToString("yyyy-MM-dd") : "N/A",
                    ["BCP"] = o.BCP == true ? "Sí" : (o.BCP == false ? "No" : "N/A"),
                    ["RTO"] = Na(o.RTO),
                    ["RPO"] = Na(o.RPO),
                    ["Descripción"] = Na(o.Observaciones),
                    ["Clasificación de la información"] = Na(o.ClasificacionInformacion)
                }).Cast<IReadOnlyDictionary<string, object?>>().ToList()
                : new List<IReadOnlyDictionary<string, object?>> { emptyRow };
        }
        else
        {
            var list = await db.Operaciones.AsNoTracking()
                .OrderBy(o => o.Hostname)
                .Select(o => new { o.Hostname, o.Serial, EstatusNombre = o.Estatus != null ? o.Estatus.Nombre : null })
                .ToListAsync();
            var emptyRow = new Dictionary<string, object?> { ["Hostname"] = null, ["Serial"] = null, ["Estatus"] = null };
            datos = list.Count > 0
                ? list.Select(o => new Dictionary<string, object?> { ["Hostname"] = Na(o.Hostname), ["Serial"] = Na(o.Serial), ["Estatus"] = Na(o.EstatusNombre) }).Cast<IReadOnlyDictionary<string, object?>>().ToList()
                : new List<IReadOnlyDictionary<string, object?>> { emptyRow };
        }
        await EscribirExportacion(ctx, exportSvc, "Operaciones", datos, formato, fecha);
    }
    else if (modulo.Equals("Cuentas", StringComparison.OrdinalIgnoreCase))
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var areasExisten = await db.TableExistsAsync("Areas");
        List<IReadOnlyDictionary<string, object?>> datos;
        Guid? estatusIdFiltro = null;
        if (!string.IsNullOrWhiteSpace(estatusFilter))
        {
            var est = await db.Estatus.AsNoTracking().OrderBy(e => e.Codigo).FirstOrDefaultAsync(e =>
                (estatusFilter.Equals("Activo", StringComparison.OrdinalIgnoreCase) && (e.Nombre == "Activo" || e.Codigo == 1000)) ||
                (estatusFilter.Equals("Desincorporado", StringComparison.OrdinalIgnoreCase) && (e.Nombre == "Desincorporado" || e.Codigo == 2000)) ||
                (estatusFilter.Equals("Inactivo", StringComparison.OrdinalIgnoreCase) && (e.Nombre == "Inactivo" || e.Codigo == 1500)));
            estatusIdFiltro = est?.Id;
        }
        bool incluirPriv = string.IsNullOrWhiteSpace(tipoFilter) || tipoFilter.Equals("Privilegiada", StringComparison.OrdinalIgnoreCase);
        bool incluirServ = string.IsNullOrWhiteSpace(tipoFilter) || tipoFilter.Equals("Servicio", StringComparison.OrdinalIgnoreCase);
        if (areasExisten)
        {
            IQueryable<IITS.Entities.CuentaPrivilegiada> qPriv = db.CuentasPrivilegiadas.AsNoTracking().Include(c => c.Estatus).Include(c => c.Area).Include(c => c.Aplicacion);
            IQueryable<IITS.Entities.CuentaServicio> qServ = db.CuentasServicio.AsNoTracking().Include(c => c.Estatus).Include(c => c.Area).Include(c => c.Aplicacion);
            if (estatusIdFiltro.HasValue) { qPriv = qPriv.Where(c => c.EstatusId == estatusIdFiltro.Value); qServ = qServ.Where(c => c.EstatusId == estatusIdFiltro.Value); }
            var priv = incluirPriv ? await qPriv.OrderBy(c => c.Nombre).ToListAsync() : new List<IITS.Entities.CuentaPrivilegiada>();
            var serv = incluirServ ? await qServ.OrderBy(c => c.Nombre).ToListAsync() : new List<IITS.Entities.CuentaServicio>();
            datos = priv.Count > 0 || serv.Count > 0
                ? priv.Select(p => new Dictionary<string, object?> { ["Tipo"] = "Privilegiada", ["Nombre"] = Na(p.Nombre), ["Área"] = Na(p.Area?.Name), ["Responsable"] = Na(p.Responsable), ["Origen"] = Na(p.Origen), ["ServicioAplicacion"] = Na(p.Aplicacion?.Nombre ?? p.ServicioRelacionado), ["TipoConfiguracionCambio"] = Na(p.TipoConfiguracionCambio), ["IntervaloCambioDias"] = p.IntervaloCambioDias?.ToString() ?? "N/A", ["GruposSeguridad"] = Na(p.GruposSeguridad), ["Descripcion"] = Na(p.Descripcion), ["Estatus"] = Na(p.Estatus?.Nombre) }).Cast<IReadOnlyDictionary<string, object?>>()
                    .Concat(serv.Select(s => new Dictionary<string, object?> { ["Tipo"] = "Servicio", ["Nombre"] = Na(s.Nombre), ["Área"] = Na(s.Area?.Name), ["Responsable"] = Na(s.Responsable), ["Origen"] = Na(s.Origen), ["ServicioAplicacion"] = Na(s.Aplicacion?.Nombre ?? s.ServicioRelacionado), ["TipoConfiguracionCambio"] = Na(s.TipoConfiguracionCambio), ["IntervaloCambioDias"] = s.IntervaloCambioDias?.ToString() ?? "N/A", ["GruposSeguridad"] = Na(s.GruposSeguridad), ["Descripcion"] = Na(s.Descripcion), ["Estatus"] = Na(s.Estatus?.Nombre) }).Cast<IReadOnlyDictionary<string, object?>>()).ToList()
                : new List<IReadOnlyDictionary<string, object?>> { new Dictionary<string, object?> { ["Tipo"] = null, ["Nombre"] = null, ["Área"] = null, ["Responsable"] = null, ["Origen"] = null, ["ServicioAplicacion"] = null, ["TipoConfiguracionCambio"] = null, ["IntervaloCambioDias"] = null, ["GruposSeguridad"] = null, ["Descripcion"] = null, ["Estatus"] = null } };
        }
        else
        {
            IQueryable<IITS.Entities.CuentaPrivilegiada> qPriv2 = db.CuentasPrivilegiadas.AsNoTracking().Include(c => c.Estatus).Include(c => c.Aplicacion);
            IQueryable<IITS.Entities.CuentaServicio> qServ2 = db.CuentasServicio.AsNoTracking().Include(c => c.Estatus).Include(c => c.Aplicacion);
            if (estatusIdFiltro.HasValue) { qPriv2 = qPriv2.Where(c => c.EstatusId == estatusIdFiltro.Value); qServ2 = qServ2.Where(c => c.EstatusId == estatusIdFiltro.Value); }
            var priv = incluirPriv ? await qPriv2.OrderBy(c => c.Nombre).ToListAsync() : new List<IITS.Entities.CuentaPrivilegiada>();
            var serv = incluirServ ? await qServ2.OrderBy(c => c.Nombre).ToListAsync() : new List<IITS.Entities.CuentaServicio>();
            datos = priv.Count > 0 || serv.Count > 0
                ? priv.Select(p => new Dictionary<string, object?> { ["Tipo"] = "Privilegiada", ["Nombre"] = Na(p.Nombre), ["Área"] = "N/A", ["Responsable"] = Na(p.Responsable), ["Origen"] = Na(p.Origen), ["ServicioAplicacion"] = Na(p.Aplicacion?.Nombre ?? p.ServicioRelacionado), ["TipoConfiguracionCambio"] = Na(p.TipoConfiguracionCambio), ["IntervaloCambioDias"] = p.IntervaloCambioDias?.ToString() ?? "N/A", ["GruposSeguridad"] = Na(p.GruposSeguridad), ["Descripcion"] = Na(p.Descripcion), ["Estatus"] = Na(p.Estatus?.Nombre) }).Cast<IReadOnlyDictionary<string, object?>>()
                    .Concat(serv.Select(s => new Dictionary<string, object?> { ["Tipo"] = "Servicio", ["Nombre"] = Na(s.Nombre), ["Área"] = "N/A", ["Responsable"] = Na(s.Responsable), ["Origen"] = Na(s.Origen), ["ServicioAplicacion"] = Na(s.Aplicacion?.Nombre ?? s.ServicioRelacionado), ["TipoConfiguracionCambio"] = Na(s.TipoConfiguracionCambio), ["IntervaloCambioDias"] = s.IntervaloCambioDias?.ToString() ?? "N/A", ["GruposSeguridad"] = Na(s.GruposSeguridad), ["Descripcion"] = Na(s.Descripcion), ["Estatus"] = Na(s.Estatus?.Nombre) }).Cast<IReadOnlyDictionary<string, object?>>()).ToList()
                : new List<IReadOnlyDictionary<string, object?>> { new Dictionary<string, object?> { ["Tipo"] = null, ["Nombre"] = null, ["Área"] = null, ["Responsable"] = null, ["Origen"] = null, ["ServicioAplicacion"] = null, ["TipoConfiguracionCambio"] = null, ["IntervaloCambioDias"] = null, ["GruposSeguridad"] = null, ["Descripcion"] = null, ["Estatus"] = null } };
        }
        await EscribirExportacion(ctx, exportSvc, "Cuentas", datos, formato, fecha);
    }
    else if (modulo.Equals("Aprobaciones", StringComparison.OrdinalIgnoreCase))
    {
        var aprobacionSvc = scope.ServiceProvider.GetRequiredService<IAprobacionService>();
        var list = await aprobacionSvc.GetAllAsync(string.IsNullOrWhiteSpace(moduloFilter) ? null : moduloFilter);
        var estadoFilter = ctx.Request.Query["estado"].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(estadoFilter))
            list = list.Where(a => (a.Estado ?? "").Equals(estadoFilter, StringComparison.OrdinalIgnoreCase)).ToList();
        var datos = list.Count > 0
            ? list.Select(a => new Dictionary<string, object?> { ["Fecha"] = a.Fecha.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"), ["Modulo"] = a.Modulo, ["EntidadId"] = a.EntidadId.ToString(), ["Estado"] = a.Estado ?? "", ["Comentario"] = a.Comentario ?? "" }).Cast<IReadOnlyDictionary<string, object?>>().ToList()
            : new List<IReadOnlyDictionary<string, object?>> { new Dictionary<string, object?> { ["Fecha"] = null, ["Modulo"] = null, ["EntidadId"] = null, ["Estado"] = null, ["Comentario"] = null } };
        await EscribirExportacion(ctx, exportSvc, "Aprobaciones", datos, formato, fecha);
    }
    else if (modulo.Equals("Usuarios", StringComparison.OrdinalIgnoreCase))
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var users = await db.Users.AsNoTracking().Include(u => u.UserRoles).ThenInclude(ur => ur.Role).OrderBy(u => u.Username).ToListAsync();
        var datos = users.Count > 0
            ? users.Select(u => new Dictionary<string, object?>
            {
                ["Usuario"] = Na(u.Username),
                ["Nombre"] = Na($"{u.Nombre} {u.Apellido}".Trim()),
                ["Email"] = Na(u.Email),
                ["Roles"] = string.Join(", ", u.UserRoles.Select(ur => ur.Role?.Nombre).Where(n => !string.IsNullOrEmpty(n)) ?? Array.Empty<string?>())
            }).Cast<IReadOnlyDictionary<string, object?>>().ToList()
            : new List<IReadOnlyDictionary<string, object?>> { new Dictionary<string, object?> { ["Usuario"] = null, ["Nombre"] = null, ["Email"] = null, ["Roles"] = null } };
        await EscribirExportacion(ctx, exportSvc, "Usuarios", datos, formato, fecha);
    }
    else if (modulo.Equals("Roles", StringComparison.OrdinalIgnoreCase))
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var roles = await db.Roles.AsNoTracking().OrderBy(r => r.Nombre).ToListAsync();
        var datos = roles.Count > 0
            ? roles.Select(r => new Dictionary<string, object?> { ["Nombre"] = Na(r.Nombre) }).Cast<IReadOnlyDictionary<string, object?>>().ToList()
            : new List<IReadOnlyDictionary<string, object?>> { new Dictionary<string, object?> { ["Nombre"] = null } };
        await EscribirExportacion(ctx, exportSvc, "Roles", datos, formato, fecha);
    }
    else if (modulo.Equals("PermisosRol", StringComparison.OrdinalIgnoreCase))
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var rps = await db.RolePermissions.AsNoTracking().Include(rp => rp.Role).Include(rp => rp.Permission).OrderBy(rp => rp.Role!.Nombre).ThenBy(rp => rp.Permission!.Code).ToListAsync();
        var datos = rps.Count > 0
            ? rps.Select(rp => new Dictionary<string, object?> { ["Rol"] = Na(rp.Role?.Nombre), ["Permiso"] = Na(rp.Permission?.Code) }).Cast<IReadOnlyDictionary<string, object?>>().ToList()
            : new List<IReadOnlyDictionary<string, object?>> { new Dictionary<string, object?> { ["Rol"] = null, ["Permiso"] = null } };
        await EscribirExportacion(ctx, exportSvc, "PermisosRol", datos, formato, fecha);
    }
    else if (modulo.Equals("PermisosAprobacion", StringComparison.OrdinalIgnoreCase))
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var aps = await db.AprobacionPermisos.AsNoTracking().Include(ap => ap.User).OrderBy(ap => ap.User != null ? ap.User.Username : "").ThenBy(ap => ap.Modulo).ToListAsync();
        static string NombreMod(string? m) => (m ?? "").ToLowerInvariant() switch { "aplicaciones" => "Aplicaciones", "operaciones" => "Tecnología", "cuentas" => "Cuentas", _ => m ?? "N/A" };
        var datos = aps.Count > 0
            ? aps.Select(ap => new Dictionary<string, object?>
            {
                ["Usuario"] = Na(ap.User?.Username),
                ["Nombre"] = Na(ap.User != null ? $"{ap.User.Nombre} {ap.User.Apellido}".Trim() : null),
                ["Módulo"] = NombreMod(ap.Modulo)
            }).Cast<IReadOnlyDictionary<string, object?>>().ToList()
            : new List<IReadOnlyDictionary<string, object?>> { new Dictionary<string, object?> { ["Usuario"] = null, ["Nombre"] = null, ["Módulo"] = null } };
        await EscribirExportacion(ctx, exportSvc, "PermisosAprobacion", datos, formato, fecha);
    }
    else
    {
        ctx.Response.StatusCode = 404;
    }
}).RequireRateLimiting("export");

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

static async Task EscribirExportacion(HttpContext ctx, IExportService exportSvc, string nombreModulo, IReadOnlyList<IReadOnlyDictionary<string, object?>> datos, string formato, string timeStamp)
{
    byte[] bytes; string contentType; string fileName;
    if (formato.Equals("xlsx", StringComparison.OrdinalIgnoreCase) || formato.Equals("excel", StringComparison.OrdinalIgnoreCase))
    {
        bytes = await exportSvc.ExportToExcelAsync(nombreModulo, datos);
        contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        fileName = $"{nombreModulo}_{timeStamp}.xlsx";
    }
    else if (formato.Equals("pdf", StringComparison.OrdinalIgnoreCase))
    {
        bytes = await exportSvc.ExportToPdfAsync(nombreModulo, datos);
        contentType = "application/pdf";
        fileName = $"{nombreModulo}_{timeStamp}.pdf";
    }
    else if (formato.Equals("csv", StringComparison.OrdinalIgnoreCase))
    {
        bytes = await exportSvc.ExportToCsvAsync(nombreModulo, datos);
        contentType = "text/csv; charset=utf-8";
        fileName = $"{nombreModulo}_{timeStamp}.csv";
    }
    else
    {
        ctx.Response.StatusCode = 400;
        return;
    }
    ctx.Response.ContentType = contentType;
    ctx.Response.Headers.Append("Content-Disposition", $"attachment; filename=\"{fileName}\"");
    await ctx.Response.Body.WriteAsync(bytes);
}

app.Run();
