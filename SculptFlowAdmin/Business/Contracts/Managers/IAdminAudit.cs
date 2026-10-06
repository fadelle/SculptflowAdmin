namespace SculptFlowAdmin.Business.Contracts.Managers;

/// <summary>Records every change an admin makes (admin_audit_log).</summary>
public interface IAdminAudit
{
    Task LogAsync(string action, string entityType, object? entityId, Guid? clinicId = null,
        object? details = null, CancellationToken ct = default);
}
