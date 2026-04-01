# Lógica de módulos y Dashboard

## Resumen

- **Operaciones**, **Aplicaciones** y **Cuentas** son los módulos de inventario.
- **Telecomunicaciones** se unificó en Operaciones (dispositivos de red van en Operaciones).
- **Partes** fue eliminado; en Aplicaciones Propietario y Responsable son texto libre.
- Cada módulo alimenta el **Dashboard** con sus totales (Activos / Desincorporados).
- El **Dashboard** solo muestra las tarjetas según permisos del usuario.
- **Páginas Web** unificado en Aplicaciones (legacy). **Proveedores** eliminado.

---

## Módulos de inventario

| Módulo | Tabla | Descripción | Área |
|--------|-------|-------------|------|
| Aplicaciones | Aplicaciones | Apps, alojamiento, propietario y responsable (texto libre) | - |
| Operaciones | Operaciones | Servidores, equipos, dispositivos de red, área, oficina, etc. | AreaId, OwnerAreaId |
| Cuentas | CuentasServicio, CuentasPrivilegiadas | Cuentas de servicio y privilegiadas | AreaId, AplicacionId (opcional) |

---

## Dashboard según permisos

El Dashboard muestra:

1. **Tarjetas de módulo** (Aplicaciones, Operaciones, Cuentas Servicio, Cuentas Privilegiadas): solo las que corresponden a módulos con permiso del usuario.
2. **Botones "Agregar"**: solo los módulos con permiso.
3. **Resumen total**: suma solo los módulos visibles.
4. **Admin** ve todo (todas las tarjetas, todos los botones, resumen global).

### Permisos

| Permiso | Módulos visibles |
|---------|------------------|
| Perm.Inventory.Aplicaciones | Aplicaciones |
| Perm.Inventory.Operaciones | Operaciones |
| Perm.Inventory.Cuentas | Cuentas Servicio + Cuentas Privilegiadas |
| Perm.Admin | Todos |

---

## Relaciones entre tablas

```
Estatus ──────────── Aplicacion, Operacion, CuentaPrivilegiada, CuentaServicio
Alojamiento ──────── Aplicacion, Operacion
Area ─────────────── Operacion, CuentaPrivilegiada, CuentaServicio
Aplicacion ───────── CuentaPrivilegiada, CuentaServicio (AplicacionId opcional)
```

- **Cuentas** (CuentaPrivilegiada, CuentaServicio) pueden vincularse a una **Aplicación** mediante AplicacionId.
- **Operaciones** y **Cuentas** usan **Area** para clasificar por departamento.
- **Aplicaciones** usa Propietario y Responsable como texto libre.

---

## Operaciones como “maestro”

Operaciones es el inventario principal de infraestructura (servidores, equipos, dispositivos de red). Junto con Aplicaciones y Cuentas, alimenta el Dashboard. El Dashboard agrega totales según permisos del usuario.
