# SculptFlow Admin — instructions for AI assistants

## How the portal talks to the main app (standing rule)

The main app (`..\SculptFlowApp`) owns the business rules and every write. It exposes them under
`/api/platform-admin/{domain}/...` with the shared key (`X-Platform-Admin-Key`, actor in `X-Admin-Actor`,
`Idempotency-Key` on money writes). This portal calls those APIs for every read and every change; it has no access to
the main app's tables (its database connection is only for its own `admin` schema). Each successful change is also
written to `admin_audit_log`, filed under the clinic the API's answer names.

## Code architecture (standing rule, agreed with Mohammad 2026-10-06)

Same layout as the main app (its `CLAUDE.md` has the full table). The namespace always equals the folder path
(`SculptFlowAdmin.Business.Services.Clinics`).

| Folder | What goes there |
|---|---|
| `Controllers/Admin` | The portal's JSON API (`/api/admin/*`, signed-in admin) |
| `Pages/` | Razor Pages. `Pages/Shared` holds the base PageModels (`AdminPageModel`, `MainAppPageModel`) and partials |
| `Business/Services/<Feature>` | What controllers and PageModels call |
| `Business/Engines/<Feature>` | Backend logic that isn't called by a page (e.g. the `create-admin` CLI) |
| `Business/Managers` | Cross-cutting helpers (`AdminAudit`) |
| `Business/HttpClients/MainApp` | Clients for the main app's platform-admin APIs |
| `Business/Contracts/<same path>` | Interfaces of the classes above |
| `Entities/Models` | The portal's own tables only: `AdminUser`, `AdminAuditEntry` |
| `Entities/Requests`, `Entities/Responses`, `Entities/Dtos` (`/<Feature>`) | Copies of the main app's API shapes (see `MAIN_APP_SYNC.md`) and the portal's own |
| `Common/Enums`, `Common/Statics`, `Common/Configs`, `Common/Helpers` (static only), `Common/Exceptions` | As in the main app |
| `Persistence/Contexts` | `AdminDbContext` (schema `admin`) only |
| `Persistence/Repositories/<Feature>`, `Persistence/Contracts/<Feature>` | Repositories of the portal's own tables and their interfaces; `IAdminUnitOfWork` at the root |
| `Persistence/Helpers` | EF-specific helpers (`QueryPaging.ToPagedAsync`) — nothing outside Persistence references EF Core |

Rules:

1. **One top-level type per file**, named after the type. Only private nested helper types may stay inside a class.
2. **Request flow: Controller or PageModel → Service → main-app API client** for everything about clinics, and
   **→ Service → Repository** only for the portal's own tables (admins, audit log). Pages never call an API client
   directly; the service audits each write with the `clinicId` the API answered. API clients derive from
   `Business/HttpClients/MainApp/MainAppApiClient` (key, actor, idempotency, query strings, readable errors as
   `MainAppApiException`). A GET that can't reach the main app lands on the Unavailable page (`AdminPageModel`); pages
   that want an inline message instead derive from `MainAppPageModel` and use `LoadAsync`. Controllers and PageModels
   contain no logic and never touch a DbContext.
3. **Database access only in repositories** (portal tables only), one per area with intent-named methods, never a generic
   `Repository<T>` or an `IQueryable` handed out. Repositories never save.
4. **Services save through `IAdminUnitOfWork`** (`admin_users`, `admin_audit_log`). Every change is followed by
   `IAdminAudit.LogAsync`.
5. Non-static Services, Engines, Managers, HttpClients and Repositories have an interface in the mirrored `Contracts`
   path and are registered against it in DI; pages and controllers inject the interface.
6. **A new admin feature** = an endpoint in the main app's `/api/platform-admin/{domain}` (its CLAUDE.md, "Platform-admin
   APIs") + a client, service and page here, with the request/response shapes copied (see `MAIN_APP_SYNC.md`).

## Other docs (read on demand)

`README.md` (coverage, setup), `MAIN_APP_SYNC.md` (last-checked main-app commit, how to sync).
**Do not read the main app's `PROJECT_HANDOFF.md` unless Mohammad asks.**
