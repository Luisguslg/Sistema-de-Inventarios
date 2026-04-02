# ERS — Especificación de Requisitos de Software

**Sistema:** IITS — Inventario de Infraestructura Tecnológica y Seguridad  
**Versión:** 1.0  
**Estándar:** IEEE 830-1998  
**Fecha:** 2026-04-01  
**Clasificación:** Interno KPMG Venezuela  

---

## 1. Introducción

### 1.1 Propósito
Este documento especifica los requisitos funcionales y no funcionales del sistema IITS. Está dirigido al equipo de desarrollo, revisores técnicos de KPMG y equipos de QA y seguridad.

### 1.2 Alcance
IITS cubre el registro, gestión, aprobación y auditoría del inventario tecnológico de KPMG Venezuela: aplicaciones, activos de operaciones, cuentas privilegiadas y cuentas de servicio. El sistema opera en la intranet corporativa, accesible solo para empleados autenticados en el dominio AD de KPMG.

### 1.3 Definiciones

| Término | Definición |
|---|---|
| AD | Active Directory (directorio de identidades corporativo de KPMG) |
| Negotiate | Protocolo de autenticación Windows (Kerberos/NTLM) |
| RTO | Recovery Time Objective — tiempo máximo tolerable de interrupción del servicio |
| RPO | Recovery Point Objective — pérdida máxima de datos tolerable tras una interrupción |
| BCP | Business Continuity Plan — indicador booleano de participación en el plan de continuidad |
| EstatusId | Referencia al catálogo Estatus (Activo=1000, Inactivo=1500, Desincorporado=2000) |
| SuperAdmin | Rol con acceso total al sistema; asignado al usuario configurado en `Auth:SuperAdminUsername` |

### 1.4 Referencias
- Estándares ISO internos KPMG: ISO-057-ESC, ISO-040-ADU, ISO-082-API, ISO-067-GCS, ISO-032-EDR, ISO-034-EDT, ISO-033-ECP, ISO-010B-INC, ISO-064-EAS y otros 16 estándares aplicables.
- "Catálogo de Aplicaciones.xlsx" — Maestro de campos de aplicaciones.
- "Campos.xlsx" — Maestro de campos de activos de operaciones.

---

## 2. Descripción General del Sistema

### 2.1 Perspectiva del Producto
IITS es una aplicación web Blazor Server hospedada en IIS bajo Windows Server en la intranet de KPMG. No es accesible desde internet. Depende de SQL Server para persistencia y del AD corporativo para autenticación.

### 2.2 Funciones Principales
1. Gestión CRUD de inventario tecnológico (Aplicaciones, Operaciones, Cuentas).
2. Flujo de aprobación multi-aprobador con votos y notificaciones por correo.
3. Control de acceso basado en roles y permisos granulares.
4. Auditoría completa de todas las acciones (quién, qué, cuándo, estado anterior y nuevo).
5. Exportación de datos en Excel, PDF y CSV.
6. Administración de usuarios, roles y permisos.

### 2.3 Características de los Usuarios

| Rol | Capacidades |
|---|---|
| SuperAdmin | Acceso irrestricto a todo el sistema |
| Administrador | Gestión de usuarios/roles, visualización completa, edición de datos maestros |
| Operador | Alta y edición de registros en módulos permitidos; sujeto a aprobación |
| Aprobador | Votación en solicitudes de su módulo asignado |
| Auditor | Solo lectura: logs, auditoría, exportaciones |
| Usuario | Solo lectura de inventario |

### 2.4 Restricciones Generales
- La aplicación solo opera dentro de la intranet de KPMG (no expuesta a internet).
- Toda autenticación requiere cuenta de dominio AD.
- El modo de autenticación en producción es exclusivamente `Auth:Mode=Windows`.
- Las credenciales de base de datos y SMTP nunca se almacenan en el repositorio de código.

---

## 3. Requisitos Funcionales

### RF-01: Autenticación Windows

**Módulo:** Transversal  
**Prioridad:** Alta

