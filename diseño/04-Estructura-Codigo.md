# Estructura de Código

## Raíz del proyecto

```
Sistema de Inventarios/
├── IITS/                 # Aplicación principal Blazor
├── ReadCampos/           # Herramienta de consola (lectura Excel)
├── infraestructura/      # Documentación de infraestructura
├── Ejemplo/              # Archivos Excel de ejemplo
├── diseño/               # Documentación de diseño
└── IITS.sln
```

---

## IITS (aplicación principal)

| Carpeta | Contenido |
|---------|-----------|
| **Data** | AppDbContext, DbSeed, PermissionCodes |
| **Entities** | Modelos de dominio (User, Role, Aplicacion, Operacion, etc.) |
| **Services** | Interfaces y servicios (IAplicacionService, AplicacionService, etc.) |
| **Pages** | UI Blazor por módulo |
| **Pages/Admin** | Roles, Usuarios, Aprobaciones, Permisos, MaestroDatos |
| **Pages/Data** | RedirectToMaestro |
| **Shared** | NavMenu, MainLayout, ExportarDropdown |
| **Modelo** | FormAplicacionModel, FormUsuarioModel, FormRolModel, ToastBase |
| **Middleware** | DevAuthMiddleware, SessionCookieSignInMiddleware |
| **Migrations** | Migraciones Entity Framework Core |
| **wwwroot** | CSS (Bulma), JS, imágenes |
| **docs** | Documentación técnica |

---

## Flujo de datos

1. **Pages** invocan **Services** mediante inyección de dependencias.
2. **Services** usan **AppDbContext** para acceder a **Entities**.
3. **Middleware** gestiona autenticación y sesión.
4. **IITSClaimsTransformation** carga usuario y roles desde BD y añade claims.

---

## Comandos de consola (IITS)

| Argumento | Función |
|-----------|---------|
| `read-catalogo-aplicaciones` | Lista columnas del Excel "Catalogo de Aplicaciones.xlsx" |
| `reset-migrations` | Borra BD, recrea y aplica migraciones |
| `fix-migrations` | Repara migraciones huérfanas |
| `seed-catalogs` | Carga catálogos y datos de ejemplo |

---

## ReadCampos

Proyecto de consola .NET 8 que lee archivos Excel (`Campos.xlsx`) con ClosedXML y exporta columnas y filas a `Campos_export.txt`.
