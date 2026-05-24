using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace IITS.Middleware;

/// <summary>
/// En Development con Auth:Mode=Dev, si no hay usuario autenticado (p. ej. sin dominio Windows),
/// inyecta un principal con identity.Name = Auth:DevUsername para que IITSClaimsTransformation cargue el User desde BD.
/// No se usa en producción.
/// </summary>
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
