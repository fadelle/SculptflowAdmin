# Main app sync record

Since 2026-10-07 the admin portal has no access to the main app's tables: every read and write goes through the main
app's platform-admin APIs (`/api/platform-admin/{domain}`). The only thing to keep in step is the portal's copy of
each API's request/response shapes. This file records how far that has been checked.

## Last synced

| | |
|---|---|
| Synced on | 2026-10-07 |
| Main app commit | `9f9912f` on branch `refactor/architecture`, plus that branch's uncommitted caching and platform-admin API work |

## How to update the portal ("update the admin portal")

1. In `SculptFlowApp`: `git fetch`, then `git log --oneline <commit above>..origin/main` (also check feature branches),
   plus `git branch -a` and `git status` for work not merged yet.
2. For each new commit, look at `PlasticSurgery/Controllers/Admin/*`, `PlasticSurgery/Entities/{Dtos,Requests,Responses}/`
   for the areas below, and new pages/features. `docs/system-design.html` §4 lists every platform-admin endpoint and §12
   summarises each change.
3. Tell Mohammad what is new and which admin changes are suggested, before building.
4. Update the shapes in the right-hand column, the API clients and the pages; build; test against a throwaway database
   with both apps running; update the table above.

## Areas and the shapes the portal copies

All use `PlatformAdmin:ApiKey` (`X-Platform-Admin-Key`), actor `X-Admin-Actor`, and `Idempotency-Key` on money writes.
Writes outside billing answer `{ clinicId }` (`PlatformAdminChange`) so the portal can file its audit entry.

| Area | Main-app API | Portal copy |
|---|---|---|
| Billing | `/api/platform-admin/billing/*` | `Entities/{Requests,Responses,Dtos}/Billing/*` |
| Configuration | `/api/platform-admin/settings` | `Entities/Requests/Configuration`, `Entities/Responses/Configuration` |
| Cache | `/api/platform-admin/cache` | `Entities/Responses/Caching` |
| Clinics, channels, staff, leads, content, overview | `/api/platform-admin/{clinics,channels,staff,leads,content,overview}` | List rows: `Entities/Dtos/{Clinics,Channels,Staff,Leads,Content,Overview}` (main app: `Entities/Dtos/PlatformAdmin`). Details and `PlatformAdminChange`: `Entities/Responses/PlatformAdmin`. Bodies: `Entities/Requests/{Clinics,Channels,Leads,Content,Staff,PlatformAdmin}` |

The value vocabularies in `Common/Enums` (statuses, channels, modes) are still copies the pages use for filters and
badges; refresh them when the main app adds a value.

## Known gaps in the main app that limit admin control

- **`clinics.is_active` is not enforced.** Deactivating a clinic marks it, but the main app still lets its staff in and
  its AI keep replying. Fix in the main app: check it in `CurrentClinicContext` and before AI triggers.
- **No security-stamp check on the staff cookie.** Locking a staff account renews its security stamp, but an open
  session lasts until its cookie expires. Fix in the main app: add `SecurityStampValidator` to the Identity cookie.
- Lead edits, clinic edits and conversation status changes from the portal don't send SignalR events, so a clinic's
  open Inbox shows them on its next refresh. (Mode changes, appointment status, campaign cancel, procedure and
  document switches go through the clinic's own services.)
