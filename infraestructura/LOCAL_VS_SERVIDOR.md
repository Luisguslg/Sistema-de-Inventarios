# Local vs servidor (IIS + Active Directory)

## No es un problema

Es normal y recomendable:

- **Local:** base de datos en tu máquina, pruebas sin desplegar.
- **Servidor (IIS):** misma aplicación, pero apuntando al SQL del servidor y con autenticación por Active Directory.

Se controla todo por **configuración** (entorno y cadenas de conexión). No hace falta montar en el servidor para desarrollar.

---

## Cómo está pensado

| | Local (desarrollo) | Servidor (IIS) |
|---|--------------------|----------------|
| **Base de datos** | `localhost` / instancia local (IITS) | Servidor SQL de la red (misma BD IITS) |
| **Autenticación** | Opcional: sin AD para probar todo; o con Windows si estás en dominio | Active Directory (Windows Authentication / Negotiate) |
| **Config** | `appsettings.Development.json` + `appsettings.json` | En el servidor: variables de entorno o `appsettings.Production.json` |

---

## Qué hacer ahora (todo local)

1. **Base de datos local**
   - Tener SQL Server en tu PC y la BD **IITS** (creada con `dotnet ef database update --project IITS` desde la solución).
   - La conexión por defecto ya apunta a `Server=localhost;Database=IITS;...` en `appsettings.json`.

2. **Probar sin ir al servidor**
   - Desarrollar y probar en local hasta que todo funcione.
   - No hace falta publicar en IIS ni tocar el servidor hasta que quieras hacer la prueba en entorno real.

3. **Cuando quieras probar con AD**
   - Opción A: en tu PC, si estás en dominio, se puede activar autenticación Windows y que use tu usuario de dominio (misma BD local).
   - Opción B: desplegar en IIS en el servidor, configurar ahí la cadena al SQL del servidor y dejar que IIS use Windows Authentication (AD).

---

## Qué necesito de ti (por ahora)

- **Nada en el servidor** hasta que lo local funcione bien.
- Si tu SQL no es `localhost` (por ejemplo `.\SQLEXPRESS` o un nombre de instancia), dímelo y ajustamos la cadena en `appsettings.Development.json`.
- Cuando tengamos que preparar el paso a IIS: nombre del servidor (o URL), si la BD IITS estará en el mismo servidor o en otro, y que la app en IIS vaya a usar “Autenticación de Windows” (AD). Con eso se deja documentado o se añade un `appsettings.Production.json` de ejemplo.

---

## Alternativas resumidas

1. **Seguir solo en local**  
   BD local, sin AD. Cuando todo esté bien, desplegamos en IIS y cambiamos solo la configuración (cadena + AD).

2. **Probar AD en local**  
   Si estás en dominio, podemos activar autenticación Windows en el perfil Development y seguir usando la BD local; así ves el flujo de AD sin tocar el servidor.

3. **Probar en el servidor cuando toque**  
   Publicas en IIS, configuras en el servidor la cadena de conexión a la BD del servidor y activas Windows Authentication. No hace falta ir subiendo al servidor a cada rato; solo cuando quieras validar en entorno real.

La aplicación está preparada para usar la cadena de conexión `IITS` y, cuando la integremos, autenticación por AD; el cambio entre local y servidor es solo de configuración.
