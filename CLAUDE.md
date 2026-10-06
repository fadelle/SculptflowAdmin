# SculptFlow Admin — instructions for AI assistants

## How the portal talks to the main app (standing rule)

The main app (`..\SculptFlowApp`) owns the business rules and every write. It exposes them under
`/api/platform-admin/{domain}/...` with the shared key (`X-Platform-Admin-Key`, actor in `X-Admin-Actor`,
`Idempotency-Key` on money writes). This portal calls those APIs for every change and reads the database directly only
for read-only dashboards. Each successful change is also written to `admin_audit_log`. Some older areas still write
directly; see `MAIN_APP_SYNC.md`.

## Code architecture (standing rule, agreed with Mohammad 2026-10-06)

Same layout as the main app (its `CLAUDE.md` has the full table). The namespace always equals the folder path
(`SculptFlowAdmin.Business.Services.Clinics`).

| Folder | What goes there |
|---|---|
| `Controllers/Admin` | The portal's JSON API (`/api/admin/*`, signed-in admin) |
| `Pages/` | Razor Pages. `Pages/Shared` holds the base PageModels (`AdminPageModel`, `BillingPageModel`) and partials |
| `Business/Services/<Feature>` | What controllers and PageModels call |
| `Business/Engines/<Feature>` | Backend logic that isn't called by a page (e.g. the `create-admin` CLI) |
| `Business/Managers` | Cross-cutting helpers (`AdminAudit`) |
| `Business/HttpClients/MainApp` | Clients for the main app's platform-admin APIs |
| `Business/Contracts/<same path>` | Interfaces of the classes above |
| `Entities/Models` | Table classes. The main-app tables are copies of the main app's `Entities/Models` (see `MAIN_APP_SYNC.md`); `AdminUser` and `AdminAuditEntry` are the portal's own |
| `Entities/Requests`, `Entities/Responses`, `Entities/Dtos` (`/<Feature>`) | Input, output and other data shapes |
| `Common/Enums`, `Common/Statics`, `Common/Configs`, `Common/Helpers` (static only), `Common/Exceptions` | As in the main app |
| `Persistence/Contexts` | `ApplicationDbContext` (copy of the main app's mapping) and `AdminDbContext` |
| `Persistence/Repositories/<Feature>`, `Persistence/Contracts/<Feature>` | Repositories and their interfaces; `IUnitOfWork` (main-app tables) and `IAdminUnitOfWork` (portal tables) at the root |
| `Persistence/Helpers` | EF-specific helpers (`QueryPaging.ToPagedAsync`) — nothing outside Persistence references EF Core |

Rules:

1. **One top-level type per file**, named after the type. Only private nested helper types may stay inside a class.
2. **Request flow: Controller or PageModel → Service → Repository** (or → a main-app API client for writes). Controllers
   and PageModels contain no logic and never touch a DbContext.
3. **Database access only in repositories**, one per area with intent-named methods, never a generic `Repository<T>` or an
   `IQueryable` handed out. Read-only lookups return `Entities/Dtos` rows or untracked entities; `...ForUpdateAsync` returns
   a tracked entity. Repositories never save.
4. **Services save through a unit of work**: `IUnitOfWork` for the main app's tables, `IAdminUnitOfWork` for `admin_users`
   and `admin_audit_log`. Every change is followed by `IAdminAudit.LogAsync`.
5. Non-static Services, Engines, Managers, HttpClients and Repositories have an interface in the mirrored `Contracts`
   path and are registered against it in DI; pages and controllers inject the interface.
6. Never commit or push unless Mohammad says so.

## Other docs

- `README.md` — what the portal covers and setup.
- `MAIN_APP_SYNC.md` — which main-app commit the portal was last checked against and how to update it.
