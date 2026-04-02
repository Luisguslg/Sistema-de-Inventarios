# DAS — Documento de Arquitectura del Sistema

**Sistema:** IITS — Inventario de Infraestructura Tecnológica y Seguridad  
**Versión:** 1.0  
**Fecha:** 2026-04-01  
**Clasificación:** Interno KPMG Venezuela  

---

## 1. Introducción

Este documento describe la arquitectura del sistema IITS siguiendo el modelo C4 (Context, Container, Component). Cubre la topología de despliegue, la pila de middleware, los flujos de datos clave y la arquitectura de seguridad.

---

## 2. Diagrama de Contexto (C4 — Nivel 1)

```
+------------------------------------------------------------------+
|                     KPMG Venezuela Intranet                      |
|                                                                  |
|   +----------+          HTTPS/Negotiate          +----------+    |
|   | Empleado |  --------------------------->     |          |    |
|   | (Browser)|                                   |  IITS    |    |
|   +----------+                                   |  Web App |    |
|                                                   |          |    |
|   +----------+                                   +----+-----+    |
|   | Active   |  <--- Windows Auth (Kerberos) -------->|          |
|   | Directory|                                        |          |
|   +----------+                                   +----+-----+    |
|                                                   | SQL      |    |
|   +----------+                                   | Server   |    |
|   | SMTP     |  <--- Correos salientes ---------->|          |    |
|   | Relay    |       (EmailOutbox)                +----------+    |
|   +----------+                                                   |
+------------------------------------------------------------------+

Actores externos:
  - Empleado KPMG: Accede vía browser (Chrome/Edge) en intranet
  - Active Directory: Autentica mediante Negotiate (Kerberos/NTLM)
  - SQL Server: Persistencia de datos (IITSN en VECCSAPP10,61057)
  - SMTP Relay: goemairs.go.kworld.kpmg.com:25 (notificaciones)
```

---

## 3. Diagrama de Contenedores (C4 — Nivel 2)

```
+--------------------------------------------------------------------------+
|                        Servidor VECCSAPP10 (Windows Server)              |
|                                                                          |
|  +------------------+       +------------------------------------------+|
|  |  IIS             |       |  IITS Application (Blazor Server / .NET 8)||
|  |  Application     |       |                                            ||
|  |  Pool            | ----> |  +----------------+  +------------------+ ||
|  |  (No Managed     |       |  | Razor Pages    |  | Blazor Server    | ||
|  |   Code,          |       |  | (_Host.cshtml, |  | Components       | ||
|  |   Windows Auth)  |       |  |  Error.cshtml) |  | (.razor pages)   | ||
|  +------------------+       |  +-------+--------+  +--------+---------+ ||
|                              |          |                    |            ||
|  forwardWindowsAuthToken=true|  +-------v--------------------v---------+ ||
|                              |  |         Middleware Pipeline           | ||
|                              |  |  UseStaticFiles → UseRouting →        | ||
|                              |  |  UseAuthentication →                  | ||
|                              |  |  DevAuthMiddleware →                  | ||
|                              |  |  SessionCookieSignInMiddleware →       | ||
|                              |  |  UseAuthorization → UseRateLimiter     | ||
|                              |  +-------+-------------------------------+ ||
|                              |          |                                 ||
|                              |  +-------v-------------------------------+ ||
|                              |  |         Services Layer                | ||
|                              |  |  AplicacionService, AprobacionService,| ||
|                              |  |  AuditLogService, AuditEventService,  | ||
|                              |  |  ExportService, UsuarioService,       | ||
|                              |  |  RolService, MasterDataService,       | ||
|                              |  |  EmailOutboxService, CurrentUserService| ||
|                              |  +-------+-------------------------------+ ||
|                              |          |                                 ||
|                              |  +-------v-------------------------------+ ||
|                              |  |         Data Layer (EF Core 8)        | ||
|                              |  |  AppDbContext (DbContext)             | ||
|                              |  |  DbSeed (métodos Ensure*)             | ||
|                              |  |  PermissionCodes (constantes)         | ||
|                              |  +-------+-------------------------------+ ||
|                              |          |                                 ||
|                              |  +-------v-------------------------------+ ||
|                              |  |  Background Services                  | ||
|                              |  |  EmailOutboxHostedService             | ||
|                              |  |  (IHostedService, ciclo 30 segundos) | ||
|                              |  +--------------------------------------+ ||
|                              +------------------------------------------+|
|                                          |                               |
|  +----------------------+       +--------v--------+                      |
|  |  SQL Server          |       |  IITSClaimsTransformation             |
|  |  IITSN database      | <-----|  (IClaimsTransformation)              |
|  |  (VECCSAPP10,61057)  |       |  Enriquece claims con BD              |
|  +----------------------+       +---------------------------------+      |
|                                                                          |
+--------------------------------------------------------------------------+
```

