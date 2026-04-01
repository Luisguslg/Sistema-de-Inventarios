# Verificación frente a la app de ejemplo

Referencia: `C:\Users\luisperdomo\Desktop\Sistema de Inventarios\Ejemplo\OneDrive_1_2-6-2026 (1)`

Este documento resume lo contrastado con la aplicación vieja de ejemplo para alinear comportamiento y asegurar que lo implementado en IITS (nueva app) esté verificado contra esa referencia.

---

## 1. Cuentas y asociación con servicio/aplicación

### En la app de ejemplo
- **Cuentas Privilegiadas** (`T_CuentasPrivilegiadas`): Nombre, Tipo, con tablas relacionadas (Estatus, Responsables, TiposCambio, Intervalos, GruposSeguridad). La vista `CuentasPrivilegiadasView` incluye un campo **CuentaServicio** (relación con “cuenta de servicio”).
- **Cuentas Servicio** (`T_CuentasServicio`): Nombre, IntervaloPass, con tablas relacionadas (Estatus, Origenes, Departamentos/Área, TiposCambio, GruposSeguridad). La vista `CuentasServicioView` incluye **ServicioAplicacion** (relación con aplicación/servicio).
- En las páginas Administrador (CuentasPrivilegiadas.razor, CuentasServicio.razor) la grilla muestra: Responsable, Nombre, Tipo, Área, Origen, Intervalo, Grupos, Estatus, etc. No se muestra en grilla el campo “ServicioAplicacion” en el código revisado, pero el modelo de vista sí lo tiene.

### En la app nueva (IITS actual)
- Una sola página **Cuentas** para privilegiadas y de servicio (filtro por tipo).
- Cada cuenta (privilegiada o de servicio) tiene:
  - **AplicacionId** (opcional): FK a **Aplicaciones** → la cuenta queda asociada a una aplicación del inventario (“servicio relacionado”).
  - **ServicioRelacionado** (texto libre): para detalle o cuando no se elige aplicación.
- En el formulario: desplegable **“Aplicación (servicio relacionado)”** (si existe tabla Aplicaciones) y campo **“Servicio relacionado (texto libre)”**.
- En la grilla y en el export se muestra el servicio relacionado como: nombre de la aplicación si hay `AplicacionId`, si no el texto libre.

**Conclusión:** La idea de “cuenta asociada a un servicio/aplicación” está cubierta en la app nueva con **AplicacionId** + texto libre, alineado con el concepto de **ServicioAplicacion** de la vista de ejemplo.

---

## 2. Validaciones y duplicados (case-insensitive)

### En la app de ejemplo
- **FuncionesIITS.cs**: `AplicacionHostnameAnalyzer` y otras funciones usan **ToLower()** y **RemoveDiacritics** para comparar nombres (evitar duplicados por mayúsculas/acentos).
- **CuentasServicio.razor / CuentasPrivilegiadas.razor**: al editar, se busca la cuenta con `Nombre.ToLower() == ...ToLower()`. No se vio una validación explícita “no permitir duplicados” al crear.

### En la app nueva (IITS actual)
- **CatalogItems** (agregar en el acto en Operaciones): se valida duplicado por **Kind + Nombre** con comparación **case-insensitive** (y trim). No se inserta si ya existe (p. ej. "Windows" / "windows" / "WINDOWS").
- **Cuentas**: al crear/editar se valida que no exista otra cuenta del **mismo tipo** (Privilegiada o Servicio) con el **mismo nombre** (case-insensitive).
- **Telecomunicaciones**: mismo nombre (case-insensitive) no permitido al guardar.
- **Alojamientos**: mismo nombre (case-insensitive) no permitido al crear/editar.

**Conclusión:** La app nueva aplica validaciones de duplicados case-insensitive de forma explícita en los puntos críticos; la de ejemplo usa ToLower en búsquedas y en analizadores. Comportamiento verificado y reforzado en la nueva app.

---

## 3. Telecomunicaciones

### En la app de ejemplo
- **T_Telecoms**: modelo rico con **ID_Dispositivo**, **ID_Fabricante**, **ID_Modelo**, Hostname, Serial, Garantia y colecciones (Ambientes, Categorias, Criticidades, DireccionesIP, Estatus, Firmware, MacAddresses, Observaciones, Oficinas, Responsables).
- **TelecomunicacionesViewModel**: HostName, Oficina, TipoDispositivo, Criticidad, Ambiente, Responsable, Estado, Fabricante, Modelo, Serial, DireccionIP, MacAddress, FirmwareSisOperativo, Garantia.

### En la app nueva (IITS actual)
- Modelo **Telecom** más simple: Nombre, EstatusId, **Tipo**, **Ubicacion**, **Descripcion** (además de Id, CreatedAt).
- La nueva app no replica el esquema completo de dispositivos/telecom del ejemplo; está pensada como catálogo más sencillo. Se añadieron **Tipo**, **Ubicación** y **Descripción** para que la pantalla no quede vacía y sea útil.

**Conclusión:** Diferente alcance (catálogo simple vs. inventario detallado de telecom). En la nueva app se verifica que Telecomunicaciones tenga campos suficientes (Tipo, Ubicación, Descripción) para no quedar vacía, en línea con la petición de “no dejar tan vacío”.

---

## 4. Tablas opcionales y que la app no se caiga

### En la app de ejemplo
- Uso directo de tablas y relaciones; no se revisaron comprobaciones de existencia de tablas antes de consultar (arquitectura distinta).

### En la app nueva (IITS actual)
- **Cuentas**: si no existe la tabla **Areas**, se cargan cuentas sin `Include(Area)` y la columna Área se muestra vacía; el export no hace JOIN a Areas.
- **Operaciones**: la carga con catálogos (Offices, Areas, Alojamientos, etc.) solo se hace si existen **Offices**, **Areas** y **Alojamientos**; si falta alguna, se usa la rama sin esos Includes y listas vacías.
- **Aplicaciones**: el servicio usa `TableExistsAsync("Alojamientos")` y `TableExistsAsync("Partes")` antes de incluir relaciones.
- **Alojamientos.razor**: comprueba `TableExistsAsync("Alojamientos")` y muestra mensaje si no está disponible.
- **CatalogItems**: se comprueba existencia de tabla antes de cargar/agregar.

**Conclusión:** En la app nueva se verifica el uso de tablas opcionales con `TableExistsAsync` (o lógica equivalente) en los puntos críticos para que no se caiga al faltar tablas de migraciones no aplicadas.

---

## 5. Resumen de comprobaciones

| Tema                         | App ejemplo                    | App nueva (IITS)                                      | Estado        |
|-----------------------------|--------------------------------|--------------------------------------------------------|---------------|
| Cuenta ↔ Servicio/Aplicación | Vista ServicioAplicacion       | AplicacionId + texto libre + desplegable Aplicación   | Verificado    |
| Duplicados case-insensitive | ToLower en búsquedas/analizadores | Validación explícita en Cuentas, CatalogItems, Telecom, Alojamientos | Verificado    |
| Telecom no vacío            | Muchos campos (Hostname, IP, etc.) | Tipo, Ubicación, Descripción añadidos                 | Verificado    |
| No caerse por tablas faltantes | No revisado                    | TableExistsAsync / ramas condicionales en Cuentas, Operaciones, Aplicaciones, Alojamientos | Verificado    |

---

Cuando cambies algo en la app nueva (cuentas, servicio, validaciones, telecom o tablas opcionales), conviene volver a contrastar con la carpeta de ejemplo y actualizar este documento si aplica.
