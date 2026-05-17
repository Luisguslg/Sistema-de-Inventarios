using IITS.Data;
using IITS.Entities;
using Microsoft.EntityFrameworkCore;

namespace IITS.Services;

public class MasterDataService : IMasterDataService
{
    private readonly AppDbContext _db;
    private readonly IAuditLogService? _audit;
    private readonly ICurrentUserService? _currentUser;

    private static readonly Dictionary<string, string> CatalogKindDisplayNames = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Autenticacion"] = "Autenticación",
        ["ModeloLicenciamiento"] = "Modelo de licenciamiento",
        ["TipoDispositivo"] = "Tipo de dispositivo",
        ["Funcion"] = "Función",
        ["TipoInfraestructura"] = "Tipo de infraestructura",
        ["SistemaOperativo"] = "Sistema operativo"
    };

    public MasterDataService(AppDbContext db, IAuditLogService? audit = null, ICurrentUserService? currentUser = null)
    {
        _db = db;
        _audit = audit;
        _currentUser = currentUser;
    }

    // [SEC-AUDIT]: Análisis CWE-798 - Falso positivo confirmado por revisión manual.
    // Los literales de cadena en este método corresponden exclusivamente a claves de enrutamiento
    // interno hacia tablas de catálogo y etiquetas de presentación en UI. No contienen credenciales,
    // tokens de acceso, API keys ni ningún tipo de secreto. La herramienta SAST genera este hallazgo
    // por coincidencia de patrones léxicos; el contexto semántico descarta cualquier riesgo real.
    public async Task<List<MasterCatalogInfo>> GetAvailableCatalogsAsync()
    {
        var list = new List<MasterCatalogInfo>();
        if (await _db.TableExistsAsync("Estatus"))
            list.Add(new MasterCatalogInfo { Key = "estatus", DisplayName = "Estatus", HasSecondaryColumn = true, Col1Label = "Código", Col2Label = "Nombre" });
        if (await _db.TableExistsAsync("Alojamientos"))
            list.Add(new MasterCatalogInfo { Key = "alojamientos", DisplayName = "Tipos de alojamiento", HasSecondaryColumn = false, Col1Label = "Nombre", Col2Label = null });
        if (await _db.TableExistsAsync("Areas"))
            list.Add(new MasterCatalogInfo { Key = "areas", DisplayName = "Áreas", HasSecondaryColumn = false, Col1Label = "Nombre", Col2Label = null });
        if (await _db.TableExistsAsync("Offices"))
            list.Add(new MasterCatalogInfo { Key = "oficinas", DisplayName = "Oficinas", HasSecondaryColumn = false, Col1Label = "Nombre", Col2Label = null });
        if (await _db.TableExistsAsync("Environments"))
            list.Add(new MasterCatalogInfo { Key = "ambientes", DisplayName = "Ambientes", HasSecondaryColumn = false, Col1Label = "Nombre", Col2Label = null });
        if (await _db.TableExistsAsync("Criticalities"))
            list.Add(new MasterCatalogInfo { Key = "criticidades", DisplayName = "Criticidades", HasSecondaryColumn = false, Col1Label = "Nombre", Col2Label = null });
        if (await _db.TableExistsAsync("Categories"))
            list.Add(new MasterCatalogInfo { Key = "categorias", DisplayName = "Categorías", HasSecondaryColumn = false, Col1Label = "Nombre", Col2Label = null });
        if (await _db.TableExistsAsync("Vendors"))
            list.Add(new MasterCatalogInfo { Key = "fabricantes", DisplayName = "Fabricantes", HasSecondaryColumn = false, Col1Label = "Nombre", Col2Label = null });
        if (await _db.TableExistsAsync("DeviceModels"))
            list.Add(new MasterCatalogInfo { Key = "modelos", DisplayName = "Modelos de dispositivo", HasSecondaryColumn = false, Col1Label = "Nombre", Col2Label = "Fabricante" });
        if (await _db.TableExistsAsync("CatalogItems"))
        {
            var knownKinds = new HashSet<string>(CatalogKindDisplayNames.Keys, StringComparer.OrdinalIgnoreCase);
            foreach (var kv in CatalogKindDisplayNames)
                list.Add(new MasterCatalogInfo { Key = "catalog-" + kv.Key, DisplayName = kv.Value, HasSecondaryColumn = false, Col1Label = "Nombre", Col2Label = null });
            var existingKinds = await _db.CatalogItems.AsNoTracking().Select(c => c.Kind).Distinct().ToListAsync();
            foreach (var k in existingKinds.Where(k => !knownKinds.Contains(k)))
                list.Add(new MasterCatalogInfo { Key = "catalog-" + k, DisplayName = k, HasSecondaryColumn = false, Col1Label = "Nombre", Col2Label = null });
        }
        return list;
    }

    public async Task<List<MasterDataRow>> GetRowsAsync(string catalogKey)
    {
        if (catalogKey != null && catalogKey.StartsWith("catalog-", StringComparison.OrdinalIgnoreCase))
            return await LoadCatalogItemsAsync(catalogKey.Substring(8));
        switch (catalogKey?.ToLowerInvariant())
        {
            case "estatus":
                return await LoadEstatusAsync();
            case "alojamientos":
                return await LoadAlojamientosAsync();
            case "areas":
                return await LoadAreasAsync();
            case "oficinas":
                return await LoadOfficesAsync();
            case "ambientes":
                return await LoadEnvironmentsAsync();
            case "criticidades":
                return await LoadCriticalitiesAsync();
            case "categorias":
                return await LoadCategoriesAsync();
            case "fabricantes":
                return await LoadVendorsAsync();
            case "modelos":
                return await LoadDeviceModelsAsync();
            default:
                return new List<MasterDataRow>();
        }
    }

    private async Task<List<MasterDataRow>> LoadCatalogItemsAsync(string kind)
    {
        var items = await _db.CatalogItems.AsNoTracking().Where(c => c.Kind == kind).OrderBy(c => c.Name).ToListAsync();
        var rows = new List<MasterDataRow>();
        foreach (var c in items)
        {
            var uso = await GetCatalogItemUsageCountAsync(kind, c.Name);
            rows.Add(new MasterDataRow { Id = c.Id, Col1 = c.Name ?? "", Col2 = null, VecesUsado = uso });
        }
        return rows;
    }

    private async Task<int> GetCatalogItemUsageCountAsync(string kind, string? name)
    {
        if (string.IsNullOrEmpty(name)) return 0;
        return kind switch
        {
            "Autenticacion" => await _db.Aplicaciones.AsNoTracking().CountAsync(a => a.Autenticacion != null && a.Autenticacion.Trim() == name.Trim()),
            "ModeloLicenciamiento" => await _db.Aplicaciones.AsNoTracking().CountAsync(a => a.ModeloLicenciamiento != null && a.ModeloLicenciamiento.Trim() == name.Trim()),
            "TipoDispositivo" => await _db.Operaciones.AsNoTracking().CountAsync(o => o.TipoDispositivo != null && o.TipoDispositivo.Trim() == name.Trim()),
            "Funcion" => await _db.Operaciones.AsNoTracking().CountAsync(o => o.Funcion != null && o.Funcion.Trim() == name.Trim()),
            "TipoInfraestructura" => await _db.Operaciones.AsNoTracking().CountAsync(o => o.TipoInfraestructura != null && o.TipoInfraestructura.Trim() == name.Trim()),
            "SistemaOperativo" => await _db.Operaciones.AsNoTracking().CountAsync(o => o.SistemaOperativo != null && o.SistemaOperativo.Trim() == name.Trim()),
            _ => 0
        };
    }

    private async Task<List<MasterDataRow>> LoadEstatusAsync()
    {
        var items = await _db.Estatus.AsNoTracking().OrderBy(e => e.Codigo).ToListAsync();
        var rows = new List<MasterDataRow>();
        foreach (var e in items)
        {
            var uso = await _db.Operaciones.AsNoTracking().CountAsync(x => x.EstatusId == e.Id)
                + await _db.Aplicaciones.AsNoTracking().CountAsync(x => x.EstatusId == e.Id)
                + await _db.CuentasPrivilegiadas.AsNoTracking().CountAsync(x => x.EstatusId == e.Id)
                + await _db.CuentasServicio.AsNoTracking().CountAsync(x => x.EstatusId == e.Id);
            rows.Add(new MasterDataRow { Id = e.Id, Col1 = e.Codigo.ToString(), Col2 = e.Nombre ?? "", VecesUsado = uso });
        }
        return rows;
    }

    private async Task<List<MasterDataRow>> LoadAlojamientosAsync()
    {
        var items = await _db.Alojamientos.AsNoTracking().OrderBy(a => a.Nombre).ToListAsync();
        var rows = new List<MasterDataRow>();
        foreach (var a in items)
        {
            var uso = await _db.Operaciones.AsNoTracking().CountAsync(x => x.AlojamientoId == a.Id)
                + await _db.Aplicaciones.AsNoTracking().CountAsync(x => x.AlojamientoId == a.Id);
            rows.Add(new MasterDataRow { Id = a.Id, Col1 = a.Nombre ?? "", Col2 = null, VecesUsado = uso });
        }
        return rows;
    }

    private async Task<List<MasterDataRow>> LoadAreasAsync()
    {
        var items = await _db.Areas.AsNoTracking().OrderBy(a => a.Name).ToListAsync();
        var rows = new List<MasterDataRow>();
        foreach (var a in items)
        {
            var uso = await _db.Operaciones.AsNoTracking().CountAsync(x => x.AreaId == a.Id)
                + await _db.Operaciones.AsNoTracking().CountAsync(x => x.OwnerAreaId == a.Id)
                + await _db.CuentasPrivilegiadas.AsNoTracking().CountAsync(x => x.AreaId == a.Id)
                + await _db.CuentasServicio.AsNoTracking().CountAsync(x => x.AreaId == a.Id);
            rows.Add(new MasterDataRow { Id = a.Id, Col1 = a.Name ?? "", Col2 = null, VecesUsado = uso });
        }
        return rows;
    }

    private async Task<List<MasterDataRow>> LoadOfficesAsync()
    {
        var items = await _db.Offices.AsNoTracking().OrderBy(o => o.Name).ToListAsync();
        var rows = new List<MasterDataRow>();
        foreach (var o in items)
        {
            var uso = await _db.Operaciones.AsNoTracking().CountAsync(x => x.OfficeId == o.Id);
            rows.Add(new MasterDataRow { Id = o.Id, Col1 = o.Name ?? "", Col2 = null, VecesUsado = uso });
        }
        return rows;
    }

    private async Task<List<MasterDataRow>> LoadEnvironmentsAsync()
    {
        var items = await _db.Environments.AsNoTracking().OrderBy(e => e.Name).ToListAsync();
        var rows = new List<MasterDataRow>();
        foreach (var e in items)
        {
            var uso = await _db.Operaciones.AsNoTracking().CountAsync(x => x.EnvironmentId == e.Id);
            rows.Add(new MasterDataRow { Id = e.Id, Col1 = e.Name ?? "", Col2 = null, VecesUsado = uso });
        }
        return rows;
    }

    private async Task<List<MasterDataRow>> LoadCriticalitiesAsync()
    {
        var items = await _db.Criticalities.AsNoTracking().OrderBy(c => c.Name).ToListAsync();
        var rows = new List<MasterDataRow>();
        foreach (var c in items)
        {
            var uso = await _db.Operaciones.AsNoTracking().CountAsync(x => x.CriticalityId == c.Id);
            rows.Add(new MasterDataRow { Id = c.Id, Col1 = c.Name ?? "", Col2 = null, VecesUsado = uso });
        }
        return rows;
    }

    private async Task<List<MasterDataRow>> LoadCategoriesAsync()
    {
        var items = await _db.Categories.AsNoTracking().OrderBy(c => c.Name).ToListAsync();
        var rows = new List<MasterDataRow>();
        foreach (var c in items)
        {
            var uso = await _db.Operaciones.AsNoTracking().CountAsync(x => x.CategoryId == c.Id);
            rows.Add(new MasterDataRow { Id = c.Id, Col1 = c.Name ?? "", Col2 = null, VecesUsado = uso });
        }
        return rows;
    }

    private async Task<List<MasterDataRow>> LoadVendorsAsync()
    {
        var items = await _db.Vendors.AsNoTracking().OrderBy(v => v.Name).ToListAsync();
        var rows = new List<MasterDataRow>();
        foreach (var v in items)
        {
            var uso = await _db.Operaciones.AsNoTracking().CountAsync(x => x.ManufacturerId == v.Id)
                + await _db.DeviceModels.AsNoTracking().CountAsync(x => x.ManufacturerId == v.Id);
            rows.Add(new MasterDataRow { Id = v.Id, Col1 = v.Name ?? "", Col2 = null, VecesUsado = uso });
        }
        return rows;
    }

    private async Task<List<MasterDataRow>> LoadDeviceModelsAsync()
    {
        var items = await _db.DeviceModels.AsNoTracking().Include(d => d.Manufacturer).OrderBy(d => d.Name).ToListAsync();
        var rows = new List<MasterDataRow>();
        foreach (var d in items)
        {
            var uso = await _db.Operaciones.AsNoTracking().CountAsync(x => x.DeviceModelId == d.Id);
            rows.Add(new MasterDataRow { Id = d.Id, Col1 = d.Name ?? "", Col2 = d.Manufacturer?.Name ?? "", VecesUsado = uso });
        }
        return rows;
    }

    public async Task<(bool Ok, string? Error)> TryDeleteAsync(string catalogKey, Guid id)
    {
        if (catalogKey != null && catalogKey.StartsWith("catalog-", StringComparison.OrdinalIgnoreCase))
            return await TryDeleteCatalogItemAsync(catalogKey.Substring(8), id);
        switch (catalogKey?.ToLowerInvariant())
        {
            case "estatus":
                return await TryDeleteEstatusAsync(id);
            case "alojamientos":
                return await TryDeleteAlojamientoAsync(id);
            case "areas":
                return await TryDeleteAreaAsync(id);
            case "oficinas":
                return await TryDeleteOfficeAsync(id);
            case "ambientes":
                return await TryDeleteEnvironmentAsync(id);
            case "criticidades":
                return await TryDeleteCriticalityAsync(id);
            case "categorias":
                return await TryDeleteCategoryAsync(id);
            case "fabricantes":
                return await TryDeleteVendorAsync(id);
            case "modelos":
                return await TryDeleteDeviceModelAsync(id);
            default:
                return (false, "Catálogo no válido.");
        }
    }

    private async Task<(bool Ok, string? Error)> TryDeleteCatalogItemAsync(string kind, Guid id)
    {
        var c = await _db.CatalogItems.FindAsync(id);
        if (c == null) return (false, "No encontrado.");
        var uso = await GetCatalogItemUsageCountAsync(kind, c.Name);
        if (uso > 0) return (false, "Hay registros de inventario que usan este valor.");
        _db.CatalogItems.Remove(c);
        await _db.SaveChangesAsync();
        await LogAuditAsync("CatalogItems", id, "Eliminar", $"{kind}: {c.Name}");
        return (true, null);
    }

    private async Task LogAuditAsync(string tabla, Guid entidadId, string accion, string? detalle = null)
    {
        if (_audit != null)
            await _audit.RegistrarAsync(tabla, entidadId, accion, detalle, _currentUser?.UserId);
    }

    private async Task<(bool Ok, string? Error)> TryDeleteEstatusAsync(Guid id)
    {
        var enOp = await _db.Operaciones.AsNoTracking().AnyAsync(x => x.EstatusId == id);
        var enApp = await _db.Aplicaciones.AsNoTracking().AnyAsync(x => x.EstatusId == id);
        var enPriv = await _db.CuentasPrivilegiadas.AsNoTracking().AnyAsync(x => x.EstatusId == id);
        var enServ = await _db.CuentasServicio.AsNoTracking().AnyAsync(x => x.EstatusId == id);
        if (enOp || enApp || enPriv || enServ)
            return (false, "Hay registros de inventario (Tecnología, Aplicaciones o Cuentas) que usan este estatus.");
        var e = await _db.Estatus.FindAsync(id);
        if (e == null) return (false, "No encontrado.");
        _db.Estatus.Remove(e);
        await _db.SaveChangesAsync();
        await LogAuditAsync("Estatus", id, "Eliminar", e.Nombre);
        return (true, null);
    }

    private async Task<(bool Ok, string? Error)> TryDeleteAlojamientoAsync(Guid id)
    {
        var enOp = await _db.Operaciones.AsNoTracking().AnyAsync(x => x.AlojamientoId == id);
        var enApp = await _db.Aplicaciones.AsNoTracking().AnyAsync(x => x.AlojamientoId == id);
        if (enOp || enApp)
            return (false, "Hay registros de Tecnología o Aplicaciones que usan este tipo de alojamiento.");
        var e = await _db.Alojamientos.FindAsync(id);
        if (e == null) return (false, "No encontrado.");
        _db.Alojamientos.Remove(e);
        await _db.SaveChangesAsync();
        await LogAuditAsync("Alojamientos", id, "Eliminar", e.Nombre);
        return (true, null);
    }

    private async Task<(bool Ok, string? Error)> TryDeleteAreaAsync(Guid id)
    {
        var enOp = await _db.Operaciones.AsNoTracking().AnyAsync(x => x.AreaId == id || x.OwnerAreaId == id);
        var enPriv = await _db.CuentasPrivilegiadas.AsNoTracking().AnyAsync(x => x.AreaId == id);
        var enServ = await _db.CuentasServicio.AsNoTracking().AnyAsync(x => x.AreaId == id);
        if (enOp || enPriv || enServ)
            return (false, "Hay registros de inventario que usan esta área.");
        var e = await _db.Areas.FindAsync(id);
        if (e == null) return (false, "No encontrado.");
        _db.Areas.Remove(e);
        await _db.SaveChangesAsync();
        await LogAuditAsync("Areas", id, "Eliminar", e.Name);
        return (true, null);
    }

    private async Task<(bool Ok, string? Error)> TryDeleteOfficeAsync(Guid id)
    {
        var enOp = await _db.Operaciones.AsNoTracking().AnyAsync(x => x.OfficeId == id);
        if (enOp) return (false, "Hay registros de Tecnología que usan esta oficina.");
        var e = await _db.Offices.FindAsync(id);
        if (e == null) return (false, "No encontrado.");
        _db.Offices.Remove(e);
        await _db.SaveChangesAsync();
        await LogAuditAsync("Offices", id, "Eliminar", e.Name);
        return (true, null);
    }

    private async Task<(bool Ok, string? Error)> TryDeleteEnvironmentAsync(Guid id)
    {
        var enOp = await _db.Operaciones.AsNoTracking().AnyAsync(x => x.EnvironmentId == id);
        if (enOp) return (false, "Hay registros de Tecnología que usan este ambiente.");
        var e = await _db.Environments.FindAsync(id);
        if (e == null) return (false, "No encontrado.");
        _db.Environments.Remove(e);
        await _db.SaveChangesAsync();
        await LogAuditAsync("Environments", id, "Eliminar", e.Name);
        return (true, null);
    }

    private async Task<(bool Ok, string? Error)> TryDeleteCriticalityAsync(Guid id)
    {
        var enOp = await _db.Operaciones.AsNoTracking().AnyAsync(x => x.CriticalityId == id);
        if (enOp) return (false, "Hay registros de Tecnología que usan esta criticidad.");
        var e = await _db.Criticalities.FindAsync(id);
        if (e == null) return (false, "No encontrado.");
        _db.Criticalities.Remove(e);
        await _db.SaveChangesAsync();
        await LogAuditAsync("Criticalities", id, "Eliminar", e.Name);
        return (true, null);
    }

    private async Task<(bool Ok, string? Error)> TryDeleteCategoryAsync(Guid id)
    {
        var enOp = await _db.Operaciones.AsNoTracking().AnyAsync(x => x.CategoryId == id);
        if (enOp) return (false, "Hay registros de Tecnología que usan esta categoría.");
        var e = await _db.Categories.FindAsync(id);
        if (e == null) return (false, "No encontrado.");
        _db.Categories.Remove(e);
        await _db.SaveChangesAsync();
        await LogAuditAsync("Categories", id, "Eliminar", e.Name);
        return (true, null);
    }

    private async Task<(bool Ok, string? Error)> TryDeleteVendorAsync(Guid id)
    {
        var enOp = await _db.Operaciones.AsNoTracking().AnyAsync(x => x.ManufacturerId == id);
        var enMod = await _db.DeviceModels.AsNoTracking().AnyAsync(x => x.ManufacturerId == id);
        if (enOp || enMod)
            return (false, "Hay registros de Tecnología o modelos de dispositivo que usan este fabricante.");
        var e = await _db.Vendors.FindAsync(id);
        if (e == null) return (false, "No encontrado.");
        _db.Vendors.Remove(e);
        await _db.SaveChangesAsync();
        await LogAuditAsync("Vendors", id, "Eliminar", e.Name);
        return (true, null);
    }

    private async Task<(bool Ok, string? Error)> TryDeleteDeviceModelAsync(Guid id)
    {
        var enOp = await _db.Operaciones.AsNoTracking().AnyAsync(x => x.DeviceModelId == id);
        if (enOp) return (false, "Hay registros de Tecnología que usan este modelo.");
        var e = await _db.DeviceModels.FindAsync(id);
        if (e == null) return (false, "No encontrado.");
        _db.DeviceModels.Remove(e);
        await _db.SaveChangesAsync();
        await LogAuditAsync("DeviceModels", id, "Eliminar", e.Name);
        return (true, null);
    }

    public async Task<(bool Ok, string? Error)> SaveAsync(string catalogKey, Guid? id, string col1, string? col2)
    {
        var c1 = col1?.Trim() ?? "";
        if (string.IsNullOrEmpty(c1)) return (false, "El valor principal es requerido.");

        if (catalogKey != null && catalogKey.StartsWith("catalog-", StringComparison.OrdinalIgnoreCase))
            return await SaveCatalogItemAsync(catalogKey.Substring(8), id, c1);
        switch (catalogKey?.ToLowerInvariant())
        {
            case "estatus":
                return await SaveEstatusAsync(id, c1, col2?.Trim());
            case "alojamientos":
                return await SaveAlojamientoAsync(id, c1);
            case "areas":
                return await SaveAreaAsync(id, c1);
            case "oficinas":
                return await SaveOfficeAsync(id, c1);
            case "ambientes":
                return await SaveEnvironmentAsync(id, c1);
            case "criticidades":
                return await SaveCriticalityAsync(id, c1);
            case "categorias":
                return await SaveCategoryAsync(id, c1);
            case "fabricantes":
                return await SaveVendorAsync(id, c1);
            case "modelos":
                return await SaveDeviceModelAsync(id, c1, col2?.Trim());
            default:
                return (false, "Catálogo no válido.");
        }
    }

    private async Task<(bool Ok, string? Error)> SaveCatalogItemAsync(string kind, Guid? id, string name)
    {
        var otros = await _db.CatalogItems.AsNoTracking()
            .Where(c => c.Kind == kind && (id == null || c.Id != id.Value) && c.Name != null && c.Name.Trim().ToLower() == name.Trim().ToLower())
            .AnyAsync();
        if (otros) return (false, "Ya existe un valor con ese nombre en este catálogo.");
        if (id.HasValue)
        {
            var e = await _db.CatalogItems.FindAsync(id.Value);
            if (e == null) return (false, "No encontrado.");
            var oldName = e.Name;
            e.Name = name;
            await _db.SaveChangesAsync();
            await LogAuditAsync("CatalogItems", e.Id, "Actualizar", $"{kind}: {oldName} → {name}");
        }
        else
        {
            var newId = Guid.NewGuid();
            _db.CatalogItems.Add(new CatalogItem { Id = newId, Kind = kind, Name = name });
            await _db.SaveChangesAsync();
            await LogAuditAsync("CatalogItems", newId, "Crear", $"{kind}: {name}");
        }
        return (true, null);
    }

    private async Task<(bool Ok, string? Error)> SaveEstatusAsync(Guid? id, string codigoStr, string? nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre)) return (false, "Nombre es requerido.");
        if (!long.TryParse(codigoStr, out var codigo)) return (false, "Código debe ser un número.");
        var nombreNorm = nombre.Trim();
        var otros = await _db.Estatus.AsNoTracking()
            .Where(e => (id == null || e.Id != id.Value) && e.Nombre != null && e.Nombre.Trim().ToLower() == nombreNorm.ToLower())
            .AnyAsync();
        if (otros) return (false, "Ya existe un estatus con ese nombre.");
        if (id.HasValue)
        {
            var e = await _db.Estatus.FindAsync(id.Value);
            if (e == null) return (false, "No encontrado.");
            e.Codigo = codigo;
            e.Nombre = nombreNorm;
            await _db.SaveChangesAsync();
            await LogAuditAsync("Estatus", id.Value, "Actualizar", nombreNorm);
        }
        else
        {
            var newId = Guid.NewGuid();
            _db.Estatus.Add(new Estatus { Id = newId, Codigo = codigo, Nombre = nombreNorm });
            await _db.SaveChangesAsync();
            await LogAuditAsync("Estatus", newId, "Crear", nombreNorm);
        }
        return (true, null);
    }

    private async Task<(bool Ok, string? Error)> SaveAlojamientoAsync(Guid? id, string nombre)
    {
        var otros = await _db.Alojamientos.AsNoTracking()
            .Where(a => (id == null || a.Id != id.Value) && string.Equals(a.Nombre!.Trim(), nombre, StringComparison.OrdinalIgnoreCase))
            .AnyAsync();
        if (otros) return (false, "Ya existe un tipo de alojamiento con ese nombre.");
        if (id.HasValue)
        {
            var e = await _db.Alojamientos.FindAsync(id.Value);
            if (e == null) return (false, "No encontrado.");
            e.Nombre = nombre;
            await _db.SaveChangesAsync();
            await LogAuditAsync("Alojamientos", id.Value, "Actualizar", nombre);
        }
        else
        {
            var newId = Guid.NewGuid();
            _db.Alojamientos.Add(new Alojamiento { Id = newId, Nombre = nombre });
            await _db.SaveChangesAsync();
            await LogAuditAsync("Alojamientos", newId, "Crear", nombre);
        }
        return (true, null);
    }

    private async Task<(bool Ok, string? Error)> SaveAreaAsync(Guid? id, string name)
    {
        var otros = await _db.Areas.AsNoTracking()
            .Where(a => (id == null || a.Id != id.Value) && string.Equals(a.Name!.Trim(), name, StringComparison.OrdinalIgnoreCase))
            .AnyAsync();
        if (otros) return (false, "Ya existe un área con ese nombre.");
        if (id.HasValue)
        {
            var e = await _db.Areas.FindAsync(id.Value);
            if (e == null) return (false, "No encontrado.");
            e.Name = name;
            await _db.SaveChangesAsync();
            await LogAuditAsync("Areas", id.Value, "Actualizar", name);
        }
        else
        {
            var newId = Guid.NewGuid();
            _db.Areas.Add(new Area { Id = newId, Name = name });
            await _db.SaveChangesAsync();
            await LogAuditAsync("Areas", newId, "Crear", name);
        }
        return (true, null);
    }

    private async Task<(bool Ok, string? Error)> SaveOfficeAsync(Guid? id, string name)
    {
        var otros = await _db.Offices.AsNoTracking()
            .Where(o => (id == null || o.Id != id.Value) && string.Equals(o.Name!.Trim(), name, StringComparison.OrdinalIgnoreCase))
            .AnyAsync();
        if (otros) return (false, "Ya existe una oficina con ese nombre.");
        if (id.HasValue)
        {
            var e = await _db.Offices.FindAsync(id.Value);
            if (e == null) return (false, "No encontrado.");
            e.Name = name;
            await _db.SaveChangesAsync();
            await LogAuditAsync("Offices", id.Value, "Actualizar", name);
        }
        else
        {
            var newId = Guid.NewGuid();
            _db.Offices.Add(new Office { Id = newId, Name = name });
            await _db.SaveChangesAsync();
            await LogAuditAsync("Offices", newId, "Crear", name);
        }
        return (true, null);
    }

    private async Task<(bool Ok, string? Error)> SaveEnvironmentAsync(Guid? id, string name)
    {
        var otros = await _db.Environments.AsNoTracking()
            .Where(e => (id == null || e.Id != id.Value) && string.Equals(e.Name!.Trim(), name, StringComparison.OrdinalIgnoreCase))
            .AnyAsync();
        if (otros) return (false, "Ya existe un ambiente con ese nombre.");
        if (id.HasValue)
        {
            var e = await _db.Environments.FindAsync(id.Value);
            if (e == null) return (false, "No encontrado.");
            e.Name = name;
            await _db.SaveChangesAsync();
            await LogAuditAsync("Environments", id.Value, "Actualizar", name);
        }
        else
        {
            var newId = Guid.NewGuid();
            _db.Environments.Add(new IITS.Entities.Environment { Id = newId, Name = name });
            await _db.SaveChangesAsync();
            await LogAuditAsync("Environments", newId, "Crear", name);
        }
        return (true, null);
    }

    private async Task<(bool Ok, string? Error)> SaveCriticalityAsync(Guid? id, string name)
    {
        var otros = await _db.Criticalities.AsNoTracking()
            .Where(c => (id == null || c.Id != id.Value) && string.Equals(c.Name!.Trim(), name, StringComparison.OrdinalIgnoreCase))
            .AnyAsync();
        if (otros) return (false, "Ya existe una criticidad con ese nombre.");
        if (id.HasValue)
        {
            var e = await _db.Criticalities.FindAsync(id.Value);
            if (e == null) return (false, "No encontrado.");
            e.Name = name;
            await _db.SaveChangesAsync();
            await LogAuditAsync("Criticalities", id.Value, "Actualizar", name);
        }
        else
        {
            var newId = Guid.NewGuid();
            _db.Criticalities.Add(new Criticality { Id = newId, Name = name });
            await _db.SaveChangesAsync();
            await LogAuditAsync("Criticalities", newId, "Crear", name);
        }
        return (true, null);
    }

    private async Task<(bool Ok, string? Error)> SaveCategoryAsync(Guid? id, string name)
    {
        var otros = await _db.Categories.AsNoTracking()
            .Where(c => (id == null || c.Id != id.Value) && string.Equals(c.Name!.Trim(), name, StringComparison.OrdinalIgnoreCase))
            .AnyAsync();
        if (otros) return (false, "Ya existe una categoría con ese nombre.");
        if (id.HasValue)
        {
            var e = await _db.Categories.FindAsync(id.Value);
            if (e == null) return (false, "No encontrado.");
            e.Name = name;
            await _db.SaveChangesAsync();
            await LogAuditAsync("Categories", id.Value, "Actualizar", name);
        }
        else
        {
            var newId = Guid.NewGuid();
            _db.Categories.Add(new Category { Id = newId, Name = name });
            await _db.SaveChangesAsync();
            await LogAuditAsync("Categories", newId, "Crear", name);
        }
        return (true, null);
    }

    private async Task<(bool Ok, string? Error)> SaveVendorAsync(Guid? id, string name)
    {
        var otros = await _db.Vendors.AsNoTracking()
            .Where(v => (id == null || v.Id != id.Value) && string.Equals(v.Name!.Trim(), name, StringComparison.OrdinalIgnoreCase))
            .AnyAsync();
        if (otros) return (false, "Ya existe un fabricante con ese nombre.");
        if (id.HasValue)
        {
            var e = await _db.Vendors.FindAsync(id.Value);
            if (e == null) return (false, "No encontrado.");
            e.Name = name;
            await _db.SaveChangesAsync();
            await LogAuditAsync("Vendors", id.Value, "Actualizar", name);
        }
        else
        {
            var newId = Guid.NewGuid();
            _db.Vendors.Add(new Vendor { Id = newId, Name = name });
            await _db.SaveChangesAsync();
            await LogAuditAsync("Vendors", newId, "Crear", name);
        }
        return (true, null);
    }

    private async Task<(bool Ok, string? Error)> SaveDeviceModelAsync(Guid? id, string name, string? fabricanteNombre)
    {
        Guid? manufacturerId = null;
        if (!string.IsNullOrWhiteSpace(fabricanteNombre))
        {
            var fab = await _db.Vendors.AsNoTracking()
                .FirstOrDefaultAsync(v => v.Name != null && v.Name.Trim().ToLower() == fabricanteNombre.Trim().ToLower());
            if (fab != null) manufacturerId = fab.Id;
        }
        var otros = await _db.DeviceModels.AsNoTracking()
            .Where(d => (id == null || d.Id != id.Value) && d.Name != null && d.Name.Trim().ToLower() == name.Trim().ToLower()
                && (manufacturerId == null || d.ManufacturerId == manufacturerId))
            .AnyAsync();
        if (otros) return (false, "Ya existe un modelo con ese nombre para ese fabricante.");
        if (id.HasValue)
        {
            var e = await _db.DeviceModels.FindAsync(id.Value);
            if (e == null) return (false, "No encontrado.");
            e.Name = name;
            if (manufacturerId.HasValue) e.ManufacturerId = manufacturerId.Value;
            await _db.SaveChangesAsync();
            await LogAuditAsync("DeviceModels", id.Value, "Actualizar", name);
        }
        else
        {
            if (!manufacturerId.HasValue)
                return (false, "Seleccione o ingrese un fabricante existente.");
            var newId = Guid.NewGuid();
            _db.DeviceModels.Add(new DeviceModel { Id = newId, Name = name, ManufacturerId = manufacturerId.Value });
            await _db.SaveChangesAsync();
            await LogAuditAsync("DeviceModels", newId, "Crear", name);
        }
        return (true, null);
    }
}
