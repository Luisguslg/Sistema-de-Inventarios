using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace IITS.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IConfiguration _config;
    private readonly IHttpContextAccessor? _httpContextAccessor;
    private readonly IAprobacionPermisoService? _aprobacionPermisoService;

    public CurrentUserService(IConfiguration config, IHttpContextAccessor? httpContextAccessor = null, IAprobacionPermisoService? aprobacionPermisoService = null)
    {
        _config = config;
        _httpContextAccessor = httpContextAccessor;
        _aprobacionPermisoService = aprobacionPermisoService;
    }

    private ClaimsPrincipal? User => _httpContextAccessor?.HttpContext?.User;
    private bool IsAuthenticated => User?.Identity?.IsAuthenticated == true;

    public Guid? UserId
    {
        get
        {
            if (!IsAuthenticated) return null;
            var sid = User!.FindFirst("UserId")?.Value;
            return Guid.TryParse(sid, out var id) ? id : null;
        }
    }

    public string? UserName => User?.Identity?.Name ?? (IsAuthenticated ? null : "Usuario");

    private bool HasRole(string roleName) =>
        IsAuthenticated && User!.IsInRole(roleName);

    public bool IsAdmin =>
        HasRole("SuperAdmin") || HasRole("Administrador") || _config.GetValue<bool>("App:IsAdminByDefault") || CanSeeDataMenu;

    public bool CanSeeDataMenu =>
        HasRole("SuperAdmin") || HasRole("Administrador") || _config.GetValue<bool>("App:ShowDataMenu");

    public bool CanSeeLogs =>
        HasRole("SuperAdmin") || HasRole("Administrador") || HasRole("Auditor") || _config.GetValue<bool>("App:ShowLogsMenu");

    public bool CanEdit(string modulo) =>
        HasRole("SuperAdmin") || HasRole("Administrador") || HasRole("Operador") || _config.GetValue<bool>("App:AllowEditWithoutAuth");

    public bool HasPermission(string permissionCode) =>
        IsAuthenticated && (User!.HasClaim("Permission", permissionCode) || User!.HasClaim("Permission", IITS.Data.PermissionCodes.Admin));

    public async Task<bool> CanApproveModuloAsync(string modulo)
    {
        if (IsAdmin) return true;
        if (UserId == null || _aprobacionPermisoService == null) return false;
        return await _aprobacionPermisoService.CanApproveAsync(UserId, modulo);
    }
}
