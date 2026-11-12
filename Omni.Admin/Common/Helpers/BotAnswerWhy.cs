using Omni.Admin.Entities.Responses.Monitoring;

namespace Omni.Admin.Common.Helpers;

/// <summary>The conversation page's "Why didn't the bot answer?" box: what happened to the customer's last message.</summary>
public sealed record BotAnswerNote(string Headline, string Detail, bool Answered, string? FixLabel = null, string? FixLink = null);

/// <summary>
/// Plain words for a message trace's outcome and reason (QA t6-admin-28: an admin had to open the trace and read technical
/// step names). The codes are the main app's TraceResults and its skip reasons (AiBotBlockReason, MessageService, BotEngine).
/// </summary>
public static class BotAnswerWhy
{
    public static BotAnswerNote Explain(MessageTrace? trace, Guid clientId)
    {
        var client = $"/Clients/Details?id={clientId}";
        var billing = $"/Billing/Client?id={clientId}";
        if (trace is null)
            return new("No trace for the last customer message", "It was saved before tracing, or its trace is past retention.", false);

        var link = $"/Traces/Details?traceId={Uri.EscapeDataString(trace.TraceId)}";
        switch (trace.Outcome)
        {
            case "replied": return new("Answered", "A reply went out for the customer's last message.", true, "Open the trace", link);
            case "handed_off": return new("Handed to the team", "The bot passed the conversation to the client's team (its handoff, or after a failed run).", true, "Open the trace", link);
            case "in_progress": return new("Still working", "The bot was asked a moment ago; a reply may still come.", false, "Open the trace", link);
            case "no_reply": return new("The bot was asked, but nothing came back", "The AI engine didn't answer (n8n or the model). Look at the trace's last steps and the error log.", false, "Open the trace", link);
            case "received": return new("The bot wasn't asked", "The message was saved and no step says why.", false, "Open the trace", link);
            case "failed": return new("Sending failed", $"It failed at \"{Plain(trace.OutcomeReason)}\".", false, "Open the trace", link);
        }

        if (trace.Outcome != "skipped") return new("Nothing to explain", "The last trace isn't a customer message's story.", false, "Open the trace", link);

        // Skipped on purpose: the most useful reason is why a conversation started with the team, when the trace says so.
        var startedWithTeam = trace.Entries.Select(e => e.Detail.GetValueOrDefault("startedWithTeam")).FirstOrDefault(v => !string.IsNullOrEmpty(v));
        var reason = trace.OutcomeReason == "conversation_in_human_mode" && startedWithTeam is not null ? startedWithTeam : trace.OutcomeReason;
        return reason switch
        {
            "plan_without_ai_assistant" => new("The plan doesn't include the AI assistant", "Change the client's plan or add the AI add-on.", false, "Billing", billing),
            "client_inactive" => new("The client is deactivated", "No bot answers for a deactivated client.", false, "Client page", client),
            "account_not_connected" => new("No connected account on this channel", "The client connects it from its Settings → Integrations page.", false, "Client page", client + "#channels"),
            "ai_disabled" => new("AI is off on this account", "Turn it on under AI per account.", false, "AI per account", client + "#account-ai"),
            "no_bot" => new("No bot is picked for this account", "AI is on, but no bot answers it. Pick one under AI per account.", false, "AI per account", client + "#account-ai"),
            "bot_inactive" => new("The account's bot is switched off", "The bot is a draft or inactive; the client activates it on its Bots page.", false, "AI per account", client + "#account-ai"),
            "ai_bot_limit" => new("Too many bots for the plan", "This bot is beyond the plan's bot limit (after a downgrade).", false, "Billing", billing),
            "reopened_to_human" => new("Reopened conversations go to the team", "This account hands conversations a contact reopens to the team.", false, "AI per account", client + "#account-ai"),
            "new_conversations_off" => new("New conversations go to the team", "This account doesn't let the bot start new conversations.", false, "AI per account", client + "#account-ai"),
            "media_message" or "message_type_not_supported" => new("The bot can't read that kind of message", "Photos, voice notes, files, stickers and locations go to the team.", false),
            "interactive_without_text" => new("The reply had no text", "A form answer (WhatsApp Flows) or a button without words gives the bot nothing to read; the team sees it in the Inbox.", false),
            "conversation_in_human_mode" or "member_replied_from_business_app" or "reopened" => new("The team has this conversation", "It is in human mode, so the bot stays quiet until it is returned to AI.", false),
            "opt_out_keyword" => new("The customer sent a stop word", "They opted out, so nothing is sent.", false),
            "service_window_closed" => new("WhatsApp's 24-hour window is closed", "Only an approved template can be sent now.", false),
            "conversation_gone" => new("The conversation was deleted", "It was removed before the bot answered.", false),
            "duplicate_reply" => new("A reply had already gone out", "The second reply was dropped.", true),
            "n8n_not_configured" => new("The AI engine isn't set up", "A developer has to set the n8n address in Configuration.", false, "Configuration", "/Configuration"),
            "ai_failed" => new("The AI failed", "The run failed and the conversation went to the team.", false, "Open the trace", link),
            { } r when r.StartsWith("ai_reply_", StringComparison.Ordinal) => new("The AI's reply couldn't be sent", $"Reason: {Plain(r["ai_reply_".Length..])}. The conversation went to the team.", false, "Open the trace", link),
            _ => new("Not answered on purpose", $"Reason: {Plain(reason)}.", false, "Open the trace", link)
        };
    }

    private static string Plain(string? code) => string.IsNullOrWhiteSpace(code) ? "unknown" : code.Replace('_', ' ').Replace('.', ' ');
}