- RF-01.1: El sistema debe autenticar usuarios exclusivamente a través de Windows Authentication (Negotiate/Kerberos) en el entorno de producción.
- RF-01.2: Tras autenticación Windows exitosa, el sistema debe crear una sesión cookie con duración configurable (`Auth:SessionTimeoutMinutes`, entre 5 y 480 minutos; por defecto 30).
- RF-01.3: La cookie de sesión debe contener únicamente el `ClaimTypes.Name` (nombre de dominio del usuario) para evitar headers HTTP excesivos.
- RF-01.4: En entorno de desarrollo (`Auth:Mode=Dev`), el middleware `DevAuthMiddleware` debe inyectar un usuario configurado en `Auth:DevUsername` cuando no hay autenticación Windows disponible.
- RF-01.5: El servicio `IITSClaimsTransformation` debe, en cada solicitud autenticada, consultar la tabla `Users` y agregar claims de UserId, Nombre, Apellido, Email, CodSap, Roles y Permisos.
- RF-01.6: Si el usuario AD no existe en la tabla `Users`, no debe obtener roles ni permisos (acceso denegado efectivo), pero no debe ocurrir un error de aplicación.

---

### RF-02: Aprovisionamiento de Usuario SuperAdmin

**Módulo:** Administración  
**Prioridad:** Alta

- RF-02.1: Al arrancar la aplicación, debe ejecutarse `EnsureSuperAdminUserAsync` que crea el usuario configurado en `Auth:SuperAdminUsername` (si no existe) y le asigna el rol SuperAdmin.
- RF-02.2: El comando `dotnet run reset-migrations` debe recrear la base de datos y ejecutar este aprovisionamiento automáticamente.

---

### RF-03: Gestión de Aplicaciones

**Módulo:** Aplicaciones  
**Prioridad:** Alta

- RF-03.1: El sistema debe permitir crear, editar, visualizar y cambiar el estatus de registros de aplicaciones con los campos definidos en el maestro "Catálogo de Aplicaciones.xlsx".
- RF-03.2: Los campos RTO y RPO deben ser campos de texto independientes (máx. 100 caracteres cada uno). No se permite el campo combinado RPORTO.
- RF-03.3: El campo `Critico` (booleano) debe indicar si la aplicación es crítica para la operación de la firma.
- RF-03.4: El campo `AlojamientoId` debe hacer referencia al catálogo `Alojamientos` (On-Premise, Cloud, Híbrido, etc.).
- RF-03.5: Los campos `Propietario` y `Responsable` son texto libre (no referencia a usuario del sistema).
- RF-03.6: Al crear o editar, si el usuario es Operador, el registro debe quedar en estado "Por aprobar" hasta completar el flujo de aprobación.
- RF-03.7: El módulo debe soportar exportación filtrada por `Estatus` (Activo, Inactivo, Desincorporado) en los formatos XLSX, PDF y CSV.

---

### RF-04: Gestión de Operaciones (Tecnología)

**Módulo:** Operaciones  
**Prioridad:** Alta

- RF-04.1: El sistema debe permitir registrar activos de infraestructura con todos los campos del maestro "Campos.xlsx": Hostname (requerido), Serial, Oficina, Área, Alojamiento, Área propietaria, Criticidad, Entorno, Categoría, Tipo de dispositivo, Función, Tipo de infraestructura, Host, RAM, CPUs, Velocidad CPU, Capacidad DAS, Capacidad SAN, Sistema Operativo, Fabricante, Modelo, IP, MAC, Firmware, Fecha expiración garantía, Observaciones, BCP (booleano), RTO y RPO.
- RF-04.2: Los campos de catálogo (Criticidad, Ambiente, Categoría, Fabricante, Modelo) deben referenciar sus tablas respectivas.
- RF-04.3: El campo `TipoDispositivo` puede seleccionarse de `CatalogItems` (Kind = "TipoDispositivo") o ingresarse como texto libre que se agrega al catálogo.
- RF-04.4: BCP, RTO y RPO son campos opcionales.
- RF-04.5: La exportación debe incluir todos los campos mencionados y soportar filtro por Área y Estatus.

---

### RF-05: Gestión de Cuentas Privilegiadas

**Módulo:** Cuentas Privilegiadas  
**Prioridad:** Alta

