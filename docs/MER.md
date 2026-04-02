# MER — Modelo Entidad-Relación

**Sistema:** IITS — Inventario de Infraestructura Tecnológica y Seguridad  
**Versión:** 1.0  
**Fecha:** 2026-04-01  
**Clasificación:** Interno KPMG Venezuela  

---

## 1. Introducción

Este documento describe el modelo de datos de IITS. Todas las tablas usan `uniqueidentifier` (GUID) como clave primaria, excepto las tablas de unión con clave compuesta. Las fechas se almacenan en UTC (`datetime2`). El modelo se gestiona con EF Core 8 code-first.

---

## 2. Diagrama General de Relaciones (texto)

```
Users ──── UserRole ──── Roles ──── RolePermission ──── Permissions
  |
  |─── AprobacionPermisos (módulo)
  |─── AprobacionVotos.UserId
  |─── ApprovalRequest.SubmittedByUserId
  |─── ApprovalDecision.DecidedByUserId
  |─── AuditLog.UsuarioId
  |─── AuditEvent.PerformedByUserId

Aplicaciones ──── Estatus
             ──── Alojamientos
             <─── CuentaPrivilegiada.AplicacionId
             <─── CuentaServicio.AplicacionId

Operaciones ──── Estatus
            ──── Offices
            ──── Areas (x2: Area, OwnerArea)
            ──── Alojamientos
            ──── Environments
            ──── Criticalities
            ──── Categories
            ──── Vendors (como Manufacturer)
            ──── DeviceModels

CuentaPrivilegiada ──── Estatus
                   ──── Areas
                   ──── Aplicaciones

CuentaServicio ──── Estatus
               ──── Areas
               ──── Aplicaciones

Aprobaciones <──── AprobacionVotos (multi-aprobador)
ApprovalRequests <──── ApprovalDecisions

AuditLogs  (registro simple de auditoría)
AuditEvents (registro estructurado con JSON before/after)
EmailOutbox (cola de correos salientes)

DeviceModels ──── Vendors (Manufacturer)
Assets ──── Offices, Areas (x2), Estatus, Environments (x2),
            Criticalities, Categories, Vendors, DeviceModels
ManagedAccounts ──── Areas, Estatus
ManagedAccountSecurityGroups ──── ManagedAccounts
CatalogItems  (catálogo genérico: TipoDispositivo, Funcion, etc.)
```

---

## 3. Tablas del Modelo

### 3.1 Users

Usuarios del sistema. El campo `Username` corresponde al nombre de dominio AD (puede ser `DOMINIO\usuario` o solo `usuario`).

| Campo | Tipo | Restricciones | Descripción |
|---|---|---|---|
| `Id` | `uniqueidentifier` | PK, NOT NULL | Identificador único |
| `Username` | `nvarchar(100)` | NOT NULL, UNIQUE | Nombre de dominio AD |
| `Nombre` | `nvarchar(150)` | NULL | Nombre de pila |
| `Apellido` | `nvarchar(150)` | NULL | Apellido |
| `Email` | `nvarchar(200)` | NULL | Correo electrónico |
| `CodSap` | `nvarchar(50)` | NULL | Código SAP del empleado |

---

### 3.2 Roles

Roles del sistema (SuperAdmin, Administrador, Operador, Aprobador, Auditor, Usuario).

| Campo | Tipo | Restricciones | Descripción |
|---|---|---|---|
| `Id` | `uniqueidentifier` | PK, NOT NULL | Identificador único |
| `Nombre` | `nvarchar(50)` | NOT NULL | Nombre del rol |

---

### 3.3 UserRole

Tabla de unión N:M entre `Users` y `Roles`.

| Campo | Tipo | Restricciones | Descripción |
|---|---|---|---|
| `UserId` | `uniqueidentifier` | PK (parte), FK → Users | ID del usuario |
| `RoleId` | `uniqueidentifier` | PK (parte), FK → Roles | ID del rol |

**Cardinalidad:** Users (1) — (N) UserRole (N) — (1) Roles

---

### 3.4 Permissions

Permisos granulares del sistema.

