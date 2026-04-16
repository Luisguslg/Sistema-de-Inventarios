using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace IITS.Middleware;

// [ISO-057-ESC] Bypass de autenticación Windows exclusivo para entorno Development.
// Requiere Auth:Mode=Dev en configuración — en producción (Auth:Mode=Windows) este
// middleware no actúa sobre ninguna petición.
public class DevAuthMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _config;

    public DevAuthMiddleware(RequestDelegate next, IConfiguration config)
    {
        _next = next;
        _config = config;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var authMode = _config["Auth:Mode"];
        if (string.Equals(authMode, "Dev", StringComparison.OrdinalIgnoreCase) && context.User?.Identity?.IsAuthenticated != true)
        {
            var devUsername = _config["Auth:DevUsername"]?.Trim() ?? "dev.local";
            var identity = new ClaimsIdentity("DevAuth");
            identity.AddClaim(new Claim(ClaimTypes.Name, devUsername));
            context.User = new ClaimsPrincipal(identity);
        }

        await _next(context);
    }
}
