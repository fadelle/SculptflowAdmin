# Main app sync record

Since 2026-10-07 the admin portal has no access to the main app's tables: every read and write goes through the main
app's platform-admin APIs (`/api/platform-admin/{domain}`). The only thing to keep in step is the portal's copy of
each API's request/response shapes. This file records how far that has been checked.

## Last synced

| | |
|---|---|
| Synced on | 2026-10-22 (Milestone C P2: add-ons, trial plans, AI bots, contact export/delete); 2026-10-21 (Milestone C P1: member reset email and invites); before: 2026-10-20 (P3: admin sign-in and admins; P2: the rest; plan fixes: `bookings` entitlement, `ai_reply` usage; Milestone B P2: language fields) |
| Main app commit | Milestone C P2: `e4a86df` on branch `feature/go-live` (S3 opt-out: no shape change, `ContactDetail.OptedOutAt` already copied; S4 `GET contacts/{id}/export` passed through as JSON (not copied) and `DELETE contacts/{id}` → `PlatformAdminChange`; S6 `billing/addons` (`GET`, `PUT {code}` with `AddonRequest`), `billing/clients/{id}/addons` (`GET`, `PUT {code}` with `SetClientAddonRequest` + Idempotency-Key → `ClientAddonRow[]`), `PlanRequest`/`PlanResponse.TrialDays`, `ClientBillingSummary.TrialDays` + `Addons`, `InboxSettingsDto.ChannelsWithoutAiBot` + `AiBotLimit`; ledger type `addon_charge` arrives labelled "Add-ons"; S5 reminders have no platform-admin API). Milestone C P1: `4e683cf` on branch `feature/go-live` (S1 `POST members/{userId}/password-reset-email` → `PlatformAdminChange`; S2 `GET members/invites?clientId` → `InviteResponse[]`, `DELETE members/invites/{id}` → `PlatformAdminChange`; copy: `Entities/Responses/Members/InviteResponse`). Before: `bb95a8c` on branch `refactor/generic-platform` (the Omni rename, business types, one users table); the portal's copies match its `Omni.App/Entities/{Dtos,Requests,Responses}/PlatformAdmin` and `BusinessTypes`. Checked again against the uncommitted plan fixes on top of `2b72d25` (no shape change: the new `bookings` feature comes through `GET billing/entitlements` into the plan editor by itself; `ai_reply` added to `BillingVocab.EventTypes` and labelled "AI reply"). Milestone B P2 checked against `09ded40` (S1-S3): `MemberRow.Language` (new last field), `PUT clients/{id}/default-language` with `DefaultLanguageBody`; contacts' `preferredLanguage` is now always a two-letter code or null (no shape change); the other S1-S3 changes are not platform-admin shapes |

## How to update the portal ("update the admin portal")

1. In `SculptFlowApp`: `git fetch`, then `git log --oneline <commit above>..origin/main` (also check feature branches),
   plus `git branch -a` and `git status` for work not merged yet.
2. For each new commit, look at `Omni.App/Controllers/Admin/*`, `Omni.App/Entities/{Dtos,Requests,Responses}/`
   for the areas below, and new pages/features. `docs/system-design.html` §4 lists every platform-admin endpoint and §12
   summarises each change.
3. Tell Mohammad what is new and which admin changes are suggested, before building.
4. Update the shapes in the right-hand column, the API clients and the pages; build; test against a throwaway database
   with both apps running; update the table above.

## Areas and the shapes the portal copies

All use `PlatformAdmin:ApiKey` (`X-Platform-Admin-Key`), actor `X-Admin-Actor`, and `Idempotency-Key` on money writes.
Writes outside billing answer `{ clientId }` (`PlatformAdminChange`) so the portal can file its audit entry.