| Campo | Tipo | Restricciones | Descripción |
|---|---|---|---|
| `Id` | `uniqueidentifier` | PK, NOT NULL | Identificador único |
| `Code` | `nvarchar(100)` | NOT NULL | Código del permiso (ej: `Perm.Inventory.View`) |
| `Description` | `nvarchar(200)` | NULL | Descripción legible |

---

### 3.5 RolePermission

Tabla de unión N:M entre `Roles` y `Permissions`. Clave compuesta.

| Campo | Tipo | Restricciones | Descripción |
|---|---|---|---|
| `RoleId` | `uniqueidentifier` | PK (parte), FK → Roles (Cascade) | ID del rol |
| `PermissionId` | `uniqueidentifier` | PK (parte), FK → Permissions (Cascade) | ID del permiso |

**Cardinalidad:** Roles (1) — (N) RolePermission (N) — (1) Permissions

---

### 3.6 Estatus

Catálogo de estados de los registros de inventario.

| Campo | Tipo | Restricciones | Descripción |
|---|---|---|---|
| `Id` | `uniqueidentifier` | PK, NOT NULL | Identificador único |
| `Nombre` | `nvarchar(100)` | NOT NULL | Nombre del estado |
| `Codigo` | `bigint` | NOT NULL | Código numérico (Activo=1000, Inactivo=1500, Desincorporado=2000) |

---

### 3.7 Aplicaciones

Catálogo de aplicaciones de la firma.

| Campo | Tipo | Restricciones | Descripción |
|---|---|---|---|
| `Id` | `uniqueidentifier` | PK, NOT NULL | Identificador único |
| `Nombre` | `nvarchar(300)` | NOT NULL | Nombre de la aplicación |
| `Funcionalidad` | `nvarchar(500)` | NULL | Descripción funcional |
| `Propietario` | `nvarchar(200)` | NULL | Propietario (texto libre) |
| `Responsable` | `nvarchar(200)` | NULL | Responsable técnico (texto libre) |
| `TipoAlojamiento` | `nvarchar(200)` | NULL | Tipo de alojamiento (texto, campo legacy) |
| `Proveedor` | `nvarchar(300)` | NULL | Proveedor de la aplicación |
| `ClasificacionInformacion` | `nvarchar(200)` | NULL | Clasificación de la información |
| `Critico` | `bit` | NOT NULL | Indicador de criticidad |
| `IntegracionesRelevantes` | `nvarchar(500)` | NULL | Integraciones con otros sistemas |
| `DependenciasTecnicas` | `nvarchar(500)` | NULL | Dependencias técnicas |
| `ModeloLicenciamiento` | `nvarchar(200)` | NULL | Modelo de licenciamiento |
| `CostoAnualEstimado` | `decimal(18,2)` | NULL | Costo anual estimado |
| `FechaAdquisicionImplementacion` | `datetime2` | NULL | Fecha de adquisición o implementación |
| `VersionActual` | `nvarchar(100)` | NULL | Versión actual de la aplicación |
| `SLA` | `nvarchar(200)` | NULL | Acuerdo de nivel de servicio |
| `RTO` | `nvarchar(100)` | NULL | Recovery Time Objective (ISO-067-GCS) |
| `RPO` | `nvarchar(100)` | NULL | Recovery Point Objective (ISO-067-GCS) |
| `Autenticacion` | `nvarchar(200)` | NULL | Método de autenticación de la aplicación |
| `EstatusId` | `uniqueidentifier` | NOT NULL, FK → Estatus | Estado del registro |
| `CreatedAt` | `datetime2` | NOT NULL | Fecha/hora UTC de creación |
| `AlojamientoId` | `uniqueidentifier` | NULL, FK → Alojamientos | Tipo de alojamiento (catálogo) |

> **Nota importante:** Los campos `RTO` y `RPO` son campos separados e independientes. En versiones anteriores existía un campo combinado `RPORTO`; este fue migrado y eliminado. El script de migración copia el valor de `RPORTO` a ambos campos si `RTO` es nulo.

**Cardinalidades:**
- `Aplicaciones` (N) → (1) `Estatus`
- `Aplicaciones` (N) → (0..1) `Alojamientos`

