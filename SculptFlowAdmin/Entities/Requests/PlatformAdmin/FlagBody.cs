namespace SculptFlowAdmin.Entities.Requests.PlatformAdmin;

/// <summary>A yes/no switch (locked, email confirmed, calendar sync). Copy of the main app's FlagBody.</summary>
public record FlagBody(bool Value);
