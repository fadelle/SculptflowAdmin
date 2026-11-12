namespace Omni.Admin.Entities.Dtos.Channels;

/// <summary>(Copy of the main app's Entities/Dtos/PlatformAdmin ChannelCheckResult, QA t8-20.) A WhatsApp connection checked with
/// its stored token: what Meta says about the number, or why it refused.</summary>
public sealed record ChannelCheckResult(bool Ok, string? DisplayPhoneNumber, string? VerifiedName, string? Error, DateTimeOffset CheckedAt)
{
    public Guid? ClientId { get; init; }
}
