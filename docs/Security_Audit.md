# Reporte de Auditoría de Seguridad

**Sistema:** IITS — Inventario de Infraestructura Tecnológica y Seguridad  
**Versión:** 1.0  
**Fecha:** 2026-04-01  
**Clasificación:** Confidencial — Interno KPMG Venezuela  
**Marco de referencia:** Estándares ISO internos KPMG  

---

## 1. Resumen Ejecutivo

Se realizó una revisión de seguridad del código base, la configuración y la arquitectura de IITS frente a los 25 estándares ISO internos de KPMG aplicables a sistemas web internos. Se identificaron hallazgos críticos que han sido resueltos en la versión actual, así como brechas residuales que requieren atención en los próximos ciclos.

**Estado general:** Parcialmente Compliant  
**Hallazgos críticos resueltos:** 4  
**Brechas activas de alta prioridad:** 2  
**Brechas activas de prioridad media:** 4  

---

## 2. Mapeo de Estándares ISO KPMG

### ISO-057-ESC — Política de Contraseñas

| Campo | Valor |
|---|---|
| **Estado** | Parcialmente Compliant |
| **Hallazgo** | IITS no gestiona contraseñas propias. La autenticación está completamente delegada a Active Directory mediante Windows Authentication (Negotiate/Kerberos). Las políticas de complejidad, expiración y bloqueo de contraseñas son responsabilidad del dominio AD de KPMG. |
| **Controles existentes** | La sesión de IITS expira según `Auth:SessionTimeoutMinutes` (configurable entre 5 y 480 minutos; por defecto 30). La cookie de sesión es `HttpOnly`. |
| **Brecha** | Si en el futuro se añaden usuarios locales (no-AD), IITS no tiene mecanismo propio de política de contraseñas. |
| **Recomendación** | Mantener la restricción de solo usuarios de dominio AD. Documentar explícitamente que no se admiten usuarios locales. |

---

### ISO-040-ADU — Autenticación Multifactor (MFA)

| Campo | Valor |
|---|---|
| **Estado** | Brecha parcial |
| **Hallazgo** | IITS utiliza exclusivamente Windows Negotiate (Kerberos/NTLM). No implementa MFA propio. En escenarios donde los usuarios acceden desde equipos con SSO de dominio (la mayoría de los casos en intranet KPMG), el MFA corporativo de KPMG (si está habilitado en AD) protege el acceso. |
| **Brecha** | No hay MFA en IITS mismo. Si el dominio AD no tiene MFA habilitado, el acceso a IITS queda protegido solo por credenciales de dominio. |
| **Recomendación** | Coordinar con el equipo de Infraestructura de KPMG para verificar que el dominio AD tiene MFA habilitado para acceso a aplicaciones internas. Si no, considerar implementar Azure AD Conditional Access o similar. |

---

### ISO-082-API — Protección de Interfaces y APIs

| Campo | Valor |
|---|---|
| **Estado** | Compliant |
| **Hallazgo** | IITS expone endpoints de API para exportación (`/api/export/{modulo}/{formato}`) y generación de PDF de auditoría (`/api/auditoria/pdf`). Estos endpoints están protegidos con: |
| | - Autenticación requerida (FallbackPolicy: `RequireAuthenticatedUser`) |
| | - Rate limiting: Fixed Window, 10 solicitudes/minuto, QueueLimit=0 (HTTP 429 inmediato al exceder) |
| | - Política "export" aplicada explícitamente vía `.RequireRateLimiting("export")` |
| **Implementación** | `builder.Services.AddRateLimiter` con política `"export"`, aplicada en `app.MapGet("/api/export/...")` y `app.MapGet("/api/auditoria/pdf")`. |
| **Recomendación** | Considerar agregar logging específico cuando se activa el rate limiter (HTTP 429) para detectar intentos de abuso. |

---

### ISO-067-GCS — Gestión de Continuidad del Servicio (RTO/RPO)

