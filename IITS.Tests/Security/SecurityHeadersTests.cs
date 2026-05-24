using FluentAssertions;
using IITS.Tests.Infrastructure;

namespace IITS.Tests.Security;

/// <summary>
/// Pruebas de integración: verifica que todas las cabeceras de seguridad HTTP están presentes.
/// CWE-16, CWE-693, CWE-200.
/// </summary>
public class SecurityHeadersTests : IClassFixture<WebAppFactory>
{
    private readonly HttpClient _client;

    public SecurityHeadersTests(WebAppFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    [Fact]
    public async Task Response_DoesNotInclude_ServerHeader()
    {
        var response = await _client.GetAsync("/");

        response.Headers.Contains("Server").Should().BeFalse(
            "el header Server revela la tecnología del servidor (CWE-200)");
    }

    [Fact]
    public async Task Response_Includes_XContentTypeOptions_Nosniff()
    {
        var response = await _client.GetAsync("/");

        response.Headers.TryGetValues("X-Content-Type-Options", out var values).Should().BeTrue();
        values!.Should().ContainSingle(v => v == "nosniff",
            "previene MIME-type sniffing (CWE-693)");
    }

    [Fact]
    public async Task Response_Includes_XFrameOptions_SAMEORIGIN()
    {
        var response = await _client.GetAsync("/");

        response.Headers.TryGetValues("X-Frame-Options", out var values).Should().BeTrue();
        values!.Should().ContainSingle(v => v == "SAMEORIGIN",
            "bloquea framing externo, mitiga clickjacking (CWE-1021)");
    }

    [Fact]
    public async Task Response_Includes_ReferrerPolicy()
    {
        var response = await _client.GetAsync("/");

        response.Headers.TryGetValues("Referrer-Policy", out var values).Should().BeTrue();
        values!.First().Should().Be("strict-origin-when-cross-origin");
    }

    [Fact]
    public async Task Response_Includes_PermissionsPolicy_DisablingHardwareAPIs()
    {
        var response = await _client.GetAsync("/");

        // Permissions-Policy usa sintaxis con '()' que algunos parsers de HttpClient no validan;
        // usamos NonValidated para acceder al valor crudo sin restricciones de parser.
        response.Headers.NonValidated.Contains("Permissions-Policy").Should().BeTrue(
            "el header Permissions-Policy debe estar presente (CWE-16)");
        var policy = string.Join(", ", response.Headers.NonValidated["Permissions-Policy"]);
        policy.Should().Contain("geolocation=()");
        policy.Should().Contain("microphone=()");
        policy.Should().Contain("camera=()");
    }

    [Fact]
    public async Task Response_Includes_ContentSecurityPolicy()
    {
        var response = await _client.GetAsync("/");

        response.Headers.TryGetValues("Content-Security-Policy", out var values).Should().BeTrue();
        var csp = values!.First();
        csp.Should().Contain("default-src 'self'");
    }

    [Fact]
    public async Task CSP_FrameAncestors_IsRestricted_NotWildcard()
    {
        var response = await _client.GetAsync("/");

        response.Headers.TryGetValues("Content-Security-Policy", out var values).Should().BeTrue();
        var csp = values!.First();
        csp.Should().Contain("frame-ancestors 'self'",
            "debe restringir quién puede embedder la app, no usar wildcard (CWE-1021)");
        csp.Should().NotContain("frame-ancestors *");
    }

    [Fact]
    public async Task SecurityHeaders_ArePresent_OnAllResponses_Including404()
    {
        var response = await _client.GetAsync("/ruta-que-no-existe-1234");

        response.Headers.TryGetValues("X-Content-Type-Options", out _).Should().BeTrue(
            "las cabeceras de seguridad deben estar en toda respuesta, incluyendo 404");
    }
}