| Area | Main-app API | Portal copy |
|---|---|---|
| Billing | `/api/platform-admin/billing/*` | `Entities/{Requests,Responses,Dtos}/Billing/*` (add-ons since 2026-10-22: `Responses/Billing/AddonResponse`, `Dtos/Billing/ClientAddonRow`, `Requests/Billing/{AddonRequest,SetClientAddonRequest}`) |
| Configuration | `/api/platform-admin/settings` | `Entities/Requests/Configuration`, `Entities/Responses/Configuration` |
| Cache | `/api/platform-admin/cache` | `Entities/Responses/Caching` |
| Message trace (read-only, added 2026-10-15) | `/api/platform-admin/message-traces` (list, `{traceId}`, `by-message/{id}`, `resolve?q=`, `summary`) | `Entities/Responses/Monitoring/{MessageTrace,MessageTracePage,MessageTraceSummary,TraceReference}`, `Entities/Dtos/Monitoring/{MessageTraceEntry,TraceSummaryRow}` (main app: `Entities/Dtos/PlatformAdmin`). Periods: `Common/Statics/MessageTraceOptions` |
| Error log (read-only, added 2026-10-15) | `/api/platform-admin/logs` (`groups`, list, `{id}`, `summary`) | `Entities/Responses/Monitoring/{AppLogGroupReport,AppLogPage,AppLogDetail,AppLogSummary}`, `Entities/Dtos/Monitoring/{AppLogGroupRow,AppLogRow}` (main app: `Entities/Dtos/PlatformAdmin`). Levels, sorts and limits: `Common/Statics/AppLogOptions` |
| API timings (read-only, added 2026-10-14) | `/api/platform-admin/api-timings` | `Entities/Responses/Monitoring/ApiTimingReport`, `Entities/Dtos/Monitoring/RouteTimingRow` (main app: `Entities/Dtos/PlatformAdmin`). Sort keys and limits: `Common/Statics/ApiTimingOptions` |
| Admin sign-in and admins (added 2026-10-20, P3) | `/api/platform-admin/auth` (`POST login` → 200 `AdminIdentity` or one 401 for every refusal; `GET admins/{id}` → 200 while an active admin, else 404) and `/api/platform-admin/admins` (list, create, `PUT {id}/active` with `actingAdminId`, `PUT {id}/password`; writes answer `AdminRow`) | `Entities/Dtos/Admins/{AdminIdentity,AdminRow}`, `Entities/Requests/Admins/{AdminLoginRequest,CreateAdminRequest,AdminActiveBody}`, `PasswordBody` (main app: `Entities/{Dtos,Requests}/PlatformAdmin`). The minimum admin password length (10, main app `AccountRules.AdminMinPasswordLength`) is copied in `AdminAccountService.MinPasswordLength` for the form hint. The login 401 is told apart from a wrong portal key by the key filter's message, which names `X-Platform-Admin-Key` |
| Business types (added 2026-10-19) | `/api/platform-admin/business-types` (list, `{code}`, create, `PUT {code}`, `{code}/active`) and, on a client, `clients/{id}/business-type/preview?to=` and `PUT clients/{id}/business-type` | `Entities/Responses/BusinessTypes/{BusinessTypeResponse,PipelineStageResponse}`, `Entities/Dtos/BusinessTypes/{TermForms,StageMove,BusinessTypeChangePreview}`, `Entities/Requests/BusinessTypes/{SaveBusinessTypeRequest,SaveStageRequest,ChangeBusinessTypeRequest}` (main app: same names under `Omni.App/Entities`). Vocabularies: `Common/Enums/{StageCategory,TermName,AppLanguage}`. The portal's own form shape (`BusinessTypeForm`, `StageForm`) and its mapper are not copies |
| Clients, channels (incl. `whatsapp/debug-token` → `DebugTokenRequest`, `WhatsAppTokenDebugResult`), member, contacts, content, overview | `/api/platform-admin/{clients,channels,member,contacts,content,overview}` | List rows: `Entities/Dtos/{Clients,Channels,Member,Contacts,Content,Overview}` (main app: `Entities/Dtos/PlatformAdmin`). Details and `PlatformAdminChange`: `Entities/Responses/PlatformAdmin`. Bodies: `Entities/Requests/{Clients,Channels,Contacts,Content,Member,PlatformAdmin}`. Client default language (added 2026-10-20, Milestone B P2, `PUT clients/{id}/default-language`): `Entities/Requests/Clients/DefaultLanguageBody` (main app: `Entities/Requests/PlatformAdmin`); `MemberRow.Language` (null = the client's default). Client Inbox settings (added 2026-10-15, `GET`/`PUT clients/{id}/inbox-settings`): `Entities/Responses/PlatformAdmin/InboxSettingsDto` (main app: `Entities/Dtos/Inbox`, its computed `AiBotAnswers*` included; `ChannelsWithoutAiBot`, `AiBotLimit` since 2026-10-22). Contact export (`GET contacts/{id}/export`, added 2026-10-22) is passed through as indented JSON for the download, so new fields need no copy and `Entities/Requests/Clients/UpdateInboxSettingsRequest` (main app: `Entities/Requests/Inbox`) |
| Knowledge benchmark | `/api/platform-admin/knowledge-benchmark/clients/{clientId}/*` | Bodies: `Entities/Requests/KnowledgeBenchmark` (main app: `Entities/Requests/KnowledgeBenchmark`). Answers are passed through as JSON to the Search benchmark scripts, so the copy of the response shapes (main app: `Entities/Responses/KnowledgeBenchmark`, `Entities/Dtos/KnowledgeBenchmark`) is the field names those scripts read (`wwwroot/js/knowledge-benchmark*.js`). Writes answer the changed record, not `{ clientId }`; the portal audits under the route's client |

The value vocabularies in `Common/Enums` (statuses, channels, modes) and the billable event types the rate forms suggest
(`Common/Statics/BillingVocab.EventTypes`, main app `BillableEventTypes`) are still copies the pages use for filters,
suggestions and badges; refresh them when the main app adds a value. Plan features and limits are not copied: the plan
editor reads the main app's catalog (`GET billing/entitlements`).

## Known gaps in the main app that limit admin control

- **`clients.is_active` is not enforced.** Deactivating a client marks it, but the main app still lets its member in and
  its AI keep replying. Fix in the main app: check it in `CurrentClientContext` and before AI triggers.
- **No security-stamp check on the member cookie.** Locking a member account renews its security stamp, but an open
  session lasts until its cookie expires. Fix in the main app: add `SecurityStampValidator` to the Identity cookie.
- Contact edits, client edits and conversation status changes from the portal don't send SignalR events, so a client's
  open Inbox shows them on its next refresh. (Mode changes, booking status, campaign cancel, service and
  document switches go through the client's own services.)
