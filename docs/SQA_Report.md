# Reporte de Aseguramiento de Calidad de Software (SQA)

**Sistema:** IITS — Inventario de Infraestructura Tecnológica y Seguridad  
**Versión:** 1.0  
**Fecha:** 2026-04-01  
**Clasificación:** Interno KPMG Venezuela  

---

## 1. Resumen Ejecutivo

Este reporte documenta el estado de calidad del código base de IITS, los hallazgos identificados durante la revisión, las correcciones aplicadas en el ciclo actual y las recomendaciones para mejorar la cobertura de pruebas en iteraciones futuras.

El sistema carece de pruebas automatizadas formales (unitarias, de integración o E2E) en esta versión. La validación se ha realizado mediante revisión de código estático, análisis de flujos lógicos y pruebas manuales funcionales. Se recomienda implementar una suite de pruebas automatizadas antes de la siguiente versión mayor.

---

## 2. Alcance de la Revisión

| Artefacto revisado | Método |
|---|---|
| `Program.cs` | Revisión estática completa |
| `Entities/*.cs` | Revisión de modelos y anotaciones |
| `Data/AppDbContext.cs` | Revisión de relaciones y configuraciones EF |
| `Data/DbSeed.cs` | Revisión de idempotencia y manejo de errores |
| `Services/*.cs` | Revisión de lógica de negocio |
| `Middleware/*.cs` | Revisión de cadena de autenticación |
| `appsettings.*.json` | Revisión de configuración y secretos |
| `web.config` | Revisión de configuración IIS |
| `IITS.csproj` | Revisión de dependencias |

---

## 3. Análisis de Cobertura de Pruebas

### 3.1 Cobertura Actual

| Tipo de prueba | Estado | Cobertura estimada |
|---|---|---|
| Pruebas unitarias | No implementadas | 0% |
| Pruebas de integración | No implementadas | 0% |
| Pruebas E2E (Playwright/Selenium) | No implementadas | 0% |
| Pruebas manuales funcionales | Realizadas informalmente | ~60% de flujos principales |
| Revisión de código estático | Realizada | 100% de archivos clave |

### 3.2 Flujos Cubiertos por Pruebas Manuales

- Arranque de la aplicación y aplicación de migraciones.
- Autenticación Windows en entorno de desarrollo con `Auth:Mode=Dev`.
- Creación de usuario SuperAdmin en primer arranque.
- Alta de aplicaciones y visualización en lista.
- Alta de activos de operaciones con catálogos relacionados.
- Flujo de aprobación: creación de solicitud, votación, aprobación final.
- Generación de exportación Excel para módulo Aplicaciones.
- Generación de PDF de auditoría.
- Verificación de rate limiting en endpoints de exportación (respuesta HTTP 429).

### 3.3 Flujos NO Cubiertos por Pruebas Automáticas

- Validación de campos requeridos en todos los formularios.
- Comportamiento ante fallo del servidor SMTP (retry en `EmailOutboxHostedService`).
- Concurrencia en el flujo de votos (dos aprobadores votando simultáneamente).
- Pruebas de rendimiento bajo carga (más de 100 usuarios concurrentes).
- Comportamiento con base de datos vacía pero sin migraciones aplicadas.
- Escenarios de sesión expirada durante operación activa en Blazor.
- Validación del límite de rate limiting bajo carga sostenida.
- Pruebas de exportación con conjuntos de datos grandes (>2000 registros).

---

## 4. Hallazgos de Calidad del Código

### HQ-01: SQL Raw en DbSeed para Compatibilidad (Bajo)

**Archivo:** `IITS/Data/DbSeed.cs`  
**Descripción:** Los métodos `EnsureAplicacionesOptionalColumnsAsync`, `EnsureOperacionesOptionalColumnsAsync` y `EnsureCuentasOptionalColumnsAsync` ejecutan SQL raw (`ALTER TABLE`) para agregar columnas que deberían gestionarse vía migración EF. Este patrón existe por compatibilidad hacia atrás con bases de datos que tienen el esquema inicial aplicado pero no las migraciones posteriores.  
**Riesgo:** Si se ejecuta en un entorno donde el modelo de EF no coincide con el esquema real, puede producir inconsistencias difíciles de diagnosticar.  
**Recomendación:** Una vez que todos los ambientes de producción hayan aplicado todas las migraciones, estos métodos pueden eliminarse. En su lugar, usar exclusivamente migraciones EF.

---

### HQ-02: Catch Silencioso en `EnsureColumnsAsync` (Bajo)

**Archivo:** `IITS/Data/DbSeed.cs`  
**Descripción:** Los bloques `catch` en los métodos `Ensure*ColumnsAsync` capturan todas las excepciones y las ignoran (`// Ignorar si ya existen o no se puede alterar`). Esto puede ocultar errores de permisos o problemas de esquema.  
**Recomendación:** Loguear la excepción (al menos con `_logger.LogWarning`) antes de ignorarla, para facilitar el diagnóstico en producción.

---

### HQ-03: Ausencia de Validación en Formularios Blazor (Medio)