**Índice:** `IX_Aplicaciones_AlojamientoId`

---

### 3.8 Operaciones

Inventario de activos de infraestructura tecnológica.

| Campo | Tipo | Restricciones | Descripción |
|---|---|---|---|
| `Id` | `uniqueidentifier` | PK, NOT NULL | Identificador único |
| `CreatedAt` | `datetime2` | NOT NULL | Fecha/hora UTC de creación |
| `EstatusId` | `uniqueidentifier` | NOT NULL, FK → Estatus | Estado del registro |
| `Hostname` | `nvarchar(200)` | NOT NULL | Hostname del activo |
| `Serial` | `nvarchar(100)` | NULL | Número de serie |
| `OfficeId` | `uniqueidentifier` | NULL, FK → Offices | Oficina |
| `AreaId` | `uniqueidentifier` | NULL, FK → Areas | Área |
| `AlojamientoId` | `uniqueidentifier` | NULL, FK → Alojamientos | Alojamiento |
| `OwnerAreaId` | `uniqueidentifier` | NULL, FK → Areas | Área propietaria |
| `CriticalityId` | `uniqueidentifier` | NULL, FK → Criticalities | Criticidad |
| `EnvironmentId` | `uniqueidentifier` | NULL, FK → Environments | Ambiente |
| `CategoryId` | `uniqueidentifier` | NULL, FK → Categories | Categoría |
| `TipoDispositivo` | `nvarchar(100)` | NULL | Tipo de dispositivo |
| `Funcion` | `nvarchar(200)` | NULL | Función o uso del activo |
| `TipoInfraestructura` | `nvarchar(50)` | NULL | Tipo de infraestructura (física/virtual/nube) |
| `Host` | `nvarchar(200)` | NULL | Host físico (si es virtual) |
| `RAM` | `nvarchar(100)` | NULL | Memoria RAM |
| `CantidadCPU` | `int` | NULL | Número de CPUs/núcleos |
| `VelocidadCPU` | `nvarchar(50)` | NULL | Velocidad de CPU |
| `CapacidadDAS` | `nvarchar(100)` | NULL | Capacidad de almacenamiento DAS |
| `CapacidadSAN` | `nvarchar(100)` | NULL | Capacidad de almacenamiento SAN |
| `SistemaOperativo` | `nvarchar(200)` | NULL | Sistema operativo |
| `ManufacturerId` | `uniqueidentifier` | NULL, FK → Vendors | Fabricante |
| `DeviceModelId` | `uniqueidentifier` | NULL, FK → DeviceModels | Modelo |
| `IP` | `nvarchar(50)` | NULL | Dirección IP |
| `MAC` | `nvarchar(100)` | NULL | Dirección MAC |
| `Firmware` | `nvarchar(200)` | NULL | Versión de firmware |
| `GarantiaExpira` | `datetime2` | NULL | Fecha de expiración de garantía |
| `Observaciones` | `nvarchar(500)` | NULL | Observaciones generales |
| `BCP` | `bit` | NULL | Participa en el BCP (Business Continuity Plan) |
| `RTO` | `nvarchar(100)` | NULL | Recovery Time Objective (ISO-067-GCS) |
| `RPO` | `nvarchar(100)` | NULL | Recovery Point Objective (ISO-067-GCS) |
| `Propietario` | `nvarchar(100)` | NULL | Propietario (Auditoría, Impuesto, Asesoría, etc.) |
| `ClasificacionInformacion` | `nvarchar(200)` | NULL | Clasificación de la información |

**Cardinalidades:**
- `Operaciones` (N) → (1) `Estatus`
- `Operaciones` (N) → (0..1) `Offices`
- `Operaciones` (N) → (0..1) `Areas` (x2: AreaId y OwnerAreaId)
- `Operaciones` (N) → (0..1) `Alojamientos`
- `Operaciones` (N) → (0..1) `Environments`
- `Operaciones` (N) → (0..1) `Criticalities`
- `Operaciones` (N) → (0..1) `Categories`
- `Operaciones` (N) → (0..1) `Vendors` (Manufacturer)
- `Operaciones` (N) → (0..1) `DeviceModels`

