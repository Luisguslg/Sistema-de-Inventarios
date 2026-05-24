# Guía de Despliegue — IITS

**Sistema:** IITS — Inventario de Infraestructura Tecnológica y Seguridad  
**Versión:** 1.0  
**Fecha:** 2026-04-01  
**Clasificación:** Interno KPMG Venezuela  

---

## 1. Prerrequisitos

### 1.1 Sistema Operativo

- Windows Server 2019 o 2022 (recomendado)
- El servidor debe estar unido al dominio Active Directory de KPMG

### 1.2 .NET 8 Runtime

Instalar el **ASP.NET Core Hosting Bundle** para .NET 8 (incluye Runtime + IIS Module):

```
Descargar desde: https://dotnet.microsoft.com/download/dotnet/8.0
→ "Hosting Bundle" (no solo Runtime ni SDK)
```

Verificar instalación:
```cmd
dotnet --version
# Debe mostrar 8.x.x
```

> Si se va a compilar la aplicación en el mismo servidor (no recomendado), instalar también el .NET 8 SDK.

### 1.3 SQL Server

- SQL Server 2019 o 2022 (Express, Standard o Enterprise)
- Instancia accesible desde el servidor IIS (TCP habilitado, puerto configurado)
- Autenticación Windows habilitada en SQL Server
- El identity del Application Pool de IIS debe tener permisos `db_owner` en la base de datos (para ejecutar migraciones EF)

Verificar conectividad desde el servidor IIS:
```cmd
sqlcmd -S SERVIDOR,PUERTO -E -Q "SELECT @@VERSION"
```

### 1.4 IIS

- IIS habilitado con los módulos:
  - ASP.NET Core Module V2 (instalado por el Hosting Bundle)
  - Windows Authentication
  - Static Content Handler

Habilitar IIS vía PowerShell:
```powershell
Install-WindowsFeature -Name Web-Server, Web-Windows-Auth, Web-Asp-Net45 -IncludeManagementTools
```

### 1.5 Herramientas de Desarrollo (solo para despliegue desde código fuente)

- Git
- .NET 8 SDK
- EF Core CLI: `dotnet tool install --global dotnet-ef`

---

## 2. Obtención del Código Fuente

```bash
git clone <URL_del_repositorio>
cd "Sistema-de-Inventarios"
```

O bien, usar el archivo ZIP publicado de la release correspondiente.

---

## 3. Configuración del Entorno

### 3.1 Archivo `appsettings.Production.json`

El archivo `appsettings.Production.json` en el repositorio es una **plantilla vacía**. Los valores reales se configuran como **variables de entorno** en `web.config` (ver sección 5.3). No editar este archivo; dejarlo como está.

```json
{
  "ConnectionStrings": { "IITS": "" },
  "Auth": {
    "Mode": "Windows",
    "SuperAdminUsername": "",
    "SessionTimeoutMinutes": 30
  },
  "Email": { "Mode": "Smtp", "From": "", "SmtpServer": "", "Port": 25, "EnableSsl": false, "UserName": "", "Password": "" }
}
```

### 3.2 Variables de Entorno a Configurar

Las siguientes variables deben configurarse en `web.config` en el servidor de producción (ver sección 5.3). **Nunca** guardarlas en el repositorio.

| Variable de entorno | Descripción | Ejemplo |
|---|---|---|
| `ASPNETCORE_ENVIRONMENT` | Nombre del entorno | `Production` |
| `ConnectionStrings__IITS` | Cadena de conexión a SQL Server | `Server=SERVIDOR,PUERTO;Database=IITSN;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True;` |
| `Auth__SuperAdminUsername` | Cuenta AD del primer SuperAdmin | `DOMINIO\usuario` |
| `Auth__SessionTimeoutMinutes` | Minutos de inactividad antes de expirar sesión | `30` |
| `Email__Mode` | `Smtp` para producción, `Dev` para desarrollo | `Smtp` |
| `Email__From` | Dirección de origen de correos | `IITS <nombre@relay.kpmg.com>` |
| `Email__SmtpServer` | Servidor SMTP relay | `goemairs.go.kworld.kpmg.com` |
| `Email__Port` | Puerto SMTP | `25` |
| `Email__EnableSsl` | SSL para SMTP | `false` |
| `Email__UserName` | Usuario SMTP (si el relay requiere auth) | *(dejar vacío si es relay de confianza)* |
| `Email__Password` | Contraseña SMTP | *(dejar vacío si es relay de confianza)* |
| `PathBase` | Ruta base de la aplicación en IIS | `IITSN` |
| `App__BaseUrl` | URL base pública para enlaces en correos | `https://servidor.intranet.kpmg.com/IITSN` |

