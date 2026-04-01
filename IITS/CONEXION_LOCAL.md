# Conexión a base de datos local

Si estás probando en local y los datos (seed, dashboard con 0s) no aparecen, verifica que la app use tu base de datos local.

## 1. Comprobar qué BD usa la app

La cadena de conexión está en:
- `appsettings.json` – valores por defecto
- `appsettings.Development.json` – se usa cuando `ASPNETCORE_ENVIRONMENT=Development` (típico con `dotnet run`)

## 2. Asegurar uso de BD local

En `appsettings.Development.json` revisa `ConnectionStrings:IITS`:

```json
"ConnectionStrings": {
  "IITS": "Server=localhost;Database=IITS;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=true;Encrypt=Optional"
}
```

- **SQL Server (local):** `Server=localhost` o `Server=.\SQLEXPRESS`
- **LocalDB:** `Server=(localdb)\mssqllocaldb;Database=IITS;Trusted_Connection=True;...`

## 3. Comprobar que estás en Development

Al ejecutar con `dotnet run`, normalmente se usa el entorno Development. En la consola deberías ver:
```
Hosting environment: Development
```

Si usas otro entorno (Production, etc.), la app usará `appsettings.json` y puede que otra BD.

## 4. Regenerar BD y seed

```bash
dotnet run --project IITS reset-migrations
```

Esto borra la BD, aplica migraciones y carga el seed. Luego:

```bash
dotnet run --project IITS
```
