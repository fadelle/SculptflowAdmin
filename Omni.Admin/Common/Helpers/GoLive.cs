using Omni.Admin.Entities.Dtos.Channels;
using Omni.Admin.Entities.Dtos.Members;
using Omni.Admin.Entities.Responses.Billing;
using Omni.Admin.Entities.Responses.PlatformAdmin;

namespace Omni.Admin.Common.Helpers;

/// <summary>One go-live step on the client page: done or not, what to do, and where.</summary>
public sealed record GoLiveStep(string Label, bool Done, string Hint, string? Link);

/// <summary>
/// What a client still needs before it can really serve its customers (QA t6-admin-28: nothing told the admin what was
/// missing for "live"). Built only from what the client page already loads; a part the main app didn't answer counts as
/// not done, with a hint saying so.
/// </summary>
public static class GoLive
{
    /// <summary>The main app's role of the person who signed the client up (UserRole.Client).</summary>
    public const string OwnerRole = "client";

    public static IReadOnlyList<GoLiveStep> Steps(Guid clientId, bool clientActive, IReadOnlyList<MemberRow> members,
        IReadOnlyList<ChannelRow> channels, InboxSettingsDto? inbox, ClientBillingSummary? billing)
    {
        var billingLink = $"/Billing/Client?id={clientId}";
        var owner = members.FirstOrDefault(m => m.Role == OwnerRole);
        // The main app's own answer per account (AccountAiSettingsDto.BotAnswers: AI on, an active bot within the plan's limit).
        var aiAnswering = inbox is not null && inbox.Accounts.Any(a => a.BotAnswers);

        return
        [
            new("Client active", clientActive, "Activate the client (button at the top).", null),
            new("Owner signed in", owner is { LastLoginAt: not null, MembershipActive: true, UserActive: true },
                owner is null ? "Nobody has joined yet: send the owner invite again." : "The owner joined but hasn't signed in yet.",
                owner is null ? "#owner-invite" : "#members"),
            new("Channel connected", channels.Any(c => c.Status == "connected"),
                "The client connects WhatsApp, Telegram or Instagram from its Settings → Integrations page.", "#channels"),
            new("AI answering", aiAnswering,
                inbox is null ? "The main app didn't answer for the AI settings."
                : !inbox.PlanIncludesAiBot ? "The plan doesn't include the AI assistant: change the plan on Billing."
                : "The client builds a bot on its Bots page; then pick it for an account under AI per account.",
                inbox is { PlanIncludesAiBot: false } ? billingLink : "#account-ai"),
            new("Plan active", billing is { HasAccess: true },
                billing is null ? "The main app didn't answer for billing." : "Start a plan on the Billing page.", billingLink),
            new("Wallet funded", billing is { Spendable: > 0 },
                billing is null ? "The main app didn't answer for billing." : "Top up the wallet so charged messages can be sent.",
                billingLink + "&open=top-up"),
        ];
    }
}