---

## 4. Compilación y Publicación

### 4.1 Publicar la Aplicación

Desde la raíz del repositorio:

```bash
cd IITS
dotnet publish -c Release -o C:\inetpub\wwwroot\IITSN
```

Esto genera la aplicación compilada en la carpeta destino. La carpeta debe ser accesible por el Application Pool de IIS.

### 4.2 Verificar la Compilación

```bash
dotnet build -c Release
# Debe completarse sin errores
```

---

## 5. Configuración de IIS

### 5.1 Crear el Application Pool

1. Abrir **IIS Manager** → **Application Pools** → **Add Application Pool**
2. Nombre: `IITSN_Pool`
3. **.NET CLR Version:** `No Managed Code`
4. **Managed Pipeline Mode:** `Integrated`
5. Hacer clic en OK

Configurar la identidad del Application Pool:
1. Seleccionar `IITSN_Pool` → **Advanced Settings**
2. **Identity:** `ApplicationPoolIdentity` (o una cuenta de servicio de dominio si se requiere acceso a recursos de red)
3. **Enable 32-Bit Applications:** `False`

### 5.2 Crear el Sitio / Aplicación Web en IIS

**Opción A: Aplicación bajo un sitio existente**
1. En IIS Manager, expandir el sitio existente (ej. "Default Web Site")
2. Clic derecho → **Add Application**
3. **Alias:** `IITSN`
4. **Application Pool:** `IITSN_Pool`
5. **Physical Path:** `C:\inetpub\wwwroot\IITSN`

**Opción B: Sitio independiente**
1. Clic derecho en **Sites** → **Add Website**
2. Configurar nombre, Application Pool, ruta física y binding (puerto/HTTPS)

### 5.3 Configurar la Autenticación en IIS

Para el sitio/aplicación IITSN:

1. Seleccionar la aplicación en IIS Manager
2. Doble clic en **Authentication**
3. Configurar:
   - **Anonymous Authentication:** `Disabled`
   - **Windows Authentication:** `Enabled`

Configurar el orden de proveedores de Windows Authentication:
1. Con **Windows Authentication** seleccionado → clic en **Providers** (panel derecho)
2. Asegurar que `Negotiate` esté **primero** en la lista, y `NTLM` segundo
3. Si `Negotiate` no aparece, agregar: clic en **Add** → seleccionar `Negotiate`
4. Usar las flechas para mover `Negotiate` al primer lugar

> **Importante:** El orden `Negotiate` antes de `NTLM` favorece Kerberos sobre NTLM, lo cual es requerido para que la autenticación Windows funcione correctamente con el middleware de la aplicación.

### 5.4 Configurar `web.config`

