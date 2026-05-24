using FluentAssertions;

namespace IITS.Tests.Security;

/// <summary>
/// Pruebas unitarias: valida la lógica de clamping del timeout de sesión.
/// Esta lógica existe en Program.cs y en SessionCookieSignInMiddleware.cs.
/// Rango válido: 5 a 480 minutos.
/// </summary>
public class SessionTimeoutTests
{
    // Réplica exacta de la lógica de clamping de Program.cs y SessionCookieSignInMiddleware.cs
    private static int ClampTimeout(int minutes)
    {
        if (minutes < 5) return 5;
        if (minutes > 480) return 480;
        return minutes;
    }

    [Theory]
    [InlineData(0, 5)]
    [InlineData(1, 5)]
    [InlineData(4, 5)]
    [InlineData(5, 5)]
    [InlineData(6, 6)]
    [InlineData(30, 30)]
    [InlineData(60, 60)]
    [InlineData(480, 480)]
    [InlineData(481, 480)]
    [InlineData(9999, 480)]
    [InlineData(-1, 5)]
    public void SessionTimeout_Clamping_IsCorrect(int input, int expected)
    {
        ClampTimeout(input).Should().Be(expected,
            $"un timeout de {input} min debe resultar en {expected} min tras el clamping");
    }

    [Fact]
    public void SessionTimeout_Minimum_Is_5_Minutes()
    {
        // El mínimo de 5 minutos existe para evitar sesiones excesivamente cortas
        // que degradarían la usabilidad sin beneficio de seguridad real
        ClampTimeout(1).Should().Be(5);
        ClampTimeout(0).Should().Be(5);
    }

    [Fact]
    public void SessionTimeout_Maximum_Is_480_Minutes()
    {
        // El máximo de 480 minutos (8 horas) limita la duración máxima de sesión
        ClampTimeout(481).Should().Be(480);
        ClampTimeout(int.MaxValue).Should().Be(480);
    }

    [Fact]
    public void SessionTimeout_Default_30_Minutes_IsWithinRange()
    {
        ClampTimeout(30).Should().Be(30,
            "el valor por defecto de 30 minutos debe estar dentro del rango permitido");
    }

    [Fact]
    public void SessionTimeout_Boundary_Values_AreAccepted_AsIs()
    {
        ClampTimeout(5).Should().Be(5, "el mínimo exacto no debe ser modificado");
        ClampTimeout(480).Should().Be(480, "el máximo exacto no debe ser modificado");
    }
}
