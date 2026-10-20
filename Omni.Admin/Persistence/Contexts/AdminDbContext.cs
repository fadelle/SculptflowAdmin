using Microsoft.EntityFrameworkCore;
using Omni.Admin.Entities.Models;

namespace Omni.Admin.Persistence.Contexts;

/// <summary>
/// The admin portal's own table (Database/admin-schema.sql): the audit log. The portal has no access to the main app's
/// tables; it reads and changes those (admin users included) only through the main app's platform-admin APIs.
/// </summary>
public class AdminDbContext : DbContext
{
    public AdminDbContext(DbContextOptions<AdminDbContext> options) : base(options)
    {
    }

    public DbSet<AdminAuditEntry> AuditLog => Set<AdminAuditEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AdminAuditEntry>(e =>
        {
            e.ToTable("admin_audit_log", "admin");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.UserId).HasColumnName("user_id");
            e.Property(x => x.AdminEmail).HasColumnName("admin_email").IsRequired();
            e.Property(x => x.Action).HasColumnName("action").IsRequired();
            e.Property(x => x.EntityType).HasColumnName("entity_type").IsRequired();
            e.Property(x => x.EntityId).HasColumnName("entity_id");
            e.Property(x => x.ClientId).HasColumnName("client_id");
            e.Property(x => x.Details).HasColumnName("details");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
        });
    }
}
