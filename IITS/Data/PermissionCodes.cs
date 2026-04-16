namespace IITS.Data;

// [ISO-064-EAS] Catálogo de permisos del sistema. Cada permiso se asocia a uno o más roles
// en RolePermission. Program.cs registra una política de autorización por cada entrada.
public static class PermissionCodes
{
    public const string Admin = "Perm.Admin";
    public const string InventoryView = "Perm.Inventory.View";
    public const string InventoryCreate = "Perm.Inventory.Create";
    public const string InventoryEdit = "Perm.Inventory.Edit";
    public const string InventoryExport = "Perm.Inventory.Export";
    public const string InventoryAplicaciones = "Perm.Inventory.Aplicaciones";
    public const string InventoryOperaciones = "Perm.Inventory.Operaciones";
    public const string InventoryCuentas = "Perm.Inventory.Cuentas";
    public const string AuditView = "Perm.Audit.View";
    public const string AuditApprove = "Perm.Audit.Approve";
    public const string LogsView = "Perm.Logs.View";
    public const string LogsExport = "Perm.Logs.Export";
    public const string UsersManage = "Perm.Users.Manage";
    public const string RolesManage = "Perm.Roles.Manage";

    public static (string Code, string Description)[] All => new[]
    {
        (Admin, "Administración total"),
        (InventoryView, "Ver inventarios"),
        (InventoryCreate, "Crear en inventario"),
        (InventoryEdit, "Editar inventario"),
        (InventoryExport, "Exportar inventarios"),
        (InventoryAplicaciones, "Ver y operar módulo Aplicaciones"),
        (InventoryOperaciones, "Ver y operar módulo Tecnología"),
        (InventoryCuentas, "Ver y operar módulo Cuentas"),
        (AuditView, "Ver auditoría"),
        (AuditApprove, "Aprobar solicitudes"),
        (LogsView, "Ver logs"),
        (LogsExport, "Exportar logs"),
        (UsersManage, "Gestionar usuarios"),
        (RolesManage, "Gestionar roles y permisos")
    };
}
