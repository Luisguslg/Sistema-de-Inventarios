using FluentAssertions;
using IITS.Tests.Infrastructure;
using System.Net;

namespace IITS.Tests.Security;

/// <summary>
/// Pruebas de integración: verifica que el rate limiting bloquea peticiones excesivas.
/// ISO-082-API. Límite configurado: 10 req/minuto por IP para endpoints de export y auditoría.
/// Cada test crea su propia fábrica para aislar el estado del rate limiter en memoria.
/// </summary>
public class RateLimitingTests
{
    [Fact]
    public async Task ExportEndpoint_Returns200_ForValidRequests_UnderLimit()
    {
        await using var factory = new WebAppFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var response = await client.GetAsync("/api/export/Roles/csv");

        ((int)response.StatusCode).Should().BeOneOf(new[] { 200, 204 },
            "el endpoint de exportación debe estar disponible bajo el límite");
    }

    [Fact]
    public async Task ExportEndpoint_Returns429_AfterExceedingLimit()
    {
        await using var factory = new WebAppFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var statusCodes = new List<int>();

        // Enviar 12 peticiones — se espera que en algún punto se active el 429
        for (int i = 0; i < 12; i++)
        {
            var resp = await client.GetAsync("/api/export/Roles/csv");
            statusCodes.Add((int)resp.StatusCode);
        }

        statusCodes.Should().Contain(429,
            "el rate limiter debe devolver 429 Too Many Requests al superar el límite (ISO-082-API)");

        var firstRateLimitedIndex = statusCodes.IndexOf(429);
        firstRateLimitedIndex.Should().BeGreaterThanOrEqualTo(1,
            "debe haber al menos una respuesta exitosa antes de que se active el rate limiting");

        statusCodes.Skip(firstRateLimitedIndex).Should().AllSatisfy(s => s.Should().Be(429),
            "una vez activado el rate limiting, todas las respuestas siguientes deben ser 429");
    }

    [Fact]
    public async Task AuditoriaPdfEndpoint_Returns400_WithInvalidModulo()
    {
        await using var factory = new WebAppFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/auditoria/pdf?modulo=../../../etc/passwd");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest,
            "módulos inválidos o intentos de path traversal deben rechazarse con 400 (CWE-23)");
    }

    [Fact]
    public async Task AuditoriaPdfEndpoint_Returns400_WithEmptyModulo()
    {
        await using var factory = new WebAppFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/auditoria/pdf?modulo=");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest,
            "el parámetro módulo vacío debe rechazarse con 400");
    }

    [Fact]
    public async Task ExportEndpoint_Returns400_WithInvalidFormat()
    {
        await using var factory = new WebAppFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/export/Aplicaciones/sh");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest,
            "formatos de exportación inválidos deben devolver 400");
    }
}
