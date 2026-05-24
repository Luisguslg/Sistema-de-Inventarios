# Brief de Proyecto — IITS (Inventario de Infraestructura Tecnológica y Seguridad)

**Versión:** 1.0  
**Fecha:** 2026-04-01  
**Clasificación:** Interno KPMG Venezuela  

---

## 1. Propósito del Sistema

IITS (Inventario de Infraestructura Tecnológica y Seguridad) es una aplicación web interna desarrollada para KPMG Venezuela con el objetivo de centralizar, gestionar y auditar el inventario de activos tecnológicos de la firma. El sistema reemplaza registros dispersos en hojas de cálculo (como el "Catálogo de Aplicaciones.xlsx") y establece un repositorio único y trazable con flujo de aprobación multi-aprobador, trazabilidad de auditoría completa y control de acceso basado en roles y permisos.

---

## 2. Contexto de Negocio

KPMG Venezuela, como firma de servicios profesionales de auditoría, impuestos y asesoría, gestiona una infraestructura tecnológica compleja que incluye aplicaciones corporativas, activos de operaciones (servidores, equipos de red), cuentas privilegiadas de sistemas y cuentas de servicio. El cumplimiento de los estándares ISO internos de KPMG (incluyendo ISO-057-ESC, ISO-040-ADU, ISO-082-API, ISO-067-GCS, ISO-032-EDR, ISO-034-EDT, ISO-033-ECP, ISO-010B-INC, ISO-064-EAS, entre otros) exige que todo cambio al inventario pase por un proceso formal de aprobación, que los datos de recuperación (RTO/RPO) estén documentados por activo, y que exista una pista de auditoría inalterable de cada acción.

---

## 3. Actores del Sistema

| Actor | Rol en IITS | Descripción |
|---|---|---|
| **SuperAdmin** | SuperAdmin | Acceso total al sistema. Configurable vía `Auth:SuperAdminUsername`. Único rol asignado durante el primer arranque. |
| **Administrador** | Administrador | Gestión de usuarios, roles y datos maestros. Visualización completa del inventario. |
| **Operador** | Operador | Alta, edición y gestión de registros en los módulos de inventario. Sujeto al flujo de aprobación. |
| **Aprobador** | Aprobador | Revisa y vota sobre solicitudes pendientes en los módulos para los que tiene permiso de aprobación (configurado por módulo en `AprobacionPermisos`). |
| **Auditor** | Auditor | Lectura de logs, registros de auditoría y generación de reportes. No puede editar registros. |
| **Usuario** | Usuario | Acceso de solo lectura a los módulos permitidos. |

---

## 4. Contexto Organizacional KPMG