---

## 4. Diagrama de Componentes (C4 — Nivel 3)

### 4.1 Capa de Presentación (Blazor Server)

```
Pages/
├── Index.razor              — Dashboard / página de inicio
├── Aplicaciones.razor       — Lista y formulario de Aplicaciones
├── Operaciones.razor        — Lista y formulario de Operaciones
├── Cuentas.razor            — Selector de módulo de cuentas
├── CuentasPrivilegiadas.razor
├── CuentasServicio.razor
├── Auditoria.razor          — Vista de auditoría
├── Logs.razor               — Vista de logs (solo Auditor/Admin)
├── PaginasWeb.razor         — Módulo de páginas web
├── Admin/
│   ├── Usuarios.razor       — Gestión de usuarios
│   ├── Roles.razor          — Gestión de roles
│   ├── PermisosRol.razor    — Asignación de permisos a roles
│   ├── Aprobaciones.razor   — Panel de aprobaciones pendientes
│   ├── PermisosAprobacion.razor — Configurar aprobadores por módulo
│   └── MaestroDatos.razor   — Catálogos y datos maestros
└── Shared/
    └── (Layouts y componentes compartidos)
```

### 4.2 Capa de Servicios

| Servicio | Interfaz | Responsabilidad |
|---|---|---|
| `AplicacionService` | `IAplicacionService` | CRUD de Aplicaciones con auditoría |
| `AprobacionService` | `IAprobacionService` | Flujo de aprobación multi-aprobador y votos |
| `AprobacionPermisoService` | `IAprobacionPermisoService` | Gestión de permisos de aprobadores por módulo |
| `AuditLogService` | `IAuditLogService` | Escritura y consulta de AuditLog |
| `AuditEventService` | `IAuditEventService` | Escritura y consulta de AuditEvent (con JSON before/after) |
| `AuditoriaPdfService` | `IAuditoriaPdfService` | Generación de PDF de auditoría vía QuestPDF |
| `ExportService` | `IExportService` | Exportación a Excel (ClosedXML), PDF (QuestPDF) y CSV |
| `CurrentUserService` | `ICurrentUserService` | Lectura de claims del usuario en sesión |
| `RolService` | `IRolService` | CRUD de Roles y RolePermissions |
| `UsuarioService` | `IUsuarioService` | CRUD de Users y UserRoles |
| `MasterDataService` | `IMasterDataService` | Lectura de catálogos (Offices, Areas, Vendors, etc.) |
| `EmailOutboxService` | `IEmailOutboxService` | Escritura en EmailOutbox y procesamiento |
| `SmtpEmailSender` | `IEmailSender` | Envío SMTP real (producción) |
| `DevEmailSender` | `IEmailSender` | No-op (desarrollo) |
| `DashboardService` | — | Estadísticas para el dashboard |
| `ToastService` | — | Notificaciones UI (Blazor) |

### 4.3 Capa de Datos (EF Core)

```
AppDbContext
├── DbSets: Estatus, Aplicaciones, Operaciones, CuentasPrivilegiadas,
│   CuentasServicio, PaginasWeb, Users, Roles, Permissions,
│   RolePermissions, UserRoles, Aprobaciones, AprobacionVotos,
│   ApprovalRequests, ApprovalDecisions, AuditLogs, AuditEvents,
│   EmailOutbox, AprobacionPermisos, Alojamientos, Areas, Offices,
│   Environments, Criticalities, Categories, Vendors, DeviceModels,
│   Assets, ManagedAccounts, ManagedAccountSecurityGroups, CatalogItems
│
└── OnModelCreating: relaciones, claves compuestas, índices únicos,
    comportamientos de eliminación (Cascade / Restrict / NoAction)
```

### 4.4 Middleware Stack (orden de ejecución)

