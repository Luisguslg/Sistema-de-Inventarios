# Cómo ejecutar y publicar IITS

## Ejecutar en local (desarrollo)

1. **Desde la raíz de la solución** (donde está `IITS.sln`):

   ```powershell
   cd "C:\Users\luisperdomo\Desktop\Sistema de Inventarios"
   dotnet run --project IITS
   ```

   La app arranca en `https://localhost:7xxx` (o la URL que muestre la consola). En desarrollo usa `appsettings.Development.json` y, si está configurado `Auth:Mode: Dev`, el usuario emulado de `Auth:DevUsername`.

2. **Base de datos:** Si la connection string en `appsettings.Development.json` apunta a una BD local (por ejemplo `Server=(localdb)\mssqllocaldb;Database=IITS;...`), al arrancar la app aplica migraciones y ejecuta seeds (Estatus, Roles, Permissions, etc.). La primera vez puede tardar un poco.

3. **Comandos de consola útiles** (pasados como argumentos a `dotnet run --project IITS -- ...`):
   - `read-catalogo-aplicaciones` — lista las columnas del Excel maestro "Catalogo de Aplicaciones.xlsx".
   - `reset-migrations` — borra la BD, la recrea y aplica migraciones (¡cuidado en producción!).
   - `fix-migrations` — repara historial de migraciones si faltan tablas.

## Publicar en la ruta del IIS (producción)

- **Base de datos en producción:** **IITSN** en instancia `VECCSAPP10\KPMGDV`.
- **Carpeta de la app en el servidor:** `\\veccsapp10\app` (el sitio en IIS ya apunta ahí).

### Pasos para publicar

1. **Aplicar migraciones a la BD de producción** (si hay migraciones nuevas, por ejemplo `RemoveProveedores`). Desde tu PC, con acceso a la red del servidor:

   ```powershell
   cd "C:\Users\luisperdomo\Desktop\Sistema de Inventarios"
   $env:ConnectionStrings__IITS = "Server=VECCSAPP10\KPMGDV;Database=IITSN;Trusted_Connection=True;MultipleActiveResultSets=true;Encrypt=True;TrustServerCertificate=True"
   dotnet ef database update --project IITS
   ```

   Si no tienes acceso desde tu PC, ejecuta lo anterior desde una máquina que sí tenga acceso a `VECCSAPP10\KPMGDV`, o aplica el script de la migración a mano en SSMS.

2. **Detener el Application Pool** en el servidor (IIS → Application Pools → Stop en el pool de IITSN), para evitar warnings de archivos en uso.

3. **Publicar la aplicación** en la ruta del servidor:

   ```powershell
   dotnet publish IITS/IITS.csproj -c Release -o "\\veccsapp10\app"
   ```

4. **Iniciar el Application Pool** de nuevo.

   Si da "Access denied" al escribir en la UNC: publica en `C:\Temp\IITSN-publish` y copia (tras detener el pool) con `xcopy "C:\Temp\IITSN-publish\*" "\\veccsapp10\app\" /E /Y`.

5. **Revisar en el servidor:**
   - Application Pool con identidad que tenga acceso a SQL (IITSN) y a la carpeta `\\veccsapp10\app`.
   - Autenticación de Windows habilitada en el sitio/aplicación en IIS.
   - `ASPNETCORE_ENVIRONMENT = Production` y, si usas subaplicación, `PathBase` en `web.config`.
   - `Auth:SuperAdminUsername` en `appsettings.Production.json` o en variables de entorno del App Pool.

**Duración de sesión:** Aunque la autenticación es Windows (AD), la sesión expira tras `Auth:SessionTimeoutMinutes` (por defecto 30) de inactividad; el usuario debe volver a autenticarse. Se puede configurar en `appsettings` (entre 5 y 480 minutos). Con `SlidingExpiration` activo, cada petición renueva el plazo.

Más detalle en [DESPLIEGUE_IIS.md](DESPLIEGUE_IIS.md).
