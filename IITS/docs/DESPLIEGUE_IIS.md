# Despliegue en IIS – Guía completa

---

## Despliegue IITSN en \\veccsapp10\app (producción actual)

- **Base de datos:** **IITSN** en instancia `VECCSAPP10\KPMGDV` (no se toca la base antigua IITS).
- **Carpeta de la app:** `\\veccsapp10\app` (el sitio en IIS ya está configurado para usar esta ruta).
- **Configuración:** `appsettings.Production.json` ya trae ConnectionString (IITSN), Email (relay KPMG) y Auth. Solo falta ajustar `Auth:SuperAdminUsername` con el usuario real (ej. `VE\tu_usuario`) si no es `VE\usuario_superadmin`.

### Publicar en la ruta del servidor

**Para evitar warnings de archivos en uso:** Detener el Application Pool en el servidor (IIS → Application Pools → Stop) antes de publicar; luego iniciarlo de nuevo.

Desde la raíz de la solución (donde está `IITS.sln`), con permisos de escritura en `\\veccsapp10\app`:

```powershell
dotnet publish IITS/IITS.csproj -c Release -o "\\veccsapp10\app"
```

Si da “Access denied” al publicar directo a la UNC, publica en una carpeta local y copia después:

```powershell
dotnet publish IITS/IITS.csproj -c Release -o "C:\Temp\IITSN-publish"
xcopy "C:\Temp\IITSN-publish\*" "\\veccsapp10\app\" /E /Y
```

Asegúrate de que `\\veccsapp10\app` existe y que tu usuario tiene permisos de escritura.

### Crear la base IITSN y aplicar migraciones (recomendado desde tu PC)

1. **Crear la base** en SSMS conectado a `VECCSAPP10\KPMGDV`: ejecutar `CREATE DATABASE IITSN;`.

2. **Aplicar migraciones desde tu PC** (así la BD queda lista y la app en el servidor no depende de que MigrateAsync conecte bien al arranque):

   En PowerShell, desde la raíz de la solución:

   ```powershell
   $env:ConnectionStrings__IITS = "Server=VECCSAPP10\KPMGDV;Database=IITSN;Trusted_Connection=True;MultipleActiveResultSets=true;Encrypt=True;TrustServerCertificate=True"
   dotnet ef database update --project IITS
   ```

   (Si tu PC no está en el mismo dominio o no tiene acceso a `VECCSAPP10\KPMGDV`, hazlo desde una máquina que sí tenga acceso o desde el propio servidor.)

3. **Publicar** y desplegar en `\\veccsapp10\app`.

La app al arrancar seguirá intentando conectar para el seed y las comprobaciones; si la connection string en el servidor no funciona, verás el error en `logs\stdout_*.log` o en `startup_error.txt`.

### Connection string en el servidor (igual que en Sumando Valor)

En **Sumando Valor** la connection string y el resto de la config del servidor van en el **web.config**, dentro de `<aspNetCore><environmentVariables>`. Así no dependen de appsettings en disco y evitan Named Pipes.

En IITSN el **web.config** del proyecto ya incluye esas variables de entorno. La connection string usa **Server=VECCSAPP10,61057** (host + puerto TCP), igual que Sumando Valor, para forzar TCP y no usar Named Pipes. Al publicar, ese web.config se despliega con la app.

- Si la instancia **KPMGDV** usa **otro puerto** (no 61057), en el servidor edita `\\veccsapp10\app\web.config` y cambia en `ConnectionStrings__IITS` el puerto: `Server=VECCSAPP10,PUERTO`. El puerto se ve en SQL Server Configuration Manager → Protocolos para la instancia → TCP/IP → Puertos.
- Si quieres usar nombre de instancia en lugar de puerto, en esa misma variable puedes poner `Server=VECCSAPP10\KPMGDV;...` y asegurarte de que TCP/IP esté habilitado para esa instancia.

### Login failed for user 'VE\VECCSAPP10$'

Ese usuario es la **cuenta del equipo** (Application Pool con Identity = ApplicationPoolIdentity). **No hace falta crear logins ni usuarios en la BD de producción.** En su lugar, cambia el Application Pool para que use la **misma cuenta de servicio** que el resto de apps (por ejemplo **ve-svcDevelopment** u otra que ya tenga acceso a SQL):

