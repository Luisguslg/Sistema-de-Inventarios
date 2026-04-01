using IITS.Entities;
using Microsoft.EntityFrameworkCore;

namespace IITS.Data;

public static class DbSeed
{
    /// <summary>Comprueba si existe la tabla en [dbo] sin lanzar excepciones.</summary>
    public static async Task<bool> TableExistsAsync(this AppDbContext db, string tableName, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(tableName)) return false;
        var safeName = tableName.Replace("]", "]]");
        try
        {
            var conn = db.Database.GetDbConnection();
            if (conn.State != System.Data.ConnectionState.Open) await conn.OpenAsync(ct);
            try
            {
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT OBJECT_ID(N'[dbo].[" + safeName + "]', N'U')";
                var objId = await cmd.ExecuteScalarAsync(ct);
                return objId != null && objId != DBNull.Value && objId is int i && i != 0;
            }
            finally
            {
                if (conn.State == System.Data.ConnectionState.Open) conn.Close();
            }
        }
        catch
        {
            return false;
        }
    }

    /// <summary>Añade columnas opcionales a Aplicaciones si no existen. Idempotente. Campos según Catalogo de Aplicaciones.xlsx.</summary>
    public static async Task EnsureAplicacionesOptionalColumnsAsync(this AppDbContext db, CancellationToken ct = default)
    {
        if (!await db.TableExistsAsync("Aplicaciones", ct)) return;
        try
        {
            await db.Database.ExecuteSqlRawAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Aplicaciones') AND name = 'AlojamientoId')
    ALTER TABLE [Aplicaciones] ADD [AlojamientoId] uniqueidentifier NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Aplicaciones') AND name = 'ModeloLicenciamiento')
    ALTER TABLE [Aplicaciones] ADD [ModeloLicenciamiento] nvarchar(200) NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Aplicaciones') AND name = 'CostoAnualEstimado')
    ALTER TABLE [Aplicaciones] ADD [CostoAnualEstimado] decimal(18,2) NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Aplicaciones') AND name = 'FechaAdquisicionImplementacion')
    ALTER TABLE [Aplicaciones] ADD [FechaAdquisicionImplementacion] datetime2 NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Aplicaciones') AND name = 'VersionActual')
    ALTER TABLE [Aplicaciones] ADD [VersionActual] nvarchar(100) NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Aplicaciones') AND name = 'SLA')
    ALTER TABLE [Aplicaciones] ADD [SLA] nvarchar(200) NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Aplicaciones') AND name = 'RPORTO')
    ALTER TABLE [Aplicaciones] ADD [RPORTO] nvarchar(200) NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Aplicaciones') AND name = 'Autenticacion')
    ALTER TABLE [Aplicaciones] ADD [Autenticacion] nvarchar(200) NULL;
", ct);
            await db.Database.ExecuteSqlRawAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Aplicaciones_AlojamientoId' AND object_id = OBJECT_ID('Aplicaciones'))
    CREATE INDEX [IX_Aplicaciones_AlojamientoId] ON [Aplicaciones]([AlojamientoId]);
", ct);
        }
        catch
        {
            // Ignorar si ya existen o no se puede alterar
        }
    }

    /// <summary>Añade columnas opcionales a Operaciones si no existen. Idempotente. Evita que la app se caiga cuando solo está aplicada InitialSchema.</summary>
    public static async Task EnsureOperacionesOptionalColumnsAsync(this AppDbContext db, CancellationToken ct = default)
    {
        if (!await db.TableExistsAsync("Operaciones", ct)) return;
        try
        {
            await db.Database.ExecuteSqlRawAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Operaciones') AND name = 'OfficeId') ALTER TABLE [Operaciones] ADD [OfficeId] uniqueidentifier NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Operaciones') AND name = 'AreaId') ALTER TABLE [Operaciones] ADD [AreaId] uniqueidentifier NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Operaciones') AND name = 'AlojamientoId') ALTER TABLE [Operaciones] ADD [AlojamientoId] uniqueidentifier NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Operaciones') AND name = 'OwnerAreaId') ALTER TABLE [Operaciones] ADD [OwnerAreaId] uniqueidentifier NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Operaciones') AND name = 'CriticalityId') ALTER TABLE [Operaciones] ADD [CriticalityId] uniqueidentifier NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Operaciones') AND name = 'EnvironmentId') ALTER TABLE [Operaciones] ADD [EnvironmentId] uniqueidentifier NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Operaciones') AND name = 'CategoryId') ALTER TABLE [Operaciones] ADD [CategoryId] uniqueidentifier NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Operaciones') AND name = 'TipoDispositivo') ALTER TABLE [Operaciones] ADD [TipoDispositivo] nvarchar(100) NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Operaciones') AND name = 'Funcion') ALTER TABLE [Operaciones] ADD [Funcion] nvarchar(200) NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Operaciones') AND name = 'TipoInfraestructura') ALTER TABLE [Operaciones] ADD [TipoInfraestructura] nvarchar(50) NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Operaciones') AND name = 'Host') ALTER TABLE [Operaciones] ADD [Host] nvarchar(200) NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Operaciones') AND name = 'RAM') ALTER TABLE [Operaciones] ADD [RAM] nvarchar(100) NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Operaciones') AND name = 'CantidadCPU') ALTER TABLE [Operaciones] ADD [CantidadCPU] int NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Operaciones') AND name = 'VelocidadCPU') ALTER TABLE [Operaciones] ADD [VelocidadCPU] nvarchar(50) NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Operaciones') AND name = 'CapacidadDAS') ALTER TABLE [Operaciones] ADD [CapacidadDAS] nvarchar(100) NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Operaciones') AND name = 'CapacidadSAN') ALTER TABLE [Operaciones] ADD [CapacidadSAN] nvarchar(100) NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Operaciones') AND name = 'SistemaOperativo') ALTER TABLE [Operaciones] ADD [SistemaOperativo] nvarchar(200) NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Operaciones') AND name = 'ManufacturerId') ALTER TABLE [Operaciones] ADD [ManufacturerId] uniqueidentifier NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Operaciones') AND name = 'DeviceModelId') ALTER TABLE [Operaciones] ADD [DeviceModelId] uniqueidentifier NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Operaciones') AND name = 'IP') ALTER TABLE [Operaciones] ADD [IP] nvarchar(50) NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Operaciones') AND name = 'MAC') ALTER TABLE [Operaciones] ADD [MAC] nvarchar(100) NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Operaciones') AND name = 'Firmware') ALTER TABLE [Operaciones] ADD [Firmware] nvarchar(200) NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Operaciones') AND name = 'GarantiaExpira') ALTER TABLE [Operaciones] ADD [GarantiaExpira] datetime2 NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Operaciones') AND name = 'Observaciones') ALTER TABLE [Operaciones] ADD [Observaciones] nvarchar(500) NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Operaciones') AND name = 'BCP') ALTER TABLE [Operaciones] ADD [BCP] bit NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Operaciones') AND name = 'RPORTO') ALTER TABLE [Operaciones] ADD [RPORTO] nvarchar(200) NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Operaciones') AND name = 'Propietario') ALTER TABLE [Operaciones] ADD [Propietario] nvarchar(100) NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Operaciones') AND name = 'ClasificacionInformacion') ALTER TABLE [Operaciones] ADD [ClasificacionInformacion] nvarchar(200) NULL;
