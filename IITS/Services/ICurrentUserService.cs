namespace IITS.Services;

/// <summary>
/// Usuario actual y permisos. Por ahora sin autenticación real: se usa config o valor por defecto.
/// Cuando se integre AD/Negotiate, aquí se resuelve el usuario y sus roles.
/// </summary>
public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? UserName { get; }
    bool IsAdmin { get; }
    bool CanSeeDataMenu { get; }
    bool CanEdit(string modulo);
    bool CanSeeLogs { get; }
    bool HasPermission(string permissionCode);
    Task<bool> CanApproveModuloAsync(string modulo);
}
