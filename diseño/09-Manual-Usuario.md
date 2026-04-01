# Manual de Usuario

## Acceso al sistema

1. Abrir el navegador y entrar a la URL del sistema.
2. Si está configurado con Active Directory, la sesión se inicia automáticamente con el usuario de Windows.
3. En modo desarrollo, usar el usuario configurado en `Auth:DevUsername`.

---

## Dashboard

Al entrar se muestra el dashboard con tarjetas por módulo (Aplicaciones, Operaciones, Cuentas Servicio, Cuentas Privilegiadas). Solo se ven los módulos para los que el usuario tiene permiso.

Cada tarjeta muestra totales (Activos / Desincorporados) y un botón para agregar registros (si tiene permiso).

---

## Módulos de inventario

### Aplicaciones

- Ver listado de aplicaciones.
- Agregar, editar y desincorporar aplicaciones (según permisos).
- Campos principales: Nombre, Funcionalidad, Propietario, Responsable, Tipo de alojamiento, Proveedor, Clasificación de información, Criticidad, Integraciones, Dependencias, Costo anual, SLA, RPO/RTO, etc.

### Operaciones (Tecnología)

- Ver listado de activos (servidores, equipos, dispositivos de red).
- Agregar, editar y desincorporar (según permisos).
- Campos: Oficina, Área, Dispositivo, Hostname, Entorno de operación, Propietario, Criticidad, Ambiente, Fabricante, Modelo, Serial, Sistema Operativo, Garantía, BCP, RPO/RTO, etc.

### Cuentas

- Ver cuentas de servicio y privilegiadas.
- Agregar, editar y desincorporar (según permisos).
- Campos: Nombre, Área, Responsable, Origen, Servicio/Aplicación relacionada, Tipo de configuración de cambio, Intervalo de cambio, Grupos de seguridad.

---

## Exportación

En cada listado hay opciones para exportar a:
- Excel (xlsx)
- PDF
- CSV

Se descarga un archivo con nombre `{Modulo}_{fecha}_{hora}.{ext}`.

---

## Aprobaciones

Si el módulo requiere aprobación, los cambios quedan pendientes hasta que un aprobador los apruebe o rechace. El usuario puede ver el estado de sus solicitudes en Logs → Aprobaciones (según permisos).

---

## Logs

En Logs se consultan:
- Registros de auditoría (tabla, entidad, acción, usuario, fecha).
- Aprobaciones (pendientes, aprobadas, rechazadas).

Filtros por tabla, módulo, área, estatus, etc. Exportación a Excel, PDF o CSV.

---

## Notificaciones

Las notificaciones (toasts) aparecen en la esquina de la pantalla al realizar operaciones (guardar, aprobar, rechazar, errores).
