# Roles en IITS

Cada rol define qué puede hacer un usuario en el sistema. Los permisos concretos (por módulo) se ajustan en **Administración → Permisos por rol**.

---

## Resumen por rol

| Rol | Qué hace |
|-----|----------|
| **SuperAdmin** | Acceso total. Ve y hace todo: inventarios, auditoría, aprobaciones, usuarios, roles, logs, datos. Usuario inicial definido en configuración (`Auth:SuperAdminUsername`). |
| **Administrador** | Igual que SuperAdmin a nivel de permisos: ve y opera en todos los módulos, auditoría, usuarios, roles, logs. |
| **Operador** | Puede **crear, editar y exportar** en los inventarios (Aplicaciones, Tecnología, Cuentas). No gestiona usuarios ni roles. Qué módulos ve se define en Permisos por rol (puede tener solo uno o varios). |
| **Auditor** | **Solo lectura**: ve inventarios y auditoría, y puede ver/exportar logs. No edita ni aprueba. |
| **Aprobador** | Ve inventarios y auditoría, y puede **aprobar o rechazar** solicitudes en los módulos que se le asignen en **Permisos de aprobación** (Aplicaciones, Tecnología, Cuentas). No gestiona usuarios ni roles. |
| **Usuario** | **Solo consulta** de inventarios en los módulos que tenga en Permisos por rol. No edita, no aprueba, no ve logs ni administración. |

---

## Detalle de qué ve cada uno

|                      | SuperAdmin | Administrador | Operador | Auditor | Aprobador | Usuario |
|----------------------|------------|---------------|----------|---------|-----------|---------|
| Menú Inventarios     | Sí (todo)  | Sí (todo)     | Sí*      | Sí*     | Sí*       | Sí*     |
| Crear/editar inventario | Sí     | Sí            | Sí*      | No      | No        | No      |
| Exportar inventarios | Sí         | Sí            | Sí*      | No      | No        | No      |
| Menú Auditoría      | Sí         | Sí            | Sí       | Sí      | Sí        | No      |
| Aprobar / Rechazar   | Sí (todo)  | Sí (todo)     | No       | No      | Sí**      | No      |
| Menú Administración | Sí         | Sí            | No       | No      | No       | No      |
| Usuarios y roles     | Sí         | Sí            | No       | No      | No       | No      |
| Permisos de aprobación | Sí       | Sí            | No       | No      | No       | No      |
| Logs                 | Sí         | Sí            | No       | Sí      | No       | No      |
| Data (Estatus, etc.) | Sí         | Sí            | No       | No      | No       | No      |

\* Solo en los módulos que tenga asignados en **Permisos por rol** (Aplicaciones, Tecnología, Cuentas).  
\** Solo en los módulos que tenga en **Permisos de aprobación**.

---

## Cómo limitar por “área”

- **Inventarios (ver/operar):** En **Permisos por rol** marcas o desmarcas, por rol, los permisos de cada módulo (Aplicaciones, Tecnología, Cuentas). Así un Operador puede tener solo Aplicaciones, solo Tecnología, o los tres.
- **Aprobar:** En **Permisos de aprobación** asignas usuario + módulo. Un mismo usuario puede aparecer varias veces (un registro por módulo) para aprobar en una, dos o las tres áreas.

Los **roles** dan el tipo de acceso (operar, auditar, aprobar); los **permisos por rol** y los **permisos de aprobación** definen en qué módulos/áreas aplica.
