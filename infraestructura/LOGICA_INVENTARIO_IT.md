# Lógica del sistema de inventario IT (IITS)

## Objetivo

Registrar activos de IT por tipo, llevar estatus (activo/desincorporado), sacar reportes y pasar cambios por aprobación. Es un inventario para reportes y control, no un CMDB completo.

## Tipos de inventario (7)

| Tipo | Tabla principal | Tabla estatus | Qué se guarda |
|------|-----------------|---------------|----------------|
| Aplicaciones | T_Aplicaciones | T_AplicacionEstatus | Apps, hostname, alojamiento, autenticaciones, contactos, IPs, etc. |
| Operaciones | T_Operaciones | T_OperacionEstatus | Servidores/equipos, oficina, ambiente, categoría, criticidad, IP, MAC, SO, etc. |
| Telecomunicaciones | T_Telecoms | T_TelecomEstatus | Dispositivos de red, fabricante, modelo, firmware, IP, etc. |
| Cuentas de servicio | T_CuentasServicio | T_CuentasServicioEstatus | Cuentas de servicio, origen, intervalo, área, grupos de seguridad |
| Cuentas privilegiadas | T_CuentasPrivilegiadas | T_CuentasPrivilegiadasEstatus | Cuentas privilegiadas, tipo, responsable, grupos |
| Páginas web | T_PaginasWeb | T_PaginasWebEstatus | Sitios web, área, servidor, autenticación, enlace |
| Proveedores | T_Proveedores | T_ProveedorEstatus | Proveedores, contactos, áreas, vencimientos |

Cada tipo tiene una tabla “maestra” y una tabla de relación con **T_Estatus** (activo = código 1000, desincorporado = 2000). El dashboard y los listados usan eso para totales y filtros.

## Roles y flujo

- **Usuario:** ve el dashboard (resumen por tipo).
- **Operador / Delegado:** da de alta y edita ítems en Inventarios (según rol por módulo).
- **Auditor:** ve solo lectura / logs por módulo.
- **Administrador:** ve logs, aprobaciones, usuarios y data (CRUD de catálogos).
- **Aprobador:** rol usado en flujo de aprobación (T_Aprobaciones).

Al crear o modificar ítems que requieren aprobación, se registra en **T_Aprobaciones** (identificador del objeto, revisor, fechas). Los administradores ven la lista en Logs → Aprobaciones.

## Tablas que realmente usa la app (resumen)

- **Seguridad:** T_Users, T_UserRoles, T_Role (y T_Roles/T_UserRole según uso).
- **Estatus y catálogos:** T_Estatus, T_Areas, T_Ambientes, T_Categoria, T_Oficinas, T_Dispositivos, T_Autentificaciones, etc.
- **Por cada tipo de inventario:** la tabla principal + su tabla *Estatus + tablas de detalle (ej. T_OperacionDireccionesIP, T_OperacionObservaciones, T_AplicacionContactos…).
- **Aprobaciones:** T_Aprobaciones.

Las ~80 tablas vienen de normalizar cada aspecto por separado (estatus, contactos, direcciones, observaciones, etc.). Para un sistema solo de reportes y aprobaciones, en el futuro se podría simplificar el modelo (menos tablas, más columnas o JSON en unas pocas tablas) sin cambiar aún la BD actual.

## Reportes y exportación

- Cada módulo (Inventarios y Logs) puede exportar a Excel; Aprobaciones además a PDF.
- Nombre de archivo: `NombreModulo_yyyy-MM-dd.xlsx` o `.pdf`.
- Los datos exportados son los que se ven en pantalla (filtros aplicados).
