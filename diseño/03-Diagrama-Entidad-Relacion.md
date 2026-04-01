# Diagrama Entidad-Relación

## Vista general

El esquema de base de datos soporta inventarios (Aplicaciones, Operaciones, Cuentas), seguridad (Users, Roles, Permissions), aprobación y auditoría.

![Diagrama Entidad-Relación](imagenes/diagrama-er.png)

---

## Relaciones principales

### Estatus

- Estatus → Aplicacion (EstatusId)
- Estatus → Operacion (EstatusId)
- Estatus → CuentaPrivilegiada (EstatusId)
- Estatus → CuentaServicio (EstatusId)
- Estatus → Asset (StatusId)

### Seguridad

- User ↔ Role (UserRole, N:M)
- Role ↔ Permission (RolePermission, N:M)
- User → AprobacionPermiso (UserId, único por UserId + Modulo)
- User → ApprovalRequest (SubmittedByUserId)
- User → ApprovalDecision (DecidedByUserId)
- User → AuditLog (UsuarioId)
- User → AuditEvent (PerformedByUserId)

### Catálogos e inventario

- Alojamiento → Aplicacion (AlojamientoId)
- Area → Operacion, CuentaPrivilegiada, CuentaServicio
- Office → Operacion, Asset
- Environment → Operacion, Asset
- Criticality → Operacion, Asset
- Category → Operacion, Asset
- Vendor → DeviceModel (ManufacturerId)
- DeviceModel → Operacion, Asset
- Aplicacion → CuentaPrivilegiada, CuentaServicio (opcional)

### Aprobación

- ApprovalRequest → ApprovalDecision (1:N)
- ManagedAccount → ManagedAccountSecurityGroup (1:N)

---

## Tablas por grupo

### Catálogos

| Tabla | Descripción |
|-------|-------------|
| Estatus | Activo, Inactivo, Desincorporado |
| Area | Áreas responsables |
| Office | Oficinas |
| Environment | Ambientes |
| Criticality | Criticidades |
| Category | Categorías |
| Vendor | Fabricantes |
| DeviceModel | Modelos |
| Alojamiento | Tipos de alojamiento |
| CatalogItem | Catálogo genérico (Kind, Name) |

### Seguridad

| Tabla | Descripción |
|-------|-------------|
| Users | Usuarios (Username = DOMINIO\user o UPN) |
| Roles | Roles del sistema |
| Permissions | Códigos de permiso |
| UserRole | Usuario-Rol (N:M) |
| RolePermission | Rol-Permiso (N:M) |
| AprobacionPermiso | Aprobadores por módulo |

### Inventario

| Tabla | Descripción |
|-------|-------------|
| Aplicaciones | Catálogo de aplicaciones |
| Operaciones | Activos (servidores, equipos, dispositivos) |
| CuentasServicio | Cuentas de servicio |
| CuentasPrivilegiadas | Cuentas privilegiadas |
| PaginasWeb | Páginas web (legacy) |
| Assets | Activos normalizados |
| ManagedAccounts | Cuentas gestionadas |
| ManagedAccountSecurityGroups | Grupos de seguridad por cuenta |

### Aprobación y auditoría

| Tabla | Descripción |
|-------|-------------|
| Aprobaciones | Historial de aprobaciones |
| AprobacionVotos | Votos por aprobación |
| ApprovalRequests | Solicitudes de aprobación |
| ApprovalDecisions | Decisiones (Aprobar/Rechazar) |
| AuditLogs | Log técnico |
| AuditEvents | Auditoría funcional (Before/After) |
| EmailOutbox | Cola de correos |

---

## Índices únicos

- `AprobacionPermiso (UserId, Modulo)`
- `AprobacionVoto (AprobacionId, UserId)`