- RF-05.1: El sistema debe permitir registrar cuentas con acceso privilegiado con los campos: Nombre (requerido), Estatus, Área, Responsable, Origen, Servicio relacionado, Tipo de configuración de cambio, Intervalo de cambio (días), Grupos de seguridad AD, Descripción y Aplicación asociada.
- RF-05.2: El campo `IntervaloCambioDias` debe almacenar la frecuencia de rotación de credenciales, pero el sistema NO genera recordatorios automáticos (brecha identificada; ver Auditoría de Seguridad).
- RF-05.3: El campo `GruposSeguridad` es un texto multivalor (hasta 2000 caracteres) que lista los grupos AD a los que pertenece la cuenta.

---

### RF-06: Gestión de Cuentas de Servicio

**Módulo:** Cuentas de Servicio  
**Prioridad:** Alta

- RF-06.1: Idéntico en estructura a RF-05. Las cuentas de servicio son cuentas técnicas (no interactivas) usadas por aplicaciones y procesos automatizados.
- RF-06.2: Ambos módulos de cuentas deben mostrar el área y la aplicación relacionada para facilitar el análisis de impacto.

---

### RF-07: Flujo de Aprobación Multi-Aprobador

**Módulo:** Aprobaciones  
**Prioridad:** Alta

- RF-07.1: Cualquier creación o modificación de un registro en los módulos Aplicaciones, Operaciones o Cuentas realizada por un Operador debe generar un registro en `Aprobaciones` con estado "Por aprobar".
- RF-07.2: Todos los usuarios con permiso de aprobación para el módulo afectado (configurados en `AprobacionPermisos`) deben recibir una notificación por correo electrónico vía `EmailOutbox`.
- RF-07.3: Cada aprobador puede votar una sola vez por solicitud (restricción única en `AprobacionVotos` por `AprobacionId` + `UserId`).
- RF-07.4: Si cualquier aprobador vota "Rechazado", la solicitud pasa inmediatamente a estado "Rechazado".
- RF-07.5: Si todos los aprobadores configurados para el módulo han votado "Aprobado", la solicitud pasa a estado "Aprobado".
- RF-07.6: Los administradores y SuperAdmins NO tienen bypass del flujo de aprobación; deben votar como cualquier otro aprobador asignado.
- RF-07.7: El módulo de Aprobaciones debe mostrar a cada aprobador el estado de su voto en las solicitudes pendientes.
- RF-07.8: Un usuario puede configurarse como aprobador de múltiples módulos (registros adicionales en `AprobacionPermisos`).

---

### RF-08: Control de Acceso Basado en Roles y Permisos

**Módulo:** Transversal / Administración  
**Prioridad:** Alta

- RF-08.1: El sistema debe implementar control de acceso de dos niveles: Roles (agrupaciones) y Permisos (granulares por acción/módulo).
- RF-08.2: Los permisos disponibles son:

| Código | Descripción |
|---|---|
| `Perm.Admin` | Administración total |
| `Perm.Inventory.View` | Ver inventarios |
| `Perm.Inventory.Create` | Crear en inventario |
| `Perm.Inventory.Edit` | Editar inventario |
| `Perm.Inventory.Export` | Exportar inventarios |
| `Perm.Inventory.Aplicaciones` | Ver y operar módulo Aplicaciones |
| `Perm.Inventory.Operaciones` | Ver y operar módulo Tecnología |
| `Perm.Inventory.Cuentas` | Ver y operar módulo Cuentas |
| `Perm.Audit.View` | Ver auditoría |
| `Perm.Audit.Approve` | Aprobar solicitudes |
| `Perm.Logs.View` | Ver logs |
| `Perm.Logs.Export` | Exportar logs |
| `Perm.Users.Manage` | Gestionar usuarios |
| `Perm.Roles.Manage` | Gestionar roles y permisos |

- RF-08.3: Los permisos se asignan a roles y los roles a usuarios. Un usuario puede tener múltiples roles.
- RF-08.4: El permiso `Perm.Admin` otorga acceso a todas las funciones del sistema.
- RF-08.5: Los permisos de aprobación por módulo se gestionan en la tabla `AprobacionPermisos` (separada del RBAC general).

---

