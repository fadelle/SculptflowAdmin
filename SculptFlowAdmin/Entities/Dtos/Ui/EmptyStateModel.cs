namespace SculptFlowAdmin.Entities.Dtos.Ui;

/// <summary>What the _EmptyState partial shows: say what's empty and why ("No leads match your filters").</summary>
public sealed record EmptyStateModel(string Message, string Icon = "inbox", string? ActionHref = null, string? ActionLabel = null);
