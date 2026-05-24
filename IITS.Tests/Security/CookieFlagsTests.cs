using FluentAssertions;
using IITS.Tests.Infrastructure;

namespace IITS.Tests.Security;

/// <summary>
/// Pruebas de integración: verifica los atributos de seguridad de la cookie de sesión.
/// CWE-614 (falta Secure/SameSite), CWE-1004 (falta HttpOnly).
/// </summary>
public class CookieFlagsTests : IClassFixture<WebAppFactory>
{
    private readonly HttpClient _client;

    public CookieFlagsTests(WebAppFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = false  // necesario para inspeccionar el header Set-Cookie crudo
        });
    }

    private async Task<string?> GetSessionCookieHeaderAsync()
    {
        var response = await _client.GetAsync("/");
        if (!response.Headers.TryGetValues("Set-Cookie", out var cookies))
            return null;
        return cookies.FirstOrDefault(c => c.Contains(".IITS.Session"));
    }

    [Fact]
    public async Task SessionCookie_Name_Is_IITS_Session()
    {
        var cookie = await GetSessionCookieHeaderAsync();

        cookie.Should().NotBeNull("la cookie de sesión debe emitirse en la primera petición");
        cookie!.Should().StartWith(".IITS.Session=");
    }

    [Fact]
    public async Task SessionCookie_HasHttpOnly_Flag()
    {
        var cookie = await GetSessionCookieHeaderAsync();

        cookie.Should().NotBeNull();
        cookie!.ToLowerInvariant().Should().Contain("httponly",
            "HttpOnly previene acceso desde JavaScript, mitiga XSS (CWE-1004)");
    }

    [Fact]
    public async Task SessionCookie_HasSameSite_Strict()
    {
        var cookie = await GetSessionCookieHeaderAsync();

        cookie.Should().NotBeNull();
        cookie!.ToLowerInvariant().Should().Contain("samesite=strict",
            "SameSite=Strict bloquea envío en peticiones cross-site, mitiga CSRF (CWE-614)");
    }

    [Fact]
    public async Task SessionCookie_HasExpiry_Set()
    {
        var cookie = await GetSessionCookieHeaderAsync();

        cookie.Should().NotBeNull();
        cookie!.ToLowerInvariant().Should().Contain("expires=",
            "la cookie debe tener expiración para limitar la duración de la sesión");
    }

    [Fact]
    public async Task SessionCookie_DoesNotUseSessionOnly_Expiry()
    {
        // Una cookie sin expires es "session cookie" y no tiene límite de tiempo.
        // La remediación obliga a emitirla con expires (IsPersistent = true).
        var cookie = await GetSessionCookieHeaderAsync();

        cookie.Should().NotBeNull();
        // Si tiene expires, no es session-only
        cookie!.ToLowerInvariant().Should().Contain("expires=");
    }
}