Editar el archivo `web.config` en `C:\inetpub\wwwroot\IITSN\web.config` para agregar las variables de entorno con los valores reales del entorno de producción:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <location path="." inheritInChildApplications="false">
    <system.webServer>
      <handlers>
        <add name="aspNetCore" path="*" verb="*" modules="AspNetCoreModuleV2" resourceType="Unspecified" />
      </handlers>
      <aspNetCore processPath="dotnet"
                  arguments=".\IITS.dll"
                  stdoutLogEnabled="true"
                  stdoutLogFile=".\logs\stdout"
                  hostingModel="inprocess"
                  forwardWindowsAuthToken="true">
        <environmentVariables>
          <environmentVariable name="ASPNETCORE_ENVIRONMENT" value="Production" />
          <environmentVariable name="ConnectionStrings__IITS" value="Server=SERVIDOR,PUERTO;Database=IITSN;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True;" />
          <environmentVariable name="Auth__SuperAdminUsername" value="DOMINIO\usuario.admin" />
          <environmentVariable name="Auth__SessionTimeoutMinutes" value="30" />
          <environmentVariable name="Email__Mode" value="Smtp" />
          <environmentVariable name="Email__From" value="IITS &lt;correo@relay.kpmg.com&gt;" />
          <environmentVariable name="Email__SmtpServer" value="servidor.smtp.kpmg.com" />
          <environmentVariable name="Email__Port" value="25" />
          <environmentVariable name="Email__EnableSsl" value="false" />
          <environmentVariable name="PathBase" value="IITSN" />
          <environmentVariable name="App__BaseUrl" value="https://servidor.intranet.kpmg.com/IITSN" />
        </environmentVariables>
      </aspNetCore>
    </system.webServer>
  </location>
</configuration>
```

> **Nota de seguridad:** `web.config` contiene credenciales y debe tener permisos de sistema de archivos restringidos. Solo el Application Pool identity y los administradores del servidor deben tener acceso de lectura. No incluir `web.config` en el repositorio si contiene valores reales (ya está en `.gitignore`).

---

## 6. Inicialización de la Base de Datos

### 6.1 Opción A: Migraciones Automáticas (Recomendado)

Al arrancar por primera vez, IITS ejecuta automáticamente:
```
db.Database.MigrateAsync()
```

Esto crea la base de datos (si no existe) y aplica todas las migraciones pendientes. También ejecuta los métodos `Ensure*Async` para poblar los catálogos semilla (Estatus, Roles, Permisos, Alojamientos, Áreas, Oficinas, Ambientes, Criticidades, Categorías, Fabricantes, Modelos).

**Requisito:** El identity del Application Pool debe tener permiso `db_owner` en la base de datos, o bien permiso para crear la base de datos si no existe.

### 6.2 Opción B: Migraciones Manuales con EF CLI

Si se prefiere controlar manualmente las migraciones:

```bash
# Desde el directorio IITS/ del repositorio
cd IITS

# Aplicar migraciones y seed inicial
dotnet run reset-migrations
```

> **ADVERTENCIA:** `reset-migrations` **elimina y recrea la base de datos**. Usar solo en instalaciones nuevas o entornos de desarrollo. En producción con datos existentes, usar `dotnet ef database update` o dejar que la app aplique las migraciones al arrancar.

```bash
# Alternativa segura para actualizar solo:
dotnet ef database update
```

### 6.3 Primer Usuario SuperAdmin

El usuario SuperAdmin se crea automáticamente al arrancar la aplicación si la cuenta configurada en `Auth__SuperAdminUsername` no existe en la tabla `Users`.

**Para verificar:** Ingresar a la aplicación con la cuenta de dominio configurada. Si el sistema muestra la interfaz completa con acceso al menú de Administración, el SuperAdmin se aprovisionó correctamente.

**Para verificar en base de datos:**
```sql
SELECT u.Username, r.Nombre as Rol
FROM Users u
JOIN UserRole ur ON u.Id = ur.UserId
JOIN Roles r ON ur.RoleId = r.Id
WHERE r.Nombre = 'SuperAdmin'
```

---

## 7. Verificación Post-Instalación

### 7.1 Lista de Verificación

- [ ] La aplicación responde en `http(s)://servidor/IITSN/`
- [ ] La autenticación Windows redirige correctamente (no aparece el cuadro de credenciales si el navegador tiene SSO de dominio configurado)
- [ ] El usuario SuperAdmin puede acceder a Administración → Usuarios
- [ ] Los catálogos están poblados (Administración → Maestro de Datos muestra Oficinas, Áreas, etc.)
- [ ] La exportación de un módulo vacío genera un archivo válido sin error
- [ ] El log de auditoría registra el primer acceso
- [ ] Los correos en `EmailOutbox` se procesan (verificar en BD: `SELECT Status, COUNT(*) FROM EmailOutbox GROUP BY Status`)

### 7.2 Prueba de Autenticación