---

### 3.9 CuentasPrivilegiadas

Registro de cuentas con acceso privilegiado.

| Campo | Tipo | Restricciones | Descripción |
|---|---|---|---|
| `Id` | `uniqueidentifier` | PK, NOT NULL | Identificador único |
| `Nombre` | `nvarchar(300)` | NOT NULL | Nombre de la cuenta |
| `EstatusId` | `uniqueidentifier` | NOT NULL, FK → Estatus | Estado |
| `AreaId` | `uniqueidentifier` | NULL, FK → Areas | Área responsable |
| `Responsable` | `nvarchar(200)` | NULL | Responsable de la cuenta |
| `Origen` | `nvarchar(200)` | NULL | Origen de la cuenta (AD, Local, etc.) |
| `ServicioRelacionado` | `nvarchar(300)` | NULL | Servicio o aplicación relacionada |
| `TipoConfiguracionCambio` | `nvarchar(50)` | NULL | Tipo de configuración de cambio |
| `IntervaloCambioDias` | `int` | NULL | Intervalo de cambio de credenciales (días) |
| `GruposSeguridad` | `nvarchar(2000)` | NULL | Grupos AD (texto multivalor) |
| `Descripcion` | `nvarchar(500)` | NULL | Descripción de la cuenta |
| `AplicacionId` | `uniqueidentifier` | NULL, FK → Aplicaciones | Aplicación asociada |
| `CreatedAt` | `datetime2` | NOT NULL | Fecha/hora UTC de creación |

**Cardinalidades:**
- `CuentasPrivilegiadas` (N) → (1) `Estatus`
- `CuentasPrivilegiadas` (N) → (0..1) `Areas`
- `CuentasPrivilegiadas` (N) → (0..1) `Aplicaciones`

---

### 3.10 CuentasServicio

Registro de cuentas de servicio (técnicas, no interactivas). Estructura idéntica a `CuentasPrivilegiadas`.

| Campo | Tipo | Restricciones | Descripción |
|---|---|---|---|
| `Id` | `uniqueidentifier` | PK, NOT NULL | Identificador único |
| `Nombre` | `nvarchar(300)` | NOT NULL | Nombre de la cuenta |
| `EstatusId` | `uniqueidentifier` | NOT NULL, FK → Estatus | Estado |
| `AreaId` | `uniqueidentifier` | NULL, FK → Areas | Área responsable |
| `Responsable` | `nvarchar(200)` | NULL | Responsable de la cuenta |
| `Origen` | `nvarchar(200)` | NULL | Origen de la cuenta |
| `ServicioRelacionado` | `nvarchar(300)` | NULL | Servicio o aplicación relacionada |
| `TipoConfiguracionCambio` | `nvarchar(50)` | NULL | Tipo de configuración de cambio |
| `IntervaloCambioDias` | `int` | NULL | Intervalo de cambio de credenciales (días) |
| `GruposSeguridad` | `nvarchar(2000)` | NULL | Grupos AD |
| `Descripcion` | `nvarchar(500)` | NULL | Descripción |
| `AplicacionId` | `uniqueidentifier` | NULL, FK → Aplicaciones | Aplicación asociada |
| `CreatedAt` | `datetime2` | NOT NULL | Fecha/hora UTC de creación |

---

### 3.11 Aprobaciones

Registro de solicitudes de aprobación del flujo de inventario.

| Campo | Tipo | Restricciones | Descripción |
|---|---|---|---|
| `Id` | `uniqueidentifier` | PK, NOT NULL | Identificador único |
| `Modulo` | `nvarchar(50)` | NOT NULL | Módulo afectado (Aplicaciones, Operaciones, CuentasPrivilegiadas, CuentasServicio) |
| `EntidadId` | `uniqueidentifier` | NOT NULL | ID del registro que requiere aprobación |
| `Estado` | `nvarchar(50)` | NOT NULL | Estado de la solicitud ("Por aprobar", "Aprobado", "Rechazado") |
| `Comentario` | `nvarchar(MAX)` | NULL | Comentario general |
| `UsuarioId` | `uniqueidentifier` | NULL, FK → Users | Usuario que realizó la última acción |
| `Fecha` | `datetime2` | NOT NULL | Fecha/hora UTC de la última actualización |

