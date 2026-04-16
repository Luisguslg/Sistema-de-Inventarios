using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Negotiate;

namespace IITS.Middleware;

// [ISO-038-TER] [ISO-057-ESC]
// Establece sesión cookie tras autenticación Windows. Guarda únicamente el username
// en la cookie para evitar headers excesivos; roles y permisos se cargan desde BD en
// cada petición via IITSClaimsTransformation.
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
            var result = await context.AuthenticateAsync(NegotiateDefaults.AuthenticationScheme);
            if (result.Succeeded && result.Principal != null)
                context.User = result.Principal;
        }

        if (context.User?.Identity?.IsAuthenticated == true &&
            context.User.Identity.AuthenticationType != CookieAuthenticationDefaults.AuthenticationScheme)
        {
            // [ISO-038-TER] Límites de timeout: mínimo 5 min, máximo 8 horas.
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
