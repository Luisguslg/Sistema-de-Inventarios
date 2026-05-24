# Diagrama de Componentes

## Vista general

La aplicación IITS se estructura en capas: UI, Servicios, Infraestructura y Entidades.

![Diagrama de componentes](imagenes/diagrama-componentes.png)

---

## Capas

```
┌──────────────────────────────────────────────────────────────┐
│  UI (Blazor Server)                                          │
│  Pages, Shared, Modelo/Form*                                 │
├──────────────────────────────────────────────────────────────┤
│  Servicios (Application)                                     │
│  *Service, I*Service                                         │
├──────────────────────────────────────────────────────────────┤
│  Infraestructura                                             │
│  AppDbContext, Migraciones, Middleware, Export               │
├──────────────────────────────────────────────────────────────┤
│  Entidades (Entities)                                        │
└──────────────────────────────────────────────────────────────┘
```

---

## Componentes

### UI (Blazor)

| Componente | Función |
|------------|---------|
| Pages | Páginas por módulo (Aplicaciones, Operaciones, Cuentas, Admin, Data) |
| Pages/Admin | Roles, Usuarios, Aprobaciones, Permisos, MaestroDatos |
| Shared | NavMenu, MainLayout, ExportarDropdown |
| Modelo | FormAplicacionModel, FormUsuarioModel, FormRolModel, ToastBase |

### Servicios

| Componente | Función |
|------------|---------|
| AplicacionService | CRUD aplicaciones |
| AuditLogService | Registro y consulta de logs |
| AuditEventService | Auditoría funcional Before/After |
| AprobacionService | Gestión de aprobaciones |
| AprobacionPermisoService | Aprobadores por módulo |
| ExportService | Excel, PDF, CSV |
| AuditoriaPdfService | PDF de auditoría por módulo |
| UsuarioService, RolService | Gestión de usuarios y roles |
| MasterDataService | Catálogos maestros |
| EmailOutboxService | Cola de correos |
| CurrentUserService | Usuario actual, permisos |
| DashboardService | Totales por módulo |
| ToastService | Notificaciones en UI |

### Infraestructura

| Componente | Función |
|------------|---------|
| AppDbContext | Acceso a datos, DbSets |
| DbSeed | Carga inicial de catálogos |
| PermissionCodes | Códigos de permisos |
| DevAuthMiddleware | Usuario emulado en desarrollo |
| SessionCookieSignInMiddleware | Cookie de sesión |
| Migraciones EF Core | Esquema de BD |

### Dependencias externas

| Componente | Uso |
|------------|-----|
| SQL Server | Base de datos |
| Active Directory | Autenticación Windows (IIS) |
| SMTP | Envío de correos |
| ClosedXML | Excel |
| QuestPDF | PDF |
| EPPlus / NPOI | Excel (alternativos) |

---

## Flujo de datos

1. **UI** llama a **Servicios**.
2. **Servicios** usan **AppDbContext** y **Entidades**.
3. **Middleware** gestiona autenticación (Cookie + Negotiate).
4. **IITSClaimsTransformation** añade claims (UserId, roles) desde BD.
5. **ExportService** genera archivos vía `/api/export/`.
