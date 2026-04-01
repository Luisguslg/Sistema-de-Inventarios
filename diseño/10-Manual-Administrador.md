# Manual de Administrador

## Acceso

El administrador tiene rol **Administrador** o **SuperAdmin** y accede a todas las secciones, incluida la administración de usuarios, roles y catálogos.

---

## Admin → Usuarios

- Ver listado de usuarios.
- Crear usuario (Username, Nombre, Apellido, Email, CodSap).
- Asignar roles a cada usuario.
- Editar y desactivar usuarios.

---

## Admin → Roles

- Ver y gestionar roles (SuperAdmin, Administrador, Operador, Auditor, Aprobador).
- Asignar permisos a cada rol mediante la tabla RolePermission.

---

## Admin → Permisos de Aprobación

- Definir qué usuarios pueden aprobar por módulo (Aplicaciones, Operaciones, Cuentas).
- Un usuario puede ser aprobador en uno o varios módulos.

---

## Admin → Maestro de Datos

Gestión de catálogos maestros:
- Estatus
- Áreas
- Oficinas
- Ambientes
- Criticidades
- Categorías
- Fabricantes
- Modelos
- Alojamientos
- Catálogo genérico (CatalogItems) para TipoDispositivo, Función, TipoInfraestructura, SistemaOperativo, etc.

---

## Configuración

### appsettings.json / appsettings.Production.json

| Sección | Parámetro | Descripción |
|---------|-----------|-------------|
| ConnectionStrings | IITS | Cadena de conexión a SQL Server |
| Auth | SessionTimeoutMinutes | Tiempo de sesión (5–480 min) |
| Auth | SuperAdminUsername | Usuario SuperAdmin |
| Auth | Mode | "Dev" para desarrollo sin AD |
| Auth | DevUsername | Usuario emulado en desarrollo |
| Email | Mode | "Smtp" o vacío para DevEmailSender |
| Email | Host, Port, etc. | Configuración SMTP |

---

## Despliegue IIS

1. Publicar la aplicación: `dotnet publish -c Release`.
2. Crear sitio o aplicación en IIS.
3. Configurar App Pool con identidad de dominio (para AD).
4. Habilitar Windows Authentication, deshabilitar Anonymous si aplica.
5. Configurar cadena de conexión y variables de entorno en el servidor.
6. Aplicar migraciones: `dotnet ef database update --project IITS` o mediante comando `reset-migrations` en la consola de la app.

---

## Comandos de consola

| Comando | Uso |
|---------|-----|
| `dotnet run -- reset-migrations` | Borra BD, recrea y aplica migraciones |
| `dotnet run -- fix-migrations` | Repara migraciones huérfanas |
| `dotnet run -- seed-catalogs` | Carga catálogos y datos de ejemplo |
| `dotnet run -- read-catalogo-aplicaciones` | Lista columnas del Excel de aplicaciones |

---

## Logs y diagnóstico

- Errores de arranque: `startup_error.txt` en ContentRootPath.
- Logs de aplicación: carpeta `Logs`.
- Verificar que el App Pool tenga permisos de escritura en `Logs` y `wwwroot` si se escriben archivos temporales.
