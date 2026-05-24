using FluentAssertions;

namespace IITS.Tests.Security;

/// <summary>
/// Pruebas unitarias: valida la lógica de canonicalización y validación de rutas.
/// CWE-23 (Path Traversal).
/// La lógica corresponde al bloque de Program.cs que valida el directorio Logs.
/// </summary>
public class PathCanonicalizationTests
{
    // Réplica exacta de la lógica de Program.cs para testearla de forma unitaria
    private static bool IsPathOutsideRoot(string contentRoot, string logsDir)
    {
        return !logsDir.StartsWith(contentRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
            && !logsDir.Equals(contentRoot, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("C:/app", "C:/app/Logs")]
    [InlineData("C:/app", "C:/app/Logs/sub")]
    [InlineData("C:/app/project", "C:/app/project/Logs")]
    public void LogsDir_Within_ContentRoot_IsValid(string root, string logsPath)
    {
        var canonicalized = Path.GetFullPath(logsPath);
        var canonicalRoot = Path.GetFullPath(root);

        IsPathOutsideRoot(canonicalRoot, canonicalized).Should().BeFalse(
            "un directorio subordinado al ContentRoot es una ruta válida");
    }

    [Theory]
    [InlineData("C:/app", "C:/other/Logs")]
    [InlineData("C:/app", "C:/Logs")]
    public void LogsDir_Outside_ContentRoot_IsInvalid(string root, string logsPath)
    {
        var canonicalized = Path.GetFullPath(logsPath);
        var canonicalRoot = Path.GetFullPath(root);

        IsPathOutsideRoot(canonicalRoot, canonicalized).Should().BeTrue(
            "una ruta fuera del ContentRoot debe ser detectada como inválida (CWE-23)");
    }

    [Fact]
    public void GetFullPath_Canonicalizes_DotDot_Traversal()
    {
        // Simula un input manipulado con traversal
        var baseDir = Path.GetFullPath("C:/app");
        var traversal = Path.Combine(baseDir, "../etc/passwd");
        var canonicalized = Path.GetFullPath(traversal);

        // Después de canonicalizar, la ruta debe estar fuera de C:/app
        IsPathOutsideRoot(baseDir, canonicalized).Should().BeTrue(
            "Path.GetFullPath() debe resolver los segmentos .. antes de la validación");
    }

    [Fact]
    public void GetFullPath_Canonicalizes_DoubleDot_In_Middle()
    {
        var baseDir = Path.GetFullPath("C:/app");
        var traversal = Path.Combine(baseDir, "Logs", "..", "..", "sensitive");
        var canonicalized = Path.GetFullPath(traversal);

        IsPathOutsideRoot(baseDir, canonicalized).Should().BeTrue(
            "los segmentos .. en medio del path también deben ser detectados");
    }

    [Fact]
    public void LogsPath_IsSubordinateTo_ContentRoot_After_Canonicalization()
    {
        // AppContext.BaseDirectory termina con separador en .NET; GetFullPath lo preserva → hay que quitarlo.
        var contentRoot = Path.GetFullPath(AppContext.BaseDirectory)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var logsDir = Path.GetFullPath(Path.Combine(contentRoot, "Logs"));

        IsPathOutsideRoot(contentRoot, logsDir).Should().BeFalse(
            "el directorio Logs/ dentro del ContentRoot es siempre válido");
    }
}