---

### 3.12 AprobacionVotos

Votos individuales de aprobadores sobre una solicitud. Soporta el flujo multi-aprobador.

| Campo | Tipo | Restricciones | Descripción |
|---|---|---|---|
| `Id` | `uniqueidentifier` | PK, NOT NULL | Identificador único |
| `AprobacionId` | `uniqueidentifier` | NOT NULL, FK → Aprobaciones (Cascade) | Solicitud a la que pertenece el voto |
| `UserId` | `uniqueidentifier` | NOT NULL, FK → Users (NoAction) | Aprobador que emitió el voto |
| `Estado` | `nvarchar(20)` | NOT NULL | "Aprobado" o "Rechazado" |
| `Fecha` | `datetime2` | NOT NULL | Fecha/hora UTC del voto |
| `Comentario` | `nvarchar(MAX)` | NULL | Comentario del aprobador |

**Índice único:** `(AprobacionId, UserId)` — cada aprobador vota una sola vez por solicitud.

**Cardinalidades:**
- `AprobacionVotos` (N) → (1) `Aprobaciones`
- `AprobacionVotos` (N) → (1) `Users`

---

### 3.13 ApprovalRequests

Solicitudes de aprobación del subsistema alternativo (workflow formal por pasos).

| Campo | Tipo | Restricciones | Descripción |
|---|---|---|---|
| `Id` | `uniqueidentifier` | PK, NOT NULL | Identificador único |
| `EntityType` | `nvarchar(50)` | NOT NULL | Tipo de entidad |
| `EntityId` | `nvarchar(50)` | NOT NULL | ID de la entidad (string) |
| `AreaId` | `uniqueidentifier` | NULL | Área relacionada |
| `Status` | `nvarchar(20)` | NOT NULL | "Pending", "Approved", "Rejected" |
| `SubmittedByUserId` | `uniqueidentifier` | NULL, FK → Users (NoAction) | Usuario solicitante |
| `SubmittedAt` | `datetime2` | NOT NULL | Fecha de envío |
| `CurrentStep` | `int` | NOT NULL, DEFAULT 1 | Paso actual del flujo |
| `Summary` | `nvarchar(500)` | NULL | Resumen de la solicitud |

---

### 3.14 ApprovalDecisions

Decisiones individuales sobre `ApprovalRequests`.

| Campo | Tipo | Restricciones | Descripción |
|---|---|---|---|
| `Id` | `uniqueidentifier` | PK, NOT NULL | Identificador único |
| `ApprovalRequestId` | `uniqueidentifier` | NOT NULL, FK → ApprovalRequests (Cascade) | Solicitud relacionada |
| `Decision` | `nvarchar(20)` | NOT NULL | "Approved" o "Rejected" |
| `Comment` | `nvarchar(MAX)` | NOT NULL | Comentario obligatorio |
| `DecidedByUserId` | `uniqueidentifier` | NULL, FK → Users (NoAction) | Usuario que decidió |
| `DecidedAt` | `datetime2` | NOT NULL | Fecha/hora UTC de la decisión |

---

### 3.15 AprobacionPermisos

Configuración de usuarios autorizados a aprobar en cada módulo.

| Campo | Tipo | Restricciones | Descripción |
|---|---|---|---|
| `Id` | `uniqueidentifier` | PK, NOT NULL | Identificador único |
| `UserId` | `uniqueidentifier` | NOT NULL, FK → Users (Cascade) | Usuario aprobador |
| `Modulo` | `nvarchar(50)` | NOT NULL | Módulo (Aplicaciones, Operaciones, Telecomunicaciones, Cuentas) |

**Índice único:** `(UserId, Modulo)` — un usuario tiene un permiso de aprobación por módulo.

---

### 3.16 AuditLog

Registro simple de auditoría para todas las acciones del sistema.

