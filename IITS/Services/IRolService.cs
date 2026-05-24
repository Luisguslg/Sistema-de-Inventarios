using IITS.Entities;

namespace IITS.Services;

public interface IRolService
{
    Task<List<Role>> GetAllAsync();
    Task<Role?> GetByIdAsync(Guid id);
    Task<Role> CreateAsync(string nombre);
    Task UpdateAsync(Role role);
    Task DeleteAsync(Guid id);
    Task<List<Permission>> GetAllPermissionsAsync();
    Task<HashSet<Guid>> GetRolePermissionIdsAsync(Guid roleId);
    Task SetRolePermissionsAsync(Guid roleId, IEnumerable<Guid> permissionIds);
}
