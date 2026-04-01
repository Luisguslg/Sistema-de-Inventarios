namespace IITS.Services;

/// <summary>Fila de un catálogo maestro para la vista unificada.</summary>
public class MasterDataRow
{
    public Guid Id { get; set; }
    /// <summary>Primera columna (Código, Nombre, etc.).</summary>
    public string Col1 { get; set; } = "";
    /// <summary>Segunda columna opcional (ej. Nombre cuando Col1 es Código).</summary>
    public string? Col2 { get; set; }
    /// <summary>Total de referencias en otras tablas (inventarios).</summary>
    public int VecesUsado { get; set; }
}

/// <summary>Catálogo disponible en Maestro de datos.</summary>
public class MasterCatalogInfo
{
    public string Key { get; set; } = "";
    public string DisplayName { get; set; } = "";
    /// <summary>Si true, la tabla tiene dos columnas editables (ej. Código + Nombre).</summary>
    public bool HasSecondaryColumn { get; set; }
    public string? Col1Label { get; set; }
    public string? Col2Label { get; set; }
}

public interface IMasterDataService
{
    /// <summary>Catálogos disponibles (tablas que existen en BD).</summary>
    Task<List<MasterCatalogInfo>> GetAvailableCatalogsAsync();
    /// <summary>Filas del catálogo con conteo de uso.</summary>
    Task<List<MasterDataRow>> GetRowsAsync(string catalogKey);
    /// <summary>Intenta eliminar; devuelve error si hay referencias.</summary>
    Task<(bool Ok, string? Error)> TryDeleteAsync(string catalogKey, Guid id);
    /// <summary>Crea o actualiza. Col2 puede ser null si el catálogo no lo usa.</summary>
    Task<(bool Ok, string? Error)> SaveAsync(string catalogKey, Guid? id, string col1, string? col2);
}
