using System.Security.Claims;
using IITS.Data;
using IITS.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace IITS.Tests.Infrastructure;

/// <summary>
/// Fábrica compartida para tests de integración.
/// Reemplaza SQL Server por InMemory y autentica mediante DevAuth (Auth:Mode=Dev).
/// </summary>
public class WebAppFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // "Development" activa CookieSecurePolicy.SameAsRequest y omite HTTPS redirect,
        // lo que permite que el TestServer emita y reciba cookies sobre HTTP.
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Auth:Mode"] = "Dev",
                ["Auth:DevUsername"] = "testuser",
                ["Auth:SuperAdminUsername"] = "testuser",
                ["Auth:SessionTimeoutMinutes"] = "30",
                ["ConnectionStrings:IITS"] = "InMemory",
                ["Email:Mode"] = "Dev"
            });
        });

        builder.ConfigureServices(services =>
        {
            // Reemplazar DbContext con InMemory (misma versión que IITS usa EF 8)
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.AddDbContext<AppDbContext>(opts =>
                opts.UseInMemoryDatabase("IITSTests_" + Guid.NewGuid()));

            // Reemplazar IClaimsTransformation por una que no use EF.Functions.Like (no soportado en InMemory)
            services.RemoveAll<IClaimsTransformation>();
            services.AddTransient<IClaimsTransformation, TestClaimsTransformation>();
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);

        // TestServer no soporta IConnectionItemsFeature (requerida por NegotiateHandler).
        // Removemos el esquema Negotiate del proveedor para que AuthenticationMiddleware no lo invoque.
        var schemeProvider = host.Services.GetRequiredService<IAuthenticationSchemeProvider>();
        if (schemeProvider is AuthenticationSchemeProvider concreteProvider)
            concreteProvider.RemoveScheme(NegotiateDefaults.AuthenticationScheme);

        // Seed del usuario de test DESPUÉS de que el host esté construido
        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.EnsureCreated();
        if (!db.Users.Any(u => u.Username == "testuser"))
        {
            db.Users.Add(new User
            {
                Id = Guid.NewGuid(),
                Username = "testuser",
                Nombre = "Test",
                Apellido = "User",
                Email = "test@test.local"
            });
            db.SaveChanges();
        }

        return host;
    }
}

/// <summary>
/// Reemplaza IITSClaimsTransformation en tests.
/// Devuelve un principal con permisos completos para que los endpoints no rechacen.
/// </summary>
internal class TestClaimsTransformation : IClaimsTransformation
{
    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (principal.Identity?.IsAuthenticated != true)
            return Task.FromResult(principal);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, principal.Identity.Name ?? "testuser"),
            new Claim("UserId", Guid.Empty.ToString()),
            new Claim(ClaimTypes.Role, "SuperAdmin"),
        };
        var identity = new ClaimsIdentity(claims, "DevAuth", ClaimTypes.Name, ClaimTypes.Role);
        return Task.FromResult(new ClaimsPrincipal(identity));
    }
}
