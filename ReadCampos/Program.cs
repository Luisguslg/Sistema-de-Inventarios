using ClosedXML.Excel;

var baseDir = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", ".."));
var path = args.Length > 0 ? args[0] : Path.Combine(baseDir, "Ejemplo", "Campos.xlsx");
if (!File.Exists(path))
    path = Path.Combine(baseDir, "Campos.xlsx");
if (!File.Exists(path))
    path = Path.Combine(Environment.CurrentDirectory, "Ejemplo", "Campos.xlsx");
if (!File.Exists(path))
{
    Console.WriteLine("No se encontró Campos.xlsx. Buscado: " + path);
    return 1;
}
using var book = new XLWorkbook(path);
var outPath = Path.Combine(Path.GetDirectoryName(path) ?? ".", "Campos_export.txt");
using var sw = new StreamWriter(outPath, false, System.Text.Encoding.UTF8);
foreach (var sheet in book.Worksheets)
{
    sw.WriteLine("=== Hoja: " + sheet.Name + " ===");
    var used = sheet.RangeUsed();
    if (used == null) { sw.WriteLine("(vacía)"); sw.WriteLine(); continue; }
    for (int r = 1; r <= Math.Min(used.LastRow().RowNumber(), 50); r++)
    {
        var row = new List<string>();
        for (int c = 1; c <= used.LastColumn().ColumnNumber(); c++)
            row.Add(sheet.Cell(r, c).GetString().Trim());
        sw.WriteLine(string.Join(" | ", row));
    }
    sw.WriteLine();
}
Console.WriteLine("Escrito: " + outPath);
return 0;