| Campo | Valor |
|---|---|
| **Estado** | Compliant (resuelto en esta versión) |
| **Hallazgo previo** | Existía un campo combinado `RPORTO` que mezclaba Recovery Time Objective y Recovery Point Objective en un solo texto libre. |
| **Corrección aplicada** | Los campos `RTO` y `RPO` son ahora columnas independientes en las tablas `Aplicaciones` y `Operaciones`. El script `EnsureAplicacionesOptionalColumnsAsync` migra automáticamente el valor de `RPORTO` a ambos campos para bases de datos existentes. |
| **Evidencia** | `Aplicacion.RTO: nvarchar(100)`, `Aplicacion.RPO: nvarchar(100)` (campos separados en el modelo EF). |
| **Brecha residual** | No hay validación de formato ni rangos para RTO/RPO; se aceptan valores de texto libre. Considerar listas desplegables con valores estandarizados (ej: "4 horas", "8 horas", "24 horas"). |

---

### ISO-032-EDR — Cifrado de Datos en Reposo

| Campo | Valor |
|---|---|
| **Estado** | Parcialmente Compliant |
| **Hallazgo** | La cadena de conexión en `appsettings.json` (entorno de desarrollo) usa `Encrypt=Optional`, lo que es correcto para desarrollo local. En producción, la cadena de conexión inyectada vía variable de entorno tiene `TrustServerCertificate=True;MultipleActiveResultSets=True` pero no especifica `Encrypt=True`. |
| **Evidencia** | `web.config`: `TrustServerCertificate=True` implica que el tráfico puede estar cifrado pero sin validar el certificado del servidor. |
| **Brecha** | SQL Server debe tener habilitado TDE (Transparent Data Encryption) para cumplir con cifrado de datos en reposo. IITS no controla esto directamente, pero debe verificarse en el servidor de base de datos. |
| **Recomendación (Alta prioridad)** | Verificar que SQL Server tiene TDE habilitado en la instancia `VECCSAPP10,61057`. Agregar `Encrypt=True` explícito a la cadena de conexión de producción. |

---

### ISO-034-EDT — Cifrado de Datos en Tránsito

| Campo | Valor |
|---|---|
| **Estado** | Parcialmente Compliant |
| **Hallazgo** | La aplicación fuerza HTTPS en producción (`app.UseHsts()`, `app.UseHttpsRedirection()`). Sin embargo, la cadena de conexión a SQL Server tiene `TrustServerCertificate=True`, lo que omite la validación del certificado TLS en la comunicación app-BD. |
| **Brecha** | `TrustServerCertificate=True` en producción permite ataques man-in-the-middle entre la aplicación IIS y SQL Server si están en redes diferentes. |
| **Recomendación (Alta prioridad)** | Instalar un certificado válido en SQL Server, cambiar a `TrustServerCertificate=False` en producción y verificar que `Encrypt=True` está configurado. |

---

### ISO-033-ECP — Control de Cuentas Privilegiadas

| Campo | Valor |
|---|---|
| **Estado** | Parcialmente Compliant |
| **Hallazgo** | IITS tiene un módulo dedicado `CuentasPrivilegiadas` que registra cuentas con acceso elevado, incluyendo el campo `IntervaloCambioDias` para documentar la frecuencia de rotación de credenciales. |
| **Brecha identificada** | El sistema NO genera recordatorios automáticos cuando una cuenta privilegiada supera su intervalo de cambio configurado. No existe ningún mecanismo de alerta (email, notificación en dashboard) para cuentas vencidas. |
| **Impacto** | Cuentas privilegiadas pueden tener credenciales sin rotar por períodos superiores al definido sin que nadie sea notificado. |
| **Recomendación (Alta prioridad)** | Implementar un `BackgroundService` (similar a `EmailOutboxHostedService`) que consulte diariamente las cuentas privilegiadas cuyo `IntervaloCambioDias` haya vencido y envíe notificaciones al responsable y al equipo de seguridad. |