", ct);
        }
        catch
        {
            // Ignorar si ya existen o no se puede alterar
        }
    }

    /// <summary>Añade columnas opcionales a CuentasPrivilegiadas y CuentasServicio si no existen. Idempotente.</summary>
    public static async Task EnsureCuentasOptionalColumnsAsync(this AppDbContext db, CancellationToken ct = default)
    {
        try
        {
            if (await db.TableExistsAsync("CuentasPrivilegiadas", ct))
            {
                await db.Database.ExecuteSqlRawAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('CuentasPrivilegiadas') AND name = 'AreaId') ALTER TABLE [CuentasPrivilegiadas] ADD [AreaId] uniqueidentifier NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('CuentasPrivilegiadas') AND name = 'Origen') ALTER TABLE [CuentasPrivilegiadas] ADD [Origen] nvarchar(200) NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('CuentasPrivilegiadas') AND name = 'Responsable') ALTER TABLE [CuentasPrivilegiadas] ADD [Responsable] nvarchar(200) NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('CuentasPrivilegiadas') AND name = 'ServicioRelacionado') ALTER TABLE [CuentasPrivilegiadas] ADD [ServicioRelacionado] nvarchar(300) NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('CuentasPrivilegiadas') AND name = 'AplicacionId') ALTER TABLE [CuentasPrivilegiadas] ADD [AplicacionId] uniqueidentifier NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('CuentasPrivilegiadas') AND name = 'TipoConfiguracionCambio') ALTER TABLE [CuentasPrivilegiadas] ADD [TipoConfiguracionCambio] nvarchar(50) NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('CuentasPrivilegiadas') AND name = 'IntervaloCambioDias') ALTER TABLE [CuentasPrivilegiadas] ADD [IntervaloCambioDias] int NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('CuentasPrivilegiadas') AND name = 'GruposSeguridad') ALTER TABLE [CuentasPrivilegiadas] ADD [GruposSeguridad] nvarchar(2000) NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('CuentasPrivilegiadas') AND name = 'Descripcion') ALTER TABLE [CuentasPrivilegiadas] ADD [Descripcion] nvarchar(500) NULL;