**Archivo:** `IITS/Pages/*.razor`  
**Descripción:** No se identificó el uso consistente de `<EditForm>` con `<DataAnnotationsValidator>` y `<ValidationSummary>` en todos los formularios de módulos. Las anotaciones de validación en las entidades (como `[Required]`, `[MaxLength]`) no se aplican automáticamente en la UI si los componentes Blazor no usan el sistema de validación de formularios.  
**Riesgo:** Datos incompletos o inválidos pueden llegar a la base de datos sin validación del lado del servidor.  
**Recomendación:** Verificar que todos los formularios de alta y edición implementen validación Blazor con `DataAnnotationsValidator`.

---

### HQ-04: CurrentUserService.CanApproveModuloAsync con Bypass de Admin (Resuelto)

**Archivo:** `IITS/Services/CurrentUserService.cs`  
**Descripción original:** El método `CanApproveModuloAsync` retornaba `true` automáticamente si `IsAdmin` era verdadero, permitiendo que administradores bypaseen el flujo de aprobación.  
**Estado:** **RESUELTO** en esta versión. El método fue corregido y ahora los administradores deben tener un registro explícito en `AprobacionPermisos` para poder votar, igual que cualquier otro aprobador.

---

### HQ-05: Cookie con Solo `ClaimTypes.Name` (Diseño Correcto)

**Archivo:** `IITS/Middleware/SessionCookieSignInMiddleware.cs`  
**Descripción:** La cookie de sesión almacena únicamente el claim `ClaimTypes.Name`. A primera vista esto puede parecer una limitación, pero es una decisión de diseño deliberada para evitar el error HTTP 400 "Request Too Long" por headers excesivos cuando hay muchos claims de roles y permisos.  
**Evaluación:** Correcta. Los roles y permisos se recargan desde BD en cada request vía `IITSClaimsTransformation`. Esto introduce una consulta a BD por request, pero garantiza que los cambios de permisos surtan efecto inmediatamente.  
**Recomendación:** Considerar caché en memoria con TTL corto (30 segundos) para la transformación de claims si el rendimiento de la BD se convierte en un cuello de botella.

---

### HQ-06: Falta de `TrustServerCertificate=False` en Producción (Medio — Seguridad)

**Archivo:** `IITS/web.config` (variable de entorno `ConnectionStrings__IITS`)  
**Descripción:** La cadena de conexión de producción en `web.config` tiene `TrustServerCertificate=True`. Esto omite la validación del certificado TLS de SQL Server, creando una vulnerabilidad potencial a ataques man-in-the-middle en la comunicación con la base de datos.  
**Estado:** Identificado como brecha. Pendiente de corrección.  
**Recomendación:** Instalar un certificado válido en SQL Server y cambiar a `TrustServerCertificate=False` en la cadena de conexión de producción.

---

### HQ-07: `appsettings.Production.json` en `.gitignore` (Correcto)

**Descripción:** El archivo `appsettings.Production.json` es una plantilla vacía y está incluido en `.gitignore`. Los valores reales se inyectan como variables de entorno en `web.config` en el servidor.  
**Evaluación:** Correcto y compliant con el estándar ISO-032-EDR y las prácticas de seguridad de KPMG.

---

### HQ-08: Comando `reset-migrations` Elimina la Base de Datos (Alto — Operacional)

**Archivo:** `IITS/Program.cs`  
**Descripción:** El comando `dotnet run reset-migrations` ejecuta `db.Database.EnsureDeletedAsync()` seguido de `db.Database.MigrateAsync()`. Este comando destruye completamente la base de datos antes de recrearla.  
**Riesgo:** Ejecución accidental en producción resulta en pérdida total de datos.  
**Recomendación:** Agregar confirmación explícita del nombre de la base de datos o variable de entorno adicional (`RESET_CONFIRMED=true`) antes de ejecutar en entornos no-Development.

---

### HQ-09: No hay Logging Estructurado (Medio)

**Descripción:** La aplicación usa el sistema de logging predeterminado de ASP.NET Core (`ILogger<T>`). No hay implementación de Serilog, ni CorrelationId sistemático en los logs, ni salida a archivo estructurada (más allá de los stdout de IIS).  
**Recomendación:** Implementar Serilog con sink a archivo rotativo diario y output a `Logs/iits-{date}.log`. Agregar `CorrelationId` middleware para trazabilidad por request.

---

### HQ-10: `QueueLimit = 0` en Rate Limiter (Diseño Correcto)

**Archivo:** `IITS/Program.cs`  
**Descripción:** El rate limiter de exports tiene `QueueLimit = 0`, lo que significa que las solicitudes que excedan el límite son rechazadas inmediatamente con HTTP 429, sin hacer cola.  
**Evaluación:** Correcto para este caso de uso. Las exportaciones son operaciones pesadas (CPU/memoria); ponerlas en cola introduciría latencia impredecible y consumo de memoria.

---

## 5. Correcciones Aplicadas en Esta Versión

