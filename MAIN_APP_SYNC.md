# Main app sync record

The admin portal reads the main app's database directly, so it has to keep up with the main app
(`..\SculptFlowApp`). This file records how far it has been checked.

## Last synced

| | |
|---|---|
| Synced on | 2026-10-05 |
| Main app commit | `12fad04` on branch `feature/infobip-whatsapp-provider` (Add Infobip as a WhatsApp provider behind a provider seam), on top of `main` `febef3c` |
| Main app tables mapped | 32 DbSets in `Persistence/Contexts/ApplicationDbContext.cs` + Identity users/claims/logins/tokens |

## How to update the portal ("update the admin portal")

1. In `SculptFlowApp`: `git fetch`, then `git log --oneline 12fad04..origin/main` (use the commit above; also check feature branches), plus
   `git branch -a` and `git status` for work not merged yet.
2. For each new commit, look at what changed in `Database/schema.sql`, `PlasticSurgery/Entities/Models/*`,
   `PlasticSurgery/Common/Enums/*`, `PlasticSurgery/Persistence/Contexts/ApplicationDbContext.cs`,
   `PlasticSurgery/Business/*` (business rules the admin mirrors) and
   new pages/features. `docs/system-design.html` §12 change log summarises each change.
3. Tell Mohammad what is new and which admin changes are suggested, before building.
4. Copy the main app's `Entities/Models/*.cs` (main-app tables, minus the `billing.*` ones), the entity vocabularies
   in `Common/Enums/*.cs`, `Common/Statics/EventTypes.cs`, `Common/Helpers/ConversationModeSync.cs` and
   `Persistence/Contexts/ApplicationDbContext.cs` into the same paths under `SculptFlowAdmin/`, then change the
   namespace prefix from `PlasticSurgery.` to `SculptFlowAdmin.`. Never edit the copies by hand.
5. Update the admin repositories (`Persistence/Repositories/*`), services and pages, build, test against a throwaway
   database, and update the table above.

## Rules the admin portal mirrors from the main app

If any of these change in the main app, the admin's copy must change too.

| Admin action | Mirrors | Where in the main app |
|---|---|---|
| Hand to staff / Return to AI | `ConversationModeSync.Apply` (copied entity code) | `Common/Helpers/ConversationModeSync.cs` |
| Cancel campaign | `CampaignService.CancelAsync` | `Business/Services/Campaigns/CampaignService.cs` |
| Disconnect channel | `ChannelIntegrationService.DisconnectAsync` (Telegram webhook is not deleted at Telegram from here) | `Business/Services/Channels/ChannelIntegrationService.cs` |
| Connect Infobip sender | `InfobipWhatsAppIntegrationService.ConnectAsync` (minus the live Infobip number check) | `Business/Services/Channels/InfobipWhatsAppIntegrationService.cs` |
| Infobip webhook URL | `InfobipWebhookUrls.Build` | `Common/Helpers/InfobipWebhookUrls.cs` |
| Disconnect calendar / TikTok | `CalendarIntegrationService.DisconnectAsync`, `TikTokIntegrationService.DisconnectAsync` (tokens forgotten, not revoked) | `Business/Services/Calendars/`, `Business/Services/TikTok/` |
| Staff lock / password | ASP.NET Identity lockout + `PasswordHasher<IdentityUser>` | `Program.cs` (Identity setup) |
| Staff name | `full_name` user claim | `Business/Services/Clinics/ClinicRegistrationService.cs` |

## Areas the portal drives through the main app's API (no copied rules)

| Area | Main-app API | Portal copy to keep in step |
|---|---|---|
| Billing (plans, rate cards, subscriptions, wallets, provider billing, usage, ledger, report) | `/api/platform-admin/billing/*`, `PlatformAdmin:ApiKey` (`X-Platform-Admin-Key`), actor `X-Admin-Actor`, `Idempotency-Key` on money writes | `Entities/{Requests,Responses,Dtos}/Billing/*` = the admin part of the main app's `Entities/{Requests,Responses,Dtos}/Billing/*`. If those shapes change, update the copy. |

Added 2026-10-06 against the main app's `feature/subscription-billing` branch (not committed there yet). Its tables
(`billing.*`) are deliberately not mapped in `Persistence/Contexts/ApplicationDbContext.cs`.

## Known gaps in the main app that limit admin control

- **`clinics.is_active` is not enforced.** Deactivating a clinic in the portal marks it, but the main app still lets its
  staff in and its AI keep replying. Fix in the main app: check it in `CurrentClinicContext` and before AI triggers.
- **No security-stamp check on the staff cookie.** Locking a staff account blocks new sign-ins only; an open session
  lasts until its cookie expires. Fix in the main app: add `SecurityStampValidator` to the Identity cookie.
- Admin writes don't send SignalR events, so a clinic's open Inbox shows them on its next refresh.
- Appointment status changes from the portal don't push to the clinic's Google/Outlook calendar.
