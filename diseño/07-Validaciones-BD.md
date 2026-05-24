# Validaciones de Base de Datos

## Entity Framework Core

### OnModelCreating

Las relaciones y restricciones se definen en `AppDbContext.OnModelCreating`:

- Claves compuestas: `UserRole (UserId, RoleId)`, `RolePermission (RoleId, PermissionId)`
- Índices únicos: `AprobacionPermiso (UserId, Modulo)`, `AprobacionVoto (AprobacionId, UserId)`
- Comportamiento de borrado: Cascade, Restrict, NoAction según entidad

### Migraciones

Las migraciones definen tablas, columnas, FKs e índices. Ejecución: `dotnet ef database update --project IITS`.

---

## DataAnnotations

### Entidades

- `[Required]` en campos obligatorios
- `[MaxLength(n)]` en cadenas
- `[StringLength(n)]` cuando aplica

### Modelos de formulario

- `FormAplicacionModel`, `FormUsuarioModel`, `FormRolModel` usan DataAnnotations
- Validación en formularios Blazor con `DataAnnotationsValidator`

---

## Restricciones únicas

| Tabla | Columnas |
|-------|----------|
| AprobacionPermiso | UserId, Modulo |
| AprobacionVoto | AprobacionId, UserId |

---

## Relaciones y borrado

| Entidad | Comportamiento |
|---------|----------------|
| RolePermission | Cascade al borrar Role o Permission |
| AprobacionPermiso | Cascade al borrar User |
| AprobacionVoto | Cascade al borrar Aprobacion, NoAction al borrar User |
| ApprovalDecision | Cascade al borrar ApprovalRequest |
| ManagedAccountSecurityGroup | Cascade al borrar ManagedAccount |
| Asset, Operacion, etc. | Restrict o NoAction en FKs a catálogos |

---

## Catálogos de seed

Al iniciar, la aplicación ejecuta `EnsureEstatusAsync`, `EnsureRolesAsync`, `EnsurePermissionsAsync`, `EnsureAlojamientosAsync`, `EnsureAreasAsync`, `EnsureOfficesAsync`, etc., para garantizar datos mínimos si las tablas existen.
