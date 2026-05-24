# Migraciones y arranque de IITS

## Requisitos

- .NET 8 SDK
- SQL Server (local o remoto) accesible con la cadena de conexión de `appsettings.json` / `appsettings.Development.json`
- Herramienta EF Core (solo si quieres aplicar migraciones desde la consola):

  ```powershell
  dotnet tool install --global dotnet-ef
  # o actualizar:
  dotnet tool update --global dotnet-ef
  ```

## Cadena de conexión

En `appsettings.Development.json` (o `appsettings.json`) debe existir algo como:

```json
"ConnectionStrings": {
  "IITS": "Server=localhost;Database=IITS;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=true;Encrypt=Optional"
}
```

Ajusta `Server` y `Database` según tu entorno.

---

## Cómo aplicar las migraciones

### Opción A: Arrancar la aplicación (recomendado)

Al ejecutar la app **sin argumentos**, el arranque:

1. Ejecuta `Database.MigrateAsync()` y aplica todas las migraciones pendientes.
2. Si detecta que faltan tablas pero el historial tiene migraciones (por ejemplo tras un fallo previo), ejecuta la reparación (`fix-migrations` interno) y vuelve a aplicar migraciones.
3. Ejecuta el seed: Estatus, Roles, Permissions, Alojamientos, Areas, Offices, Environments, Criticalities, Categories, Vendors, DeviceModels y 2 operaciones de ejemplo (si existe la tabla `Offices`).

**No necesitas** ejecutar `dotnet ef database update` a mano si solo quieres levantar la app; la propia app actualiza la BD al iniciar.

```powershell
cd "c:\Users\luisperdomo\Desktop\Sistema de Inventarios"
dotnet run --project IITS
```

Luego abre en el navegador la URL que indique (por ejemplo `https://localhost:7xxx` o `http://localhost:5xxx`).

---

### Opción B: Aplicar migraciones desde la consola (dotnet ef)

Si prefieres aplicar migraciones **antes** de arrancar la app:

```powershell
cd "c:\Users\luisperdomo\Desktop\Sistema de Inventarios"
dotnet ef database update --project IITS --startup-project IITS
```

Esto aplica todas las migraciones pendientes según `__EFMigrationsHistory`. Después puedes arrancar la app con `dotnet run --project IITS`; el seed se ejecutará en el arranque.

---

### Opción C: Empezar desde cero (borrar BD y recrear todo)

Si quieres **eliminar la base de datos**, volver a crearla, aplicar todas las migraciones y ejecutar todo el seed (catálogos + operaciones de ejemplo + SuperAdmin):

```powershell
cd "c:\Users\luisperdomo\Desktop\Sistema de Inventarios"
dotnet run --project IITS -- reset-migrations
```

**Cuidado:** esto borra la BD actual. Úsalo en desarrollo o cuando quieras un estado limpio.

---

### Opción D: Reparar historial cuando faltan tablas

Si la BD tiene registros en `__EFMigrationsHistory` pero faltan tablas (por ejemplo tras un error a mitad de una migración), puedes “desmarcar” esas migraciones y volver a aplicarlas:

```powershell
cd "c:\Users\luisperdomo\Desktop\Sistema de Inventarios"
dotnet run --project IITS -- fix-migrations
```

Luego arranca la app **sin argumentos** para que siga el flujo normal:

```powershell
dotnet run --project IITS
```

---

## Resumen rápido

| Objetivo                         | Comando |
|----------------------------------|---------|
| Aplicar migraciones y levantar   | `dotnet run --project IITS` |
| Aplicar solo migraciones (CLI)   | `dotnet ef database update --project IITS --startup-project IITS` |
| Borrar BD, recrear y seed completo | `dotnet run --project IITS -- reset-migrations` |
| Reparar historial y reaplicar    | `dotnet run --project IITS -- fix-migrations` |

---

## Crear una nueva migración (desarrolladores)

Después de cambiar entidades en el código:

```powershell
cd "c:\Users\luisperdomo\Desktop\Sistema de Inventarios"
dotnet ef migrations add NombreDescriptivo --project IITS --startup-project IITS
```

Luego aplica con uno de los métodos anteriores (arrancar la app o `dotnet ef database update`).
