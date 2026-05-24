using IITS.Data;
using IITS.Entities;
using Microsoft.EntityFrameworkCore;

namespace IITS.Services;

public class UsuarioService : IUsuarioService
{
    private readonly AppDbContext _db;
    private readonly IAuditLogService _audit;
    private readonly ICurrentUserService? _currentUser;

    public UsuarioService(AppDbContext db, IAuditLogService audit, ICurrentUserService? currentUser = null)
    {
        _db = db;
        _audit = audit;
        _currentUser = currentUser;
    }

    public async Task<List<User>> GetAllWithRolesAsync() =>
        await _db.Users.Include(u => u.UserRoles).ThenInclude(ur => ur.Role).AsNoTracking().OrderBy(u => u.Username).ToListAsync();

    public async Task<User?> GetByIdAsync(Guid id) =>
        await _db.Users.Include(u => u.UserRoles).FirstOrDefaultAsync(u => u.Id == id);

    public async Task<List<Role>> GetRolesAsync() =>
        await _db.Roles.AsNoTracking().OrderBy(r => r.Nombre).ToListAsync();

    public async Task<List<Guid>> GetUserRoleIdsAsync(Guid userId) =>
        await _db.UserRoles.Where(ur => ur.UserId == userId).Select(ur => ur.RoleId).ToListAsync();

    public async Task<User> CreateAsync(User user, IEnumerable<Guid> roleIds)
    {
        var userId = _currentUser?.UserId;
        user.Id = Guid.NewGuid();
        _db.Users.Add(user);
        foreach (var rid in roleIds.Distinct())
            _db.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = rid });
        await _db.SaveChangesAsync();
        await _audit.RegistrarAsync("Users", user.Id, "Crear", user.Username ?? "", userId);
        return user;
    }

    public async Task UpdateAsync(User user, IEnumerable<Guid> roleIds)
    {
        var userId = _currentUser?.UserId;
        _db.Users.Update(user);
        var current = await _db.UserRoles.Where(ur => ur.UserId == user.Id).ToListAsync();
        _db.UserRoles.RemoveRange(current);
        foreach (var rid in roleIds.Distinct())
            _db.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = rid });
        await _db.SaveChangesAsync();
        await _audit.RegistrarAsync("Users", user.Id, "Actualizar", user.Username ?? "", userId);
    }

    public async Task DeleteAsync(Guid id)
    {
        var u = await _db.Users.FindAsync(id);
        if (u != null)
        {
            var username = u.Username ?? "";
            var userId = _currentUser?.UserId;
            _db.UserRoles.RemoveRange(_db.UserRoles.Where(ur => ur.UserId == id));
            _db.Users.Remove(u);
            await _db.SaveChangesAsync();
            await _audit.RegistrarAsync("Users", id, "Eliminar", username, userId);
        }
    }
}