### RF-09: Auditoría

**Módulo:** Auditoría  
**Prioridad:** Alta

- RF-09.1: Toda acción de creación, modificación o cambio de estatus sobre registros de inventario debe registrarse en `AuditLog` con: tabla, ID de entidad, acción, ID de usuario y fecha/hora UTC.
- RF-09.2: Para acciones críticas (aprobación, rechazo, cambios de roles/permisos), debe registrarse además en `AuditEvent` con snapshot JSON del estado antes y después.
- RF-09.3: Los Auditores y Administradores deben poder consultar el log de auditoría filtrado por tabla.
- RF-09.4: El sistema debe generar un reporte PDF de auditoría por módulo (aplicaciones, operaciones, cuentas) desde el endpoint `/api/auditoria/pdf?modulo=X`.
- RF-09.5: Los registros de auditoría no deben ser modificables ni eliminables por ningún usuario desde la interfaz.

---

### RF-10: Exportación de Datos

**Módulo:** Transversal  
**Prioridad:** Media

- RF-10.1: El sistema debe permitir exportar los datos de cada módulo en tres formatos: Excel (XLSX via ClosedXML), PDF (QuestPDF) y CSV.
- RF-10.2: Los archivos exportados deben incluir la fecha/hora en el nombre (formato `yyyyMMdd_HHmm`).
- RF-10.3: Los endpoints de exportación (`/api/export/{modulo}/{formato}`) deben estar protegidos por rate limiting: máximo 10 solicitudes por minuto por ventana fija.
- RF-10.4: La exportación debe soportar filtros por área, estatus y otros campos según el módulo.
- RF-10.5: Solo usuarios con el permiso `Perm.Inventory.Export` o `Perm.Logs.Export` pueden acceder a las exportaciones correspondientes.

---

### RF-11: Notificaciones por Correo Electrónico

**Módulo:** Transversal  
**Prioridad:** Media

- RF-11.1: El sistema debe enrutar todos los correos salientes a través de la tabla `EmailOutbox` (patrón outbox).
- RF-11.2: El servicio `EmailOutboxHostedService` debe procesar los correos pendientes cada 30 segundos, enviando hasta 20 mensajes por ciclo.
- RF-11.3: En caso de fallo de envío, el campo `Error` y `RetryCount` deben actualizarse para diagnóstico.
- RF-11.4: En entorno de desarrollo (`Email:Mode=Dev`), los correos no se envían (se utiliza `DevEmailSender`). En producción (`Email:Mode=Smtp`), se utiliza `SmtpEmailSender`.

---

### RF-12: Gestión de Usuarios

**Módulo:** Administración  
**Prioridad:** Alta

- RF-12.1: Los usuarios se crean en la tabla `Users` con: Username (nombre de dominio AD), Nombre, Apellido, Email y CodSap.
- RF-12.2: Un Administrador o SuperAdmin puede asignar y revocar roles a usuarios desde la interfaz de administración.
- RF-12.3: El sistema debe soportar la búsqueda de usuarios para asignación de roles y permisos de aprobación.

---

### RF-13: Datos Maestros (Catálogos)

**Módulo:** Administración  
**Prioridad:** Media

- RF-13.1: Los siguientes catálogos deben ser administrables desde la interfaz (Administración > Maestro de Datos): Oficinas, Áreas, Alojamientos, Ambientes, Criticidades, Categorías, Fabricantes, Modelos de dispositivo, Ítems de catálogo genérico.
- RF-13.2: Los catálogos se inicializan con datos semilla mediante los métodos `Ensure*Async` al arrancar la aplicación.
- RF-13.3: Los ítems de `CatalogItems` (TipoDispositivo, Funcion, TipoInfraestructura, SistemaOperativo) pueden agregarse en el acto desde los formularios de Operaciones.

---

## 4. Casos de Uso Clave

### CU-01: Alta de Aplicación por Operador

**Actor principal:** Operador  
**Precondición:** Usuario autenticado con permiso `Perm.Inventory.Create` y `Perm.Inventory.Aplicaciones`.

