namespace IITS.Services;

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
