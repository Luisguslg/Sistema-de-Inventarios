using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Negotiate;

namespace IITS.Middleware;

/// <summary>
/// 1) Si no hay usuario (cookie expirada o primera visita), intenta autenticar con Negotiate (Windows).
/// 2) Si hay usuario por Negotiate o DevAuth pero no por Cookie, firma con Cookie para limitar la duración de la sesión (Auth:SessionTimeoutMinutes).
/// Guarda solo el username en la cookie (no todos los claims) para evitar HTTP 400 "Request Too Long" por headers excesivos.
/// IITSClaimsTransformation añade roles y permisos desde la BD en cada petición.
/// </summary>
public class SessionCookieSignInMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _config;

    public SessionCookieSignInMiddleware(RequestDelegate next, IConfiguration config)
    {
        _next = next;
        _config = config;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User?.Identity?.IsAuthenticated != true)
        {
            try
            {
                var result = await context.AuthenticateAsync(NegotiateDefaults.AuthenticationScheme);
                if (result.Succeeded && result.Principal != null)
                    context.User = result.Principal;
            }
            catch (InvalidOperationException)
            {
                // Negotiate no disponible (entorno de test); DevAuthMiddleware ya habrá inyectado el usuario.
            }
        }

        if (context.User?.Identity?.IsAuthenticated == true &&
            context.User.Identity.AuthenticationType != CookieAuthenticationDefaults.AuthenticationScheme)
        {
            var sessionTimeoutMinutes = _config.GetValue("Auth:SessionTimeoutMinutes", 30);
            if (sessionTimeoutMinutes < 5) sessionTimeoutMinutes = 5;
            if (sessionTimeoutMinutes > 480) sessionTimeoutMinutes = 480;

            var props = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(sessionTimeoutMinutes),
                AllowRefresh = true
            };

            var name = context.User.Identity?.Name;
            if (string.IsNullOrEmpty(name)) { await _next(context); return; }

            var minimalPrincipal = new ClaimsPrincipal(new ClaimsIdentity(
                new[] { new Claim(ClaimTypes.Name, name) },
                CookieAuthenticationDefaults.AuthenticationScheme,
                ClaimTypes.Name,
                ClaimTypes.Role));

            await context.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                minimalPrincipal,
                props);
        }

        await _next(context);
    }
}
