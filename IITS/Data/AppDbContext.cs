using IITS.Entities;
using Microsoft.EntityFrameworkCore;

namespace IITS.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Estatus> Estatus => Set<Estatus>();
    public DbSet<Aplicacion> Aplicaciones => Set<Aplicacion>();
    public DbSet<Operacion> Operaciones => Set<Operacion>();
    public DbSet<CuentaServicio> CuentasServicio => Set<CuentaServicio>();
    public DbSet<CuentaPrivilegiada> CuentasPrivilegiadas => Set<CuentaPrivilegiada>();
    public DbSet<PaginaWeb> PaginasWeb => Set<PaginaWeb>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<Aprobacion> Aprobaciones => Set<Aprobacion>();
    public DbSet<AprobacionVoto> AprobacionVotos => Set<AprobacionVoto>();
    public DbSet<ApprovalRequest> ApprovalRequests => Set<ApprovalRequest>();
    public DbSet<ApprovalDecision> ApprovalDecisions => Set<ApprovalDecision>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<AuditEvent> AuditEvents => Set<AuditEvent>();
    public DbSet<EmailOutbox> EmailOutbox => Set<EmailOutbox>();
    public DbSet<AprobacionPermiso> AprobacionPermisos => Set<AprobacionPermiso>();
    public DbSet<Alojamiento> Alojamientos => Set<Alojamiento>();
    public DbSet<Area> Areas => Set<Area>();
    public DbSet<Office> Offices => Set<Office>();
    public DbSet<IITS.Entities.Environment> Environments => Set<IITS.Entities.Environment>();
    public DbSet<Criticality> Criticalities => Set<Criticality>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Vendor> Vendors => Set<Vendor>();
    public DbSet<DeviceModel> DeviceModels => Set<DeviceModel>();
    public DbSet<Asset> Assets => Set<Asset>();
    public DbSet<ManagedAccount> ManagedAccounts => Set<ManagedAccount>();
    public DbSet<ManagedAccountSecurityGroup> ManagedAccountSecurityGroups => Set<ManagedAccountSecurityGroup>();
    public DbSet<CatalogItem> CatalogItems => Set<CatalogItem>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<UserRole>().ToTable("UserRole");
        b.Entity<UserRole>().HasKey(ur => new { ur.UserId, ur.RoleId });
        b.Entity<UserRole>()
            .HasOne(ur => ur.User).WithMany(u => u.UserRoles).HasForeignKey(ur => ur.UserId);
        b.Entity<UserRole>()
            .HasOne(ur => ur.Role).WithMany(r => r.UserRoles).HasForeignKey(ur => ur.RoleId);

        b.Entity<RolePermission>().ToTable("RolePermission");
        b.Entity<RolePermission>().HasKey(rp => new { rp.RoleId, rp.PermissionId });
        b.Entity<RolePermission>().HasOne(rp => rp.Role).WithMany(r => r.RolePermissions).HasForeignKey(rp => rp.RoleId).OnDelete(DeleteBehavior.Cascade);
        b.Entity<RolePermission>().HasOne(rp => rp.Permission).WithMany(p => p.RolePermissions).HasForeignKey(rp => rp.PermissionId).OnDelete(DeleteBehavior.Cascade);

        b.Entity<AprobacionPermiso>().HasIndex(p => new { p.UserId, p.Modulo }).IsUnique();
        b.Entity<AprobacionPermiso>()
            .HasOne(p => p.User).WithMany().HasForeignKey(p => p.UserId).OnDelete(DeleteBehavior.Cascade);

        b.Entity<AprobacionVoto>().HasIndex(v => new { v.AprobacionId, v.UserId }).IsUnique();
        b.Entity<AprobacionVoto>().HasOne(v => v.Aprobacion).WithMany().HasForeignKey(v => v.AprobacionId).OnDelete(DeleteBehavior.Cascade);
        b.Entity<AprobacionVoto>().HasOne(v => v.User).WithMany().HasForeignKey(v => v.UserId).OnDelete(DeleteBehavior.NoAction);

        b.Entity<ApprovalDecision>().HasOne(d => d.ApprovalRequest).WithMany(r => r.Decisions).HasForeignKey(d => d.ApprovalRequestId).OnDelete(DeleteBehavior.Cascade);
        b.Entity<ApprovalDecision>().HasOne(d => d.DecidedByUser).WithMany().HasForeignKey(d => d.DecidedByUserId).OnDelete(DeleteBehavior.NoAction);
        b.Entity<ApprovalRequest>().HasOne(r => r.SubmittedByUser).WithMany().HasForeignKey(r => r.SubmittedByUserId).OnDelete(DeleteBehavior.NoAction);

        b.Entity<DeviceModel>().HasOne(d => d.Manufacturer).WithMany(v => v.DeviceModels).HasForeignKey(d => d.ManufacturerId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Asset>().HasOne(a => a.Office).WithMany().HasForeignKey(a => a.OfficeId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Asset>().HasOne(a => a.Area).WithMany().HasForeignKey(a => a.AreaId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Asset>().HasOne(a => a.Status).WithMany().HasForeignKey(a => a.StatusId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Asset>().HasOne(a => a.OperationEnvironment).WithMany().HasForeignKey(a => a.OperationEnvironmentId).OnDelete(DeleteBehavior.NoAction);
        b.Entity<Asset>().HasOne(a => a.OwnerArea).WithMany().HasForeignKey(a => a.OwnerAreaId).OnDelete(DeleteBehavior.NoAction);
        b.Entity<Asset>().HasOne(a => a.Criticality).WithMany().HasForeignKey(a => a.CriticalityId).OnDelete(DeleteBehavior.NoAction);
        b.Entity<Asset>().HasOne(a => a.Environment).WithMany().HasForeignKey(a => a.EnvironmentId).OnDelete(DeleteBehavior.NoAction);
        b.Entity<Asset>().HasOne(a => a.Category).WithMany().HasForeignKey(a => a.CategoryId).OnDelete(DeleteBehavior.NoAction);
        b.Entity<Asset>().HasOne(a => a.Manufacturer).WithMany().HasForeignKey(a => a.ManufacturerId).OnDelete(DeleteBehavior.NoAction);
        b.Entity<Asset>().HasOne(a => a.DeviceModel).WithMany().HasForeignKey(a => a.DeviceModelId).OnDelete(DeleteBehavior.NoAction);
        b.Entity<ManagedAccount>().HasOne(m => m.Area).WithMany().HasForeignKey(m => m.AreaId).OnDelete(DeleteBehavior.NoAction);
        b.Entity<ManagedAccount>().HasOne(m => m.Estatus).WithMany().HasForeignKey(m => m.EstatusId).OnDelete(DeleteBehavior.NoAction);
        b.Entity<ManagedAccountSecurityGroup>().HasOne(s => s.ManagedAccount).WithMany(m => m.SecurityGroups).HasForeignKey(s => s.ManagedAccountId).OnDelete(DeleteBehavior.Cascade);

        b.Entity<Operacion>().HasOne(o => o.Office).WithMany().HasForeignKey(o => o.OfficeId).OnDelete(DeleteBehavior.NoAction);
        b.Entity<Operacion>().HasOne(o => o.Area).WithMany().HasForeignKey(o => o.AreaId).OnDelete(DeleteBehavior.NoAction);
        b.Entity<Operacion>().HasOne(o => o.Alojamiento).WithMany().HasForeignKey(o => o.AlojamientoId).OnDelete(DeleteBehavior.NoAction);
        b.Entity<Operacion>().HasOne(o => o.OwnerArea).WithMany().HasForeignKey(o => o.OwnerAreaId).OnDelete(DeleteBehavior.NoAction);
        b.Entity<Operacion>().HasOne(o => o.Environment).WithMany().HasForeignKey(o => o.EnvironmentId).OnDelete(DeleteBehavior.NoAction);
        b.Entity<Operacion>().HasOne(o => o.Criticality).WithMany().HasForeignKey(o => o.CriticalityId).OnDelete(DeleteBehavior.NoAction);
        b.Entity<Operacion>().HasOne(o => o.Category).WithMany().HasForeignKey(o => o.CategoryId).OnDelete(DeleteBehavior.NoAction);
        b.Entity<Operacion>().HasOne(o => o.Manufacturer).WithMany().HasForeignKey(o => o.ManufacturerId).OnDelete(DeleteBehavior.NoAction);
        b.Entity<Operacion>().HasOne(o => o.DeviceModel).WithMany().HasForeignKey(o => o.DeviceModelId).OnDelete(DeleteBehavior.NoAction);
        b.Entity<CuentaPrivilegiada>().HasOne(c => c.Area).WithMany().HasForeignKey(c => c.AreaId).OnDelete(DeleteBehavior.NoAction);
        b.Entity<CuentaServicio>().HasOne(c => c.Area).WithMany().HasForeignKey(c => c.AreaId).OnDelete(DeleteBehavior.NoAction);
    }
}