1. El Operador accede al módulo Aplicaciones y selecciona "Nueva Aplicación".
2. Completa el formulario con los campos requeridos (Nombre, EstatusId) y opcionales.
3. El sistema guarda el registro con `CreatedAt = UtcNow`.
4. El sistema crea un registro en `Aprobaciones` con `Estado = "Por aprobar"`.
5. El sistema encola una notificación en `EmailOutbox` para cada aprobador del módulo Aplicaciones.
6. El sistema registra la acción en `AuditLog` (Tabla="Aplicaciones", Accion="Crear").
7. El registro aparece marcado como "Pendiente de aprobación" en la lista.

**Postcondición:** La aplicación existe en BD con estado "Por aprobar". Los aprobadores han sido notificados.

---

### CU-02: Aprobación Multi-Aprobador

**Actor principal:** Aprobador  
**Precondición:** Existe una `Aprobacion` en estado "Por aprobar". El usuario tiene `AprobacionPermiso` para el módulo.

1. El Aprobador accede al módulo Aprobaciones.
2. El sistema muestra las solicitudes pendientes con el estado del propio voto del aprobador.
3. El Aprobador selecciona "Aprobar" o "Rechazar" con comentario opcional.
4. El sistema registra el voto en `AprobacionVotos` (si no ha votado antes).
5. **Si algún voto es "Rechazado":** la `Aprobacion` pasa a estado "Rechazado" inmediatamente.
6. **Si todos los aprobadores configurados han votado "Aprobado":** la `Aprobacion` pasa a estado "Aprobado".
7. El sistema registra la acción en `AuditLog`.

**Postcondición:** La solicitud ha sido aprobada o rechazada. El registro de inventario refleja el resultado.

---

### CU-03: Exportación de Inventario

**Actor principal:** Usuario con `Perm.Inventory.Export`  
**Precondición:** Existen registros en el módulo a exportar.

1. El usuario accede al módulo y selecciona el formato de exportación (XLSX, PDF o CSV).
2. El sistema aplica los filtros seleccionados (estatus, área, etc.).
3. El sistema verifica el rate limit (máx. 10 req/min).
4. El sistema genera el archivo con nombre `Modulo_yyyyMMdd_HHmm.ext`.
5. El archivo se descarga directamente en el navegador.

**Postcondición:** El usuario recibe el archivo con los datos filtrados.

---

### CU-04: Consulta de Pista de Auditoría

**Actor principal:** Auditor  
**Precondición:** Usuario con permiso `Perm.Audit.View`.

1. El Auditor accede al módulo de Auditoría.
2. Selecciona filtros opcionales (módulo, fecha, usuario).
3. El sistema muestra los registros de `AuditLog` ordenados por fecha descendente.
4. El Auditor puede generar un reporte PDF desde `/api/auditoria/pdf?modulo=aplicaciones`.
5. El PDF incluye las solicitudes pendientes de aprobación y los datos del módulo con los aprobadores asignados.

---

### CU-05: Gestión de Roles y Permisos

**Actor principal:** Administrador o SuperAdmin  
**Precondición:** Usuario con permiso `Perm.Roles.Manage`.

1. El Administrador accede a Administración > Roles.
2. Selecciona un rol y gestiona los permisos asignados (checkbox por código de permiso).
3. El sistema actualiza `RolePermissions`.
4. En la siguiente autenticación, los usuarios con ese rol tendrán los nuevos permisos reflejados en sus claims.

---

## 5. Requisitos No Funcionales

### RNF-01: Seguridad (ISO KPMG)

| Estándar | Requisito |
|---|---|
| ISO-057-ESC | La política de contraseñas es gestionada enteramente por Active Directory. IITS no almacena ni gestiona contraseñas. La sesión expira según `Auth:SessionTimeoutMinutes`. |
| ISO-040-ADU | La autenticación multifactor se delega a la política de AD corporativo. IITS no implementa MFA propio (brecha documentada en Security Audit). |
| ISO-082-API | Los endpoints de exportación y PDF deben estar protegidos por rate limiting: máximo 10 solicitudes por minuto (Fixed Window). |
| ISO-067-GCS | RTO y RPO deben registrarse como campos separados e independientes por cada activo (Aplicaciones y Operaciones). |
| ISO-032-EDR | La cadena de conexión debe usar `Encrypt=True` (o equivalente) en producción para cifrar datos en tránsito hacia SQL Server. |
| ISO-034-EDT | `TrustServerCertificate=False` en producción. HTTPS debe estar habilitado y forzado. |
| ISO-033-ECP | Las cuentas privilegiadas deben registrarse con su intervalo de cambio de credenciales (`IntervaloCambioDias`). |
| ISO-010B-INC | Toda acción en el sistema debe quedar registrada en `AuditLog` y/o `AuditEvent` para soporte de incidentes. |
| ISO-064-EAS | El control de acceso debe ser basado en roles y permisos. El flujo de aprobación debe requerir múltiples aprobadores para cambios en inventario crítico. |