1. En IIS → Application Pools → el pool que usa el sitio IITSN (ej. el que apunta a `\\veccsapp10\app`).
2. Clic derecho → **Advanced Settings** → **Identity**.
3. Cambiar de **ApplicationPoolIdentity** a **Custom account** → **Set** → usuario tipo **VE\ve-svcDevelopment** (o el que uséis) y la contraseña correspondiente.
4. **OK** y reiniciar el Application Pool.

Así la app conecta con esa cuenta; si esa cuenta ya tiene login y acceso a la base IITSN (o el DBA la da de alta una vez), no se toca nada más en la BD.

### Qué revisar en el servidor

1. **Application Pool:** Identity con acceso a `VECCSAPP10\KPMGDV` y a la BD **IITSN** (login en SQL para esa cuenta si usas Trusted_Connection).
2. **Permisos de carpeta:** La cuenta del App Pool debe poder leer y escribir en `\\veccsapp10\app` (y en la subcarpeta `Logs` que la app crea).
3. **Variable de entorno:** En el App Pool o en el sitio, `ASPNETCORE_ENVIRONMENT = Production`.
4. **SuperAdminUsername:** En `appsettings.Production.json` está `VE\usuario_superadmin`; cámbialo por el usuario que debe ser superadmin (debe existir en la tabla **Users** con ese `Username`, o el seed lo crea la primera vez si coincide).
5. **Duración de sesión:** Aunque la autenticación es Windows (AD), la sesión tiene una duración limitada. En `Auth:SessionTimeoutMinutes` (por defecto 30) se define el tiempo en minutos; tras ese tiempo de inactividad la sesión expira y el usuario debe volver a autenticarse con Windows. Se puede ajustar entre 5 y 480 minutos (8 h). `SlidingExpiration` está activo: cada petición renueva el plazo.

Si el relay de correo pide usuario/contraseña, pon `Email:UserName` y `Email:Password` en el JSON del servidor (o mejor en variable de entorno `Email__Password`) y no las subas al repo.

### Error 500.30 (inicio de la app fallido) – Dónde ver el error

1. **Log de stdout (recomendado):** En la carpeta de la app (ej. `\\veccsapp10\app`) hay una subcarpeta **`logs`**; el módulo de ASP.NET Core escribe ahí archivos **`stdout_*.log`** con todo lo que la app escribe en consola (incluida la excepción). Revisa el más reciente.
2. **Archivo en la raíz:** Si el fallo ocurre durante el arranque (migraciones, seed, etc.), la app escribe además en **`startup_error.txt`** en la raíz de la carpeta de la app.
3. **Visor de eventos de Windows:** En el servidor, Visor de eventos → Registros de Windows → Aplicación; busca orígenes “IIS AspNetCore Module V2” o “ASP.NET Core Module”.

Asegúrate de que la cuenta del Application Pool tenga **permisos de escritura** en la carpeta de la app (para que se puedan crear `logs` y `startup_error.txt`).

### 404 "cada rato" y "Application is shutting down"

- **404:** Si entras por una **subaplicación** (ej. `http://veccsapp10/IITSN/`), en el **web.config** del servidor añade la variable de entorno **PathBase** = **IITSN** (o el path que uses). Así la app genera bien los enlaces y Blazor no pide recursos en la ruta equivocada. Si la app está en la **raíz** del sitio (`http://veccsapp10/`), no hace falta PathBase.
- **"Application is shutting down"** en el log: el **Application Pool** suele tener **Idle Time-out** (p. ej. 20 min). Al estar inactivo, IIS cierra el proceso y la siguiente petición puede devolver 404 o reiniciar la app. Para que no se cierre: en IIS → Application Pools → tu pool → **Advanced Settings** → **Process Model** → **Idle Time-out (minutes)** = **0** (desactiva el cierre por inactividad).

El 404 no depende del usuario con el que entras (Windows Auth); depende de la ruta y del PathBase si usas subaplicación, y de que el pool no esté reciclando.

### Solo se ven "Inicio" e "Inventarios" (no Auditoría, Logs, Administración, Data)

