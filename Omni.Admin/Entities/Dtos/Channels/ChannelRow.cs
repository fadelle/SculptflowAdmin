using System.Security.Cryptography;
using Microsoft.AspNetCore.WebUtilities;

namespace Omni.Admin.Entities.Dtos.Channels;

public record ChannelRow(Guid Id, Guid ClinicId, string ClinicName, string Channel, string Status,
    string? DisplayName, string? SenderId, string? HealthLevel, bool IsHealthy, string? LastProblemMessage, string? LastError,
    string? WebhookStatus, DateTimeOffset? LastWebhookAt, DateTimeOffset? LastVerifiedAt, DateTimeOffset UpdatedAt);