| Campo | Tipo | Restricciones | Descripción |
|---|---|---|---|
| `Id` | `uniqueidentifier` | PK, NOT NULL | Identificador único |
| `Tabla` | `nvarchar(100)` | NOT NULL | Tabla afectada (ej: "Aplicaciones") |
| `EntidadId` | `uniqueidentifier` | NOT NULL | ID de la entidad afectada |
| `Accion` | `nvarchar(50)` | NOT NULL | Acción realizada (Crear, Editar, Aprobar, etc.) |
| `UsuarioId` | `uniqueidentifier` | NULL | ID del usuario que realizó la acción |
| `Fecha` | `datetime2` | NOT NULL | Fecha/hora UTC de la acción |
| `Detalle` | `nvarchar(MAX)` | NULL | Detalle adicional (texto libre) |

---

### 3.17 AuditEvents

Registro estructurado de eventos de auditoría con snapshot de estado.

| Campo | Tipo | Restricciones | Descripción |
|---|---|---|---|
| `Id` | `uniqueidentifier` | PK, NOT NULL | Identificador único |
| `EntityType` | `nvarchar(50)` | NOT NULL | Tipo de entidad |
| `EntityId` | `nvarchar(50)` | NOT NULL | ID de la entidad (string) |
| `Action` | `nvarchar(50)` | NOT NULL | Acción realizada |
| `PerformedByUserId` | `uniqueidentifier` | NULL | Usuario que realizó la acción |
| `PerformedAt` | `datetime2` | NOT NULL | Fecha/hora UTC |
| `BeforeJson` | `nvarchar(MAX)` | NULL | Estado de la entidad antes del cambio (JSON) |
| `AfterJson` | `nvarchar(MAX)` | NULL | Estado de la entidad después del cambio (JSON) |
| `Comment` | `nvarchar(500)` | NULL | Comentario del evento |
| `CorrelationId` | `nvarchar(50)` | NULL | ID de correlación de la solicitud HTTP |

---

### 3.18 EmailOutbox

Cola de correos electrónicos salientes (patrón Outbox).

| Campo | Tipo | Restricciones | Descripción |
|---|---|---|---|
| `Id` | `uniqueidentifier` | PK, NOT NULL | Identificador único |
| `To` | `nvarchar(500)` | NOT NULL | Destinatario(s) |
| `Cc` | `nvarchar(500)` | NULL | Destinatario(s) en copia |
| `Subject` | `nvarchar(300)` | NOT NULL | Asunto del correo |
| `BodyHtml` | `nvarchar(MAX)` | NULL | Cuerpo HTML |
| `BodyText` | `nvarchar(MAX)` | NULL | Cuerpo texto plano |
| `CreatedAt` | `datetime2` | NOT NULL | Fecha/hora UTC de creación |
| `SentAt` | `datetime2` | NULL | Fecha/hora UTC de envío exitoso |
| `Status` | `nvarchar(20)` | NOT NULL, DEFAULT "Pending" | "Pending", "Sent", "Failed" |
| `Error` | `nvarchar(1000)` | NULL | Mensaje de error del último intento |
| `RetryCount` | `int` | NOT NULL, DEFAULT 0 | Número de reintentos |

---

### 3.19 Offices (Oficinas)

| Campo | Tipo | Restricciones | Descripción |
|---|---|---|---|
| `Id` | `uniqueidentifier` | PK, NOT NULL | Identificador único |
| `Name` | `nvarchar(150)` | NOT NULL | Nombre de la oficina |

---

### 3.20 Areas

| Campo | Tipo | Restricciones | Descripción |
|---|---|---|---|
| `Id` | `uniqueidentifier` | PK, NOT NULL | Identificador único |
| `Name` | `nvarchar(100)` | NOT NULL | Nombre del área |

---

### 3.21 Alojamientos

Tipos de alojamiento (On-Premise, Cloud, Híbrido, etc.).

| Campo | Tipo | Restricciones | Descripción |
|---|---|---|---|
| `Id` | `uniqueidentifier` | PK, NOT NULL | Identificador único |
| `Nombre` | `nvarchar(150)` | NOT NULL | Nombre del tipo de alojamiento |

---

### 3.22 Environments (Ambientes)