```
Request HTTP/HTTPS
        |
        v
[1] UsePathBase           — PathBase configurable (ej: /IITSN)
        |
        v
[2] UseExceptionHandler   — Captura excepciones (producción)
    / UseHsts / UseHttpsRedirection
        |
        v
[3] UseStaticFiles        — Archivos estáticos (wwwroot)
        |
        v
[4] UseRouting            — Mapeo de rutas
        |
        v
[5] UseAuthentication     — Valida cookie de sesión existente
        |
        v
[6] DevAuthMiddleware     — (Solo Development) Inyecta usuario dev
        |
        v
[7] SessionCookieSignInMiddleware
        |                   Si hay usuario Windows (Negotiate) pero no cookie:
        |                   → Autentica con Negotiate
        |                   → Crea cookie con solo ClaimTypes.Name
        |                   → IITSClaimsTransformation agrega roles/permisos
        v
[8] UseAuthorization      — Verifica policies y claims
        |
        v
[9] UseRateLimiter        — Rate limiting (export/pdf: 10 req/min)
        |
        v
[10] MapGet/MapBlazorHub/MapFallbackToPage
```

---

## 5. Flujo de Datos: Aprobación Multi-Aprobador

```
Operador                   IITS App                    Base de Datos            Email
   |                          |                               |                    |
   |-- POST (crear/editar) -->|                               |                    |
   |                          |-- INSERT Aplicacion/Operacion -->                  |
   |                          |-- INSERT Aprobacion (Estado="Por aprobar") -->     |
   |                          |-- INSERT AuditLog (Accion="Crear") -->             |
   |                          |                               |                    |
   |                          |-- GetApproversForModulo() --->|                    |
   |                          |<-- Lista de aprobadores -------|                    |
   |                          |                               |                    |
   |                          |-- INSERT EmailOutbox (x N aprobadores) -->         |
   |<-- Respuesta UI ---------|                               |                    |
   |                          |                               |                    |
   |                    (30 seg después)                      |                    |
   |                          |                               |                    |
   |                   EmailOutboxHostedService               |                    |
   |                          |-- SELECT Pending EmailOutbox -->                   |
   |                          |-- SmtpEmailSender.Send() ----------------------->  |
   |                          |-- UPDATE EmailOutbox (Status="Sent") -->           |
   |                          |                               |                    |
Aprobador                     |                               |                    |
   |-- GET /Aprobaciones ---->|                               |                    |
   |                          |-- SELECT Aprobaciones + Votos -->                  |
   |<-- Lista pendientes -----|                               |                    |
   |                          |                               |                    |
   |-- POST (Aprobar/Rechazar)|                               |                    |
   |                          |-- INSERT AprobacionVoto ------>                    |
   |                          |-- ¿Todos aprobaron? --------->                     |
   |                          |   [Si] UPDATE Aprobacion (Estado="Aprobado") -->   |
   |                          |   [No] Sin cambio (espera más votos)               |
   |                          |-- INSERT AuditLog (Accion="Aprobar") -->           |
   |<-- Respuesta UI ---------|                               |                    |
```

---

## 6. Flujo de Autenticación

```
Browser                     IIS                    IITS App                  AD / BD
   |                         |                         |                        |
   |-- GET / (sin cookie) -->|                         |                        |
   |                         |-- Negotiate challenge -->|                       |
   |<-- 401 Negotiate -------|                         |                        |
   |-- Kerberos token ------>|                         |                        |
   |                         |-- forward token ------->|                        |
   |                         |                         |-- ValidateToken ------->|
   |                         |                         |<-- identity.Name=VE\usr|
   |                         |                         |                        |
   |         SessionCookieSignInMiddleware              |                        |
   |                         |                         |-- SignInAsync(Cookie) ->|
   |                         |                         |   (solo ClaimTypes.Name)|
   |                         |                         |                        |
   |         IITSClaimsTransformation (en cada request)|                        |
   |                         |                         |-- SELECT User + Roles ->|
   |                         |                         |   (incluye permisos)    |
   |                         |                         |<-- Claims enriquecidos -|
   |<-- 200 OK + cookie -----|                         |                        |
   |                         |                         |                        |
   |-- GET /Aplicaciones --->| (con cookie)            |                        |
   |                         |-- UseAuthentication --->|                        |
   |                         |                         |-- IITSClaimsTransform ->|
   |                         |                         |<-- Claims completos ----|
   |                         |                         |-- Authorize(Policy) --->|
   |<-- 200 Aplicaciones ----|                         |                        |
```

