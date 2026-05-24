# Seed y catálogos

## Actualizar la base de datos en el servidor (sin perder datos)

Cuando publicas una **nueva versión de la aplicación** que añade columnas o tablas (por ejemplo los nuevos campos de Cuentas: Tipo configuración de cambio, Intervalo de cambio, Grupos de Seguridad, Descripción), **no hace falta ejecutar nada en PowerShell** ni scripts manuales. Los datos existentes se mantienen.

### Qué hacer

1. **Publicar** la nueva versión de la app en el servidor (reemplazar los archivos desplegados: por ejemplo la carpeta donde está `IITS.dll`, `web.config`, etc.).
2. **Reiniciar** el sitio o el Application Pool de IIS donde corre IITS (o reiniciar el proceso si no usas IIS).

Al arrancar, la aplicación ejecuta automáticamente:

- **Migraciones de EF** (si hubiera nuevas), que solo añaden lo que falta.
- **DbSeed** (`EnsureCuentasOptionalColumnsAsync`, `EnsureCatalogTablesIfMissingAsync`, etc.), que ejecuta SQL del tipo *“si no existe la columna X, añádela”*.

Todo eso es **idempotente**: no borra datos ni duplica columnas. Las tablas y columnas que ya existan no se tocan; solo se crean las que falten.

### Pasos típicos en el servidor (IIS)

1. **Publicar** desde Visual Studio o `dotnet publish` en la carpeta que apunta el sitio (o copiar los archivos nuevos sobre la carpeta actual).
2. En **IIS Manager**: sitio → **Reiniciar** o **Application Pools** → seleccionar el pool de IITS → **Recyclar** (o **Reiniciar**).
3. No hace falta abrir PowerShell ni ejecutar ningún comando. Al recibir la primera petición, la app arranca y aplica las columnas/tablas que falten.

### Resumen

| Acción                         | Cómo hacerlo                          | ¿Se pierden datos? |
|--------------------------------|----------------------------------------|---------------------|
| Actualizar esquema (nuevas columnas/tablas) | Desplegar nueva versión y reiniciar la app | **No**              |
| Vaciar todo y dejar solo seed  | Ejecutar `reset-migrations` (ver abajo) | **Sí** (borra todo) |

---

## Vaciar la base de datos y dejar solo el seed

Para dejar la base de datos “limpia” en el servidor y que vuelva a aplicarse el seed:

1. **Opción A – Recrear la BD (borra todo):**  
   Ejecutar la aplicación con el argumento `reset-migrations`:
   ```text
   dotnet run -- reset-migrations
   ```
   (o desde la carpeta de publicación: `IITS.exe reset-migrations`).  
   Eso elimina la base de datos, la vuelve a crear, aplica migraciones y ejecuta los seeds (Estatus, Roles, Permissions, SuperAdmin, catálogos, etc.). Al terminar, la aplicación se cierra.

2. **Opción B – Solo vaciar datos (mantener esquema):**  
   En SQL Server puede ejecutar scripts que hagan `DELETE` o `TRUNCATE` sobre las tablas de negocio (Operaciones, Aplicaciones, CuentasPrivilegiadas, CuentasServicio, Aprobaciones, etc.), dejando Users, Roles, Permissions, Estatus y catálogos si lo desea. Después, al reiniciar la app, no se vuelve a ejecutar todo el seed (es idempotente); si necesita “re-semillar” catálogos, tendría que borrar solo esas tablas o usar la opción A.

Para **seguir probando con datos de ejemplo**, la opción A es la más simple: `reset-migrations` y luego arrancar la app sin argumentos.

---

## Dónde se definen los datos del seed

Todo está en **`IITS/Data/DbSeed.cs`**.

| Qué quieres cambiar | Dónde en DbSeed.cs |
|--------------------|--------------------|
| **Tipo de infraestructura** (Físico, Virtual; quitar o agregar valores) | Método **`EnsureCatalogItemsTecnologiaAsync`**: array `defaults`, líneas con `("TipoInfraestructura", "Físico")` y `("TipoInfraestructura", "Virtual")`. Para quitar “Virtual”, elimina esa línea; para agregar otro valor, añade una línea `("TipoInfraestructura", "NuevoValor")`. |
| Otros catálogos de tecnología (TipoDispositivo, Función, SistemaOperativo) | Mismo método **`EnsureCatalogItemsTecnologiaAsync`**: mismo array `defaults`. |
| Oficinas (Barquisimeto, Caracas, etc.) | **`EnsureOfficesAsync`**: array `names`. |
| Áreas (Aplicaciones, Global, Operaciones, etc.) | **`EnsureAreasAsync`**: array `names`. |
| Alojamientos (On-Premise, Cloud, etc.) | **`EnsureAlojamientosAsync`**: array `nombres`. |
| Estatus (Activo, Inactivo, Desincorporado) | **`EnsureEstatusAsync`**: array `required`. |
| Roles (SuperAdmin, Administrador, etc.) | **`EnsureRolesAsync`**: array `nombres`. |
| Permisos por defecto de cada rol | **`EnsurePermissionsAsync`**: llamadas a `AssignRolePermissionsAsync` para cada rol. |

Los valores de **Tipo infraestructura** que ves en la pantalla (Físico / Virtual) vienen de la tabla **CatalogItems** con `Kind = "TipoInfraestructura"`. Esos registros se crean o completan en `EnsureCatalogItemsTecnologiaAsync`; si en el futuro quitas “Virtual” del seed, puedes borrar manualmente en BD el registro correspondiente o dejar de insertarlo en ese método.

---

## Roles y permisos

Los **roles** definen conjuntos de permisos por defecto. En **Administración → Permisos por rol** puedes afinar qué permisos tiene cada rol (y así cada usuario que tenga ese rol). La granularidad por **módulo** se controla con los permisos:

- **Perm.Inventory.Aplicaciones** – Ver y operar solo Aplicaciones  
- **Perm.Inventory.Operaciones** – Ver y operar solo Tecnología  
- **Perm.Inventory.Cuentas** – Ver y operar solo Cuentas  

Así puedes dar a un usuario solo el rol **Operador** y, en Permisos por rol, dejarle solo **Perm.Inventory.Aplicaciones** marcado: verá y podrá operar únicamente en Aplicaciones.

| Rol           | Uso                                                                 | Permisos por defecto (seed) |
|---------------|---------------------------------------------------------------------|-----------------------------|
| **SuperAdmin**| Acceso total; usuario inicial por configuración.                     | Todos.                      |
| **Administrador** | Acceso total a inventarios, auditoría, usuarios y roles.        | Todos.                      |
| **Operador**  | Alta/edición/export en inventarios.                                  | Ver, crear, editar, exportar y los 3 módulos (Aplicaciones, Operaciones, Cuentas). |
| **Auditor**   | Solo lectura y logs.                                                | Ver inventarios (3 módulos), Ver auditoría, Ver/exportar logs. |
| **Aprobador** | Aprobar o rechazar solicitudes en auditoría.                        | Ver inventarios (3 módulos), Ver auditoría, Aprobar. |
| **Usuario**   | Solo consulta de inventarios.                                       | Ver inventarios (3 módulos). |

No hay redundancia: cada rol tiene un conjunto distinto. Si asignas **Administrador** a alguien, tendrá todo; si quieres limitar por sector, asígnales **Operador** (o **Usuario**) y en **Permisos por rol** deja solo los módulos que quieras (por ejemplo solo Aplicaciones).
