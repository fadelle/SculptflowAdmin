# SculptFlow Admin

Internal admin portal for SculptFlow. A separate .NET 10 Razor Pages app (own repo, own deploy) that reads and
writes the **same Supabase Postgres database** as the main app (`..\SculptFlowApp`).

- Admins are their own user list (`admin_users`). Clinic staff accounts can never sign in here, and admin accounts are
  not clinic users.
- Every change made here is recorded in `admin_audit_log` (who, what, when, before/after).
- Internal only: it shows provider details (Infobip, Meta ids) that clinic-facing pages never show.

## What it covers

| Area | See | Change |
|---|---|---|
| Dashboard | Totals across clinics, messages per day, a "needs attention" list (broken channels/calendars, failing sends, stuck campaigns, failed website scrapes), newest clinics | |
| Clinics | Every tenant with counts; per clinic: details, staff, channels, KB search settings, admin history | Edit details, activate/deactivate, connect an Infobip WhatsApp sender, tune KB retrieval |
| Staff accounts | All main-app users with clinic, membership, sign-in methods | Lock/unlock sign-in, deactivate/reactivate membership, set password, mark email confirmed |
| Leads | All leads with filters; per lead: conversations, appointments, events | Status, qualification, marketing opt-in, notes |
| Conversations | All threads; full message timeline | Hand to staff / return to AI, close/reopen |
| Messages | Every message, failed-only filter | |
| Appointments | All, in clinic time | Change status |
| Billing | Every clinic's plan, wallet, included credit; per clinic: subscription, who pays the provider per messaging account, usage this period with provider cost and margin, usage records, ledger, ledger check, price check; plans; rate cards and versioned rates; revenue report | Start/change/cancel/resume plans, top up the wallet, manual adjustments, refunds, set who pays per account, create/edit plans and rate cards, add/end rates. All through the main app's billing API (below) |
| Channels & integrations | WhatsApp/Instagram/Facebook/Telegram connections with health events; calendars; TikTok | Disconnect; pause/resume calendar sync; Infobip webhook URL |
| WhatsApp templates | All templates and their approval status | |
| Campaigns | Delivery/read/reply/booking funnel, recipients | Cancel |
| Procedures | Every clinic's catalog | Activate/deactivate |
| Knowledge base | Documents and website sources | Activate/deactivate documents |
| Event log / Admin audit log | The main app's `events`; this portal's own changes | |
| Admin users | Portal accounts | Add, deactivate, set password |
| Configuration | The main app's tunable settings by section and key: default, saved value, value in use | Save a value (the main app uses it at once), reset to the default. Through the main app's settings API (below) |

The same data and actions are available as JSON under `/api/admin/*` (see `Controllers/Admin/AdminApiController.cs`); they
need a signed-in admin.

## Setup

1. Run `Database/admin-schema.sql` once against the database (Supabase SQL editor). It only adds `admin_users` and
   `admin_audit_log`; it never touches the main app's tables.
2. Connection string, same key as the main app:
   `dotnet user-secrets set ConnectionStrings:Postgres "<connection string>"` (hosted: `ConnectionStrings__Postgres`).
3. Optional config: `MainApp:PublicBaseUrl` (the main app's public URL, used to show Infobip webhook URLs) and
   `MainApp:WhatsAppProvider` (`meta` or `infobip`, shown on the Channels page). Set these to match the main app.
   For the Billing pages: `MainApp:PlatformAdminApiKey` (a SECRET, user-secrets or `MainApp__PlatformAdminApiKey`) equal
   to the main app's `PlatformAdmin:ApiKey`, and optionally `MainApp:ApiBaseUrl` (where to call the main app; blank =
   `PublicBaseUrl`). Without them the Billing and Configuration pages say they aren't connected; the rest of the portal works.
4. Create the first admin: `cd SculptFlowAdmin` then `dotnet run -- create-admin you@example.com "Your Name"` (type the
   password when asked, or pass it in `ADMIN_PASSWORD`). Add more admins from the Admin users page.
5. `dotnet run` and sign in.

## Billing and Configuration go through the main app

In these areas the main app owns the rules and the data, and this portal is only the control panel. The pages call a
service (`Business/Services/Billing/BillingAdminService`, `Business/Services/Configuration/SettingsAdminService`). The
service calls the main app's `/api/platform-admin/billing/*` or `/api/platform-admin/settings` through a client in
`Business/HttpClients/MainApp`; the clients share `MainAppApiClient` for the key, the actor header, errors and
idempotency. The portal never reads or writes the `billing.*` or `config.*` tables directly. Money-moving forms carry an
operation id that becomes the API's `Idempotency-Key`, so a double submit applies once. Each successful change is also
written to `admin_audit_log`. Future areas with money or complex rules should follow the same pattern
(`/api/platform-admin/{domain}` in the main app).

## Keeping up with the main app

`MAIN_APP_SYNC.md` records which main-app commit this portal was last checked against, how to update it, and which
main-app rules the admin actions mirror. `Entities/Models`, the entity vocabularies in `Common/Enums` and `Persistence/Contexts/ApplicationDbContext.cs` are
copies of the main app's. Code layout and rules: `CLAUDE.md`.
