using IITS.Data;
using IITS.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Server.IISIntegration;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace IITS.Services;

/// <summary>
/// Tras autenticación Windows (Negotiate), busca el usuario en la BD (Users/UserRoles/Roles)
/// y añade claims: UserId, nombre, email, y ClaimTypes.Role por cada rol.
/// Igual que en el IITS de ejemplo: identity.Name se compara con User.Username.
/// </summary>
public class IITSClaimsTransformation : IClaimsTransformation
{
    private readonly AppDbContext _db;

    public IITSClaimsTransformation(AppDbContext db) => _db = db;

    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        var identity = principal.Identity;
        if (identity?.IsAuthenticated != true) return principal;

        var name = identity.Name;
        if (string.IsNullOrEmpty(name)) return principal;

        // Buscar usuario: "DOMINIO\user", solo "user", o que termine en "\user"
        var usernameToFind = name.Contains('\\') ? name.Substring(name.IndexOf('\\') + 1) : name;
        // EF.Functions.Like traduce a SQL; EndsWith+OrdinalIgnoreCase no se traduce
        var suffix = "\\" + usernameToFind;
        var user = await _db.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .ThenInclude(r => r!.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .AsNoTracking()
            .AsSplitQuery()
            .OrderBy(u => u.Username)
            .FirstOrDefaultAsync(u =>
                u.Username == name ||
                u.Username == usernameToFind ||
                EF.Functions.Like(u.Username, "%" + suffix));

        if (user == null) return principal;

        var claims = new List<Claim>
        {
            new Claim("UserId", user.Id.ToString()),
            new Claim(ClaimTypes.Name, name),
            new Claim("Nombre", user.Nombre ?? ""),
            new Claim("Apellido", user.Apellido ?? ""),
            new Claim("Email", user.Email ?? ""),
            new Claim("CodSap", user.CodSap ?? "")
        };
        foreach (var ur in user.UserRoles)
        {
            if (ur.Role?.Nombre != null)
                claims.Add(new Claim(ClaimTypes.Role, ur.Role.Nombre));
            foreach (var rp in ur.Role?.RolePermissions ?? Array.Empty<RolePermission>())
                if (rp.Permission?.Code != null)
                    claims.Add(new Claim("Permission", rp.Permission.Code));
        }

        var newIdentity = new ClaimsIdentity(claims, IISDefaults.AuthenticationScheme);
        return new ClaimsPrincipal(newIdentity);
    }
}
