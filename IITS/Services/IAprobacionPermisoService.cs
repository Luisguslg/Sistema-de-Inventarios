using IITS.Entities;

namespace IITS.Services;

public interface IAprobacionPermisoService
{
    Task<List<AprobacionPermiso>> GetAllAsync();
    Task<List<AprobacionPermiso>> GetByUserAsync(Guid userId);
    Task<List<string>> GetModulosByUserAsync(Guid userId);
    Task<bool> CanApproveAsync(Guid? userId, string modulo);
    Task<AprobacionPermiso?> GetAsync(Guid id);
    Task<List<User>> GetApproversForModuloAsync(string modulo);
    Task<AprobacionPermiso> CreateAsync(Guid userId, string modulo);
    Task DeleteAsync(Guid id);
    IReadOnlyList<string> GetModulosDisponibles();
}