Eso lo define **Active Directory + la base de datos**: el usuario con el que entras (Windows) debe existir en la tabla **Users** y tener rol SuperAdmin, Administrador o Auditor. El seed crea el usuario que pongas en **Auth:SuperAdminUsername** y le asigna SuperAdmin al arrancar.

**Cómo ver tu usuario y ponerte tú como SuperAdmin**

1. Entra a la app (aunque solo veas Inicio e Inventarios). En la **barra superior a la derecha** verás **"Conectado como: X"** — ese **X** es el usuario que recibe la app (puede ser `VE\luisperdomo`, `luisperdomo` u otro formato según el dominio).
2. **Copia ese valor tal cual** (con o sin `VE\`, tal como salga).
3. En el servidor, edita **web.config** (carpeta de la app) y en `<environmentVariables>` pon o cambia:
   ```xml
   <environmentVariable name="Auth__SuperAdminUsername" value="X" />
   ```
   sustituyendo **X** por lo que viste en "Conectado como".
4. **Reinicia el Application Pool** del sitio (o recicla la app) para que la app arranque de nuevo y el seed cree/asigne ese usuario como SuperAdmin.
5. Vuelve a entrar; deberías ver ya Auditoría, Logs y Administración.

Si el valor tiene backslash (ej. `VE\luisperdomo`), en XML escríbelo tal cual; no hace falta escaparlo en el `value`.

**Si sale "No autenticado"** (desde el servidor o desde tu PC) la app no recibe el usuario de Windows. Hay que forzar que IIS exija Windows y reenvíe la identidad:

1. **Desactivar Autenticación anónima** en el **sitio o aplicación** que sirve IITSN (no solo en el sitio raíz). Si la anónima está habilitada, IIS responde sin pedir Windows y la app recibe usuario anónimo.
   - IIS Manager → Sitio (o la **Aplicación** donde está IITSN) → **Autenticación**.
   - **Autenticación anónima** → **Deshabilitar** (o en "Editar" poner la identidad del App Pool si prefieres que las peticiones anónimas usen esa cuenta; lo importante es que no gane siempre la anónima).
2. **Activar Autenticación de Windows** en ese mismo sitio/aplicación.
   - **Autenticación de Windows** → **Habilitar**.
   - **Proveedores** → abrir y asegurar que **Negotiate** esté primero y **NTLM** después.
3. **web.config**: en el servidor el **web.config** de la app debe tener en `<aspNetCore>` el atributo **forwardWindowsAuthToken="true"** (ya va en el del proyecto; si lo has editado a mano, compruébalo).
4. **Reiniciar** el Application Pool y volver a abrir la URL. El navegador debería pedir usuario/contraseña de dominio o usar la sesión actual; entonces "Conectado como" mostrará tu usuario.

Si IITSN es una **aplicación** bajo un sitio (ej. sitio "Default" y aplicación "IITSN"), abre **Autenticación** sobre ese **nodo de la aplicación IITSN** y aplica los pasos ahí; a veces la herencia hace que no se use la config del padre.

---

## Resumen rápido

| Tema | Estado |
|------|--------|
| **Active Directory** | **Ya integrado.** La app usa autenticación Windows (Negotiate). En IIS con “Autenticación de Windows” habilitada, el usuario que entra es el de AD (`DOMINIO\usuario`). El `IITSClaimsTransformation` busca ese usuario en la tabla **Users** y carga roles/permisos desde **UserRoles** y **RolePermissions**. |
| **Correo** | **Sí envía.** Con `Email:Mode: Smtp` y tabla **EmailOutbox** existente, el `EmailOutboxHostedService` envía los correos encolados cada ~30 s. En desarrollo con `Email:Mode: Dev` solo se registra en log. |

---

## 1. Publicar la aplicación

En la raíz de la solución (donde está `IITS.sln`):

```powershell
dotnet publish IITS/IITS.csproj -c Release -o C:\inetpub\IITS
```

- **Salida:** toda la app publicada en `C:\inetpub\IITS` (o la ruta que elijas): DLLs, `web.config`, `appsettings.json`, `appsettings.Production.json`, etc.
- Si quieres otra ruta: cambia `C:\inetpub\IITS` por la carpeta destino.

---

## 2. Base de datos en el servidor

### 2.1 Crear la base de datos

En el servidor donde esté SQL Server:

1. Crear la base de datos (por ejemplo `IITS`):
   - SQL Server Management Studio → Nueva consulta en el servidor → ejecutar:
   ```sql
   CREATE DATABASE IITS;
   ```
2. Definir el **usuario/rol** que usará la app:
   - Si la app se conecta con **Trusted_Connection=True** (cuenta del equipo o del Application Pool), la cuenta de Windows que ejecuta el App Pool debe tener acceso al SQL Server (login + usuario en `IITS` con permisos de `db_owner` o al menos para crear tablas y ejecutar migraciones).
   - Si prefieres usuario SQL: crear login y usuario en `IITS` y en la connection string usar `User Id=...;Password=...` en lugar de `Trusted_Connection=True`.

### 2.2 Aplicar el esquema (migraciones)

**Opción A – Desde el servidor de desarrollo (recomendado la primera vez)**  
Con la connection string apuntando al SQL del **servidor de producción**:

```powershell
cd "C:\Users\luisperdomo\Desktop\Sistema de Inventarios"
$env:ConnectionStrings__IITS = "Server=NombreServidorSQL;Database=IITS;Trusted_Connection=True;..."
dotnet ef database update --project IITS
```

**Opción B – Al arrancar la app en IIS**  
La aplicación ya ejecuta `MigrateAsync()` al inicio: si la connection string en el servidor apunta a una BD vacía o sin tablas, creará las tablas y datos iniciales (Estatus, Roles, etc.). Las tablas “reparables” (AprobacionPermisos, Permissions, Partes, Alojamientos, etc.) también se crean al arranque si no existen.

**Connection string típica en producción (en appsettings.Production.json o variable de entorno):**

```json
"ConnectionStrings": {
  "IITS": "Server=.;Database=IITS;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=true;Encrypt=Optional"
}
```

- `Server=.` o `Server=NombreInstancia` según tu SQL Server.
- Si el App Pool usa una cuenta de dominio que tiene acceso a SQL, `Trusted_Connection=True` es suficiente.

---

## 3. Carpeta de publicación y Logs

- **Carpeta de publish:** la que indicaste en el `-o` (ej. `C:\inetpub\IITS`). El sitio en IIS debe apuntar a esta carpeta como “ruta física”.
- **Carpeta Logs:** la app usa una subcarpeta `Logs` dentro de esa misma ruta (ContentRootPath). Se crea sola al arranque si no existe. El **identity del Application Pool** debe tener **permiso de escritura** en la carpeta de la app (incluida `Logs`) para que los logs se escriban bien.

Para dar permisos a la carpeta (ejemplo si el App Pool se llama `IITS AppPool`):

1. Clic derecho en `C:\inetpub\IITS` → Propiedades → Seguridad.
2. Editar → Agregar → `IIS AppPool\IITS AppPool` (o el nombre de tu App Pool).
3. Marcar al menos **Lectura y ejecución** y **Escritura** (o “Modificar”) para que pueda crear y escribir en `Logs`.

---

## 4. Configurar el sitio en IIS

### 4.1 Application Pool

- Crear un **Application Pool** (ej. `IITS AppPool`).
- **.NET CLR version:** **No Managed Code**.
- **Modo de canalización:** Integrado.
- **Identity:**
  - Si la app y SQL usan la misma máquina y quieres Trusted_Connection: puede ser `ApplicationPoolIdentity` y crear en SQL un login para `IIS AppPool\IITS AppPool`, o usar una **cuenta de dominio** que tenga acceso a SQL y a AD.

### 4.2 Sitio o aplicación

- Crear un **Sitio** o una **Aplicación** bajo un sitio existente.
- **Ruta física:** `C:\inetpub\IITS` (la carpeta del publish).
- Asignar el Application Pool creado.

### 4.3 Autenticación Windows (Active Directory)

- En el **Sitio** o **Aplicación** → **Autenticación**.
- **Autenticación anónima:** Deshabilitada (o habilitada solo si en algún escenario lo necesitas; con Negotiate suele ir deshabilitada).
- **Autenticación de Windows:** **Habilitada**.
- **Proveedores:** **Negotiate** y **NTLM** (Negotiate primero).

Así, IIS enviará el usuario de Windows (AD) a la app y `User.Identity.Name` será `DOMINIO\usuario`.

### 4.4 web.config

Tras `dotnet publish`, en la carpeta publicada ya viene un `web.config` correcto para el módulo ASP.NET Core. No hace falta editarlo salvo casos especiales (por ejemplo, variables de entorno). El módulo ya reenvía la identidad Windows a la app.

---

## 5. Configuración de la aplicación en el servidor

### 5.1 Variables de entorno (recomendado en producción)

En el Application Pool o en el sitio:

- `ASPNETCORE_ENVIRONMENT` = `Production`

Así la app cargará `appsettings.Production.json`.

### 5.2 Archivos de configuración

En la carpeta publicada (`C:\inetpub\IITS`) puedes:

- **Dejar** `appsettings.Production.json` que viene del publish y **sobrescribir** en el servidor solo las cadenas sensibles (connection string, SMTP, SuperAdmin), o
- Usar **variables de entorno** para no dejar secretos en disco, por ejemplo:
  - `ConnectionStrings__IITS`
  - `Auth__SuperAdminUsername`
  - `Email__SmtpServer`, `Email__UserName`, `Email__Password`, etc.

Ejemplo de **appsettings.Production.json** en el servidor (ajustar valores):

```json
{
  "ConnectionStrings": {
    "IITS": "Server=.;Database=IITS;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=true;Encrypt=Optional"
  },
  "Auth": {
    "Mode": "Windows",
    "SuperAdminUsername": "DOMINIO\\usuario_superadmin"
  },
  "Email": {
    "Mode": "Smtp",
    "From": "noreply@tudominio.com",
    "SmtpServer": "smtp.relay.tudominio.com",
    "Port": 25,
    "EnableSsl": false,
    "UserName": "",
    "Password": ""
  }
}
```

- **SuperAdminUsername:** debe coincidir con un registro en la tabla **Users** (campo `Username`), por ejemplo `DOMINIO\usuario_superadmin`. El seed puede crear ese usuario la primera vez si lo configuraste en `Auth:SuperAdminUsername`.
- **Email:** con `Mode: Smtp` y la tabla **EmailOutbox** creada, los correos encolados se envían automáticamente. Si el relay exige usuario/contraseña, rellena `UserName` y `Password` (mejor por variable de entorno).

---

## 6. HTTPS (producción)

- En IIS, en **Enlaces** del sitio, añadir un enlace **https** con el certificado correspondiente.
- La app ya usa `UseHsts()` y `UseHttpsRedirection()` cuando no está en Development, así que en producción las redirecciones HTTPS funcionan.

---

## 7. Comprobación

1. Navegar a la URL del sitio (ej. `https://tuservidor/tuis`).
2. Debe pedir autenticación Windows (o entrar con el usuario ya autenticado) y cargar la app.
3. En la app, el usuario actual corresponde al de AD; roles y permisos se leen de **Users**, **UserRoles** y **RolePermissions**.
4. Revisar **Logs** (carpeta `Logs` en la ruta de publish) y el **Visor de eventos de Windows** (IIS / ASP.NET Core Module) si hay 500 o 401.

---

## 8. Resumen de pasos (checklist)

1. **Publicar:** `dotnet publish IITS/IITS.csproj -c Release -o C:\inetpub\IITS`
2. **BD:** Crear base `IITS` en SQL Server y dar acceso al usuario/identity que usa la app.
3. **Esquema:** Ejecutar migraciones (`dotnet ef database update`) o dejar que la app las aplique al primer arranque.
4. **IIS:** Application Pool “No Managed Code”, sitio apuntando a la carpeta del publish, Autenticación de Windows habilitada (Negotiate + NTLM).
5. **Permisos:** La identity del App Pool con lectura/escritura en la carpeta de publish (incluida `Logs`).
6. **Config:** `ASPNETCORE_ENVIRONMENT=Production` y en `appsettings.Production.json` (o variables) configurar ConnectionStrings, Auth:SuperAdminUsername y Email si usas SMTP.
7. **HTTPS:** Enlace https en IIS con certificado.

Con esto la app queda integrada con Active Directory (autenticación Windows) y, con `Email:Mode: Smtp`, enviando correos desde la cola **EmailOutbox**.
