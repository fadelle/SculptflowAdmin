# Omni Admin

Internal admin portal for Omni. A separate .NET 10 Razor Pages app (own repo, own deploy) that reads and
writes the **same Supabase Postgres database** as the main app (`..\SculptFlowApp`).

- Admins are users of the main app with role `admin` (`identity.users`). The main app checks their sign-in
  (`/api/platform-admin/auth`) and keeps their accounts (`/api/platform-admin/admins`); this portal issues its own cookie and
  asks the main app on every request (remembered for a minute) whether the admin is still active. Client and member accounts
  can never sign in here, and admin accounts can never sign in to the client app.
- Every change made here is recorded in `admin_audit_log` (who, what, when, before/after).
- Internal only: it shows provider details (Meta ids) that client-facing pages never show.

## What it covers

| Area | See | Change |
|---|---|---|
| Dashboard | Totals across clients, messages per day, a "needs attention" list (broken channels/calendars, failing sends, stuck campaigns, failed website scrapes), newest clients | |
| Clients | Every tenant with counts; per client: details, member, channels, dashboard language, Inbox settings (AI bot switches), KB search settings, admin history | Edit details, activate/deactivate, set the dashboard's default language (English, Arabic, French), turn the AI bot on/off for new and reopened conversations, tune KB retrieval |
| Member accounts | All main-app users with client, membership, sign-in methods, own dashboard language ("Business default" when none) | Lock/unlock sign-in, deactivate/reactivate membership, set password, mark email confirmed |
| Contacts | All contacts with filters; per contact: conversations, bookings, events, opt-out | Status, qualification, marketing opt-in, notes; export a contact's data (JSON) and delete it for good for a privacy request |
| Conversations | All threads; full message timeline | Hand to member / return to AI, close/reopen |
| Messages | Every message, failed-only filter | |
| Bookings | All, in client time | Change status |
| Billing | Every client's plan, wallet, included credit; per client: subscription (free trials), add-ons, who pays the provider per messaging account, usage this period with provider cost and margin, usage records, ledger, ledger check, price check; plans; rate cards and versioned rates; revenue report | Start/change/cancel/resume plans, top up the wallet, manual adjustments, refunds, set who pays per account, create/edit plans (incl. free-trial days), the add-on catalog and each client's add-on units, rate cards, add/end rates. All through the main app's billing API (below) |
| Channels & integrations | WhatsApp/Instagram/Facebook/Telegram connections with health events; calendars; TikTok | Disconnect; pause/resume calendar sync |
| WhatsApp token check | Asks Meta what permissions a token has and which client WhatsApp business accounts it sees (embedded-signup diagnostic) | Inspect a token or code (nothing stored) |
| WhatsApp templates | All templates and their approval status | |
| Campaigns | Delivery/read/reply/booking funnel, recipients | Cancel |
| Services | Every client's catalog | Activate/deactivate |
| Business types | Names, terminology (per language), AI profile and pipeline stages of each business type, how many clients use it, and a "missing Arabic / missing French" badge when a name, word or stage label is still empty in that language | Create (or duplicate), edit, activate/deactivate; a client's page has **Change business type**, which previews how its contacts' stages move before confirming |
| Knowledge base | Documents and website sources | Activate/deactivate documents |
| Search benchmark | One client's Knowledge Retrieval Benchmark (pick the client in the header): case counts, latest scores (chunk/document Top-1/3/5, MRR, latency), manual cases, generations with rejected questions and n8n's raw reply, run history, per-run results and failed-case analysis | Generate test cases (n8n), stop generating, add/edit/review/delete cases, run the benchmark (all, generated, reviewed or one generation). Through the main app's benchmark API; JSON for the page at `/api/admin/knowledge-benchmark/{clientId}` |
| Event log / Admin audit log | The main app's `events`; this portal's own changes | |
| Admin users | Admin accounts (the main app's users with role `admin`) | Add, deactivate (signed out on their next click), set password (also unlocks). Through the main app's admins API |
| Configuration | The main app's tunable settings by section and key: default, saved value, value in use | Save a value (the main app uses it at once), reset to the default. Through the main app's settings API (below) |

The same data and actions are available as JSON under `/api/admin/*` (see `Controllers/Admin/AdminApiController.cs`); they
need a signed-in admin.

## Setup

1. After the main app's `Database/schema.sql`, run `Database/admin-schema.sql` against the same database (Supabase SQL
   editor). It only adds `admin.admin_audit_log` (its `user_id` references the main app's `identity.users`). A project filled
   by the main app's data copy already has it, so the file changes nothing there. **Never run it on the old (pre-Omni) live
   database**: the old portal and the data copy read the old columns. The full order of database, Render and n8n steps for
   both apps is the main app's `docs/deploy-runbook.md`.
2. Connection string, same key as the main app:
   `dotnet user-secrets set ConnectionStrings:Postgres "<connection string>"` (hosted: `ConnectionStrings__Postgres`).
3. Main app connection (required, sign-in goes through it): `MainApp:PublicBaseUrl` (the main app's public URL),
   `MainApp:PlatformAdminApiKey` (a SECRET, user-secrets or `MainApp__PlatformAdminApiKey`) equal to the main app's
   `PlatformAdmin:ApiKey`, and optionally `MainApp:ApiBaseUrl` (where to call the main app; blank = `PublicBaseUrl`).
   Without them the sign-in page says sign-in isn't available. `appsettings.json` points `PublicBaseUrl` at the live main
   app, so a staging portal must set `MainApp__PublicBaseUrl` to the staging main app.
4. The first admin is made in the main app: `dotnet Omni.App.dll create-admin you@example.com "Your Name"` (password from
   `ADMIN_PASSWORD`; see the main app's README). Add more admins from the Admin users page.
5. `dotnet run` and sign in. Tests: `dotnet test Omni.Admin.slnx` (no database needed).

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
main-app API shapes the portal copies. The portal has no access to the main app's tables; it reads and writes
everything through `/api/platform-admin`. Code layout and rules: `CLAUDE.md`.