1. Abrir el navegador **en un equipo del dominio** (no en el servidor mismo)
2. Navegar a `http(s)://servidor/IITSN/`
3. Resultado esperado: Acceso directo sin cuadro de credenciales (SSO Kerberos)
4. Verificar que el nombre del usuario aparece en la interfaz

### 7.3 Prueba de Rate Limiting

```bash
# Ejecutar más de 10 solicitudes de exportación en 1 minuto
for i in {1..12}; do
  curl -s -o /dev/null -w "%{http_code}\n" -k "https://servidor/IITSN/api/export/Aplicaciones/xlsx" --negotiate -u :
done
# Las primeras 10 deben retornar 200, las siguientes 429
```

---

## 8. Permisos de Sistema de Archivos

| Carpeta / Archivo | Identity | Permiso |
|---|---|---|
| `C:\inetpub\wwwroot\IITSN\` | `IIS AppPool\IITSN_Pool` | Lectura y Ejecución |
| `C:\inetpub\wwwroot\IITSN\Logs\` | `IIS AppPool\IITSN_Pool` | Lectura, Escritura |
| `C:\inetpub\wwwroot\IITSN\web.config` | `IIS AppPool\IITSN_Pool` | Solo Lectura |
| `C:\inetpub\wwwroot\IITSN\web.config` | Administradores | Control Total |

La carpeta `Logs\` se crea automáticamente al arrancar la aplicación. Si falla la creación, el Application Pool debe tener permisos de escritura en la carpeta raíz de la aplicación.

---

## 9. Solución de Problemas Comunes

### Problema 1: HTTP 500.30 — ASP.NET Core App Failed to Start

**Causa más común:** Falta el .NET 8 Hosting Bundle o la cadena de conexión es incorrecta.

**Diagnóstico:**
1. Revisar `startup_error.txt` en `C:\inetpub\wwwroot\IITSN\` (se crea automáticamente si la app falla al arrancar).
2. Habilitar logs stdout en `web.config` (`stdoutLogEnabled="true"`) y revisar `.\logs\stdout*.log`.
3. Verificar que el .NET 8 Runtime está instalado: `dotnet --version`

---

### Problema 2: HTTP 401 — Acceso Denegado en Bucle

**Causa:** El proveedor de autenticación IIS no tiene Negotiate configurado correctamente, o la autenticación anónima está habilitada.

**Diagnóstico:**
1. Verificar en IIS Manager que **Anonymous Authentication** está **Disabled**.
2. Verificar que **Windows Authentication** está **Enabled**.
3. Verificar que el proveedor `Negotiate` está primero en la lista de proveedores.
4. Verificar que `forwardWindowsAuthToken="true"` está en `web.config`.

---

### Problema 3: HTTP 400 — Request Too Long

**Causa:** El usuario tiene demasiados grupos de AD, generando headers HTTP excesivos al usar autenticación Kerberos.

**Diagnóstico/Solución:**
- IITS mitiga esto almacenando solo `ClaimTypes.Name` en la cookie de sesión.
- Si el error ocurre en el handshake inicial (antes de la cookie), el header Kerberos es demasiado grande.
- Solución: Reducir los grupos de AD del usuario, o configurar `MaxFieldLength` y `MaxRequestBytes` en el registro de Windows para IIS.

```reg
HKEY_LOCAL_MACHINE\System\CurrentControlSet\Services\HTTP\Parameters
MaxFieldLength = 65534 (DWORD)
MaxRequestBytes = 16777216 (DWORD)
```

---

### Problema 4: La Aplicación Carga pero el Usuario No Tiene Roles

**Causa:** El `Username` del usuario en la tabla `Users` no coincide con el formato retornado por `identity.Name` de Windows.

**Diagnóstico:**
```sql
-- Verificar el username almacenado en BD
SELECT Username FROM Users WHERE Username LIKE '%nombre.usuario%'