", ct);
            }
            if (await db.TableExistsAsync("CuentasServicio", ct))
            {
                await db.Database.ExecuteSqlRawAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('CuentasServicio') AND name = 'AreaId') ALTER TABLE [CuentasServicio] ADD [AreaId] uniqueidentifier NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('CuentasServicio') AND name = 'Origen') ALTER TABLE [CuentasServicio] ADD [Origen] nvarchar(200) NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('CuentasServicio') AND name = 'Responsable') ALTER TABLE [CuentasServicio] ADD [Responsable] nvarchar(200) NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('CuentasServicio') AND name = 'ServicioRelacionado') ALTER TABLE [CuentasServicio] ADD [ServicioRelacionado] nvarchar(300) NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('CuentasServicio') AND name = 'AplicacionId') ALTER TABLE [CuentasServicio] ADD [AplicacionId] uniqueidentifier NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('CuentasServicio') AND name = 'TipoConfiguracionCambio') ALTER TABLE [CuentasServicio] ADD [TipoConfiguracionCambio] nvarchar(50) NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('CuentasServicio') AND name = 'IntervaloCambioDias') ALTER TABLE [CuentasServicio] ADD [IntervaloCambioDias] int NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('CuentasServicio') AND name = 'GruposSeguridad') ALTER TABLE [CuentasServicio] ADD [GruposSeguridad] nvarchar(2000) NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('CuentasServicio') AND name = 'Descripcion') ALTER TABLE [CuentasServicio] ADD [Descripcion] nvarchar(500) NULL;