| Campo | Tipo | Restricciones | Descripción |
|---|---|---|---|
| `Id` | `uniqueidentifier` | PK, NOT NULL | Identificador único |
| `Name` | `nvarchar(100)` | NOT NULL | Nombre del ambiente (Producción, Desarrollo, QA, etc.) |

---

### 3.23 Criticalities (Criticidades)

| Campo | Tipo | Restricciones | Descripción |
|---|---|---|---|
| `Id` | `uniqueidentifier` | PK, NOT NULL | Identificador único |
| `Name` | `nvarchar(100)` | NOT NULL | Nombre de la criticidad (Alta, Media, Baja) |

---

### 3.24 Categories (Categorías)

| Campo | Tipo | Restricciones | Descripción |
|---|---|---|---|
| `Id` | `uniqueidentifier` | PK, NOT NULL | Identificador único |
| `Name` | `nvarchar(100)` | NOT NULL | Nombre de la categoría |

---

### 3.25 Vendors (Fabricantes)

| Campo | Tipo | Restricciones | Descripción |
|---|---|---|---|
| `Id` | `uniqueidentifier` | PK, NOT NULL | Identificador único |
| `Name` | `nvarchar(150)` | NOT NULL | Nombre del fabricante |

**Cardinalidad:** `Vendors` (1) → (N) `DeviceModels`

---

### 3.26 DeviceModels (Modelos de Dispositivo)

| Campo | Tipo | Restricciones | Descripción |
|---|---|---|---|
| `Id` | `uniqueidentifier` | PK, NOT NULL | Identificador único |
| `ManufacturerId` | `uniqueidentifier` | NOT NULL, FK → Vendors (Restrict) | Fabricante |
| `Name` | `nvarchar(150)` | NOT NULL | Nombre del modelo |

---

### 3.27 CatalogItems (Ítems de Catálogo Genérico)

Catálogo polimórfico para valores que se agregan desde formularios.

| Campo | Tipo | Restricciones | Descripción |
|---|---|---|---|
| `Id` | `uniqueidentifier` | PK, NOT NULL | Identificador único |
| `Kind` | `nvarchar(80)` | NOT NULL | Tipo: "TipoDispositivo", "Funcion", "TipoInfraestructura", "SistemaOperativo" |
| `Name` | `nvarchar(200)` | NOT NULL | Valor del ítem |

---

### 3.28 Assets (Activos — modelo alternativo)

Entidad de activo con seguimiento de creación/actualización y estado de aprobación. Coexiste con `Operaciones` durante la transición del modelo de datos.

| Campo | Tipo | Restricciones | Descripción |
|---|---|---|---|
| `Id` | `uniqueidentifier` | PK, NOT NULL | Identificador único |
| `OfficeId` | `uniqueidentifier` | NOT NULL, FK → Offices (Restrict) | Oficina |
| `AreaId` | `uniqueidentifier` | NOT NULL, FK → Areas (Restrict) | Área |
| `DeviceType` | `nvarchar(100)` | NULL | Tipo de dispositivo |
| `Hostname` | `nvarchar(200)` | NULL | Hostname |
| `OperationEnvironmentId` | `uniqueidentifier` | NULL, FK → Environments (NoAction) | Entorno de operación |
| `OwnerAreaId` | `uniqueidentifier` | NULL, FK → Areas (NoAction) | Área propietaria |
| `CriticalityId` | `uniqueidentifier` | NULL, FK → Criticalities (NoAction) | Criticidad |
| `EnvironmentId` | `uniqueidentifier` | NULL, FK → Environments (NoAction) | Ambiente |
| `CategoryId` | `uniqueidentifier` | NULL, FK → Categories (NoAction) | Categoría |
| `ManufacturerId` | `uniqueidentifier` | NULL, FK → Vendors (NoAction) | Fabricante |
| `DeviceModelId` | `uniqueidentifier` | NULL, FK → DeviceModels (NoAction) | Modelo |
| `StatusId` | `uniqueidentifier` | NOT NULL, FK → Estatus (Restrict) | Estado |
| `CreatedAt` | `datetime2` | NOT NULL | Fecha/hora UTC de creación |
| `CreatedBy` | `uniqueidentifier` | NULL | ID de usuario creador |
| `UpdatedAt` | `datetime2` | NULL | Fecha/hora UTC de última actualización |
| `UpdatedBy` | `uniqueidentifier` | NULL | ID de usuario que actualizó |
| `ApprovalStatus` | `nvarchar(20)` | NOT NULL, DEFAULT "Draft" | Estado de aprobación del activo |

