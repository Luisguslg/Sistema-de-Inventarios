using System.Text;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace IITS.Services;

public class ExportService : IExportService
{
    public async Task<byte[]> ExportToExcelAsync(string nombreModulo, IReadOnlyList<IReadOnlyDictionary<string, object?>> datos)
    {
        await Task.CompletedTask;
        using var book = new XLWorkbook();
        var sheet = book.Worksheets.Add(nombreModulo);
        var columnas = datos.Count > 0 ? datos[0].Keys.ToList() : new List<string>();
        for (int c = 0; c < columnas.Count; c++)
            sheet.Cell(1, c + 1).Value = columnas[c];
        for (int r = 0; r < datos.Count; r++)
        {
            for (int c = 0; c < columnas.Count; c++)
            {
                var val = datos[r].GetValueOrDefault(columnas[c]);
                sheet.Cell(r + 2, c + 1).Value = val?.ToString() ?? "";
            }
        }
        sheet.Columns().AdjustToContents();
        using var ms = new MemoryStream();
        book.SaveAs(ms);
        return ms.ToArray();
    }

    public async Task<byte[]> ExportToPdfAsync(string nombreModulo, IReadOnlyList<IReadOnlyDictionary<string, object?>> datos)
    {
        await Task.CompletedTask;
        var columnas = datos.Count > 0 ? datos[0].Keys.ToList() : new List<string>();
        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(20);
                page.Header().Text(nombreModulo + " - " + DateTime.Now.ToString("yyyy-MM-dd")).Bold().FontSize(14);
                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(def =>
                    {
                        foreach (var _ in columnas)
                            def.RelativeColumn();
                    });
                    table.Header(header =>
                    {
                        foreach (var col in columnas)
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text(col).Bold();
                    });
                    foreach (var row in datos)
                    {
                        foreach (var col in columnas)
                            table.Cell().Padding(4).Text(row.GetValueOrDefault(col)?.ToString() ?? "").FontSize(8);
                    }
                });
            });
        });
        return doc.GeneratePdf();
    }

    public async Task<byte[]> ExportToCsvAsync(string nombreModulo, IReadOnlyList<IReadOnlyDictionary<string, object?>> datos)
    {
        await Task.CompletedTask;
        var columnas = datos.Count > 0 ? datos[0].Keys.ToList() : new List<string>();
        var sb = new StringBuilder();
        sb.AppendLine(string.Join(",", columnas.Select(c => EscaparCsv(c))));
        foreach (var row in datos)
            sb.AppendLine(string.Join(",", columnas.Select(c => EscaparCsv(row.GetValueOrDefault(c)?.ToString() ?? ""))));
        return Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
    }

    private static string EscaparCsv(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        return value;
    }
}
