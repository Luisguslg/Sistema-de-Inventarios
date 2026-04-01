# Pasos para correr IITS en local

## 1. Liberar el puerto (si sale "address already in use")

El perfil por defecto usa el puerto **5036**. Si ya hay algo corriendo ahí:

```powershell
# Ver qué proceso usa el puerto 5036
netstat -ano | findstr :5036
# Sale una línea como:  TCP    127.0.0.1:5036   ...   LISTENING   7552
# El último número (7552 en el ejemplo) es el PID.

# Detener ese proceso (usa el PID que te salió a ti, no 7552 si el tuyo es otro):
Stop-Process -Id 7552 -Force
```

O cierra la ventana/terminal donde tenías la app corriendo.

---

## 2. Base de datos

### Opción A: Primera vez o quieres la BD vacía (reset)

Solo si quieres **borrar todo** y empezar de cero (crea de nuevo la BD y aplica migraciones + seeds):

```powershell
cd "C:\Users\luisperdomo\Desktop\Sistema de Inventarios"
dotnet run --project IITS -- reset-migrations
```

Eso borra la BD, la recrea, aplica todas las migraciones y ejecuta los seeds (Estatus, Roles, Permissions, SuperAdmin, etc.). Al terminar **sale** del programa (no deja la app corriendo).

### Opción B: BD ya existe, solo actualizar migraciones

Si la BD ya está creada y solo quieres aplicar migraciones pendientes (sin borrar datos):

```powershell
cd "C:\Users\luisperdomo\Desktop\Sistema de Inventarios"
dotnet ef database update --project IITS
```

O simplemente **no hagas nada**: al ejecutar la app (paso 3), al arrancar la app aplica las migraciones pendientes con `MigrateAsync()`.

### Opción C: Reparar historial de migraciones

Solo si ves errores del tipo "tabla ya existe" o migraciones desincronizadas:

```powershell
dotnet run --project IITS -- fix-migrations
```

---

## 3. Ejecutar la app

Desde la raíz de la solución:

```powershell
cd "C:\Users\luisperdomo\Desktop\Sistema de Inventarios"
dotnet run --project IITS
```

- Usa `appsettings.Development.json` y `launchSettings.json` (puerto 5036, entorno Development).
- La primera vez que arranque, si la BD existe, aplicará migraciones pendientes y seeds si faltan.
- Abre el navegador en: **http://localhost:5036**

En desarrollo el usuario se emula con `Auth:DevUsername` (ej. `dev.local`); no hace falta Windows Auth.

---

## Resumen rápido

| Situación | Qué hacer |
|-----------|-----------|
| Puerto 5036 ocupado | `netstat -ano \| findstr :5036` → `Stop-Process -Id <PID> -Force` |
| Primera vez o BD desde cero | `dotnet run --project IITS -- reset-migrations` (luego paso 3) |
| BD ya existe | Ir directo al paso 3 (`dotnet run --project IITS`) |
| Error raro de migraciones | `dotnet run --project IITS -- fix-migrations` |

**Para correr:** `dotnet run --project IITS` → http://localhost:5036