---

### ISO-010B-INC — Gestión de Incidentes y Trazabilidad

| Campo | Valor |
|---|---|
| **Estado** | Compliant |
| **Hallazgo** | IITS implementa dos niveles de auditoría: |
| | - `AuditLog`: Registro simple (tabla, entidad, acción, usuario, fecha, detalle). |
| | - `AuditEvent`: Registro estructurado con snapshot JSON antes/después, `CorrelationId` y timestamp UTC. |
| **Controles** | Toda creación, modificación y acción de aprobación se registra en `AuditLog`. Las acciones críticas se registran adicionalmente en `AuditEvent`. Los registros de auditoría no son modificables desde la UI. |
| **Recomendación** | Poblar el campo `CorrelationId` en `AuditEvent` desde un middleware de correlación de requests para facilitar la trazabilidad end-to-end de incidentes. |

---

### ISO-064-EAS — Control de Acceso Basado en Roles

| Campo | Valor |
|---|---|
| **Estado** | Compliant |
| **Hallazgo** | IITS implementa dos capas de control de acceso: |
| | - **Roles** (SuperAdmin, Administrador, Operador, Aprobador, Auditor, Usuario) |
| | - **Permisos granulares** (14 códigos de permiso mapeados a policies ASP.NET Core) |
| | - **Permisos de aprobación por módulo** (tabla `AprobacionPermisos`) |
| **Controles** | `FallbackPolicy = RequireAuthenticatedUser` garantiza que toda ruta requiere autenticación. Los permisos se verifican vía claims (`RequireClaim("Permission", code)`). |
| **Mejora reciente** | El bypass de administrador en `CanApproveModuloAsync` fue eliminado. Todos los usuarios, incluyendo administradores, deben tener registro en `AprobacionPermisos` para participar en el flujo de votación. |
| **Recomendación** | Documentar la matriz de roles vs. permisos en la documentación de onboarding para nuevos administradores del sistema. |

---

### ISO-061-VGS — Gestión de Vulnerabilidades

| Campo | Valor |
|---|---|
| **Estado** | Parcialmente Compliant |
| **Hallazgo** | No hay un proceso formal de escaneo de vulnerabilidades en las dependencias de NuGet. Las versiones usadas son relativamente recientes (EF Core 8.0.11, QuestPDF 2024.10.2). |
| **Recomendación** | Integrar `dotnet list package --vulnerable` en el pipeline de CI/CD. Revisar actualizaciones de dependencias semestralmente. |

---

### ISO-073-LOG — Registro y Monitoreo

| Campo | Valor |
|---|---|
| **Estado** | Parcialmente Compliant |
| **Hallazgo** | La aplicación usa el sistema de logging predeterminado de ASP.NET Core con salida a stdout (capturado por IIS en `.\logs\stdout`). Los logs de auditoría de negocio se persisten en BD (AuditLog, AuditEvent). |
| **Brecha** | No hay logging estructurado (Serilog/Seq), sin rotación automática de logs de aplicación, sin alertas sobre errores en los logs. |
| **Recomendación (Media)** | Implementar Serilog con sink a archivo rotativo (`RollingFile`) y, opcionalmente, sink a Seq para consultas estructuradas. |

---

### ISO-085-WEB — Seguridad de Aplicaciones Web

| Campo | Valor |
|---|---|
| **Estado** | Parcialmente Compliant |
| **Hallazgo** | Blazor Server mitiga varios vectores de ataque XSS por su modelo de renderizado en servidor. EF Core con LINQ parametriza todas las queries automáticamente, mitigando SQL injection. |
| **Controles existentes** | HTTPS forzado, cookies HttpOnly, autenticación requerida en todas las rutas. |
| **Brecha** | No hay encabezados de seguridad HTTP configurados (Content-Security-Policy, X-Frame-Options, X-Content-Type-Options). |
| **Recomendación (Media)** | Agregar middleware de encabezados de seguridad o configurarlos en IIS (`<customHeaders>` en `web.config`). |

