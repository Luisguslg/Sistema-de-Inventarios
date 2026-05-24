# Estructura del proyecto IITS

Organización de carpetas según las capas de la arquitectura (ver [ARQUITECTURA.md](ARQUITECTURA.md)).

## Carpetas principales

| Carpeta | Contenido |
|---------|-----------|
| **Data** | `AppDbContext`, `DbSeed`, `PermissionCodes`. Acceso a datos y configuración de permisos. |
| **Entities** | Modelos de dominio (BD): User, Role, Aplicacion, Operacion, Telecom, Estatus, Aprobacion, AuditLog, etc. |
| **Services** | Lógica de aplicación: *Service, I*Service (AplicacionService, AuditLogService, AprobacionService, UsuarioService, RolService, EmailOutboxService, etc.). |
| **Pages** | UI Blazor: páginas por módulo (Aplicaciones, Operaciones, Telecomunicaciones, Cuentas, Admin, Data). |
| **Shared** | Componentes compartidos: NavMenu, MainLayout. |
| **Modelo** | Modelos de formularios y componentes (FormAplicacionModel, FormUsuarioModel, ToastBase). |
| **Middleware** | DevAuthMiddleware (modo desarrollo sin dominio). |
| **Migrations** | Migraciones EF Core. |
| **wwwroot** | Estáticos: css, js, imágenes. |
| **docs** | Documentación técnica. |

- Ver [LOGICA_MODULOS_Y_DASHBOARD.md](LOGICA_MODULOS_Y_DASHBOARD.md) para la lógica de Operaciones, Aplicaciones, Partes, Cuentas y cómo el Dashboard se alimenta según permisos.

## Flujo de datos

- **UI (Pages)** → llama a **Services** → **Services** usan **AppDbContext** y **Entities**.
- **Autenticación:** IIS + Negotiate (Windows). Usuario en **Users**; roles en **UserRoles**; permisos en **RolePermission** y **AprobacionPermisos**.
- **Auditoría:** operaciones CRUD y aprobaciones se registran en **AuditLogs** (y opcionalmente AuditEvents).
- **Correos:** notificaciones a aprobadores vía **EmailOutbox** y **EmailOutboxHostedService**.

## Base de datos y despliegue

- **BD producción:** IITSN en `VECCSAPP10\KPMGDV`.
- **Ruta de publicación:** `\\veccsapp10\app`.
- Ver [DESPLIEGUE_IIS.md](DESPLIEGUE_IIS.md) para ejecutar localmente, aplicar migraciones y publicar.
