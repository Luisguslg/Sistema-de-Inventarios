# Evaluación de Seguridad

## Autenticación

| Entorno | Mecanismo |
|---------|-----------|
| Producción (IIS) | Windows Authentication (Negotiate). Usuario = DOMINIO\user. |
| Desarrollo (local) | Cookie + usuario emulado (`DevAuthMiddleware`) cuando `Auth:Mode: Dev`. |

La sesión usa cookie `.IITS.Session` con tiempo de expiración configurable (`Auth:SessionTimeoutMinutes`, 5–480 min).

---

## Autorización

- **FallbackPolicy:** requiere usuario autenticado en todas las rutas.
- **Políticas por permiso:** cada código de `PermissionCodes` tiene una política que exige el claim `Permission` con ese código.
- **Roles:** SuperAdmin, Administrador, Operador, Auditor, Aprobador.
- **Aprobación:** `AprobacionPermiso` define quién puede aprobar por módulo (Aplicaciones, Operaciones, Cuentas).

---

## SuperAdmin

Configurado por `Auth:SuperAdminUsername`. El seed crea el usuario y asigna rol SuperAdmin si no existen.

---

## Auditoría

| Tabla | Uso |
|-------|-----|
| AuditLog | Registro técnico (tabla, entidad, acción, usuario, detalle) |
| AuditEvent | Auditoría funcional (BeforeJson, AfterJson, CorrelationId) |

Las operaciones CRUD y aprobaciones registran en AuditLog y/o AuditEvent.

---

## Datos sensibles

- Contraseñas: no se almacenan; autenticación vía AD.
- Cadena de conexión: en configuración (appsettings, variables de entorno).
- SMTP: credenciales en configuración si se requieren.

---

## Recomendaciones

1. Usar HTTPS en producción.
2. Mantener `Auth:SessionTimeoutMinutes` en un valor razonable (30–60 min).
3. Revisar permisos de carpeta `Logs` en el servidor.
4. No exponer `appsettings.Development.json` ni credenciales en repositorio.