-- Verificar que el formato coincide con el dominio:
-- DOMINIO\nombre.usuario   o   nombre.usuario
```

**Solución:**
- Editar el `Username` del usuario en BD para que coincida con el valor retornado por Windows Authentication (puede ser `DOMINIO\usuario` o solo `usuario`).
- Alternativamente, `IITSClaimsTransformation` busca por prefijo de dominio: `u.Username == name || u.Username == usernameToFind || EF.Functions.Like(u.Username, "%" + suffix)`.

---

### Problema 5: Los Correos No Se Envían

**Causa:** El servidor SMTP no acepta conexiones desde el servidor IIS, o la configuración SMTP es incorrecta.

**Diagnóstico:**
```sql
-- Verificar correos en cola
SELECT Status, Error, RetryCount, CreatedAt 
FROM EmailOutbox 
ORDER BY CreatedAt DESC
```

- `Status = 'Failed'` con `Error` no nulo indica el error de envío.
- Verificar conectividad SMTP: `telnet goemairs.go.kworld.kpmg.com 25` desde el servidor IIS.
- Verificar que `Email__Mode` es `Smtp` (no `Dev`).

---

### Problema 6: Migraciones Fallan al Arrancar (`fix-migrations`)

**Causa:** La tabla `__EFMigrationsHistory` tiene registros de migraciones que no se aplicaron correctamente (tablas faltantes).

**Solución:**
```bash
# Desde el directorio IITS/ del repositorio, con la conexión configurada correctamente
dotnet run fix-migrations
```

Este comando elimina las entradas de migraciones problemáticas del historial y las vuelve a aplicar.

---

### Problema 7: Error "Object already exists" al Arrancar (SQL 2714)

**Causa:** La tabla existe en la BD pero no está en `__EFMigrationsHistory` (historial de migraciones vacío o inconsistente).

**Diagnóstico:** IITS detecta este error (SQL 2714) automáticamente y ejecuta `SyncMigrationHistoryAsync` para registrar las migraciones ya aplicadas. Si el error persiste:

```sql
-- Verificar qué tablas existen
SELECT name FROM sys.objects WHERE type = 'U' ORDER BY name

-- Verificar el historial de migraciones
SELECT * FROM __EFMigrationsHistory ORDER BY MigrationId
```

**Solución manual:**
```bash
dotnet run fix-migrations
```

---

## 10. Actualización de la Aplicación

### 10.1 Proceso de Actualización

1. **Detener el Application Pool** en IIS Manager (para evitar que IIS bloquee los archivos)
2. **Hacer backup** de la carpeta de despliegue y de la base de datos
3. **Publicar la nueva versión:**
   ```bash
   cd IITS
   dotnet publish -c Release -o C:\inetpub\wwwroot\IITSN
   ```
4. **Iniciar el Application Pool** en IIS Manager
5. Al arrancar, IITS aplica automáticamente las migraciones pendientes
6. Verificar `startup_error.txt` si hay problemas

### 10.2 Rollback

1. Detener el Application Pool
2. Restaurar la carpeta de la versión anterior desde backup
3. Restaurar la base de datos si hubo migraciones destructivas
4. Iniciar el Application Pool

---

## 11. Catálogos de Datos Semilla

Al arrancar, IITS inicializa los siguientes catálogos si no existen:

| Catálogo | Valores semilla |
|---|---|
| Estatus | Activo (1000), Inactivo (1500), Desincorporado (2000) |
| Roles | SuperAdmin, Administrador, Operador, Aprobador, Auditor, Usuario |
| Permisos | 14 permisos (ver PermissionCodes.cs) |
| Alojamientos | On-Premise, Cloud Privada, Cloud Pública, Híbrida, Colocación, SaaS |
| Áreas | Auditoría, Impuesto y Legal, Asesoría, Infraestructura, Otras |

Para cargar catálogos adicionales de tecnología (Oficinas, Ambientes, Criticidades, etc.) con datos de ejemplo:
```bash
dotnet run seed-catalogs
```

---

## 12. Soporte y Contacto

Para problemas relacionados con el despliegue de IITS, contactar al equipo de desarrollo o revisar:
- `startup_error.txt` en la carpeta de la aplicación
- Logs de IIS: `C:\inetpub\wwwroot\IITSN\logs\stdout*.log`
- Event Viewer de Windows: Applications and Services Logs → Microsoft → Windows → IIS-Configuration
