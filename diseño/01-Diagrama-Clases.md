# Diagrama de Clases

## Vista general

El modelo de dominio del sistema IITS se organiza en entidades de inventario, seguridad, aprobación y auditoría.

![Diagrama de clases](imagenes/diagrama-clases.png)

---

## Entidades principales

### Seguridad

| Clase | Descripción |
|-------|-------------|
| User | Usuario (Username, Nombre, Apellido, Email, CodSap) |
| Role | Rol (SuperAdmin, Administrador, Operador, Auditor, Aprobador) |
| Permission | Permiso por código (Perm.Inventory.Aplicaciones, etc.) |
| UserRole | Relación N:M Usuario–Rol |
| RolePermission | Relación N:M Rol–Permiso |
| AprobacionPermiso | Usuario autorizado para aprobar por módulo |

### Catálogos

| Clase | Descripción |
|-------|-------------|
| Estatus | Activo (1000), Inactivo (1500), Desincorporado (2000) |
| Alojamiento | Tipo de alojamiento (Data Center, Nube, etc.) |
| Area | Área responsable |
| Office | Oficina / ubicación |
| Environment | Ambiente |
| Criticality | Criticidad |
| Category | Categoría de activos |
| Vendor | Fabricante / proveedor |
| DeviceModel | Modelo (FK a Vendor) |
| CatalogItem | Catálogo genérico (Kind, Name) |

### Inventario

| Clase | Descripción |
|-------|-------------|
| Aplicacion | Aplicación (Nombre, Funcionalidad, Propietario, Responsable, etc.) |
| Operacion | Activo de operaciones (Hostname, Oficina, Área, Ambiente, etc.) |
| CuentaPrivilegiada | Cuenta privilegiada |
| CuentaServicio | Cuenta de servicio |
| Asset | Activo normalizado (nuevo modelo) |
| ManagedAccount | Cuenta gestionada (nuevo modelo) |
| ManagedAccountSecurityGroup | Grupo de seguridad por cuenta |

### Aprobación y auditoría

| Clase | Descripción |
|-------|-------------|
| Aprobacion | Registro histórico de aprobaciones (legacy) |
| AprobacionVoto | Voto por aprobación (único por AprobacionId + UserId) |
| ApprovalRequest | Solicitud de aprobación |
| ApprovalDecision | Decisión (Aprobar / Rechazar) |
| AuditLog | Log técnico (tabla, entidad, acción, usuario) |
| AuditEvent | Auditoría funcional (BeforeJson, AfterJson) |
| EmailOutbox | Cola de correos (Pending, Sent, Failed) |

---

## Servicios (interfaces)

| Interface | Métodos principales |
|-----------|---------------------|
| ICurrentUserService | UserId, UserName, IsAdmin, CanSeeLogs, CanEdit, CanApproveModuloAsync |
| IAuditLogService | RegistrarAsync, GetLogsAsync |
| IAuditEventService | Registrar eventos Before/After |
| IAplicacionService | GetAllAsync, Create, Update, Delete |
| IExportService | ExportToExcelAsync, ExportToPdfAsync, ExportToCsvAsync |
| IAprobacionService | GetAllAsync, RegistrarAsync, MarcarAprobadoAsync |
| IAprobacionPermisoService | Gestión de aprobadores por módulo |
| IAuditoriaPdfService | GenerarPdfAsync (reporte de auditoría) |
| IEmailOutboxService | Encolar correos |
| IEmailSender | SendAsync (SMTP o DevEmailSender) |
| IRolService, IUsuarioService | CRUD de roles y usuarios |
| IMasterDataService | Catálogos maestros |

---

## DbContext

`AppDbContext` expone DbSet para cada entidad y configura en `OnModelCreating`:

- Claves compuestas (UserRole, RolePermission)
- Índices únicos (AprobacionPermiso, AprobacionVoto)
- Relaciones y comportamiento de borrado (Cascade, Restrict, NoAction)