---

### ISO-091-BCM — Gestión de Continuidad del Negocio

| Campo | Valor |
|---|---|
| **Estado** | N/A (IITS es un sistema de soporte, no de producción crítica) |
| **Hallazgo** | IITS es una herramienta interna de inventario. Su indisponibilidad no afecta directamente las operaciones de cliente. |
| **Recomendación** | Incluir IITS en el plan de respaldo estándar de aplicaciones internas de KPMG Venezuela (backup de BD SQL Server, snapshot del servidor IIS). |

---

### ISO-096-CLD — Seguridad en la Nube

| Campo | Valor |
|---|---|
| **Estado** | N/A |
| **Hallazgo** | IITS está alojado on-premise en `VECCSAPP10` (intranet de KPMG Venezuela). No hay componentes en nube pública. |

---

### ISO-025-ACP — Control de Acceso Físico

| Campo | Valor |
|---|---|
| **Estado** | N/A (responsabilidad de Infraestructura KPMG) |

---

### Estándares ISO Adicionales — Tabla de Estado

| Estándar | Nombre | Estado | Nota |
|---|---|---|---|
| ISO-001-POL | Política de Seguridad de la Información | N/A | Marco corporativo, no específico a IITS |
| ISO-005-ORI | Orientación de Seguridad | N/A | Marco corporativo |
| ISO-008-GAP | Gestión de Activos de Personal | N/A | RRHH, no aplicable a sistemas |
| ISO-012-SIS | Seguridad de Sistemas | Parcial | IITS contribuye con auditoría y RBAC |
| ISO-015-RED | Seguridad de Redes | N/A | Responsabilidad de Infraestructura |
| ISO-018-CIF | Criptografía | Parcial | Ver ISO-032-EDR y ISO-034-EDT |
| ISO-021-FIS | Seguridad Física | N/A | Responsabilidad de Infraestructura |
| ISO-028-PRV | Privacidad de Datos | Parcial | IITS almacena datos de empleados (nombre, email, CodSap); considerar política de retención |
| ISO-031-GRC | Gestión de Riesgos de Cumplimiento | Parcial | Este documento contribuye al cumplimiento |
| ISO-038-TER | Terminación Segura de Sesión | Compliant | Cookie con expiración configurada + sliding expiration |
| ISO-045-PAR | Parches y Actualizaciones | Parcial | Sin proceso formal de revisión de dependencias |
| ISO-052-IDS | Detección de Intrusos | N/A | Responsabilidad de Infraestructura |
| ISO-059-PRB | Gestión de Problemas | Parcial | AuditLog y startup_error.txt para diagnóstico |
| ISO-075-CHG | Gestión de Cambios | Parcial | Flujo de aprobación multi-aprobador implementado |
| ISO-080-WAF | Web Application Firewall | N/A | Responsabilidad de Infraestructura; IITS no implementa WAF propio |

---

## 3. Hallazgos Críticos — Detalle

### SC-CRIT-01: Credenciales en Repositorio (RESUELTO)

**Descripción:** Versiones anteriores de `appsettings.Production.json` podían contener la cadena de conexión completa, la contraseña SMTP y el usuario SuperAdmin en texto plano en el repositorio de código.

**Corrección aplicada:**
1. `appsettings.Production.json` fue vaciado (plantilla con campos en blanco).
2. Todos los valores sensibles se inyectan como variables de entorno en `web.config` (que está en `.gitignore`).
3. `appsettings.Production.json` fue añadido a `.gitignore`.

**Estado:** RESUELTO. Verificado en el repositorio actual: el archivo contiene solo valores vacíos.

---

### SC-CRIT-02: Bypass de Aprobación para Administradores (RESUELTO)

