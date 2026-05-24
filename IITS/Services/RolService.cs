using IITS.Data;
using IITS.Entities;
using Microsoft.EntityFrameworkCore;

namespace IITS.Services;

public class RolService : IRolService
{
    private readonly AppDbContext _db;

    public RolService(AppDbContext db) => _db = db;

    public async Task<List<Role>> GetAllAsync() =>
        await _db.Roles.AsNoTracking().OrderBy(r => r.Nombre).ToListAsync();

    public async Task<Role?> GetByIdAsync(Guid id) => await _db.Roles.FindAsync(id);

    public async Task<Role> CreateAsync(string nombre)
    {
        var r = new Role { Id = Guid.NewGuid(), Nombre = nombre.Trim() };
        _db.Roles.Add(r);
        await _db.SaveChangesAsync();
        return r;
    }

    public async Task UpdateAsync(Role role)
    {
        _db.Roles.Update(role);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var r = await _db.Roles.FindAsync(id);
        if (r == null) return;
        var userRoles = await _db.UserRoles.Where(ur => ur.RoleId == id).ToListAsync();
        _db.UserRoles.RemoveRange(userRoles);
        if (await _db.TableExistsAsync("RolePermission"))
        {
            var rps = await _db.RolePermissions.Where(rp => rp.RoleId == id).ToListAsync();
            _db.RolePermissions.RemoveRange(rps);
        }
        _db.Roles.Remove(r);
        await _db.SaveChangesAsync();
    }

    public async Task<List<Permission>> GetAllPermissionsAsync()
    {
        if (!await _db.TableExistsAsync("Permissions")) return new List<Permission>();
        return await _db.Permissions.AsNoTracking().OrderBy(p => p.Code).ToListAsync();
    }

    public async Task<HashSet<Guid>> GetRolePermissionIdsAsync(Guid roleId)
    {
        if (!await _db.TableExistsAsync("RolePermission")) return new HashSet<Guid>();
        return (await _db.RolePermissions.AsNoTracking().Where(rp => rp.RoleId == roleId).Select(rp => rp.PermissionId).ToListAsync()).ToHashSet();
    }

    public async Task SetRolePermissionsAsync(Guid roleId, IEnumerable<Guid> permissionIds)
    {
        if (!await _db.TableExistsAsync("RolePermission")) return;
        var existing = await _db.RolePermissions.Where(rp => rp.RoleId == roleId).ToListAsync();
        _db.RolePermissions.RemoveRange(existing);
        var ids = permissionIds.ToHashSet();
        foreach (var pid in ids)
            _db.RolePermissions.Add(new RolePermission { RoleId = roleId, PermissionId = pid });
        await _db.SaveChangesAsync();
    }
}