- **Servidor de producción:** `VECCSAPP10` (servidor IIS Windows Server en la intranet de KPMG Venezuela).
- **URL pública intranet:** `https://desarrollos.ve.kworld.kpmg.com/IITSN`
- **Dominio AD:** `VE\` (Active Directory de KPMG Venezuela; el usuario SuperAdmin inicial es `VE\luisperdomo`).
- **Base de datos:** SQL Server en la instancia `VECCSAPP10,61057` (puerto TCP personalizado), base de datos `IITSN`.
- **Correo electrónico:** Relay SMTP corporativo en `goemairs.go.kworld.kpmg.com` (puerto 25), remitente `Ve-iits@apprelay.kpmg.com`.
- **Autenticación:** Totalmente integrada con Active Directory a través de Windows Authentication (Negotiate/Kerberos). Los usuarios no gestionan contraseñas dentro de IITS.

---

## 5. Stack Tecnológico

| Capa | Tecnología | Versión |
|---|---|---|
| Framework web | ASP.NET Core + Blazor Server | .NET 8 (net8.0) |
| ORM | Entity Framework Core | 8.0.11 |
| Base de datos | SQL Server (Express o Enterprise) | — |
| Autenticación | Windows Authentication (Negotiate/Kerberos) + Cookie | Microsoft.AspNetCore.Authentication.Negotiate 8.0.11 |
| Autorización | Role-based + Permission-based (claims) | ASP.NET Core Authorization |
| UI | Blazor Server + Bulma CSS | — |
| Componentes UI | Blazored.Modal | 7.1.0 |
| Exportación Excel | ClosedXML | 0.100.3 |
| Exportación Excel (alt.) | EPPlus | 6.1.3 |
| Exportación PDF | QuestPDF (licencia Community) | 2024.10.2 |
| Lectura Excel | NPOI | 2.6.0 |
| Rate Limiting | ASP.NET Core Rate Limiter (Fixed Window) | .NET 8 built-in |
| Email | SMTP (SmtpEmailSender) / Dev (DevEmailSender) | — |
| Despliegue | IIS con ASP.NET Core Hosting Bundle (in-process) | Windows Server |

---

## 6. Módulos Principales

### 6.1 Aplicaciones
Inventario del catálogo de aplicaciones de la firma. Cada registro incluye datos de funcionalidad, propietario, responsable, tipo de alojamiento, proveedor, clasificación de información, criticidad, integraciones, dependencias técnicas, modelo de licenciamiento, costo anual estimado, versión, SLA, **RTO** y **RPO** por separado (campo separado desde la versión actual; anteriormente combinado como RPORTO), y autenticación.

### 6.2 Operaciones (Tecnología)
Inventario de activos de infraestructura (servidores físicos, virtuales, equipos de red). Campos clave: Hostname, Serial, Oficina, Área, Alojamiento, Criticidad, Ambiente, Categoría, Tipo de dispositivo, Función, Sistema Operativo, RAM, CPU, Almacenamiento (DAS/SAN), Fabricante, Modelo, IP, MAC, Firmware, Garantía, **BCP**, **RTO** y **RPO**.

### 6.3 Cuentas Privilegiadas
Registro de cuentas con acceso elevado (administradores de dominio, cuentas root, etc.). Incluye nombre, responsable, origen, servicio relacionado, tipo de configuración de cambio, intervalo de cambio de contraseña (días), grupos de seguridad AD y descripción.

### 6.4 Cuentas de Servicio
Registro de cuentas técnicas usadas por aplicaciones y servicios automatizados. Campos similares a Cuentas Privilegiadas.

### 6.5 Aprobaciones
Flujo multi-aprobador que controla qué cambios en los módulos de inventario quedan en estado "Por aprobar" hasta recibir los votos necesarios. Cada aprobador registra su voto (Aprobado / Rechazado) con comentario. Si cualquier aprobador rechaza, el registro queda rechazado. Si todos aprueban, pasa a estado Aprobado. Los permisos de aprobación por módulo se configuran en `AprobacionPermisos`.

### 6.6 Usuarios y Roles
Gestión de usuarios locales (provisionados automáticamente al primer login Windows), asignación de roles, y configuración de permisos por rol. Roles predefinidos: SuperAdmin, Administrador, Operador, Aprobador, Auditor, Usuario.

### 6.7 Auditoría
Trazabilidad completa a través de dos tablas: `AuditLog` (registro simple de acciones: tabla, entidad, acción, usuario, fecha, detalle) y `AuditEvent` (registro estructurado con snapshot antes/después en JSON, CorrelationId). Los Auditores pueden generar reportes PDF de auditoría por módulo desde el endpoint `/api/auditoria/pdf`.

---

## 7. Flujo General de Aprobación

```
Operador crea/edita registro
        |
        v
Estado: "Por aprobar" (Aprobacion registrada)
        |
        v
Notificación por email a aprobadores del módulo (EmailOutbox)
        |
        v
Aprobador(es) votan (AprobacionVoto)
        |
     +--+--+
     |     |
 Todos   Alguno
aprueban rechaza
     |     |
     v     v
Aprobado Rechazado
```

---

## 8. Características de Seguridad Destacadas

- Autenticación 100% delegada a Active Directory mediante Windows Authentication (Negotiate).
- Sesión limitada por cookie con expiración configurable (5–480 minutos; por defecto 30).
- Rate limiting en endpoints de exportación y auditoría PDF: 10 solicitudes por minuto (ISO-082-API).
- Pista de auditoría inalterable en base de datos (AuditLog + AuditEvent).
- Credenciales nunca almacenadas en el repositorio: `appsettings.Production.json` es una plantilla en blanco; los valores reales se inyectan como variables de entorno en `web.config` en el servidor.
- `appsettings.Production.json` está en `.gitignore`.
- Todos los administradores, sin excepción, pasan por el mismo flujo de aprobación multi-aprobador (bypass de administrador eliminado).
- RTO y RPO separados como campos independientes por activo (cumplimiento ISO-067-GCS).
