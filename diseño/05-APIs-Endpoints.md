# APIs y Endpoints

## Rutas HTTP

La aplicación expone endpoints mínimos en `Program.cs` (Minimal API). No hay controladores MVC/API tradicionales.

---

## Endpoints disponibles

### 1. PDF de auditoría

| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/api/auditoria/pdf?modulo={modulo}` | Genera PDF de auditoría por módulo |

**Parámetro `modulo` (obligatorio):**
- `aplicaciones`
- `operaciones`
- `cuentas`

**Respuesta:** archivo PDF descargable (`Auditoria_{modulo}_{yyyyMMdd_HHmm}.pdf`)

---

### 2. Exportación

| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/api/export/{modulo}/{formato}` | Exporta datos a Excel, PDF o CSV |

**Módulos soportados:**
- Aplicaciones
- Logs
- Operaciones / Tecnologia
- Cuentas
- Aprobaciones
- Usuarios
- Roles
- PermisosRol
- PermisosAprobacion

**Formatos:** `xlsx`, `excel`, `pdf`, `csv`

**Parámetros de consulta (opcionales):**
- `tabla` – filtro para Logs
- `modulo` – filtro para Aprobaciones
- `area` – filtro para Operaciones
- `tipo` – filtro para Cuentas (Privilegiada / Servicio)
- `estatus` – Activo, Inactivo, Desincorporado
- `estado` – filtro para Aprobaciones

**Ejemplos:**
- `/api/export/Aplicaciones/xlsx`
- `/api/export/Logs/csv?tabla=Aplicaciones`
- `/api/export/Operaciones/pdf?area=Operaciones`
- `/api/export/Cuentas/xlsx?estatus=Activo&tipo=Privilegiada`

**Nombre de archivo:** `{Modulo}_{yyyyMMdd_HHmm}.{ext}`

---

## Autenticación

Todos los endpoints requieren usuario autenticado (FallbackPolicy = RequireAuthenticatedUser). El acceso a módulos concretos se controla por permisos (RolePermission).

---

## Tipos MIME

| Formato | Content-Type |
|---------|--------------|
| Excel | application/vnd.openxmlformats-officedocument.spreadsheetml.sheet |
| PDF | application/pdf |
| CSV | text/csv; charset=utf-8 |