---

## 7. Arquitectura de Seguridad

### 7.1 Cadena de Autenticación

```
IIS (forwardWindowsAuthToken=true)
    → Negotiate (Kerberos/NTLM)
        → SessionCookieSignInMiddleware
            → Cookie (.IITS.Session, HttpOnly, Secure)
                → IITSClaimsTransformation
                    → Claims: UserId, Nombre, Apellido, Email, CodSap
                              ClaimTypes.Role (por cada rol)
                              "Permission" (por cada permiso del rol)
```

### 7.2 Autorización por Permiso

Cada endpoint y componente Blazor verifica permisos mediante:
```csharp
options.AddPolicy(code, p => p.RequireClaim("Permission", code));
// FallbackPolicy: RequireAuthenticatedUser (todo requiere login)
```

### 7.3 Rate Limiting

```
Política "export" (Fixed Window):
  - Ventana: 1 minuto
  - Límite: 10 solicitudes
  - Queue: 0 (rechaza inmediatamente con HTTP 429)
  - Aplicada a: /api/export/* y /api/auditoria/pdf
```

### 7.4 Gestión de Secretos

```
Repositorio:               appsettings.Production.json (plantilla vacía, en .gitignore)
Servidor de producción:    Variables de entorno en web.config:
  - ConnectionStrings__IITS
  - Auth__SuperAdminUsername
  - Email__Mode / Email__From / Email__SmtpServer / Email__Port
  - App__BaseUrl
  - PathBase
```

### 7.5 Pista de Auditoría

```
AuditLog:     Registro simple — tabla, entidad, acción, usuario, fecha, detalle (texto)
AuditEvent:   Registro estructurado — EntityType, EntityId, Action, PerformedByUserId,
              PerformedAt, BeforeJson, AfterJson, Comment, CorrelationId
```

---

## 8. Topología de Despliegue

```
Internet
    |
  [Firewall corporativo KPMG]
    |
  Intranet KPMG Venezuela
    |
  DNS: desarrollos.ve.kworld.kpmg.com
    |
  [Load Balancer / Proxy Reverso]  (si aplica)
    |
  VECCSAPP10 (Windows Server)
    |
    +--- IIS Site: IITSN
    |       Application Pool: IITSN_Pool
    |         - No Managed Code
    |         - Identity: ApplicationPoolIdentity (o cuenta de servicio AD)
    |         - Enable 32-bit: No
    |         - Windows Authentication: Enabled (Negotiate, NTLM)
    |         - Anonymous Authentication: Disabled
    |
    +--- IITS App (in-process)
    |       Ruta física: C:\inetpub\wwwroot\IITSN\
    |       web.config: forwardWindowsAuthToken="true"
    |       Logs: .\logs\stdout
    |
    +--- SQL Server (VECCSAPP10,61057)
            Base de datos: IITSN
            Autenticación: Windows (Trusted_Connection)
```

---

## 9. Decisiones de Arquitectura Relevantes

| Decisión | Justificación |
|---|---|
| Blazor Server (no WebAssembly) | Requiere acceso directo al contexto HTTP para Windows Auth + SessionCookieSignIn. WASM no puede acceder al HttpContext del servidor. |
| Cookie con solo `ClaimTypes.Name` | Previene HTTP 400 "Request Too Long" por exceso de claims en headers. Los roles/permisos se recargan en cada request desde BD. |
| Patrón EmailOutbox | Desacopla el envío de correo del ciclo de request-response. Garantiza entrega eventual aunque el SMTP falle transientemente. |
| SplitQuery en EF Core | Evita el producto cartesiano en consultas con múltiples `Include()`. Mejora performance en listados con relaciones cargadas. |
| `EnsureColumnsAsync` (SQL idempotente) | Compatibilidad hacia atrás: permite que una BD con solo `InitialSchema` aplicado reciba las columnas nuevas sin necesidad de migración formal. |
| Rate Limiting fixed window 10/min | Protege la generación de exports (CPU/memoria intensiva) de abuso. Sin cola para evitar backpressure silencioso. |
| `appsettings.Production.json` en `.gitignore` | Evita que credenciales o configuraciones sensibles de producción se publiquen accidentalmente al repositorio. |
