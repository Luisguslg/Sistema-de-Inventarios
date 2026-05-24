namespace IITS.Services;

/// <summary>
/// Exportación por módulo: Excel, PDF, CSV. Nombre de archivo: {Modulo}_{yyyy-MM-dd}.xlsx|.pdf|.csv
/// </summary>
public interface IExportService
{
    /// <summary>Genera Excel para el módulo indicado con los datos en formato de diccionarios (columnas → valores).</summary>
    Task<byte[]> ExportToExcelAsync(string nombreModulo, IReadOnlyList<IReadOnlyDictionary<string, object?>> datos);
    /// <summary>Genera PDF para el módulo con los mismos datos.</summary>
    Task<byte[]> ExportToPdfAsync(string nombreModulo, IReadOnlyList<IReadOnlyDictionary<string, object?>> datos);
    /// <summary>Genera CSV con separador coma y UTF-8.</summary>
    Task<byte[]> ExportToCsvAsync(string nombreModulo, IReadOnlyList<IReadOnlyDictionary<string, object?>> datos);
}