- RNF-01.1: Las cookies de sesión deben marcarse como `HttpOnly` y `Secure` en producción.
- RNF-01.2: Las credenciales de base de datos y SMTP deben inyectarse vía variables de entorno, nunca en el repositorio.
- RNF-01.3: El provider de autenticación IIS debe tener "Negotiate" antes de "NTLM" para favorecer Kerberos.

---

### RNF-02: Rendimiento

- RNF-02.1: Las consultas de listado de inventario con hasta 1000 registros deben responder en menos de 3 segundos.
- RNF-02.2: La generación de exportaciones (Excel/PDF) debe completarse en menos de 30 segundos para hasta 5000 registros.
- RNF-02.3: El procesamiento del `EmailOutbox` (hasta 20 correos) debe completarse en menos de 60 segundos por ciclo.
- RNF-02.4: EF Core debe usar `SplitQuery` y `AsNoTracking()` en las consultas de solo lectura para optimizar performance.

---

### RNF-03: Disponibilidad

- RNF-03.1: La aplicación debe operar 24/7 en la intranet de KPMG con disponibilidad mínima del 99% durante horario laboral (lunes a viernes, 7:00–19:00 hora de Venezuela).
- RNF-03.2: La aplicación debe registrar errores de arranque en `startup_error.txt` en el directorio raíz para facilitar diagnóstico sin acceso al servidor.
- RNF-03.3: Fallos en la tabla `EmailOutbox` no deben impedir el arranque de la aplicación.

---

### RNF-04: Mantenibilidad

- RNF-04.1: Las migraciones de base de datos deben gestionarse exclusivamente con EF Core (`dotnet ef migrations add`). No se permite SQL raw para gestión de esquema.
- RNF-04.2: Los métodos `Ensure*ColumnsAsync` son mecanismos de compatibilidad hacia atrás y deben ejecutarse al inicio para bases de datos existentes sin migración completa.
- RNF-04.3: El patrón de inyección de dependencias debe usarse para todos los servicios.

---

### RNF-05: Portabilidad

- RNF-05.1: La aplicación debe poder desplegarse en cualquier servidor Windows con IIS y .NET 8 Runtime.
- RNF-05.2: Toda configuración específica del entorno debe estar en variables de entorno o en `appsettings.{Environment}.json` (nunca hardcodeada).

---

## 6. Supuestos y Restricciones

| # | Supuesto / Restricción |
|---|---|
| S-01 | Todos los usuarios del sistema tienen una cuenta activa en el dominio AD de KPMG (`VE\`). |
| S-02 | El servidor IIS tiene habilitada la autenticación Windows con el proveedor Negotiate configurado como primero. |
| S-03 | La autenticación anónima de IIS está deshabilitada para la aplicación. |
| S-04 | El identity del Application Pool de IIS tiene `forwardWindowsAuthToken="true"` en `web.config`. |
| S-05 | El servidor SQL tiene conectividad desde el servidor IIS por el puerto configurado. |
| S-06 | El relay SMTP corporativo (`goemairs.go.kworld.kpmg.com:25`) acepta correos del servidor IIS sin autenticación (relay de confianza interno). |
| S-07 | No existe integración con sistemas externos (SAP, AD de forma directa para creación de usuarios, etc.); el aprovisionamiento de usuarios en IITS es manual por un Administrador. |
| S-08 | El volumen máximo esperado de registros por módulo es de ~5000 en el horizonte de 2 años. |
