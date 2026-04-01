using IITS.Data;
using IITS.Entities;
using Microsoft.EntityFrameworkCore;

namespace IITS.Services;

public class AprobacionPermisoService : IAprobacionPermisoService
{
    private readonly AppDbContext _db;

    public static readonly IReadOnlyList<string> Modulos = new[]
    {
        "Aplicaciones", "Operaciones", "Cuentas"
    };

    public AprobacionPermisoService(AppDbContext db) => _db = db;

    public IReadOnlyList<string> GetModulosDisponibles() => Modulos;

    public async Task<List<AprobacionPermiso>> GetAllAsync() =>
        await _db.AprobacionPermisos.Include(p => p.User).OrderBy(p => p.User!.Nombre).ThenBy(p => p.Modulo).ToListAsync();

    public async Task<List<AprobacionPermiso>> GetByUserAsync(Guid userId) =>
        await _db.AprobacionPermisos.Where(p => p.UserId == userId).ToListAsync();

    public async Task<List<string>> GetModulosByUserAsync(Guid userId) =>
        await _db.AprobacionPermisos.Where(p => p.UserId == userId).Select(p => p.Modulo).ToListAsync();

    public async Task<bool> CanApproveAsync(Guid? userId, string modulo)
    {
        if (userId == null || string.IsNullOrWhiteSpace(modulo)) return false;
        return await _db.AprobacionPermisos.AnyAsync(p => p.UserId == userId.Value && p.Modulo == modulo);
    }

    public async Task<AprobacionPermiso?> GetAsync(Guid id) =>
        await _db.AprobacionPermisos.Include(p => p.User).FirstOrDefaultAsync(p => p.Id == id);

    public async Task<List<User>> GetApproversForModuloAsync(string modulo)
    {
        if (string.IsNullOrWhiteSpace(modulo)) return new List<User>();
        var userIds = await _db.AprobacionPermisos.Where(p => p.Modulo == modulo).Select(p => p.UserId).ToListAsync();
        if (userIds.Count == 0) return new List<User>();
        return await _db.Users.AsNoTracking().Where(u => userIds.Contains(u.Id)).ToListAsync();
    }

    public async Task<AprobacionPermiso> CreateAsync(Guid userId, string modulo)
    {
        if (!Modulos.Contains(modulo)) throw new ArgumentException("Módulo no válido.", nameof(modulo));
        var exists = await _db.AprobacionPermisos.AnyAsync(p => p.UserId == userId && p.Modulo == modulo);
        if (exists) throw new InvalidOperationException("Ese usuario ya tiene permiso para aprobar en ese módulo.");
        var e = new AprobacionPermiso { Id = Guid.NewGuid(), UserId = userId, Modulo = modulo };
        _db.AprobacionPermisos.Add(e);
        await _db.SaveChangesAsync();
        return e;
    }

    public async Task DeleteAsync(Guid id)
    {
        var e = await _db.AprobacionPermisos.FindAsync(id);
        if (e != null) { _db.AprobacionPermisos.Remove(e); await _db.SaveChangesAsync(); }
    }
}