| ID | Descripción | Impacto |
|---|---|---|
| FIX-01 | Separación de campos RPORTO en RTO y RPO (con migración de datos existentes) | ISO-067-GCS compliance |
| FIX-02 | Eliminación del bypass de administrador en `CanApproveModuloAsync` | Seguridad / Integridad del flujo de aprobación |
| FIX-03 | Implementación de rate limiting en `/api/export/*` y `/api/auditoria/pdf` | ISO-082-API compliance |
| FIX-04 | Vaciado de credenciales en `appsettings.Production.json` + adición a `.gitignore` | Seguridad / Gestión de secretos |
| FIX-05 | Corrección del flujo multi-aprobador: todos los aprobadores deben votar "Aprobado" | Integridad del negocio |

---

## 6. Recomendaciones para Pruebas Futuras

### 6.1 Pruebas Unitarias (Prioridad: Alta)

Implementar con xUnit + Moq. Priorizar:

```
Tests/Unit/
├── Services/
│   ├── AprobacionServiceTests.cs
│   │   ├── MarcarAprobadoAsync_TodosAprueban_CambiaEstadoAprobado()
│   │   ├── MarcarAprobadoAsync_UnRechaza_CambiaEstadoRechazado()
│   │   ├── MarcarAprobadoAsync_VotoDoble_NoRegistra()
│   │   └── MarcarAprobadoAsync_SinAprobadores_AprobacionAutomatica()
│   ├── AuditLogServiceTests.cs
│   │   ├── RegistrarAsync_CreaRegistroEnBD()
│   │   └── GetLogsAsync_FiltradoPorTabla()
│   └── CurrentUserServiceTests.cs
│       ├── IsAdmin_ConRolSuperAdmin_RetornaTrue()
│       ├── HasPermission_ConPermisoCorrecto_RetornaTrue()
│       └── CanApproveModuloAsync_SinPermisoEnTabla_RetornaFalse()
```

### 6.2 Pruebas de Integración (Prioridad: Alta)

Usar `WebApplicationFactory<Program>` con base de datos SQL Server LocalDB o InMemory para EF Core.

```
Tests/Integration/
├── Auth/
│   ├── WindowsAuthFlow_UsuarioNoEnBD_AccesoDenegado()
│   └── CookieSession_Expirada_RedireccionA401()
├── Export/
│   ├── ExportEndpoint_LimiteRateExcedido_Retorna429()
│   └── ExportAplicaciones_FiltroEstatus_FiltraCorrectamente()
└── Aprobaciones/
    ├── FlujoCompleto_CreadoPorOperador_EnviaCorrecto()
    └── FlujoCompleto_TodosAprueban_ActualizaEstado()
```

### 6.3 Pruebas E2E (Prioridad: Media)

Usar Playwright con una cuenta de prueba en el dominio AD de desarrollo.

```
Tests/E2E/
├── Aplicaciones/
│   ├── CrearAplicacion_CamposRequeridos_GuardaCorrectamente()
│   ├── EditarAplicacion_CambiaRTO_ActualizaRegistro()
│   └── ExportarAplicaciones_Excel_DescargaArchivo()
├── Aprobaciones/
│   └── FlujoCompleto_OperadorCrea_AprobadorVota_AprobacionFinal()
└── Admin/
    ├── AsignarRol_UsuarioNuevo_TienePermiso()
    └── ConfigurarAprobador_NuevoModulo_AparecePendiente()
```

### 6.4 Pruebas de Rendimiento (Prioridad: Baja)

Usar k6 o NBomber:

- Simular 50 usuarios concurrentes navegando el inventario.
- Simular 10 usuarios concurrentes exportando Excel.
- Verificar que el rate limiter rechaza correctamente el exceso.

### 6.5 Pruebas de Seguridad (Prioridad: Alta)

- Verificar que usuarios sin sesión no pueden acceder a ninguna página (fallback policy).
- Verificar que un usuario con solo `Perm.Inventory.View` no puede editar ni exportar.
- Verificar que el rate limiter no puede bypassearse con diferentes IPs o user-agents.
- Verificar que los logs de auditoría son de solo lectura desde la UI.
- Penetration test básico: SQL injection en campos de texto de formularios (EF Core parameterized queries protege, pero verificar SQL raw en `DbSeed`).

---

## 7. Métricas de Deuda Técnica

| Categoría | Hallazgos | Resueltos | Pendientes |
|---|---|---|---|
| Seguridad crítica | 2 | 2 (FIX-03, FIX-04) | 0 |
| Seguridad media | 2 | 1 (FIX-02) | 1 (HQ-06) |
| Calidad de código | 5 | 1 (FIX-01) | 4 (HQ-01, HQ-02, HQ-03, HQ-09) |
| Operacional | 1 | 0 | 1 (HQ-08) |
| **Total** | **10** | **4** | **6** |

---

## 8. Conclusión

IITS es un sistema funcionalmente correcto para su propósito de inventario tecnológico interno. Los riesgos de seguridad más críticos han sido mitigados en esta versión (credenciales, bypass de aprobación, rate limiting, separación RTO/RPO). 

La ausencia de pruebas automatizadas es la principal deuda técnica. Se recomienda priorizar la implementación de pruebas unitarias para los servicios `AprobacionService` y `CurrentUserService` en la siguiente iteración, dado que contienen la lógica de negocio más crítica del sistema.
