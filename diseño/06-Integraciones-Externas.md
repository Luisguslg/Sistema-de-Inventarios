# Integraciones Externas

## Sistemas y bibliotecas utilizados

---

## Base de datos

| Sistema | Uso |
|---------|-----|
| SQL Server | Base de datos principal vía Entity Framework Core 8 |

**Cadena de conexión:** se obtiene de `Configuration["ConnectionStrings:IITS"]`.

**Desarrollo:** típicamente `Server=localhost;Database=IITS;...`  
**Producción:** servidor SQL de red (ej. `VECCSAPP10\KPMGDV`)

---

## Autenticación

| Sistema | Uso |
|---------|-----|
| Active Directory | Autenticación Windows (Negotiate) en IIS |

El usuario llega como `DOMINIO\usuario`. `IITSClaimsTransformation` busca el usuario en la tabla `Users` por `Username` y añade claims (UserId, roles).

**Desarrollo sin dominio:** `Auth:Mode: Dev` y `Auth:DevUsername` para emular usuario local.

---

## Correo electrónico

| Sistema | Uso |
|---------|-----|
| SMTP | Envío de notificaciones (aprobar, rechazar, etc.) |

**Configuración:** sección `Email` en appsettings.  
**Relay típico:** `goemairs.go.kworld.kpmg.com`  
**Modo Dev:** `IEmailSender` = `DevEmailSender` (escribe en consola, no envía correo real).

Cola de correos: tabla `EmailOutbox` + `EmailOutboxHostedService`.

---

## Bibliotecas (NuGet)

| Paquete | Uso |
|---------|-----|
| ClosedXML | Importación y exportación Excel |
| QuestPDF | Generación de PDF (auditoría, reportes) |
| EPPlus / NPOI | Alternativas para manipulación Excel |
| Microsoft.EntityFrameworkCore.SqlServer | Acceso a SQL Server |
| Microsoft.AspNetCore.Authentication.Negotiate | Autenticación Windows |

---

## Otros

- **Logs:** carpeta `Logs` en ContentRootPath; el App Pool debe tener permisos de escritura.
- **Archivos estáticos:** `wwwroot` (CSS Bulma, JS, imágenes).
