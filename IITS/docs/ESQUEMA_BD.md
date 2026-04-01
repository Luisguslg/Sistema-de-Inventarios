# Esquema de base de datos

## Diagrama ER (entidad-relación)

```mermaid
erDiagram
    Estatus ||--o{ Aplicacion : "EstatusId"
    Estatus ||--o{ Operacion : "EstatusId"
    Estatus ||--o{ Telecom : "EstatusId"
    Estatus ||--o{ CuentaServicio : "EstatusId"
    Estatus ||--o{ CuentaPrivilegiada : "EstatusId"
    Estatus ||--o{ PaginaWeb : "EstatusId"
    Estatus ||--o{ Asset : "StatusId"

    User ||--o{ UserRole : ""
    Role ||--o{ UserRole : ""
    Role ||--o{ RolePermission : ""
    Permission ||--o{ RolePermission : ""

    User ||--o{ AprobacionPermiso : "UserId"
    User ||--o{ Aprobacion : "UsuarioId"
    User ||--o{ ApprovalRequest : "SubmittedByUserId"
    User ||--o{ ApprovalDecision : "DecidedByUserId"
    User ||--o{ AuditLog : "UsuarioId"
    User ||--o{ AuditEvent : "PerformedByUserId"

    Alojamiento ||--o{ Aplicacion : "AlojamientoId"
    Parte ||--o{ Aplicacion : "PropietarioId"
    Parte ||--o{ Aplicacion : "ResponsableId"

    Area ||--o{ Asset : "AreaId"
    Area ||--o{ ManagedAccount : "AreaId"
    Office ||--o{ Asset : "OfficeId"
    Estatus ||--o{ Asset : "StatusId"
    Environment ||--o{ Asset : "EnvironmentId"
    Criticality ||--o{ Asset : "CriticalityId"
    Category ||--o{ Asset : "CategoryId"
    Vendor ||--o{ DeviceModel : "ManufacturerId"
    DeviceModel ||--o{ Asset : "DeviceModelId"
    Vendor ||--o{ Asset : "ManufacturerId"

    ApprovalRequest ||--o{ ApprovalDecision : "ApprovalRequestId"
    ManagedAccount ||--o{ ManagedAccountSecurityGroup : "ManagedAccountId"

    Estatus { guid Id string Nombre long Codigo }
    User { guid Id string Username string Nombre string Apellido string Email }
    Role { guid Id string Nombre }
    Permission { guid Id string Code string Description }
    UserRole { guid UserId guid RoleId }
    RolePermission { guid RoleId guid PermissionId }
    AprobacionPermiso { guid Id guid UserId string Modulo }
    Aprobacion { guid Id string Modulo guid EntidadId string Estado string Comentario guid UsuarioId datetime Fecha }
    AuditLog { guid Id string Tabla guid EntidadId string Accion guid UsuarioId datetime Fecha string Detalle }
    AuditEvent { guid Id string EntityType string EntityId string Action guid PerformedByUserId datetime PerformedAt string BeforeJson string AfterJson string Comment string CorrelationId }
    ApprovalRequest { guid Id string EntityType string EntityId guid AreaId string Status guid SubmittedByUserId datetime SubmittedAt int CurrentStep string Summary }
    ApprovalDecision { guid Id guid ApprovalRequestId string Decision string Comment guid DecidedByUserId datetime DecidedAt }
    EmailOutbox { guid Id string To string Subject string BodyHtml datetime CreatedAt datetime SentAt string Status string Error int RetryCount }
    Area { guid Id string Name }
    Office { guid Id string Name }
    Environment { guid Id string Name }
    Criticality { guid Id string Name }
    Category { guid Id string Name }
    Vendor { guid Id string Name }
    DeviceModel { guid Id guid ManufacturerId string Name }
    Asset { guid Id guid OfficeId guid AreaId string DeviceType string Hostname guid OperationEnvironmentId guid OwnerAreaId guid CriticalityId guid EnvironmentId guid CategoryId guid ManufacturerId guid DeviceModelId guid StatusId datetime CreatedAt guid CreatedBy datetime UpdatedAt guid UpdatedBy string ApprovalStatus }
    ManagedAccount { guid Id guid AreaId string Responsible string AccountName int AccountType string Origin string RelatedService string ChangeConfigType int ChangeIntervalDays }
    ManagedAccountSecurityGroup { guid Id guid ManagedAccountId string GroupName }
    Aplicacion { guid Id string Nombre guid EstatusId guid AlojamientoId guid PropietarioId guid ResponsableId }
    Alojamiento { guid Id string Nombre }
    Parte { guid Id string Nombre }
```

## Tablas

### Catálogos

| Tabla | Descripción |
|-------|-------------|
| Estatus | Activo, Inactivo, Desincorporado |
| Area | Aplicaciones, Telecomunicaciones, Operaciones, Soportes |
| Office | Oficina/ubicación |
| Environment | Ambiente |
| Criticality | Criticidad |
| Category | Categoría (activos) |
| Vendor | Fabricante/proveedor |
| DeviceModel | Modelo (FK a Vendor) |
| Alojamiento | Tipo de alojamiento (Data Center, Nube, etc.) |
| Parte | Partes interesadas: catálogo de propietarios y responsables de aplicaciones. Se usa en Aplicaciones (PropietarioId, ResponsableId). |

### Seguridad

| Tabla | Descripción |
|-------|-------------|
| Users | Usuarios (Username = DOMINIO\user o UPN) |
| Roles | SuperAdmin, Administrador, Operador, etc. |
| Permission | Códigos Perm.Inventory.Assets.View, etc. |
| UserRole | Usuario-Rol (N:M) |
| RolePermission | Rol-Permiso (N:M) |
| AprobacionPermiso | Usuario puede aprobar por módulo (Aplicaciones, Operaciones, Telecomunicaciones, Cuentas). Los permisos Perm.Inventory.Aplicaciones, Perm.Inventory.Operaciones, etc. controlan qué módulos ve cada usuario (Active Directory + BD). |

### Inventario

| Tabla | Descripción |
|-------|-------------|
| Aplicaciones | Catálogo de aplicaciones |
| Operaciones | Activos operaciones |
| Telecoms | Telecomunicaciones |
| CuentasServicio, CuentasPrivilegiadas | Cuentas de gestión (Nombre, Área, Responsable, Origen, ServicioRelacionado, Estatus) |
| CatalogItems | Catálogo genérico (Kind, Name) para TipoDispositivo, Función, TipoInfraestructura, SistemaOperativo — valores agregables "en el acto" |
| PaginasWeb | Páginas web (legacy; el módulo está unificado en Aplicaciones) |
| Asset | Activos (gestión de activos normalizada) |
| ManagedAccount | Cuentas de servicio/privilegiadas normalizadas |
| ManagedAccountSecurityGroup | Grupos de seguridad por cuenta |

### Aprobación y auditoría

| Tabla | Descripción |
|-------|-------------|
| Aprobaciones | Registro histórico de aprobaciones (legacy) |
| ApprovalRequest | Solicitud de aprobación (entidad, estado) |
| ApprovalDecision | Decisión (Aprobar/Rechazar) con comentario |
| AuditLog | Log técnico por tabla/entidad |
| AuditEvent | Trazabilidad funcional (Before/After JSON) |
| EmailOutbox | Cola de correos (Pending/Sent/Failed) |