", ct);
            }
        }
        catch { /* ignorar */ }
    }

    /// <summary>Crea tablas de admin/permisos si no existen: AprobacionPermisos, Permissions, RolePermission. Idempotente.</summary>
    public static async Task EnsureAdminTablesAsync(this AppDbContext db, CancellationToken ct = default)
    {
        try
        {
            if (!await db.TableExistsAsync("AprobacionPermisos", ct))
            {
                await db.Database.ExecuteSqlRawAsync(@"
CREATE TABLE [AprobacionPermisos] (
    [Id] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [Modulo] nvarchar(50) NOT NULL,
    CONSTRAINT [PK_AprobacionPermisos] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AprobacionPermisos_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);
CREATE UNIQUE INDEX [IX_AprobacionPermisos_UserId_Modulo] ON [AprobacionPermisos] ([UserId], [Modulo]);
", ct);
            }
            if (!await db.TableExistsAsync("Permissions", ct))
            {
                await db.Database.ExecuteSqlRawAsync(@"
CREATE TABLE [Permissions] (
    [Id] uniqueidentifier NOT NULL,
    [Code] nvarchar(100) NOT NULL,
    [Description] nvarchar(200) NOT NULL,
    CONSTRAINT [PK_Permissions] PRIMARY KEY ([Id])
);
", ct);
            }
            if (!await db.TableExistsAsync("RolePermission", ct))
            {
                await db.Database.ExecuteSqlRawAsync(@"
CREATE TABLE [RolePermission] (
    [RoleId] uniqueidentifier NOT NULL,
    [PermissionId] uniqueidentifier NOT NULL,
    CONSTRAINT [PK_RolePermission] PRIMARY KEY ([RoleId], [PermissionId]),
    CONSTRAINT [FK_RolePermission_Roles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [Roles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_RolePermission_Permissions_PermissionId] FOREIGN KEY ([PermissionId]) REFERENCES [Permissions] ([Id]) ON DELETE CASCADE
);
CREATE INDEX [IX_RolePermission_PermissionId] ON [RolePermission] ([PermissionId]);
", ct);
            }
        }
        catch { /* ignorar si ya existen o dependencias faltan */ }
    }

    /// <summary>Crea tablas de catálogos de Tecnología si no existen (Offices, Areas, etc.). Repara BD cuando el historial de migraciones está desincronizado.</summary>
    public static async Task EnsureCatalogTablesIfMissingAsync(this AppDbContext db, CancellationToken ct = default)
    {
        try
        {
            if (!await db.TableExistsAsync("Areas", ct))
            {
                await db.Database.ExecuteSqlRawAsync(@"
CREATE TABLE [Areas] ([Id] uniqueidentifier NOT NULL, [Name] nvarchar(100) NOT NULL, CONSTRAINT [PK_Areas] PRIMARY KEY ([Id]));", ct);
            }
            if (!await db.TableExistsAsync("Offices", ct))
            {
                await db.Database.ExecuteSqlRawAsync(@"
CREATE TABLE [Offices] ([Id] uniqueidentifier NOT NULL, [Name] nvarchar(150) NOT NULL, CONSTRAINT [PK_Offices] PRIMARY KEY ([Id]));", ct);
            }
            if (!await db.TableExistsAsync("Environments", ct))
            {
                await db.Database.ExecuteSqlRawAsync(@"
CREATE TABLE [Environments] ([Id] uniqueidentifier NOT NULL, [Name] nvarchar(100) NOT NULL, CONSTRAINT [PK_Environments] PRIMARY KEY ([Id]));", ct);
            }
            if (!await db.TableExistsAsync("Criticalities", ct))
            {
                await db.Database.ExecuteSqlRawAsync(@"
CREATE TABLE [Criticalities] ([Id] uniqueidentifier NOT NULL, [Name] nvarchar(100) NOT NULL, CONSTRAINT [PK_Criticalities] PRIMARY KEY ([Id]));", ct);
            }
            if (!await db.TableExistsAsync("Categories", ct))
            {
                await db.Database.ExecuteSqlRawAsync(@"
CREATE TABLE [Categories] ([Id] uniqueidentifier NOT NULL, [Name] nvarchar(100) NOT NULL, CONSTRAINT [PK_Categories] PRIMARY KEY ([Id]));", ct);
            }
            if (!await db.TableExistsAsync("Vendors", ct))
            {
                await db.Database.ExecuteSqlRawAsync(@"
CREATE TABLE [Vendors] ([Id] uniqueidentifier NOT NULL, [Name] nvarchar(150) NOT NULL, CONSTRAINT [PK_Vendors] PRIMARY KEY ([Id]));", ct);
            }
            if (!await db.TableExistsAsync("DeviceModels", ct))
            {
                await db.Database.ExecuteSqlRawAsync(@"
CREATE TABLE [DeviceModels] ([Id] uniqueidentifier NOT NULL, [ManufacturerId] uniqueidentifier NOT NULL, [Name] nvarchar(150) NOT NULL,
    CONSTRAINT [PK_DeviceModels] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_DeviceModels_Vendors] FOREIGN KEY ([ManufacturerId]) REFERENCES [Vendors]([Id]));", ct);
            }
            if (!await db.TableExistsAsync("CatalogItems", ct))
            {
                await db.Database.ExecuteSqlRawAsync(@"
CREATE TABLE [CatalogItems] ([Id] uniqueidentifier NOT NULL, [Kind] nvarchar(50) NOT NULL, [Name] nvarchar(150) NOT NULL,
    CONSTRAINT [PK_CatalogItems] PRIMARY KEY ([Id]));", ct);
            }
            if (!await db.TableExistsAsync("EmailOutbox", ct))
            {
                await db.Database.ExecuteSqlRawAsync(@"
CREATE TABLE [EmailOutbox] (
    [Id] uniqueidentifier NOT NULL,
    [To] nvarchar(500) NOT NULL,
    [Cc] nvarchar(500) NULL,
    [Subject] nvarchar(300) NOT NULL,
    [BodyHtml] nvarchar(max) NULL,
    [BodyText] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [SentAt] datetime2 NULL,
    [Status] nvarchar(20) NOT NULL,
    [Error] nvarchar(1000) NULL,
    [RetryCount] int NOT NULL,
    CONSTRAINT [PK_EmailOutbox] PRIMARY KEY ([Id]));", ct);
            }
            if (!await db.TableExistsAsync("AprobacionVotos", ct))
            {
                await db.Database.ExecuteSqlRawAsync(@"
CREATE TABLE [AprobacionVotos] (
    [Id] uniqueidentifier NOT NULL,
    [AprobacionId] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [Estado] nvarchar(20) NOT NULL,
    [Fecha] datetime2 NOT NULL,
    [Comentario] nvarchar(max) NULL,
    CONSTRAINT [PK_AprobacionVotos] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AprobacionVotos_Aprobaciones] FOREIGN KEY ([AprobacionId]) REFERENCES [Aprobaciones]([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_AprobacionVotos_Users] FOREIGN KEY ([UserId]) REFERENCES [Users]([Id]) ON DELETE NO ACTION
);
CREATE UNIQUE INDEX [IX_AprobacionVotos_AprobacionId_UserId] ON [AprobacionVotos]([AprobacionId], [UserId]);", ct);
            }
        }
        catch { /* ignorar si falla */ }
    }

    /// <summary>Crea tabla Alojamientos si no existe. Idempotente. Alojamiento = dónde se hospeda (Data Center, Nube, etc.).</summary>
    public static async Task EnsureAlojamientosTableAsync(this AppDbContext db, CancellationToken ct = default)
    {
        try
        {
            if (!await db.TableExistsAsync("Alojamientos", ct))
            {
                await db.Database.ExecuteSqlRawAsync(@"
CREATE TABLE [Alojamientos] (
    [Id] uniqueidentifier NOT NULL,
    [Nombre] nvarchar(150) NOT NULL,
    CONSTRAINT [PK_Alojamientos] PRIMARY KEY ([Id])
);
", ct);
            }
        }
        catch { /* ignorar si ya existen */ }
    }

    public static async Task EnsureEstatusAsync(this AppDbContext db, CancellationToken ct = default)
    {
        var required = new[] { ("Activo", 1000L), ("Inactivo", 1500L), ("Desincorporado", 2000L) };
        foreach (var (nombre, codigo) in required)
        {
            if (await db.Estatus.AnyAsync(e => e.Nombre == nombre, ct)) continue;
            db.Estatus.Add(new Estatus { Id = Guid.NewGuid(), Nombre = nombre, Codigo = codigo });
        }
        await db.SaveChangesAsync(ct);
    }

    public static async Task EnsureRolesAsync(this AppDbContext db, CancellationToken ct = default)
    {
        var nombres = new[] { "SuperAdmin", "Administrador", "Operador", "Auditor", "Usuario", "Aprobador" };
        foreach (var nombre in nombres)
        {
            if (await db.Roles.AnyAsync(r => r.Nombre == nombre, ct)) continue;
            db.Roles.Add(new Role { Id = Guid.NewGuid(), Nombre = nombre });
        }
        await db.SaveChangesAsync(ct);
    }

    public static async Task EnsurePermissionsAsync(this AppDbContext db, CancellationToken ct = default)
    {
        foreach (var (code, desc) in PermissionCodes.All)
        {
            if (await db.Permissions.AnyAsync(p => p.Code == code, ct)) continue;
            db.Permissions.Add(new Permission { Id = Guid.NewGuid(), Code = code, Description = desc });
        }
        await db.SaveChangesAsync(ct);

        if (!await db.TableExistsAsync("RolePermission", ct)) return;
        var permissions = await db.Permissions.AsNoTracking().ToListAsync(ct);
        var permByCode = permissions.ToDictionary(p => p.Code, StringComparer.OrdinalIgnoreCase);

        await AssignRolePermissionsAsync(db, "SuperAdmin", permissions.Select(p => p.Id).ToList(), ct);

        await AssignRolePermissionsAsync(db, "Administrador", permissions.Select(p => p.Id).ToList(), ct);

        var operadorCodes = new[] { PermissionCodes.InventoryView, PermissionCodes.InventoryCreate, PermissionCodes.InventoryEdit, PermissionCodes.InventoryExport, PermissionCodes.InventoryAplicaciones, PermissionCodes.InventoryOperaciones, PermissionCodes.InventoryCuentas };
        await AssignRolePermissionsAsync(db, "Operador", operadorCodes.Where(c => permByCode.ContainsKey(c)).Select(c => permByCode[c].Id).ToList(), ct);

        var auditorCodes = new[] { PermissionCodes.InventoryView, PermissionCodes.InventoryAplicaciones, PermissionCodes.InventoryOperaciones, PermissionCodes.InventoryCuentas, PermissionCodes.AuditView, PermissionCodes.LogsView, PermissionCodes.LogsExport };
        await AssignRolePermissionsAsync(db, "Auditor", auditorCodes.Where(c => permByCode.ContainsKey(c)).Select(c => permByCode[c].Id).ToList(), ct);

        var aprobadorCodes = new[] { PermissionCodes.InventoryView, PermissionCodes.InventoryAplicaciones, PermissionCodes.InventoryOperaciones, PermissionCodes.InventoryCuentas, PermissionCodes.AuditView, PermissionCodes.AuditApprove };
        await AssignRolePermissionsAsync(db, "Aprobador", aprobadorCodes.Where(c => permByCode.ContainsKey(c)).Select(c => permByCode[c].Id).ToList(), ct);

        var usuarioCodes = new[] { PermissionCodes.InventoryView, PermissionCodes.InventoryAplicaciones, PermissionCodes.InventoryOperaciones, PermissionCodes.InventoryCuentas };
        await AssignRolePermissionsAsync(db, "Usuario", usuarioCodes.Where(c => permByCode.ContainsKey(c)).Select(c => permByCode[c].Id).ToList(), ct);
    }

    /// <summary>Asigna permisos a un rol por nombre. Idempotente: no duplica.</summary>
    private static async Task AssignRolePermissionsAsync(AppDbContext db, string roleName, List<Guid> permissionIds, CancellationToken ct)
    {
        var role = await db.Roles.OrderBy(r => r.Nombre).FirstOrDefaultAsync(r => r.Nombre == roleName, ct);
        if (role == null) return;
        foreach (var permId in permissionIds)
        {
            if (await db.RolePermissions.AnyAsync(rp => rp.RoleId == role.Id && rp.PermissionId == permId, ct)) continue;
            db.RolePermissions.Add(new RolePermission { RoleId = role.Id, PermissionId = permId });
        }
        await db.SaveChangesAsync(ct);
    }

    public static async Task EnsureSuperAdminUserAsync(this AppDbContext db, string? superAdminUsername, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(superAdminUsername)) return;

        var user = await db.Users.AsTracking().OrderBy(u => u.Username).FirstOrDefaultAsync(u => u.Username == superAdminUsername, ct);
        if (user == null)
        {
            user = new User
            {
                Id = Guid.NewGuid(),
                Username = superAdminUsername,
                Nombre = "SuperAdmin",
                Apellido = "",
                Email = ""
            };
            db.Users.Add(user);
            await db.SaveChangesAsync(ct);
        }

        var role = await db.Roles.OrderBy(r => r.Nombre).FirstOrDefaultAsync(r => r.Nombre == "SuperAdmin", ct);
        if (role == null) return;

        var alreadyAssigned = await db.UserRoles.AnyAsync(ur => ur.UserId == user.Id && ur.RoleId == role.Id, ct);
        if (!alreadyAssigned)
        {
            db.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = role.Id });
            await db.SaveChangesAsync(ct);
        }
    }

    /// <summary>Entorno de operación: On-Premise o Cloud (diccionario Campos.xlsx).</summary>
    public static async Task EnsureAlojamientosAsync(this AppDbContext db, CancellationToken ct = default)
    {
        var nombres = new[] { "On-Premise", "Cloud", "Data Center", "Nube (Microsoft)" };
        foreach (var n in nombres)
        {
            if (await db.Alojamientos.AnyAsync(a => a.Nombre == n, ct)) continue;
            db.Alojamientos.Add(new Alojamiento { Id = Guid.NewGuid(), Nombre = n });
        }
        await db.SaveChangesAsync(ct);
    }

    public static async Task EnsureAreasAsync(this AppDbContext db, CancellationToken ct = default)
    {
        var names = new[] { "Aplicaciones", "Global", "Operaciones", "Soporte", "Telecomunicaciones" };
        foreach (var name in names)
        {
            if (await db.Areas.AnyAsync(a => a.Name == name, ct)) continue;
            db.Areas.Add(new Area { Id = Guid.NewGuid(), Name = name });
        }
        await db.SaveChangesAsync(ct);
    }

    public static async Task EnsureOfficesAsync(this AppDbContext db, CancellationToken ct = default)
    {
        var names = new[] { "Barquisimeto", "Caracas", "Maracaibo", "Maracay", "Puerto La Cruz", "Puerto Ordaz", "Valencia", "Global" };
        foreach (var name in names)
        {
            if (await db.Offices.AnyAsync(o => o.Name == name, ct)) continue;
            db.Offices.Add(new Office { Id = Guid.NewGuid(), Name = name });
        }
        await db.SaveChangesAsync(ct);
    }

    public static async Task EnsureEnvironmentsAsync(this AppDbContext db, CancellationToken ct = default)
    {
        var names = new[] { "DEV", "PRD", "QAS" };
        foreach (var name in names)
        {
            if (await db.Environments.AnyAsync(e => e.Name == name, ct)) continue;
            db.Environments.Add(new IITS.Entities.Environment { Id = Guid.NewGuid(), Name = name });
        }
        await db.SaveChangesAsync(ct);
    }

    public static async Task EnsureCriticalitiesAsync(this AppDbContext db, CancellationToken ct = default)
    {
        if (await db.Criticalities.AnyAsync(ct)) return;
        foreach (var name in new[] { "Alto", "Medio", "Bajo" })
            db.Criticalities.Add(new Criticality { Id = Guid.NewGuid(), Name = name });
        await db.SaveChangesAsync(ct);
    }

    public static async Task EnsureCategoriesAsync(this AppDbContext db, CancellationToken ct = default)
    {
        if (await db.Categories.AnyAsync(ct)) return;
        foreach (var name in new[] { "A", "B", "C" })
            db.Categories.Add(new Category { Id = Guid.NewGuid(), Name = name });
        await db.SaveChangesAsync(ct);
    }

    public static async Task EnsureVendorsAsync(this AppDbContext db, CancellationToken ct = default)
    {
        var names = new[] { "Dell", "HP", "Cisco", "Microsoft", "N/A" };
        foreach (var name in names)
        {
            if (await db.Vendors.AnyAsync(v => v.Name == name, ct)) continue;
            db.Vendors.Add(new Vendor { Id = Guid.NewGuid(), Name = name });
        }
        await db.SaveChangesAsync(ct);
    }

    public static async Task EnsureDeviceModelsAsync(this AppDbContext db, CancellationToken ct = default)
    {
        var dell = await db.Vendors.OrderBy(v => v.Name).FirstOrDefaultAsync(v => v.Name == "Dell", ct);
        var hp = await db.Vendors.OrderBy(v => v.Name).FirstOrDefaultAsync(v => v.Name == "HP", ct);
        var na = await db.Vendors.OrderBy(v => v.Name).FirstOrDefaultAsync(v => v.Name == "N/A", ct);
        if (dell != null && !await db.DeviceModels.AnyAsync(d => d.ManufacturerId == dell.Id, ct))
        {
            db.DeviceModels.Add(new DeviceModel { Id = Guid.NewGuid(), ManufacturerId = dell.Id, Name = "PowerEdge R740" });
            db.DeviceModels.Add(new DeviceModel { Id = Guid.NewGuid(), ManufacturerId = dell.Id, Name = "OptiPlex 7090" });
        }
        if (hp != null && !await db.DeviceModels.AnyAsync(d => d.ManufacturerId == hp.Id, ct))
            db.DeviceModels.Add(new DeviceModel { Id = Guid.NewGuid(), ManufacturerId = hp.Id, Name = "ProLiant DL380" });
        if (na != null && !await db.DeviceModels.AnyAsync(d => d.ManufacturerId == na.Id, ct))
            db.DeviceModels.Add(new DeviceModel { Id = Guid.NewGuid(), ManufacturerId = na.Id, Name = "N/A" });
        await db.SaveChangesAsync(ct);
    }

    /// <summary>Valores para selects alimentables "en el acto" (TipoDispositivo, Función, TipoInfraestructura, SistemaOperativo).</summary>
    public static async Task EnsureCatalogItemsAsync(this AppDbContext db, CancellationToken ct = default)
    {
        var defaults = new[] {
            ("TipoDispositivo", "Servidor"), ("TipoDispositivo", "Workstation"), ("TipoDispositivo", "Laptop"), ("TipoDispositivo", "Storage"), ("TipoDispositivo", "Network"),
            ("Funcion", "Controlador de dominio"), ("Funcion", "Aplicación web"), ("Funcion", "Base de datos"), ("Funcion", "Archivos"), ("Funcion", "Backup"),
            ("TipoInfraestructura", "Físico"), ("TipoInfraestructura", "Virtual"), ("TipoInfraestructura", "Contenedor"), ("TipoInfraestructura", "Cloud"),
            ("SistemaOperativo", "Windows Server 2022"), ("SistemaOperativo", "Windows Server 2019"), ("SistemaOperativo", "Linux (RHEL)"), ("SistemaOperativo", "Linux (Ubuntu)"), ("SistemaOperativo", "VMware ESXi")
        };
        foreach (var (kind, name) in defaults)
        {
            var existingNames = await db.CatalogItems.Where(c => c.Kind == kind).Select(c => c.Name).ToListAsync(ct);
            if (existingNames.Any(n => string.Equals(n.Trim(), name.Trim(), StringComparison.OrdinalIgnoreCase))) continue;
            db.CatalogItems.Add(new CatalogItem { Id = Guid.NewGuid(), Kind = kind, Name = name });
        }
        await db.SaveChangesAsync(ct);
    }

    /// <summary>Catálogos de Tecnología según diccionario Campos.xlsx: Dispositivo (Access Point, Firewall, Router, Switch, SAN, Servidor...), Función, Tipo infraestructura (Físico/Virtual), Sistema operativo.</summary>
    public static async Task EnsureCatalogItemsTecnologiaAsync(this AppDbContext db, CancellationToken ct = default)
    {
        var defaults = new[] {
            ("TipoDispositivo", "Access Point"), ("TipoDispositivo", "Analytics Log Management"), ("TipoDispositivo", "Firewall"), ("TipoDispositivo", "HUB"),
            ("TipoDispositivo", "Router"), ("TipoDispositivo", "Switch"), ("TipoDispositivo", "Wireless LAN Controller"), ("TipoDispositivo", "SAN"), ("TipoDispositivo", "Servidor"),
            ("TipoDispositivo", "Workstation"), ("TipoDispositivo", "Storage"), ("TipoDispositivo", "Network"),
            ("Funcion", "Controlador de dominio"), ("Funcion", "Aplicación web"), ("Funcion", "Base de datos"), ("Funcion", "Archivos"), ("Funcion", "Backup"),
            ("TipoInfraestructura", "Físico"), ("TipoInfraestructura", "Virtual"),
            ("SistemaOperativo", "Windows Server 2022"), ("SistemaOperativo", "Windows Server 2019"), ("SistemaOperativo", "Linux (RHEL)"), ("SistemaOperativo", "Linux (Ubuntu)"), ("SistemaOperativo", "VMware ESXi")
        };
        foreach (var (kind, name) in defaults)
        {
            var exists = await db.CatalogItems.AnyAsync(c => c.Kind == kind && c.Name == name, ct);
            if (exists) continue;
            db.CatalogItems.Add(new CatalogItem { Id = Guid.NewGuid(), Kind = kind, Name = name });
        }
        await db.SaveChangesAsync(ct);
    }

    /// <summary>Catálogos para Aplicaciones: Modelo de licenciamiento y Autenticación.</summary>
    public static async Task EnsureCatalogItemsAplicacionesAsync(this AppDbContext db, CancellationToken ct = default)
    {
        var licenciamiento = new[] {
            "Licenciamiento por suscripción", "Licenciamiento concurrente", "Licenciamiento por usuario (User-based)",
            "Licenciamiento por dispositivo (Device-based)", "Licenciamiento perpetuo", "Licenciamiento por volumen",
            "Licenciamiento basado en características o módulos", "Desarrollo Local"
        };
        var autenticacion = new[] { "Active Directory", "LDAP", "SSO", "Local", "OAuth/SAML", "Integrada en aplicación" };
        foreach (var name in licenciamiento)
        {
            if (await db.CatalogItems.AnyAsync(c => c.Kind == "ModeloLicenciamiento" && c.Name == name, ct)) continue;
            db.CatalogItems.Add(new CatalogItem { Id = Guid.NewGuid(), Kind = "ModeloLicenciamiento", Name = name });
        }
        foreach (var name in autenticacion)
        {
            if (await db.CatalogItems.AnyAsync(c => c.Kind == "Autenticacion" && c.Name == name, ct)) continue;
            db.CatalogItems.Add(new CatalogItem { Id = Guid.NewGuid(), Kind = "Autenticacion", Name = name });
        }
        await db.SaveChangesAsync(ct);
    }

    /// <summary>Inserta operaciones de ejemplo que respetan todas las relaciones (Oficina, Área, Alojamiento, etc.) para simular el flujo.</summary>
    public static async Task EnsureOperacionesSampleAsync(this AppDbContext db, CancellationToken ct = default)
    {
        if (await db.Operaciones.AnyAsync(ct)) return;
        var activo = await db.Estatus.OrderBy(e => e.Codigo).FirstOrDefaultAsync(e => e.Codigo == 1000, ct);
        if (activo == null) return;
        var office = await db.Offices.OrderBy(o => o.Name).FirstOrDefaultAsync(ct);
        var area = await db.Areas.OrderBy(a => a.Name).FirstOrDefaultAsync(a => a.Name == "Operaciones", ct);
        var alojamiento = await db.Alojamientos.OrderBy(a => a.Nombre).FirstOrDefaultAsync(a => a.Nombre.Contains("Data Center") || a.Nombre.Contains("On-Premise"), ct);
        var ambiente = await db.Environments.OrderBy(e => e.Name).FirstOrDefaultAsync(e => e.Name == "PRD" || e.Name == "Producción", ct);
        var criticidad = await db.Criticalities.OrderBy(c => c.Name).FirstOrDefaultAsync(c => c.Name == "Medio", ct);
        var categoria = await db.Categories.OrderBy(c => c.Name).FirstOrDefaultAsync(c => c.Name == "A", ct);
        var now = DateTime.UtcNow;

        db.Operaciones.Add(new Operacion
        {
            Id = Guid.NewGuid(),
            Hostname = "SRV-DC-01",
            Serial = "DC01-SAMPLE",
            EstatusId = activo.Id,
            CreatedAt = now,
            OfficeId = office?.Id,
            AreaId = area?.Id,
            AlojamientoId = alojamiento?.Id,
            OwnerAreaId = area?.Id,
            EnvironmentId = ambiente?.Id,
            CriticalityId = criticidad?.Id,
            CategoryId = categoria?.Id,
            TipoDispositivo = "Servidor",
            Funcion = "Controlador de dominio",
            TipoInfraestructura = "Físico",
            RAM = "64 GB",
            CantidadCPU = 2,
            VelocidadCPU = "2.4 GHz",
            CapacidadDAS = "2 TB",
            SistemaOperativo = "Windows Server 2022",
            IP = "192.168.1.10",
            Observaciones = "Activo de ejemplo para simular flujo."
        });
        db.Operaciones.Add(new Operacion
        {
            Id = Guid.NewGuid(),
            Hostname = "SRV-APP-01",
            Serial = "APP01-SAMPLE",
            EstatusId = activo.Id,
            CreatedAt = now,
            OfficeId = office?.Id,
            AreaId = area?.Id,
            AlojamientoId = alojamiento?.Id,
            OwnerAreaId = area?.Id,
            EnvironmentId = ambiente?.Id,
            CriticalityId = criticidad?.Id,
            CategoryId = categoria?.Id,
            TipoDispositivo = "Servidor",
            Funcion = "Aplicación web",
            TipoInfraestructura = "Virtual",
            Host = "SRV-HYPERV-01",
            RAM = "16 GB",
            CantidadCPU = 4,
            VelocidadCPU = "2.0 GHz",
            CapacidadDAS = "500 GB",
            SistemaOperativo = "Windows Server 2019",
            Observaciones = "VM de ejemplo."
        });
        await db.SaveChangesAsync(ct);
    }
}
