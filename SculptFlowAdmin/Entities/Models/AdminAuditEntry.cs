namespace SculptFlowAdmin.Entities.Models;

public class AdminAuditEntry
{
    public Guid Id { get; set; }
    public Guid? AdminUserId { get; set; }
    public string AdminEmail { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public Guid? ClinicId { get; set; }
    public string? Details { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