**Descripción:** El método `CurrentUserService.CanApproveModuloAsync` retornaba `true` si el usuario tenía el rol Administrador o SuperAdmin, permitiendo que administradores aprobasen sus propios cambios sin pasar por el flujo de votación formal.

**Corrección aplicada:** El método ya no tiene la verificación de `IsAdmin` como bypass. Para que un administrador pueda votar en una solicitud, debe tener un registro explícito en `AprobacionPermisos` para el módulo correspondiente, igual que cualquier otro aprobador.

**Estado:** RESUELTO. El flujo de aprobación es ahora consistente para todos los usuarios.

---

## 4. Recomendaciones por Prioridad

### Prioridad Crítica

Ningún hallazgo crítico activo. Todos los críticos identificados han sido resueltos en esta versión.

---

### Prioridad Alta

| ID | Estándar | Recomendación | Esfuerzo estimado |
|---|---|---|---|
| REC-A01 | ISO-034-EDT | Cambiar `TrustServerCertificate=True` a `False` en la cadena de conexión de producción. Instalar certificado válido en SQL Server. | Bajo (requiere certificado en BD) |
| REC-A02 | ISO-032-EDR | Verificar y habilitar TDE en SQL Server IITSN. Agregar `Encrypt=True` explícito. | Medio (requiere DBA) |
| REC-A03 | ISO-033-ECP | Implementar servicio de recordatorios automáticos para cuentas privilegiadas con `IntervaloCambioDias` vencido. | Medio (nuevo BackgroundService + email) |

---

### Prioridad Media

| ID | Estándar | Recomendación | Esfuerzo estimado |
|---|---|---|---|
| REC-M01 | ISO-073-LOG | Implementar Serilog con sink a archivo rotativo y/o Seq. Agregar CorrelationId middleware. | Medio |
| REC-M02 | ISO-085-WEB | Agregar encabezados de seguridad HTTP: `Content-Security-Policy`, `X-Frame-Options`, `X-Content-Type-Options`, `Referrer-Policy`. | Bajo |
| REC-M03 | ISO-040-ADU | Verificar con Infraestructura KPMG si el dominio AD tiene MFA habilitado para aplicaciones internas. | Bajo (coordinación) |
| REC-M04 | ISO-061-VGS | Agregar `dotnet list package --vulnerable` al pipeline de CI/CD o revisión manual semestral. | Bajo |

---

### Prioridad Baja

| ID | Estándar | Recomendación | Esfuerzo estimado |
|---|---|---|---|
| REC-B01 | ISO-028-PRV | Definir política de retención de datos en AuditLog y AuditEvent (p. ej., 2 años). Implementar archivado o eliminación automática. | Bajo (proceso + script) |
| REC-B02 | ISO-067-GCS | Estandarizar valores de RTO/RPO con listas desplegables (p. ej., "< 4 horas", "4-8 horas", "> 24 horas") en lugar de texto libre. | Bajo |
| REC-B03 | General | Documentar la matriz de roles vs. permisos para nuevos administradores. | Bajo (documentación) |
| REC-B04 | ISO-091-BCM | Incluir IITS en el plan de respaldo de aplicaciones internas (backup de BD + snapshot IIS). | Bajo (proceso) |

---

## 5. Conclusión

IITS ha madurado significativamente en seguridad en esta versión. Los hallazgos críticos (credenciales expuestas, bypass de aprobación) han sido resueltos. Los controles de acceso (RBAC + permisos), la pista de auditoría y el rate limiting en APIs están correctamente implementados y alineados con los estándares ISO aplicables de KPMG.

Las brechas activas más importantes son la validación del certificado TLS en la comunicación con SQL Server (ISO-034-EDT / ISO-032-EDR) y la ausencia de recordatorios automáticos para rotación de cuentas privilegiadas (ISO-033-ECP). Ambas deben atenderse en el próximo ciclo de mantenimiento.
