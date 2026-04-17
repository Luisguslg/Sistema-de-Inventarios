namespace IITS.Services;

public interface IExportService
{
    Task<byte[]> ExportToExcelAsync(string nombreModulo, IReadOnlyList<IReadOnlyDictionary<string, object?>> datos);
    Task<byte[]> ExportToPdfAsync(string nombreModulo, IReadOnlyList<IReadOnlyDictionary<string, object?>> datos);
    Task<byte[]> ExportToCsvAsync(string nombreModulo, IReadOnlyList<IReadOnlyDictionary<string, object?>> datos);
}
