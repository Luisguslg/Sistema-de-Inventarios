# Arquitectura IITS

## Stack

- **Runtime:** .NET 8
- **UI:** Blazor Server, Razor Pages (_Host)
- **Datos:** Entity Framework Core 8, SQL Server
- **Auth:** Windows (Negotiate) en IIS; modo Dev con usuario emulado en local

## Capas

```
┌─────────────────────────────────────────────────────────┐
│  UI (Blazor: Pages, Shared, Modelo/Form*)               │
├─────────────────────────────────────────────────────────┤
│  Servicios (Application: *Service, I*Service)           │
├─────────────────────────────────────────────────────────┤
│  Infra: AppDbContext, Migraciones, Middleware, Export   │
├─────────────────────────────────────────────────────────┤
│  Entidades (Entities)                                   │
└─────────────────────────────────────────────────────────┘
```

- **UI:** páginas por módulo (Aplicaciones, Operaciones, Admin, etc.), layout y menú según permisos.
- **Servicios:** lógica de negocio, validaciones, aprobaciones, export, auditoría, usuario actual.
- **Infra:** EF Core, DbContext, migraciones; middleware DevAuth; export PDF/Excel/CSV.
- **Entidades:** modelos de BD (User, Role, Aplicacion, Estatus, ApprovalRequest, etc.).

## Autenticación y autorización

- **Producción (IIS):** esquema Negotiate (Windows). El usuario llega como `DOMINIO\usuario`. `IITSClaimsTransformation` busca el usuario en `Users` por `Username` y añade claims (UserId, roles).
- **Desarrollo (local sin dominio):** `Auth:Mode: Dev` en appsettings.Development. `DevAuthMiddleware` inyecta una identidad con `Auth:DevUsername` para que ClaimsTransformation cargue ese usuario desde BD.
- **SuperAdmin:** configurado por `Auth:SuperAdminUsername`. Seed crea el usuario y asigna rol SuperAdmin si no existen.
- **Autorización:** por roles (SuperAdmin, Administrador, Operador, Auditor, Aprobador) y por permisos (Permission / RolePermission) con políticas por código de permiso.

## Flujos principales

1. **Inventario:** CRUD por módulo (Aplicaciones, Operaciones, etc.). Cambios registrados en AuditLogs. Las entidades controladas generan ApprovalRequest y quedan en estado pendiente hasta aprobación.
2. **Aprobación:** solicitud → notificación a aprobadores (EmailOutbox) → decisión (ApprovalDecision) → actualización de entidad y auditoría.
3. **Export:** todas las listas exponen PDF, CSV y Excel vía `/api/export/{modulo}/{formato}` con nombre de archivo `{Modulo}_{yyyyMMdd_HHmm}.ext`.

## Referencias

- Esquema de BD: [ESQUEMA_BD.md](ESQUEMA_BD.md)
- Diagrama de clases: [DIAGRAMA_CLASES.md](DIAGRAMA_CLASES.md)
- Despliegue IIS: [DESPLIEGUE_IIS.md](DESPLIEGUE_IIS.md)
