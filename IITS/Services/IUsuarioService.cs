using IITS.Entities;

namespace IITS.Services;

public interface IUsuarioService
{
    Task<List<User>> GetAllWithRolesAsync();
    Task<User?> GetByIdAsync(Guid id);
    Task<List<Role>> GetRolesAsync();
    Task<List<Guid>> GetUserRoleIdsAsync(Guid userId);
    Task<User> CreateAsync(User user, IEnumerable<Guid> roleIds);
    Task UpdateAsync(User user, IEnumerable<Guid> roleIds);
    Task DeleteAsync(Guid id);
}
