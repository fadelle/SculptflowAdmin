namespace Omni.Admin.Entities.Models;

public class AdminAuditEntry
{
    public Guid Id { get; set; }
    /// <summary>The acting admin: the main app's identity.users id (text); null for "system" or an admin since removed.</summary>
    public string? UserId { get; set; }
    public string AdminEmail { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public Guid? ClientId { get; set; }
    public string? Details { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