---

### 3.29 ManagedAccounts (Cuentas Administradas — modelo alternativo)

| Campo | Tipo | Restricciones | Descripción |
|---|---|---|---|
| `Id` | `uniqueidentifier` | PK, NOT NULL | Identificador único |
| `AreaId` | `uniqueidentifier` | NULL, FK → Areas (NoAction) | Área |
| `Responsible` | `nvarchar(200)` | NULL | Responsable |
| `AccountName` | `nvarchar(200)` | NOT NULL | Nombre de la cuenta |
| `AccountType` | `int` | NOT NULL | Tipo: 1=Privilegiada, 2=Servicio |
| `Origin` | `nvarchar(100)` | NULL | Origen |
| `RelatedService` | `nvarchar(200)` | NULL | Servicio relacionado |
| `ChangeConfigType` | `nvarchar(100)` | NULL | Tipo de configuración de cambio |
| `ChangeIntervalDays` | `int` | NULL | Intervalo de cambio (días) |
| `EstatusId` | `uniqueidentifier` | NULL, FK → Estatus (NoAction) | Estado |
| `CreatedAt` | `datetime2` | NOT NULL | Fecha/hora UTC de creación |
| `UpdatedAt` | `datetime2` | NULL | Fecha/hora UTC de actualización |

---

### 3.30 ManagedAccountSecurityGroups

Grupos de seguridad AD asociados a una cuenta administrada.

| Campo | Tipo | Restricciones | Descripción |
|---|---|---|---|
| `Id` | `uniqueidentifier` | PK, NOT NULL | Identificador único |
| `ManagedAccountId` | `uniqueidentifier` | NOT NULL, FK → ManagedAccounts (Cascade) | Cuenta administrada |
| `GroupName` | `nvarchar(MAX)` | NOT NULL | Nombre del grupo AD |

---

## 4. Resumen de Cardinalidades Clave

| Relación | Tipo | Descripción |
|---|---|---|
| Users — UserRole — Roles | N:M | Un usuario puede tener múltiples roles |
| Roles — RolePermission — Permissions | N:M | Un rol puede tener múltiples permisos |
| Users — AprobacionPermisos | 1:N | Un usuario puede ser aprobador de múltiples módulos |
| Aprobaciones — AprobacionVotos | 1:N | Una solicitud recibe múltiples votos |
| ApprovalRequests — ApprovalDecisions | 1:N | Una solicitud tiene múltiples decisiones |
| Aplicaciones — Estatus | N:1 | Muchas aplicaciones comparten un estatus |
| Operaciones — Offices | N:1 | Muchos activos en una oficina |
| CuentasPrivilegiadas — Aplicaciones | N:1 | Muchas cuentas asociadas a una aplicación |
| Vendors — DeviceModels | 1:N | Un fabricante tiene muchos modelos |
| ManagedAccounts — ManagedAccountSecurityGroups | 1:N | Una cuenta tiene muchos grupos |

---

## 5. Nota sobre RTO y RPO

> **Cambio reciente (esta versión):** Los campos `RTO` (Recovery Time Objective) y `RPO` (Recovery Point Objective) son ahora campos **independientes** en las tablas `Aplicaciones` y `Operaciones`. En versiones anteriores del esquema existía un único campo combinado `RPORTO`. El script `EnsureAplicacionesOptionalColumnsAsync` detecta la presencia de la columna `RPORTO` y migra su contenido a ambos campos por separado antes de que la columna antigua sea ignorada por el modelo.
>
> Esta separación cumple con el estándar **ISO-067-GCS** de KPMG, que requiere documentar RTO y RPO como métricas distintas para cada activo tecnológico.
