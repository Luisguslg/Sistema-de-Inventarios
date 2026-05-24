# Documentación IITS

Índice de la documentación técnica del sistema de inventarios (ingeniería de software).

| Documento | Contenido |
|-----------|-----------|
| [ESTRUCTURA_PROYECTO.md](ESTRUCTURA_PROYECTO.md) | Organización de carpetas y capas |
| [MIGRACIONES.md](MIGRACIONES.md) | Aplicar migraciones, levantar la app, reset/fix |
| [ARQUITECTURA.md](ARQUITECTURA.md) | Capas, auth, flujos |
| [ESQUEMA_BD.md](ESQUEMA_BD.md) | Modelo de datos, diagrama ER, Partes y permisos por módulo |
| [DIAGRAMA_CLASES.md](DIAGRAMA_CLASES.md) | Diagrama de clases (dominio y servicios) |
| [DESPLIEGUE_IIS.md](DESPLIEGUE_IIS.md) | Despliegue en IIS, autenticación Windows, cómo correr y publicar |

Roles y permisos vienen de Active Directory (IIS) y se resuelven contra la BD (Users, UserRoles, RolePermission, AprobacionPermiso). Los permisos por módulo (Perm.Inventory.Aplicaciones, Operaciones, Telecomunicaciones, Cuentas) definen qué inventarios ve cada usuario.
