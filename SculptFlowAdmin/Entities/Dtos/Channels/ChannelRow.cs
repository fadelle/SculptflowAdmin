using System.Security.Cryptography;
using Microsoft.AspNetCore.WebUtilities;
using SculptFlowAdmin.Common.Enums;

namespace SculptFlowAdmin.Entities.Dtos.Channels;

public record ChannelRow(Guid Id, Guid ClinicId, string ClinicName, string Channel, string Provider, string Status,
    string? DisplayName, string? SenderId, string? HealthLevel, bool IsHealthy, string? LastProblemMessage, string? LastError,
    string? WebhookStatus, DateTimeOffset? LastWebhookAt, DateTimeOffset? LastVerifiedAt, DateTimeOffset UpdatedAt);
