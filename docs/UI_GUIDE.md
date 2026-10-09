# Aurora UI — Admin Portal UI guide (ASP.NET Core Razor Pages)

> **What this is.** The UI rules, design system and page patterns of the *Omni Support Portal*
> (React 18 + MUI 5), translated to **ASP.NET Core Razor Pages + plain CSS + vanilla JS** for the
> Admin Portal. Same look ("Aurora": indigo→violet brand, dark sidebar, glass header, soft
> glass-lit panels, light + dark mode), same UX rules, no SPA framework and no build step.
>
> **Where it lives.** Put this file at `docs/UI_GUIDE.md` and reference it from `CLAUDE.md`.
> The CSS/JS kit is embedded verbatim in the appendices (A–C). **Never retype it — extract it**
> with the command in §0.2.

---

## 0. Read this first (Claude)

### 0.1 The prime directive — restyle, don't rewrite
The Admin Portal already works. This conversion changes **how it looks and feels**, never **what
it does**. Concretely, **do not change** without the user's explicit OK:

- PageModels, handlers (`OnGet*/OnPost*`), routes/`@page` directives, `asp-page`/`asp-page-handler`/`asp-route-*` values.
- Model binding: every `name`/`asp-for`, hidden inputs, the antiforgery token, form `method`/`action`.
- Validation: `asp-validation-for`, `asp-validation-summary`, the jQuery-unobtrusive scripts.
- Any `id`/`class`/`data-*` that existing JS reads (grep `site.js` and inline scripts before renaming anything).
- Query-string parameter names (paging/sort/filter). Partials adapt to the page's names, not the reverse.
- Queries, services, authorization, business rules.

Allowed: wrapping markup, adding classes, replacing presentation-only markup (layout, navbar,
footer, headings, alerts), adding presentation partials/helpers, and the **additive** plumbing in §6
(nav tree) and §7 (clinic scope — see its own approval gate).

When in doubt: keep the old element, add the Aurora class to it.

### 0.2 Install the kit (verbatim extraction)
Run from the **web project folder** (the one containing `wwwroot/`). Set `$guide` to this file's path.

```powershell
$guide = '..\docs\UI_GUIDE.md'   # adjust
$md = [IO.File]::ReadAllText((Resolve-Path $guide))
$rx = [regex]'(?s)<!-- au-file: (?<path>\S+) -->\s*````[a-z]*\r?\n(?<body>.*?)\r?\n````'
foreach ($m in $rx.Matches($md)) {
  $path = Join-Path (Get-Location) $m.Groups['path'].Value
  New-Item -ItemType Directory -Force -Path (Split-Path $path) | Out-Null
  [IO.File]::WriteAllText($path, ($m.Groups['body'].Value -replace "`r`n", "`n") + "`n", (New-Object System.Text.UTF8Encoding($false)))
  Write-Host "wrote $path"
}
```

Node alternative (any shell): 

```bash
node -e 'const fs=require("fs"),p=require("path");const md=fs.readFileSync(process.argv[1],"utf8");for(const m of md.matchAll(/<!-- au-file: (\S+) -->\s*````[a-z]*\r?\n([\s\S]*?)\r?\n````/g)){fs.mkdirSync(p.dirname(m[1]),{recursive:true});fs.writeFileSync(m[1],m[2].replace(/\r\n/g,"\n")+"\n");console.log("wrote",m[1])}' ../docs/UI_GUIDE.md
```

It writes three files: `wwwroot/css/aurora.css` (tokens + base + components + shell),
`wwwroot/css/aurora-bootstrap.css` (only needed if the project uses Bootstrap) and
`wwwroot/js/aurora.js`. These are **vendored**: fix bugs in them, but keep page-specific styles out
of them (put those in the page's `.cshtml.css` isolation file or `site.css`).

### 0.3 Order of work
1. **Inventory** (no edits): Bootstrap version, jQuery/validation, `site.css` rules that fight the kit
   (§3.4), JS that depends on DOM, Identity UI/`_LoginPartial`, the current menu, how lists
   page/sort/filter (param names), where clinic appears. Propose a page-by-page plan.
2. **Kit + shell**: install kit, new `_Layout` (§3), nav tree (§6). Build, run, click every page in light and dark.
3. **Pages one at a time** with the recipes in §8. Re-test each page's forms after it changes.
4. **Clinic scope** (§7) — only on pages the user approves.
5. Keep this guide true: if you change a rule or the kit, update this file in the same change.

### 0.4 SculptFlow Admin — how this project applies the guide
This portal had its own hand-rolled CSS/JS before Aurora. Where the rest of this guide is generic, **this section wins**.

- **No Bootstrap, jQuery or unobtrusive validation.** Appendix B is *not* installed: `aurora-bootstrap.css` targets
  `.btn`/`.card`/`.badge`/`.alert`, which this project also uses with its own modifiers. Forms post plain `name=` fields
  (HTML5 `required`); rule errors come back as flash messages.
- **Files follow the repo's `CLAUDE.md`** (one type per file, namespace = folder) instead of §3.1:

  | Guide | Here |
  |---|---|
  | `Navigation/Nav.cs` | `Entities/Dtos/Navigation/{NavItem,NavSection,NavStep,NavNodeModel}.cs` + `Common/Statics/NavTree.cs` (the tree) |
  | `Ui/Ui.cs` | `Common/Helpers/UiExtensions.cs` (`WithQuery`), `Common/Helpers/Ui.cs` (`Badge`, `Time`, `Initials`…), `Entities/Dtos/Ui/{ErrorPanelModel,EmptyStateModel}.cs` |
  | `PagerModel` + `_Pager` | the existing `Entities/Dtos/Paging/PagerModel` + `Pages/Shared/_Pager.cshtml`: page key **`p`**, fixed 50 rows, **no page-size select** (there is no size parameter) |
  | `fonts.css` (self-hosted) | Google Fonts links in `_Layout` / `_AuthLayout` (the users' browsers reach Google) |
  | `Scoping/Scope.cs` | not added yet (§7 is a later, approval-gated step) |

- **`NavItem.Aliases`** (§6): extra path prefixes that count as a leaf — `/Billing/Clinic` is *Billing accounts*,
  `/Billing/Rates` is *Rate cards*; `/Billing` itself is `Exact` so it doesn't light up on every billing page.
- **Sections:** *Overview* (captioned, so Dashboard never reads as part of Favorites) · *Operations* (Patients, Logs) ·
  *Manage* (Tenants, Billing, Setup, System). Each group is one sidebar row with its pages as header tabs.
- **`site.js` stays the page-behaviour script.** Use its hooks, not the kit's equivalents:

  | Need | Use here | Not |
  |---|---|---|
  | Confirm before a submit (R8) | `data-confirm="…"` on the button or form → `#confirm-dialog` in `_Layout`. OK label = the button's words; danger tone when the button is `.danger` / `.au-btn--danger*` | `data-au-confirm`: site.js marks a POST form as submitting *before* the kit's confirm runs, so a confirmed form would never submit |
  | Filter form submits on change | `class="filters"` on the GET form (keep it when adding `au-toolbar`). The form carries no `p`, so paging resets | `data-au-autosubmit` (would submit twice) |
  | Popups | `data-open="dialogId"`, `data-close`, `data-autoopen` (+ backdrop click) | `data-au-open` / `data-au-close` |
  | Copy | `data-copy="#id"` (site.js; it swaps the button's text for "Copied", so keep that button text-only) or the kit's `data-au-copy="text"` | |
  | Sorting | site.js sorts the current page's rows on any header click (`data-nosort` opts out; give a table with no header row before `data-nosort` when you add one). Totals go in `<tfoot>`. There is no server-side sort, so no `_SortLink` | |
  | Find on this page | `<input data-filter="#rows">` (no `name`) + `<table id="rows">` | |
  | Local times | `Ui.Time(…)` → `<time data-utc>` (site.js) | |
  | Sign-in | `login.js`: `data-login-form` (+ `data-au-no-busy`, it shows its own "Signing in…"), `data-pw-toggle` inside the same `.login-pw` as the input, `data-pw`, `data-caps`, `data-busy-label` | |

- **Feedback (R4):** `AdminPageModel.RunAsync` already does PRG. `Flash` → a success toast (`_Toasts`); `FlashError` → a
  persistent `.au-alert--error` at the top of `.au-main` (dismiss: `data-dismiss-alert`). There is no `this.Toast()`.
- **Load errors (R6/R7):** `MainAppPageModel.LoadError` renders through `_ErrorPanel`
  (`ErrorPanelModel.From(title, message, HttpContext)` adds the call and trace id). The Unavailable page uses it too.
- **Status tones (R12):** `Ui.Badge(value, label?)` is the one status → chip map: healthy words → `au-chip--success`,
  waiting → `--warning`, broken → `--error` (all filled), informational → `--outlined --info`, `ai` → `--outlined
  --primary`, switched off (*inactive, disabled, no, expired, cancelled*) and anything unknown → neutral `--outlined`.
  Add a word to its set rather than writing a chip by hand. `Ui.OrDash(text)` for empty values, `Ui.Count(n, "lead",
  "leads")` for counts.
- **Recipes as built here** (copy an existing page of the same kind):
  - *List* (`Leads/Index`): `.au-page-head__sub` one-liner → `form.filters.au-toolbar` (notched `.au-field`s; actions
    = count + `_FilterActions` = Search · Clear · ⟳) → `.au-table-card` holding the table (or `_EmptyState` with
    `Request.EmptyMessage("leads")`) and `_Pager`. Rows with a detail page get `data-au-href`. No `<h1>` (the header has it).
  - *Shared table partials* (`_ClinicTable`, `_AppointmentTable`, `_AuditTable`, `_ConversationTable`, `_EventTable`)
    render only `.au-table-scroll` + table (or the empty state); the caller wraps them in `.au-table-card`, with the
    pager on lists or a `.au-panel__head` title row on detail pages.
  - *Detail* (`Clinics/Details`): `.au-back-header` (← back, title, sub, actions on the right) → a chips row (status +
    copyable id) → `.au-stack` of panels (`.au-panel--round --pad` with `.au-detail-rows`) and titled table cards.
  - *Popup form*: `<dialog class="au-dialog [--sm|--md]">` › `form` › `h2.au-dialog__title` (+ close `data-close`) ›
    `.au-dialog__content` (`.au-form-grid` / `.au-form-field` + `.au-form-label`, `.au-help`) › `.au-dialog__actions`.
    A dialog that sits inside a table cell needs `style="white-space:normal"` (cells are `nowrap`).
  - *Signed-out pages*: `_AuthLayout` (Login; Error when signed out). In an `@page` view use `HttpContext`, not `Context`.
- **`site.css`** now holds only project glue (site.js hooks inside kit tables, linked stat tiles, back-header actions,
  titled table cards) and page bits (conversation timeline, message chart, plan entitlements, knowledge text, sign-in
  password box), all on `--au-*` tokens. The pre-Aurora component classes are gone.
- **Kit fixes (keep the Client guide in sync):** `aurora.js` ignores `<form method="dialog">` submits — they close a
  dialog, so no progress bar and no stuck spinner on the dialog's button. `aurora.css`: `.au-fullpage-msg` gets
  `grid-template-columns: minmax(0, 1fr)` and its `__inner` `width: 100%`, so a long request id on the error page
  truncates instead of making a phone scroll sideways.

---

## 1. The look in one paragraph

A **dark gradient sidebar in both modes** (uppercase section captions with hairlines, rounded
"pill" rows, the active row lit with an indigo wash and a 3px gradient accent bar, favourites
star on hover, filter box, collapsible to a 76px icon rail). A **sticky glass header**
(translucent + 16px backdrop blur) holding a breadcrumb whose ancestors are dropdowns, the page
title, a scope chip, a Ctrl+K search pill, the clinic selector and a light/dark toggle; under it a
**tab strip** of the current section's sibling views with a gradient underline. Content sits on a
cool off‑white (`#f2f5fa`) / near‑black (`#070b15`) ground with a faint **aurora backdrop**
(three blurred radial washes). Surfaces are white/navy **panels with a 1px hairline, a soft long
shadow and a glass "top-light"** sheen. Generous radii: 10px controls, **30px** toolbars, tiles
and dashboard panels, 40px landing hero. **Inter** for UI, **JetBrains Mono** for ids/payloads,
**Material Symbols Outlined** icons. Primary CTAs are the **indigo→violet gradient** with a
coloured glow. Motion is subtle (0.12–0.26s), and `prefers-reduced-motion` turns it off.

---

## 2. Rules (carried over from the Omni portal, adapted to Razor Pages)

| # | Rule | In Razor Pages terms |
|---|------|----------------------|
| R1 | **Tokens only — never hard-code colours.** | Pages/partials use `var(--au-*)` or kit classes. The only raw colours: `aurora.css` tokens and the brand-mark SVG. A one-off colour need = a new token in `aurora.css`, light **and** dark. |
| R2 | **Light and dark are both first-class.** | Every new style is checked in both modes. Theme lives on `<html data-theme>`, stamped before first paint (§3.2). |
| R3 | **URL is the state.** | Filters, paging, sort, tab and date range are **GET query parameters**; forms that filter use `method="get"`. Links are shareable; Back works. Never keep filter state in session. |
| R4 | **Writes are POST + Post‑Redirect‑Get + feedback.** | Every mutation is a POST with the antiforgery token, redirects, and shows a toast (`this.Toast("Saved.")`, §11). No write ever succeeds or fails silently. |
| R5 | **Every list is server‑paginated** (and **scope‑aware** where the data has a clinic). | `page`/`pageSize` (or the page's existing names) in the query; the `_Pager` partial; never render an unbounded list. |
| R6 | **Every view has loading, empty and error states.** | Empty → `_EmptyState`; error → `_ErrorPanel` (specific message, *What to check*, details, copy); never a blank area or a bare "An error occurred". |
| R7 | **Errors are specific.** | Show what failed, why, and what to check, plus a trace id. Stack traces only in Development. |
| R8 | **Destructive or irreversible actions are confirm‑gated.** | `data-au-confirm` on the form/button (danger tone, default focus on *Cancel*). Say exactly what will happen ("Delete *Nile Dental*? Its 12 users lose access."). |
| R9 | **One navigation tree.** | Sidebar, breadcrumb, section tabs and the Ctrl+K palette all read `NavTree` (§6). Adding a page = one line there. |
| R10 | **Scope is global, declared per page, never re‑implemented.** | `[ScopeCapability(Clinic)]` on the PageModel + `ScopeContext.FilterId` in the query (§7). |
| R11 | **Information density for operators.** | Compact tables (44px rows), monospaced ids with copy buttons, tabular numerals, `—` for empty values. |
| R12 | **Semantic status colours, consistently.** | One status→tone map per enum (§9.3), reused everywhere that status appears. |
| R13 | **No background polling by default.** | Reloads are explicit (⟳ Refresh). A live page needs a visible user toggle. |
| R14 | **Accessible by default.** | Every icon-only button has `aria-label`; keyboard reachable; visible focus ring; AA contrast in both modes; reduced motion honoured. |
| R15 | **Every feature offers an Overview tile.** | When a feature ships, offer a status card / stat tile / link for the Overview page (§8.3). |
| R16 | **Docs are part of done.** | A change that makes this guide wrong updates this guide in the same pass. |

---

## 3. Wiring

### 3.1 Files

```
wwwroot/css/aurora.css            ← kit (Appendix A)  tokens, base, components, shell
wwwroot/css/aurora-bootstrap.css  ← kit (Appendix B)  only if Bootstrap is present
wwwroot/js/aurora.js              ← kit (Appendix C)  behaviours, opt-in by data-au-*
wwwroot/fonts/…                   ← self-hosted fonts (§3.3)
Navigation/Nav.cs                 ← the ONE nav tree + resolver (§6)
Scoping/Scope.cs                  ← clinic scope (§7)
Ui/Ui.cs                          ← WithQuery, Toast, PagerModel, ErrorPanelModel… (§11)
Pages/Shared/_Layout.cshtml       ← the shell
Pages/Shared/_Sidebar.cshtml, _NavNode.cshtml, _Header.cshtml, _SectionTabs.cshtml,
             _CommandPalette.cshtml, _ScopeSelector.cshtml, _BrandMark.cshtml,
             _Toasts.cshtml, _Pager.cshtml, _SortLink.cshtml, _EmptyState.cshtml, _ErrorPanel.cshtml
```

Add the namespaces to `Pages/_ViewImports.cshtml`:
`@using YourApp.Navigation`, `@using YourApp.Scoping`, `@using YourApp.Ui` (use the project's real root namespace).

### 3.2 `_Layout.cshtml`

Keep every script, section and partial the old layout rendered (jQuery, validation,
`@RenderSection("Scripts")`, cookie consent, `_LoginPartial`…). Only the chrome changes.

```cshtml
<!DOCTYPE html>
<html lang="en" data-au-app="admin">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>@(ViewData["Title"] is string t ? $"{t} · Clinic Admin" : "Clinic Admin")</title>
    <script>
        // Stamp theme + rail state BEFORE first paint (no white flash in dark mode).
        (function () {
            try {
                var saved = localStorage.getItem('admin.themeMode');
                var mode = saved === 'light' || saved === 'dark' ? saved
                    : (window.matchMedia && matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light');
                var r = document.documentElement;
                r.setAttribute('data-theme', mode);
                r.setAttribute('data-bs-theme', mode);
                r.style.backgroundColor = mode === 'dark' ? '#070b15' : '#f2f5fa';
                if (localStorage.getItem('admin.sidebarCollapsed') === '1') r.classList.add('au-side-collapsed');
            } catch (e) { }
        })();
    </script>
    <link rel="stylesheet" href="~/lib/bootstrap/dist/css/bootstrap.min.css" />  @* only if already used *@
    <link rel="stylesheet" href="~/css/fonts.css" asp-append-version="true" />
    <link rel="stylesheet" href="~/css/site.css" asp-append-version="true" />    @* cleaned, §3.4 *@
    <link rel="stylesheet" href="~/css/aurora.css" asp-append-version="true" />
    <link rel="stylesheet" href="~/css/aurora-bootstrap.css" asp-append-version="true" /> @* only with Bootstrap *@
    <link rel="stylesheet" href="~/YourApp.styles.css" asp-append-version="true" /> @* CSS isolation bundle, if any *@
    @await RenderSectionAsync("Styles", required: false)
</head>
<body>
    <div class="au-app">
        <partial name="_Sidebar" />
        <div class="au-scrim"></div>
        <div class="au-main-col">
            <header class="au-header">
                <partial name="_Header" />
                <partial name="_SectionTabs" />
                <div class="au-topbar-progress" aria-hidden="true"><div class="au-topbar-progress__bar"></div></div>
            </header>
            <main class="au-main" id="main">
                @RenderBody()
            </main>
        </div>
    </div>
    <partial name="_CommandPalette" />
    <partial name="_Toasts" />

    <script src="~/lib/jquery/dist/jquery.min.js"></script>                 @* keep what the project had *@
    <script src="~/lib/bootstrap/dist/js/bootstrap.bundle.min.js"></script>
    <script src="~/js/aurora.js" asp-append-version="true"></script>
    <script src="~/js/site.js" asp-append-version="true"></script>
    @await RenderSectionAsync("Scripts", required: false)
</body>
</html>
```

- `data-au-app="admin"` namespaces localStorage keys (`admin.themeMode`, `admin.sidebarCollapsed`,
  `admin.nav.favorites`, `admin.nav.recent`). The pre-paint script must use the same prefix.
- CSS order matters: **bootstrap → site → aurora → aurora-bootstrap → isolation bundle**.
- Pages that must not show the shell (sign-in, error pages for anonymous users) use a separate
  `_AuthLayout.cshtml` with the same `<head>` and a `<main class="au-auth">` body (see §8.6).

### 3.3 Fonts and icons

| Role | Family | Used via |
|------|--------|----------|
| UI text | **Inter** (variable, 400–800) | `--au-font-sans` |
| ids, codes, payloads | **JetBrains Mono** (variable, 400–700) | `.au-mono`, `<code>`, `<pre>`, `--au-font-mono` |
| icons | **Material Symbols Outlined** (FILL 0, wght 400, opsz 20) | `<span class="au-icon" aria-hidden="true">search</span>` |

- **Self-host** them in `wwwroot/fonts/` (an intranet server often cannot reach Google, and a
  missing icon font shows ligature names like `local_hospital`). Download the *latin* variable
  `woff2` of Inter and JetBrains Mono (e.g. from the `@fontsource-variable/*` packages) and the
  Material Symbols Outlined `woff2` (e.g. the `material-symbols` package), then:

```css
/* wwwroot/css/fonts.css */
@font-face { font-family: 'Inter Variable'; font-style: normal; font-weight: 100 900; font-display: swap;
  src: url('../fonts/inter-latin-wght-normal.woff2') format('woff2'); }
@font-face { font-family: 'JetBrains Mono Variable'; font-style: normal; font-weight: 100 800; font-display: swap;
  src: url('../fonts/jetbrains-mono-latin-wght-normal.woff2') format('woff2'); }
@font-face { font-family: 'Material Symbols Outlined'; font-style: normal; font-weight: 100 700; font-display: block;
  src: url('../fonts/material-symbols-outlined.woff2') format('woff2'); }
```

- Only if the user confirms the deployment reaches Google: `<link href="https://fonts.googleapis.com/css2?family=Inter:wght@400..800&family=JetBrains+Mono:wght@400..700&display=swap" rel="stylesheet">`
  and `…/css2?family=Material+Symbols+Outlined:opsz,wght,FILL,GRAD@20..24,400,0..1,0&display=block`.
- `.au-icon` is clipped to a 1em box, so a missing icon font degrades to a blank square, not a word.
- Icon names: <https://fonts.google.com/icons> (Outlined). Prefer the same glyphs MUI uses:
  `dashboard`, `search`, `filter_list`, `refresh`, `download`, `close`, `content_copy`,
  `expand_more`, `chevron_left/right`, `info`, `error`, `warning`, `check_circle`, `star`, `dark_mode`/`light_mode`, `view_sidebar`.

### 3.4 Template gotchas (fix these during inventory)
The default Razor Pages template's `site.css` fights the kit. Remove or neutralise:
- `html { font-size: 14px; }` (+ the 768px media query) — the kit's rem scale assumes **16px**.
- `body { margin-bottom: 60px; }` and `.footer { position: absolute; … }` — the shell has no sticky footer.
- `.btn:focus, .btn:active:focus, .form-control:focus … { box-shadow: 0 0 0 0.1rem white, 0 0 0 0.25rem #258cfb; }` — the kit owns focus rings.
- `.form-floating > .form-control-plaintext::placeholder …` rules — harmless, but delete if unused.
- .NET 9+ templates put `<script type="importmap"></script>` in `<head>` and use `MapStaticAssets()` — keep both.
- `Pages/Shared/_Layout.cshtml.css` (CSS isolation for the old navbar/footer) becomes dead weight — empty it once the shell is in.
- The `<header><nav class="navbar …">` and `<footer>` in the old `_Layout` are replaced by the shell;
  move `_LoginPartial` into the header tools (restyle as a `.au-dropdown` user menu, keep its form/POST to Logout intact).

---

## 4. Design tokens (summary — the values live in `aurora.css`)

| Token | Light | Dark | Use |
|-------|-------|------|-----|
| `--au-primary` | `#4f46e5` | `#818cf8` | links, active states, focus |
| `--au-secondary` | `#7c3aed` | `#a78bfa` | second brand hue |
| `--au-gradient` | `135deg #6366f1 → #8b5cf6 → #a855f7` | same | primary buttons, active bars, progress, avatar |
| `--au-bg` | `#f2f5fa` | `#070b15` | page ground |
| `--au-paper` | `#ffffff` | `#0d1526` | panels, cards, dialogs |
| `--au-text` / `-2` / `-3` | `#0b1220` / `#5b6b82` / `#9aa8bc` | `#e7edf8` / `#93a5c0` / `#556685` | primary / secondary / disabled text |
| `--au-divider` | `rgba(15,23,42,.08)` | `rgba(148,163,184,.14)` | hairlines, borders |
| `--au-success/warning/error/info` | `#059669 #d97706 #e11d48 #0284c7` | `#34d399 #fbbf24 #fb7185 #38bdf8` | status |
| `--au-input-border` | `#dbe3ee` | `rgba(148,163,184,.20)` | control borders |
| `--au-glass` | `rgba(255,255,255,.72)` | `rgba(9,13,24,.72)` | header (with blur) |
| `--au-shadow-paper` | soft 1px + long 32px | darker | panels |
| `--au-shadow-pop` | 10px + 44px | darker | menus, dialogs, drawers |
| sidebar `--au-side-*` | `#0b1020` gradient, text `#8b9bb5`, strong `#eef2f9` | same | dark in both modes |

**Shape:** `--au-r-xs 8` chips/menu items · `--au-r-sm 10` buttons/inputs/tables · `--au-r-md 12`
menus/alerts · `--au-r-card 14` · `--au-r-dialog 18` · `--au-r-pill 20` sidebar rows ·
`--au-r-control 25` header pills · **`--au-r-panel 30`** toolbars/tiles/dashboard panels · `--au-r-hero 40`.

**Type:** body 14px/1.5 · inputs 16px · h1 1.6rem/800 · h2 1.3rem/800 · header title 1.05rem/750 ·
panel title 14px/700 · eyebrow 12px/700 uppercase +0.05em · table header 11px/700 uppercase ·
stat value 26px/800 tabular · ids 12–12.5px mono.

**Spacing:** 8px grid. Page padding 24px (16px mobile). Panel padding 20px. Toolbar padding 12px.
Grid gaps 12–16px; dashboard sections 24px apart.

**Motion:** hover 0.15s; button press `scale(.98)`; page enter 260ms `cubic-bezier(.2,.8,.3,1)`
from 6px below; dropdowns 0.16s fade-slide; drawer 0.26s slide. All off under reduced motion.

**Alternate brand (optional):** `<html data-brand="teal">` swaps only the brand ramp to
teal→cyan (`#0d9488 → #0891b2 → #06b6d4`) — the same mechanism the Omni portal uses for its Bot
sub‑portal. Everything else stays identical.

---

## 5. The app shell

```
┌──────────────┬───────────────────────────────────────────────────────────────────────────┐
│ [+] Clinic   │ ▣  Access ˅                      [Clinic: Nile ✕] [⌕ Search Ctrl K] [🏥 Nile Dental ▾] [☾] │
│     Admin    │    Users                                                                  │  ← .au-header__bar (glass)
│──────────────│───────────────────────────────────────────────────────────────────────────│
│ ⌕ Filter…    │  👤 Users   🪪 Roles   ✉ Invitations                                       │  ← .au-section-tabs
│ ★ FAVORITES ─│═══════════                                                                │  ← .au-topbar-progress (2px)
│  ▢ Overview  │                                                                           │
│ OPERATIONS ──│  ╭ toolbar (30px radius): [Period ▾] [Status ▾] [⌕ Search…]   Export ⟳ ⊕ ╮ │
│  ▢ Appts     │  ┌ table card ─────────────────────────────────────────────────────────┐ │
│ MANAGE ──────│  │ NAME ↑   EMAIL   CLINIC   ROLE   STATUS   …                          │ │
│  ▢ Clinics   │  │ rows 44px, hover tint, chips, mono ids + copy                         │ │
│ ┃▣ Access ★  │  │ 51–75 of 1,284 rows                         [25 ▾] ‹  3 / 52  ›       │ │
│  ▢ Settings  │  └───────────────────────────────────────────────────────────────────────┘ │
│──────────────│                                                                           │
│ (MD) Mona D. │                                                              [✓ toast]    │
└──────────────┴───────────────────────────────────────────────────────────────────────────┘
```

**Sidebar** (`.au-sidebar`, 272px; rail 76px)
- Brand block: 38px brand mark + name (15px/800) + subtitle (11px).
- Filter box (`data-au-nav-filter`): type to flatten the whole tree to matching pages; Esc clears.
- Favourites (`data-au-favorites`): rows starred via the hover star; stored per browser.
- Meta-sections: small uppercase caption + hairline (`.au-side-section`). Suggested order:
  *(untitled: Overview)* · **Operations** (things you monitor) · **Manage** (things you configure) · **Help**.
- Rows (`.au-nav-item`): 13px, icon 20px, pill radius, hover nudges 2px right; active = indigo wash
  + 3px gradient bar + bold.
- A group whose children are **all pages** collapses to ONE row (its pages become the header tabs);
  a group that contains sub-groups expands in place (`<details>`), open when it contains the current page.
- Identity footer: gradient avatar with initials, name, email.
- `[data-au-sidebar-toggle]` collapses to the icon rail (persisted); under 900px the sidebar becomes
  an off-canvas drawer with a scrim.

**Header** (`.au-header__bar`, sticky, glass)
- Breadcrumb (`.au-crumbs`): only ancestors; each ancestor is a dropdown of its siblings, so you can
  hop to a sibling area without the sidebar. Then the **page title** (`.au-header__title`, the nav leaf's label).
- Tools, left→right: active-scope chip (with ✕), search pill (opens Ctrl+K), clinic selector,
  theme toggle, user menu.

**Section tabs** (`.au-section-tabs`): the current page's sibling views, gradient underline.
Rendered only when the page is inside a group with ≥2 pages.

**Main** (`.au-main`): 24px padding, soft enter motion on every navigation. Page content starts
directly with its toolbar or panels — the title is already in the header (no duplicate `<h1>`).

---

## 6. Navigation — one tree

`Navigation/Nav.cs` — the single source for sidebar, breadcrumb, tabs and Ctrl+K:

```csharp
namespace YourApp.Navigation;

/// <summary>A node of the ONE navigation tree. Leaf = has Path. Group = has Children.</summary>
/// Aliases = extra path prefixes that also count as this leaf (e.g. /Billing/Clinic is "Billing accounts").
public sealed record NavItem(string Label, string Icon, string? Path = null, NavItem[]? Children = null, bool Exact = false,
    string[]? Aliases = null)
{
    public bool IsLeaf => Path is not null;

    /// <summary>A group whose children are all leaves → one sidebar row; its children become header tabs.</summary>
    public bool IsLeafGroup => Children is { Length: > 0 } && Children.All(c => c.IsLeaf);

    public string? FirstLeafPath => Path ?? Children?.Select(c => c.FirstLeafPath).FirstOrDefault(p => p is not null);

    /// <summary>Segment-aware: "/Clinics" matches "/Clinics" and "/Clinics/Edit/3", not "/ClinicsArchive". Aliases always
    /// match as prefixes; Exact applies to Path only.</summary>
    public bool Matches(string path) =>
        Path is not null && (Covers(Path, path, Exact) || (Aliases?.Any(a => Covers(a, path, exact: false)) ?? false));

    public bool IsActive(string path) => Matches(path) || (Children?.Any(c => c.IsActive(path)) ?? false);

    private static bool Covers(string prefix, string path, bool exact) =>
        string.Equals(path, prefix, StringComparison.OrdinalIgnoreCase) ||
        (!exact && path.StartsWith(prefix.TrimEnd('/') + "/", StringComparison.OrdinalIgnoreCase));
}

public sealed record NavSection(string? Title, NavItem[] Items);
public sealed record NavStep(NavItem Node, NavItem[] Siblings);
public sealed record NavNodeModel(NavItem Item, string CurrentPath, string[] Trail, int Depth);

public static class NavTree
{
    // Adding a page = one line here. Paths are Razor Page routes (no PathBase).
    public static readonly NavSection[] Sections =
    {
        new(null, new[] { new NavItem("Overview", "dashboard", "/", Exact: true) }),   // "/" MUST be Exact
        new("Operations", new[]
        {
            new NavItem("Appointments", "event", "/Appointments"),
            new NavItem("Reports", "assessment", Children: new[]
            {
                new NavItem("Revenue", "payments", "/Reports/Revenue"),
                new NavItem("Usage", "query_stats", "/Reports/Usage"),
            }),
        }),
        new("Manage", new[]
        {
            new NavItem("Clinics", "local_hospital", "/Clinics"),
            new NavItem("Access", "admin_panel_settings", Children: new[]
            {
                new NavItem("Users", "person", "/Access/Users"),
                new NavItem("Roles", "badge", "/Access/Roles"),
            }),
            new NavItem("Settings", "settings_suggest", "/Settings"),
        }),
    };

    public static readonly NavItem[] TopLevel = Sections.SelectMany(s => s.Items).ToArray();

    /// <summary>Root→leaf chain for a request path; each step carries its siblings.</summary>
    public static IReadOnlyList<NavStep> Resolve(string path) => Find(TopLevel, path) ?? new List<NavStep>();

    private static List<NavStep>? Find(NavItem[] items, string path)
    {
        foreach (var item in items)
        {
            if (item.Matches(path)) return new List<NavStep> { new(item, items) };
            if (item.Children is { Length: > 0 } children && Find(children, path) is { } sub)
            {
                sub.Insert(0, new NavStep(item, items));
                return sub;
            }
        }
        return null;
    }

    /// <summary>Every leaf with its trail — feeds the Ctrl+K palette.</summary>
    public static IEnumerable<(NavItem Item, string[] Trail)> Leaves()
    {
        foreach (var section in Sections)
            foreach (var item in section.Items)
                foreach (var leaf in Walk(item, section.Title is null ? Array.Empty<string>() : new[] { section.Title }))
                    yield return leaf;
    }

    private static IEnumerable<(NavItem, string[])> Walk(NavItem item, string[] trail)
    {
        if (item.IsLeaf) { yield return (item, trail); yield break; }
        foreach (var child in item.Children ?? Array.Empty<NavItem>())
            foreach (var leaf in Walk(child, trail.Append(item.Label).ToArray()))
                yield return leaf;
    }
}
```

Rules:
- Build the tree from the **existing** menu (same destinations, same order of importance); don't drop links.
- Every leaf has an icon. Group labels are nouns ("Access"), leaves are views ("Users").
- If authorization hides menu items today, keep that: filter the tree per user (e.g. a `Policy` property on `NavItem` checked with `IAuthorizationService` in the partials) rather than deleting items.
- In-page tabs (`?tab=…`) are **not** nav items; they use `.au-tabs` inside the page (§8.4).

`Pages/Shared/_Sidebar.cshtml`:

```cshtml
@{
    var path = Context.Request.Path.Value ?? "/";
}
<nav class="au-sidebar" aria-label="Main navigation">
    <a class="au-brand" href="~/">
        <span class="au-brand__mark"><partial name="_BrandMark" model="38" /></span>
        <span class="au-brand__text">
            <span class="au-brand__name">Clinic Admin</span>
            <span class="au-brand__sub">Administration Portal</span>
        </span>
    </a>
    <div class="au-side-rule"></div>
    <div class="au-side-filter" data-au-nav-filter>
        <span class="au-icon" aria-hidden="true">filter_list</span>
        <input type="text" placeholder="Filter menu…" aria-label="Filter navigation" autocomplete="off" />
        <button type="button" aria-label="Clear filter"><span class="au-icon" aria-hidden="true">close</span></button>
    </div>
    <div class="au-side-scroll">
        <div data-au-favorites></div>
        <div data-au-nav-tree>
            @foreach (var navSection in NavTree.Sections)   @* not "section": @section is a Razor directive *@
            {
                if (navSection.Title is not null)
                {
                    <div class="au-side-section">@navSection.Title</div>
                }
                var trail = navSection.Title is null ? Array.Empty<string>() : new[] { navSection.Title };
                foreach (var item in navSection.Items)
                {
                    var node = new NavNodeModel(item, path, trail, 0);
                    <partial name="_NavNode" model="node" />
                }
            }
        </div>
        <div class="au-nav-empty" hidden></div>
    </div>
    @if (User.Identity?.IsAuthenticated == true)
    {
        var name = User.Identity.Name ?? "";
        var initials = string.Concat(name.Split(' ', StringSplitOptions.RemoveEmptyEntries).Take(2).Select(p => char.ToUpperInvariant(p[0])));
        <div class="au-side-footer">
            <span class="au-avatar" aria-hidden="true">@initials</span>
            <span class="au-side-footer__text">
                <span class="au-side-footer__name">@name</span>
                <span class="au-side-footer__email">@(User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value)</span>
            </span>
        </div>
    }
</nav>
```

`Pages/Shared/_NavNode.cshtml` (recursive):

```cshtml
@model NavNodeModel
@{
    var item = Model.Item;
    var trail = string.Join(" ", Model.Trail);
}
@if (item.IsLeaf || item.IsLeafGroup)
{
    var href = Url.Content("~" + (item.IsLeaf ? item.Path : item.FirstLeafPath));
    var active = item.IsActive(Model.CurrentPath);
    <div class="au-nav-row">
        <a class="au-nav-item @(active ? "is-active" : null)" href="@href" data-trail="@trail" aria-current="@(active ? "page" : null)">
            <span class="au-icon" aria-hidden="true">@item.Icon</span>
            <span class="au-nav-item__label">@item.Label</span>
        </a>
        <button type="button" class="au-nav-star" data-path="@href" data-label="@item.Label" aria-label="Pin @item.Label">
            <span class="au-icon" aria-hidden="true">star</span>
        </button>
    </div>
    if (item.IsLeafGroup)
    {
        @* the group's pages: hidden, listed only while the sidebar filter is active *@
        foreach (var child in item.Children!)
        {
            <div class="au-nav-row au-nav-row--search-only">
                <a class="au-nav-item" href="@Url.Content("~" + child.Path)" data-trail="@trail @item.Label">
                    <span class="au-icon" aria-hidden="true">@child.Icon</span>
                    <span class="au-nav-item__label">@child.Label</span>
                </a>
            </div>
        }
    }
}
else
{
    <details class="au-nav-group @(Model.Depth > 0 ? "au-nav-group--nested" : null)" open="@item.IsActive(Model.CurrentPath)">
        <summary>
            <span class="au-icon" aria-hidden="true">@item.Icon</span>
            <span class="au-nav-item__label">@item.Label</span>
            <span class="au-icon au-nav-chevron" aria-hidden="true">expand_more</span>
        </summary>
        <div class="au-nav-children">
            @foreach (var child in item.Children!)
            {
                var childModel = Model with { Item = child, Trail = Model.Trail.Append(item.Label).ToArray(), Depth = Model.Depth + 1 };
                <partial name="_NavNode" model="childModel" />
            }
        </div>
    </details>
}
```

`Pages/Shared/_Header.cshtml`:

```cshtml
@inject ScopeContext Scope
@inject Microsoft.Extensions.Options.IOptions<ScopeOptions> ScopeOptions
@{
    var path = Context.Request.Path.Value ?? "/";
    var steps = NavTree.Resolve(path);
    var title = (steps.Count > 0 ? steps[^1].Node.Label : null) ?? ViewData["Title"] as string ?? "Clinic Admin";
    var so = ScopeOptions.Value;
}
<div class="au-header__bar">
    <button type="button" class="au-icon-btn au-icon-btn--primary" data-au-sidebar-toggle aria-label="Toggle sidebar">
        <span class="au-icon" aria-hidden="true">view_sidebar</span>
    </button>
    <div class="au-header__title-wrap">
        @if (steps.Count > 1)
        {
            <nav class="au-crumbs" aria-label="Breadcrumb">
                @for (var i = 0; i < steps.Count - 1; i++)
                {
                    var step = steps[i];
                    var options = step.Siblings.Where(s => s.FirstLeafPath is not null).ToArray();
                    if (i > 0)
                    {
                        <span class="au-crumbs__sep" aria-hidden="true">›</span>
                    }
                    if (options.Length > 1)
                    {
                        <details class="au-dropdown">
                            <summary class="au-crumb" aria-label="Switch @step.Node.Label">@step.Node.Label<span class="au-icon" aria-hidden="true">expand_more</span></summary>
                            <div class="au-menu">
                                @foreach (var o in options)
                                {
                                    <a class="au-menu__item @(ReferenceEquals(o, step.Node) ? "is-selected" : null)" href="@Url.Content("~" + o.FirstLeafPath)">
                                        <span class="au-icon" aria-hidden="true">@o.Icon</span>@o.Label
                                    </a>
                                }
                            </div>
                        </details>
                    }
                    else
                    {
                        <span class="au-crumb">@step.Node.Label</span>
                    }
                }
            </nav>
        }
        <h1 class="au-header__title">@title</h1>
    </div>
    <div class="au-header__tools">
        @if (!Scope.IsGlobal)
        {
            <span class="au-chip au-chip--outlined au-scope-chip au-hide-md" title="Every @so.EntityLabel.ToLowerInvariant()-aware page is filtered to this @so.EntityLabel.ToLowerInvariant()">
                <span>@so.EntityLabel: @Scope.EntityName</span>
                <a class="au-chip__delete" href="@Context.Request.WithQuery((so.QueryKey, ""), ("page", null))" aria-label="Clear @so.EntityLabel.ToLowerInvariant()">
                    <span class="au-icon au-icon--sm" aria-hidden="true">close</span>
                </a>
            </span>
        }
        <button type="button" class="au-search-btn au-hide-mobile" data-au-palette-open aria-label="Search pages (Ctrl+K)">
            <span class="au-icon" aria-hidden="true">search</span>Search<span class="au-kbd">Ctrl K</span>
        </button>
        <partial name="_ScopeSelector" />
        <button type="button" class="au-icon-btn au-icon-btn--bordered" data-au-theme-toggle aria-label="Toggle color mode">
            <span class="au-icon" aria-hidden="true">dark_mode</span>
        </button>
        @* user menu: the project's _LoginPartial, restyled as <details class="au-dropdown au-user-menu"> (§9) *@
    </div>
</div>
```

`Pages/Shared/_SectionTabs.cshtml`:

```cshtml
@{
    var steps = NavTree.Resolve(Context.Request.Path.Value ?? "/");
    var last = steps.Count >= 2 ? steps[^1] : null;
    var tabs = last?.Siblings.Where(s => s.IsLeaf).ToArray() ?? Array.Empty<NavItem>();
}
@if (last is not null && tabs.Length >= 2)
{
    <nav class="au-section-tabs" aria-label="Views in this section">
        <div class="au-tabs">
            @foreach (var t in tabs)
            {
                var on = ReferenceEquals(t, last.Node);
                <a class="au-tab @(on ? "is-active" : null)" href="@Url.Content("~" + t.Path)" aria-current="@(on ? "page" : null)">
                    <span class="au-icon" aria-hidden="true">@t.Icon</span>@t.Label
                </a>
            }
        </div>
    </nav>
}
```

`Pages/Shared/_CommandPalette.cshtml` (Ctrl/⌘+K quick navigation; empty query shows Favorites → Recent → All):

```cshtml
@using System.Text.Json
@{
    var pages = NavTree.Leaves().Select(l => new { label = l.Item.Label, path = Url.Content("~" + l.Item.Path), trail = l.Trail, icon = l.Item.Icon });
}
<dialog class="au-palette" aria-label="Search pages">
    <div class="au-palette__search">
        <span class="au-icon" aria-hidden="true">search</span>
        <input type="text" placeholder="Search pages…" aria-label="Search pages" autocomplete="off" />
        <span class="au-kbd">Esc</span>
    </div>
    <ul class="au-palette__list" role="listbox"></ul>
    <div class="au-palette__foot"><span><span class="au-kbd">↑</span> <span class="au-kbd">↓</span> navigate</span><span><span class="au-kbd">Enter</span> open</span></div>
</dialog>
<script type="application/json" id="au-nav-data">@Html.Raw(JsonSerializer.Serialize(pages))</script>
```

`Pages/Shared/_BrandMark.cshtml` (gradient-stroked dark tile; the only other place raw colours are allowed):

```cshtml
@model int
@{ var gid = "au-mark-" + Guid.NewGuid().ToString("N")[..8]; }
<svg width="@Model" height="@Model" viewBox="0 0 32 32" role="img" aria-label="Clinic Admin">
    <defs>
        <linearGradient id="@(gid)" x1="0" y1="0" x2="32" y2="32" gradientUnits="userSpaceOnUse">
            <stop offset="0" stop-color="#6366f1" /><stop offset=".55" stop-color="#8b5cf6" /><stop offset="1" stop-color="#a855f7" />
        </linearGradient>
    </defs>
    <rect x="1" y="1" width="30" height="30" rx="8" fill="#0d1117" stroke="url(#@(gid))" stroke-width="2" />
    @* The glyph. Replace with the product's own mark; keep ~3px rounded strokes in the gradient. *@
    <path d="M16 9.5v13M9.5 16h13" fill="none" stroke="url(#@(gid))" stroke-width="3" stroke-linecap="round" />
</svg>
```

---

## 7. Clinic scope — the global "narrow everything to one clinic" selector

### 7.1 Model (from the Omni portal's tenant/client scoping, reduced to one entity)
- The Omni portal narrows every data view by **tenant → client**, chosen once in the header,
  synced to the URL, and each page declares what it supports (`tenant-client | tenant | none`).
- The Admin Portal has **one** scope entity today: **Clinic**. Capabilities are therefore
  **`Clinic`** (the page's data has a clinic and filters by it) or **`None`** (selector hidden).
- Levels: **Global** (all clinics) or **Clinic** (one clinic). Optional *group by clinic* is only
  legal at Global level.
- **Future-proof:** everything is named *Scope*, and the label/query key/icon live in
  `ScopeOptions`. Turning Clinic into Client later is a configuration + lookup change, not a rewrite.

### 7.2 Behaviour rules
1. The selected clinic lives in the URL as `?clinic=<id>` and is remembered in a cookie, so
   navigating keeps it and a pasted link reproduces the view. `?clinic=` (empty) means **All** and clears it.
2. The header shows a **scope chip** ("Clinic: Nile Dental ✕") whenever a clinic is selected, on every page.
3. Changing the clinic resets paging (`page` is dropped).
4. A page that is not clinic-aware hides the selector but keeps the selection.
5. At Global level, list rows show **which clinic** each row belongs to (a Clinic column); at Clinic level that column is redundant and may be hidden.
6. Ids in the URL: prefer the clinic's public/guid id if it has one; otherwise use what exists
   (do not add columns for this). Never put internal ids into new URLs when a public id exists.
7. If anything ever switches the *outer* data source (another database/environment), it must
   **reset the scope** — an id from one database can mean a different clinic in another.

### 7.3 Plumbing (`Scoping/Scope.cs`)

```csharp
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;

namespace YourApp.Scoping;

/// <summary>What a page can be narrowed by. One entity today; add a value when Client arrives.</summary>
public enum ScopeCapability { None, Clinic }

[AttributeUsage(AttributeTargets.Class, Inherited = true)]
public sealed class ScopeCapabilityAttribute(ScopeCapability capability) : Attribute
{
    public ScopeCapability Capability { get; } = capability;
}

/// <summary>Names live here so "Clinic" → "Client" later is configuration, not a rewrite.</summary>
public sealed class ScopeOptions
{
    public string EntityLabel { get; set; } = "Clinic";
    public string EntityLabelPlural { get; set; } = "Clinics";
    public string QueryKey { get; set; } = "clinic";
    public string CookieName { get; set; } = "au.scope";
    public string Icon { get; set; } = "local_hospital";
}

/// <summary>The current request's scope. Registered Scoped. Pages read it; they never re-derive it.</summary>
public sealed class ScopeContext
{
    public ScopeCapability Capability { get; internal set; }
    public string? EntityId { get; internal set; }
    public string? EntityName { get; internal set; }
    public bool IsGlobal => EntityId is null;

    /// <summary>The id to filter by — null means "all" (and is always null on non-scoped pages).</summary>
    public string? FilterId => Capability == ScopeCapability.None ? null : EntityId;
}

public sealed record ScopeOption(string Id, string Name, bool IsActive = true, string? Sub = null);
public sealed record ScopeOptionPage(IReadOnlyList<ScopeOption> Items, bool HasMore);

/// <summary>Implement ONCE over the project's existing clinic data access (no new queries elsewhere).</summary>
public interface IScopeLookup
{
    Task<string?> GetNameAsync(string id, CancellationToken ct);
    Task<ScopeOptionPage> SearchAsync(string? q, int skip, int take, CancellationToken ct);
}

/// <summary>Resolves scope for every page: ?clinic= wins (shareable links), else the cookie.</summary>
public sealed class ScopePageFilter(ScopeContext scope, IOptions<ScopeOptions> options, IScopeLookup lookup) : IAsyncPageFilter
{
    public Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context) => Task.CompletedTask;

    public async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var o = options.Value;
        var http = context.HttpContext;
        scope.Capability = context.HandlerInstance.GetType()
            .GetCustomAttribute<ScopeCapabilityAttribute>()?.Capability ?? ScopeCapability.None;

        string? id;
        if (http.Request.Query.TryGetValue(o.QueryKey, out var fromUrl))
        {
            id = string.IsNullOrWhiteSpace(fromUrl) ? null : fromUrl.ToString(); // "?clinic=" = explicit All
            if (id is null) http.Response.Cookies.Delete(o.CookieName);
            else http.Response.Cookies.Append(o.CookieName, id, new CookieOptions
            {
                HttpOnly = true, IsEssential = true, SameSite = SameSiteMode.Lax, Secure = http.Request.IsHttps,
            });
        }
        else
        {
            id = http.Request.Cookies[o.CookieName];
        }

        if (id is not null)
        {
            var name = await lookup.GetNameAsync(id, http.RequestAborted);
            if (name is null) { http.Response.Cookies.Delete(o.CookieName); id = null; } // stale/unknown id
            scope.EntityId = id;
            scope.EntityName = name;
        }

        await next();
    }
}
```

`Program.cs` (additive):

```csharp
builder.Services.Configure<ScopeOptions>(builder.Configuration.GetSection("Scope")); // section optional
builder.Services.AddScoped<ScopeContext>();
builder.Services.AddScoped<IScopeLookup, ClinicScopeLookup>(); // implement over the existing clinic service/repository
builder.Services.AddRazorPages().AddMvcOptions(o => o.Filters.Add<ScopePageFilter>()); // chain onto the EXISTING AddRazorPages()

// after app.MapRazorPages():
app.MapGet("/scope/options", async (string? q, int? skip, int? take, IScopeLookup lookup, CancellationToken ct) =>
    Results.Ok(await lookup.SearchAsync(q, Math.Max(0, skip ?? 0), Math.Clamp(take ?? 25, 1, 50), ct)));
    // add .RequireAuthorization() when the app requires sign-in (it must not be more open than the pages)
```

`Pages/Shared/_ScopeSelector.cshtml` (searchable, "All clinics" first, infinite scroll, keyboard):

```cshtml
@inject ScopeContext Scope
@inject Microsoft.Extensions.Options.IOptions<ScopeOptions> ScopeOptions
@{ var o = ScopeOptions.Value; var all = "All " + o.EntityLabelPlural.ToLowerInvariant(); }
@if (Scope.Capability != ScopeCapability.None)
{
    <div class="au-scope" data-au-scope data-endpoint="@Url.Content("~/scope/options")" data-param="@o.QueryKey"
         data-all-label="@all" data-current-id="@Scope.EntityId" data-current-name="@Scope.EntityName">
        <div class="au-combobox">
            <span class="au-icon" aria-hidden="true">@o.Icon</span>
            <input class="au-combobox__input" type="text" role="combobox" aria-label="@o.EntityLabel" aria-expanded="false"
                   aria-controls="au-scope-list" aria-autocomplete="list" autocomplete="off" placeholder="@all" value="@Scope.EntityName" />
            <span class="au-icon au-combobox__caret" aria-hidden="true">arrow_drop_down</span>
            <ul class="au-combobox__list" id="au-scope-list" role="listbox" hidden></ul>
        </div>
    </div>
}
```

A scope-aware page (the **only** per-page work):

```csharp
[ScopeCapability(ScopeCapability.Clinic)]
public class IndexModel(ScopeContext scope, IAppointmentService appointments) : PageModel
{
    public async Task OnGetAsync(/* existing filters */)
    {
        // pass scope.FilterId to the EXISTING query — only after the user approved this page (§0.3 step 4)
        Rows = await appointments.SearchAsync(clinicId: scope.FilterId /*, existing args */);
    }
}
```

> **Approval gate.** Adding the selector is UI; *filtering a query by clinic is logic*. List the
> pages whose existing queries already accept a clinic filter, ask the user, and wire only those.
> Never add a clinic filter to a query that didn't have one without asking.

---

## 8. Page recipes

### 8.1 List page (the workhorse)

```cshtml
@page
@model YourApp.Pages.Users.IndexModel
@{ ViewData["Title"] = "Users"; }

<form method="get" class="au-toolbar" data-au-autosubmit>
    <input type="hidden" name="page" value="1" />
    <div class="au-toolbar__filters">
        <div class="au-field" style="width:170px">
            <label class="au-label" asp-for="Status">Status</label>
            <select class="au-select" asp-for="Status" asp-items="Model.StatusOptions"><option value="">All statuses</option></select>
        </div>
        <div class="au-field" style="width:280px">
            <label class="au-label" asp-for="Q">Search</label>
            <span class="au-input-icon"><span class="au-icon" aria-hidden="true">search</span>
                <input class="au-input" asp-for="Q" placeholder="name, email, role…" /></span>
        </div>
    </div>
    <div class="au-toolbar__actions">
        <span class="au-toolbar__meta">@Model.Total.ToString("N0") users</span>
        <a class="au-btn au-btn--outlined" asp-page-handler="Export" asp-all-route-data="Model.RouteValues"><span class="au-icon" aria-hidden="true">download</span>Export</a>
        <button type="button" class="au-btn au-btn--outlined" data-au-refresh><span class="au-icon" aria-hidden="true">refresh</span>Refresh</button>
        <a class="au-btn au-btn--primary" asp-page="Create"><span class="au-icon" aria-hidden="true">person_add</span>Invite user</a>
    </div>
</form>

@if (Model.Error is not null)
{
    @* the error REPLACES the table — never show "no rows" after a failure *@
    <partial name="_ErrorPanel" model="Model.Error" />
}
else
{
<div class="au-table-card">
    <div class="au-table-scroll" style="--au-table-max-h: calc(100vh - 320px)">
        @if (Model.Rows.Count == 0)
        {
            <partial name="_EmptyState" model="@(new EmptyStateModel("No users match your filters"))" />
        }
        else
        {
            <table class="au-table">
                <thead><tr>
                    <th><partial name="_SortLink" model="@(new SortLinkModel("name", "Name", Model.Sort, Model.Dir))" /></th>
                    <th>Email</th>
                    @if (Model.ShowClinicColumn) { <th>Clinic</th> }
                    <th>Status</th>
                    <th>User id</th>
                    <th class="au-shrink"></th>
                </tr></thead>
                <tbody>
                @foreach (var u in Model.Rows)
                {
                    <tr data-au-href="@Url.Page("Details", new { id = u.Id })" tabindex="0">
                        <td class="au-strong">@u.Name</td>
                        <td>@u.Email</td>
                        @if (Model.ShowClinicColumn) { <td>@u.ClinicName</td> }
                        <td><span class="@UserStatusUi.Chip(u.Status)">@UserStatusUi.Label(u.Status)</span></td>
                        <td><span class="au-copy"><span class="au-mono">@u.PublicId</span>
                            <button type="button" class="au-icon-btn au-icon-btn--xs" data-au-copy="@u.PublicId" aria-label="Copy id"><span class="au-icon" aria-hidden="true">content_copy</span></button></span></td>
                        <td><span class="au-row-actions">
                            <a class="au-icon-btn au-icon-btn--sm" asp-page="Edit" asp-route-id="@u.Id" data-au-tip="Edit" aria-label="Edit"><span class="au-icon" aria-hidden="true">edit</span></a>
                        </span></td>
                    </tr>
                }
                </tbody>
            </table>
        }
    </div>
    <partial name="_Pager" model="@(new PagerModel(Model.PageIndex, Model.PageSize, Model.Total, Model.Rows.Count))" />
</div>
}
```

Rules: a failed load shows `_ErrorPanel` **in place of** the table (the toolbar stays so the user can change filters); filters are a **GET form** (`data-au-autosubmit` submits on select/checkbox change and
resets `page`; text submits on Enter); the toolbar always ends with **⟳ Refresh**; the table card
always has the **pager**; a row click opens the detail (`data-au-href`, Ctrl-click = new tab) or a
drawer (`data-au-drawer-url`, §8.2); actions inside a row are icon buttons with tooltips.
Prefer a server-rendered **drawer** for quick looks and a **detail page** for anything with tabs or actions.

### 8.2 Detail — drawer or page
- **Drawer** (`<dialog class="au-drawer">`, 460px, right): put one empty drawer in the page and give
  rows `data-au-drawer-url="@Url.Page("Index", "Detail", new { id })"`. The handler returns a partial:
  `public PartialViewResult OnGetDetail(string id) => Partial("_UserDetail", …);` — aurora.js fetches it
  (with a skeleton) and shows an error panel if it fails.

```html
<dialog class="au-drawer" id="au-drawer" aria-labelledby="drawer-title">
  <div class="au-drawer__head"><h2 class="au-drawer__title" id="drawer-title">User details</h2>
    <button type="button" class="au-icon-btn au-icon-btn--sm" data-au-close aria-label="Close"><span class="au-icon" aria-hidden="true">close</span></button></div>
  <div class="au-drawer__body" data-au-drawer-body></div>
</dialog>
```
- **Detail page**: `.au-back-header` (← back, 1.5rem title, caption subtitle), a chips row (status,
  clinic, plan), then panels with `.au-kv au-kv--2` field grids (eyebrow label + value), and in-page
  tabs (`.au-tabs` with `?tab=` links) for sub-views. Long payloads in `.au-code`; ids in `.au-copy`.

### 8.3 Overview (landing) page
Order: **hero** (`.au-hero`: 56px brand mark with glow, name, one-line purpose) → **status board**
(`.au-status-grid` of `.au-status` cards: coloured left rail by level `crit|warn|ok|idle`, eyebrow +
icon + dot, 22px metric, one-line sub; the whole card links to the area; a `.au-chip--soft` verdict
pill shows the worst level: *Attention required / Minor issues / All systems healthy*) → **KPI row**
(`.au-stat` tiles) → **panels** (`.au-panel au-panel--round au-panel--pad`, title + "View … →" link)
with meters (`.au-meter`) / charts / short lists. Cards stay visible when healthy (green) so the
board reads as a full system snapshot. Every new feature offers a card here (R15).

### 8.4 Form page (create / edit / settings)
- One `.au-panel au-panel--round au-panel--pad` per logical group; title row with status chip.
- `.au-form-grid` (2 columns, 1 on mobile); `.au-span-2` for wide fields. **Stacked labels**
  (`.au-form-field` + `.au-form-label`; required = `.au-required` asterisk) for forms; **notched
  labels** (`.au-field` + `.au-label`) for compact filter toolbars and must sit on a panel surface.
- Keep `asp-for`, `asp-validation-for` (renders `.field-validation-error`, styled by the kit) and the summary.
- Actions in `.au-form-actions`: destructive (outlined danger) far left, then spacer, Cancel (text), primary Save.
- Delete/deactivate: a POST form with `data-au-confirm="…" data-au-confirm-tone="danger"`; the
  confirmed submit keeps the clicked button's `name`/`value` (so `asp-page-handler` still works) and
  shows a spinner (`aria-busy`). Never disable the submit button yourself — a disabled button is not posted.
- After POST: `this.Toast("Clinic saved.");` then `RedirectToPage(...)` (PRG).

### 8.5 Tabs inside a page
`<div class="au-tabs">` of `<a class="au-tab is-active" href="?tab=billing">` (preserve other
query keys with `Request.WithQuery(("tab","billing"))`). Only the active tab's content is rendered.
For a toggle between 2–4 views of the *same* data, use the segmented control (`.au-segmented`).

### 8.6 Sign-in, error and not-found pages
- Sign-in (and Identity pages): `_AuthLayout` → `<main class="au-auth"><div class="au-auth__card">` with
  the brand mark, title, stacked fields, full-width `.au-btn--primary.au-btn--lg.au-btn--block`.
- `/Error`: render inside the shell when signed in — `.au-fullpage-msg` (gradient code, h3,
  explanation, **Request id** in `.au-copy`, buttons *Try again* / *Go to overview*).
- 404: same block with "404 / Page not found". (Wiring `UseStatusCodePagesWithReExecute` is a
  pipeline change — ask before adding it.)

---

## 9. Component catalogue

### 9.1 Classes

| Need | Class(es) | Notes |
|------|-----------|-------|
| Surface | `.au-panel` (+`--round` 30px, `--pad` 20px) · `.au-card` (14px) | Panel = hairline + soft shadow + glass sheen |
| Primary action | `.au-btn.au-btn--primary` | Gradient + glow; one per view region |
| Secondary action | `.au-btn.au-btn--outlined` | Export, Refresh, Cancel-with-border |
| Quiet action | `.au-btn` / `.au-btn--text-primary` | Cancel in dialogs, inline links-as-buttons |
| Destructive | `.au-btn--danger` (confirm) · `.au-btn--danger-outlined` (trigger) | Always behind a confirm |
| Sizes | `.au-btn--sm` 30px · default 36px · `--lg` 42px · `--block` | |
| Icon button | `.au-icon-btn` (+`--bordered`, `--sm`, `--xs`, `--primary`) | `aria-label` mandatory |
| Text input | `.au-input` · `.au-select` · `.au-textarea` · `.au-input-icon` | 40px tall, 16px text, focus ring |
| Label | `.au-field > .au-label` (notched) · `.au-form-field > .au-form-label` (stacked) | |
| Switch | `<input type="checkbox" class="au-switch" role="switch">` | gradient when on |
| Segmented | `.au-segmented` > `button[aria-pressed]` / `a.is-active` / radio+label | 2–4 options |
| Status chip | `.au-chip` + `--success/warning/error/info/primary` (filled) · `--outlined` · `--soft` | 22px, radius 8 |
| Dot | `.au-dot` (+tone) | inline health indicator |
| Filter toolbar | `.au-toolbar` > `__filters` + `__actions` (+ `__meta` count) | 30px radius |
| Table | `.au-table-card` > `.au-table-scroll` > `table.au-table` + `_Pager` | sticky 44px header, 11px uppercase |
| Sort header | `a.au-th-sort` (+`.is-sorted`) via `_SortLink` | arrow_upward / arrow_downward / unfold_more |
| Two-line cell | `.au-cell-2l` | code (mono) + caption |
| Empty state | `.au-empty` (+`__icon`) via `_EmptyState` | 52px round icon, message, optional action |
| KPI tile | `.au-stat` + `.au-eyebrow` + `__value` + `__hint` | 26px/800 tabular |
| Health card | `.au-status[data-level]` in `.au-status-grid` | crit/warn/ok/idle rail |
| Key/value | `.au-kv` (`--2`, `--3`) > `.au-kv__item` (eyebrow + `__value`) · `.au-detail-rows` | |
| Ids & code | `.au-mono` · `.au-copy` + `[data-au-copy]` · `.au-code` · `.au-kbd` | |
| Tabs | `.au-tabs` > `.au-tab.is-active` | gradient underline |
| Alert | `.au-alert` (+`--success/warning/error`) > `__icon` `__body` `__title` `__actions` | tinted, 12px radius |
| Error panel | `.au-alert--error.au-error-panel` via `_ErrorPanel` | headline · cause · what to check · call · details · copy |
| Toast | `Aurora.toast.*` / TempData via `_Toasts` | bottom-right, filled, 4.5s, max 4 |
| Confirm | `data-au-confirm` (+`-title`, `-label`, `-tone`) | on a form, submit button or link |
| Dialog | `<dialog class="au-dialog">` (+`--sm/md/lg`) · `[data-au-open="#id"]` · `[data-au-close]` | 18px radius, blurred backdrop |
| Drawer | `<dialog class="au-drawer">` (+`--wide`) · `data-au-drawer-url` | right side, 460px |
| Menu | `<details class="au-dropdown">` > `summary` + `.au-menu` > `.au-menu__item` | one open at a time, Esc closes |
| Tooltip | `data-au-tip="…"` (+`data-au-tip-pos="right|left"`) | CSS only |
| Progress | `.au-progress` (`--value`), `--indeterminate` · `.au-meter` (+tone) | gradient fill |
| Loading | `.au-skeleton` (`--text`) · `.au-spinner` · `.au-table-card.is-loading` | |
| Headers | `.au-back-header` · `.au-hero` · `.au-page-head` | |
| Misc | `.au-avatar` · `.au-icon-tile` (`--tile` colour) · `.au-action-card` · `.au-steps` | |
| Layout | `.au-stack` (`--au-gap`) · `.au-row` · `.au-grid--2/3/4` · `.au-spacer` | |

### 9.2 Behaviour hooks (`aurora.js`, all opt-in)

| Attribute / API | Effect |
|-----------------|--------|
| `data-au-theme-toggle` | light ⇄ dark, persisted, icon swaps |
| `data-au-sidebar-toggle` | rail on desktop, off-canvas on mobile |
| `data-au-nav-filter` / `data-au-favorites` / `.au-nav-star` | sidebar filter, favourites |
| `data-au-palette-open`, Ctrl/⌘+K | command palette (needs `#au-nav-data`) |
| `data-au-scope` | clinic combobox (needs `/scope/options`) |
| `form[data-au-autosubmit]` | submit on select/checkbox/date change; resets `page` (`data-au-page-param`, `data-au-page-first`) |
| `data-au-range` + `[data-au-range-select]` + `[data-au-range-custom]` | preset select that reveals custom from/to |
| `data-au-confirm` (+`-title`, `-label`, `-tone`) | confirm gate for forms, submit buttons, links |
| `data-au-href` | clickable row (Ctrl-click → new tab, Enter on focus) |
| `data-au-drawer-url` (+`data-au-drawer="#id"`) | load a partial into a drawer |
| `data-au-open` / `data-au-close` / `data-au-dismissable` | dialogs |
| `data-au-copy` (+`-target`, `-message`) | copy to clipboard (works on plain http too) |
| `data-au-refresh` | reload with the progress bar |
| `<div hidden data-au-toast="success">msg</div>` | server-rendered toast |
| `Aurora.toast.success/error/warning/info(msg)` · `Aurora.confirm({...})` → `Promise<bool>` · `Aurora.copy(text)` · `Aurora.progress.start/stop()` · `Aurora.theme.set('dark')` | JS API |
| `au:themechange` event | re-theme charts |

### 9.3 Status semantics
One map per enum, used everywhere that status appears (`UserStatusUi.Chip(status)`):

```csharp
public static class UserStatusUi
{
    public static string Label(UserStatus s) => s switch
    {
        UserStatus.Active => "Active", UserStatus.Invited => "Invited",
        UserStatus.Locked => "Locked", _ => "Disabled",
    };
    public static string Chip(UserStatus s) => "au-chip " + s switch
    {
        UserStatus.Active => "au-chip--success",   // healthy / done
        UserStatus.Invited => "au-chip--warning",  // waiting / needs attention
        UserStatus.Locked => "au-chip--error",     // failed / blocked
        _ => "au-chip--outlined",                  // inactive / neutral
    };
}
```

| Meaning | Tone |
|---------|------|
| success, active, delivered, paid, healthy | `success` |
| pending, in progress, waiting, trial, degraded | `warning` |
| failed, rejected, locked, overdue, critical | `error` |
| informational, scheduled, category labels | `info` (often `--outlined`) |
| inactive, disabled, unknown, archived | neutral `--outlined` |

---

## 10. Behaviour & content rules

- **Numbers:** `N0` for counts (`1,284`), `N2` for money, one decimal for percentages (`2.4%`);
  right-align numeric columns (`.au-right`) and use tabular numerals (`.au-num`, built into tables/tiles).
- **Dates:** one display format app-wide, e.g. `MMM d, HH:mm:ss` for timestamps and `MMM d, yyyy`
  for dates, in one display time zone; put the full ISO value in `title` (`<time datetime="…" title="…">`).
- **Empty values:** `—` (em dash), never blank or "null".
- **Ids:** monospace, copyable, truncated with ellipsis in tables (full value in the drawer/detail).
- **Copy:** sentence case; buttons are verbs ("Invite user", "Export"); empty states say what's
  empty *and* why ("No users match your filters"); confirms state the consequence.
- **Loading:** navigation and form submits show the 2px top progress bar automatically; async
  panels show skeletons; never block the whole page with a spinner.
- **Refresh:** ⟳ reloads the current URL (state is in the URL, so nothing is lost). No hidden polling (R13).
- **Charts** (if any, e.g. Chart.js): read colours from CSS variables at render time
  (`getComputedStyle(document.documentElement).getPropertyValue('--au-primary')`), gradient area
  fills, divider-coloured grid, `--au-text-2` ticks; re-render on the `au:themechange` event.
- **Long text in tables:** single line + ellipsis + `title` tooltip; the full value lives in the detail.
- **Production banner (only if the app can point at production data from a non-production
  host):** `<div class="au-prod-banner" role="status"><span class="au-icon">warning</span>PRODUCTION — live data</div>` under the header.

---

## 11. Shared partials & helpers

`Ui/Ui.cs`:

```csharp
using System.Diagnostics;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace YourApp.Ui;

public static class UiExtensions
{
    /// <summary>Current URL with some query keys replaced (null ⇒ removed); every other filter is kept.</summary>
    public static string WithQuery(this HttpRequest request, params (string Key, string? Value)[] changes)
    {
        var query = QueryHelpers.ParseQuery(request.QueryString.Value)
            .ToDictionary(kv => kv.Key, kv => kv.Value, StringComparer.OrdinalIgnoreCase);
        foreach (var (key, value) in changes)
        {
            if (value is null) query.Remove(key);
            else query[key] = value;
        }
        var pairs = query.SelectMany(kv => kv.Value.Select(v => new KeyValuePair<string, string>(kv.Key, v ?? "")));
        return request.PathBase + request.Path + new QueryBuilder(pairs).ToQueryString();
    }

    /// <summary>Queue a toast for the page the user lands on after a redirect (PRG).</summary>
    public static void Toast(this PageModel page, string message, string severity = "success") =>
        page.TempData[$"au.toast.{severity}"] = message;
}

/// <summary>1-based paging. Keys default to page/pageSize — pass the page's EXISTING names.</summary>
public sealed record PagerModel(int Page, int PageSize, long Total, int Shown, string PageKey = "page", string SizeKey = "pageSize")
{
    public static readonly int[] Sizes = { 10, 25, 50, 100 };
    public int PageCount => Total <= 0 ? 1 : (int)Math.Ceiling(Total / (double)Math.Max(1, PageSize));
    public long First => Shown == 0 ? 0 : (long)(Page - 1) * PageSize + 1;
    public long Last => Shown == 0 ? 0 : (long)(Page - 1) * PageSize + Shown;
}

public sealed record SortLinkModel(string Key, string Label, string? Sort, string? Dir, string SortKey = "sort", string DirKey = "dir", string PageKey = "page");
public sealed record EmptyStateModel(string Message, string Icon = "inbox", string? ActionHref = null, string? ActionLabel = null);

/// <summary>Everything the error panel shows. Build it with From(); never show a bare "An error occurred".</summary>
public sealed record ErrorPanelModel(string Title, string Message, string? Hint = null, string? Call = null,
    IReadOnlyList<KeyValuePair<string, string>>? Facts = null, string? Stack = null)
{
    public static ErrorPanelModel From(Exception ex, HttpContext http, bool includeStack)
    {
        var traceId = Activity.Current?.Id ?? http.TraceIdentifier;
        var (title, hint) = ex switch
        {
            TimeoutException or OperationCanceledException =>
                ("The operation timed out", "Narrow the filters (date range, clinic) and try again."),
            HttpRequestException => ("A dependency could not be reached", "Check that the downstream service is up and reachable from this server."),
            UnauthorizedAccessException => ("You are not allowed to do this", "Ask an administrator for the required role."),
            // extend with the project's own exceptions: DB errors (SqlException.Number / PostgresException.SqlState),
            // concurrency conflicts, validation exceptions, domain exceptions…
            _ => ("Something went wrong", "If it happens again, send the trace id below to the developers."),
        };
        var facts = new List<KeyValuePair<string, string>>
        {
            new("Exception", ex.GetType().FullName ?? ex.GetType().Name),
            new("Trace id", traceId),
        };
        if (ex.InnerException is { } inner) facts.Add(new("Inner", $"{inner.GetType().Name}: {inner.Message}"));
        return new ErrorPanelModel(title, ex.Message, hint, $"{http.Request.Method} {http.Request.Path} · trace {traceId}", facts,
            includeStack ? ex.ToString() : null);
    }

    public string Report => string.Join(Environment.NewLine, new[] { Title, Message, Hint is null ? null : "What to check: " + Hint, Call }
        .Concat((Facts ?? Array.Empty<KeyValuePair<string, string>>()).Select(f => $"{f.Key}: {f.Value}"))
        .Where(s => !string.IsNullOrEmpty(s)));
}
```

`_Toasts.cshtml`:

```cshtml
<div class="au-toasts" aria-live="polite"></div>
@foreach (var severity in new[] { "success", "info", "warning", "error" })
{
    if (TempData[$"au.toast.{severity}"] is string message)
    {
        <div hidden data-au-toast="@severity">@message</div>
    }
}
```

`_Pager.cshtml` (keeps every other query parameter; page size change goes back to page 1):

```cshtml
@model PagerModel
<div class="au-pager">
    <span class="au-pager__count">
        @if (Model.Shown == 0)
        {
            <text>No rows</text>
        }
        else
        {
            <text>@Model.First.ToString("N0")–@Model.Last.ToString("N0") of <strong>@Model.Total.ToString("N0")</strong> rows</text>
        }
    </span>
    <form method="get" class="au-pager__nav" data-au-autosubmit data-au-page-param="@Model.PageKey">
        @foreach (var kv in Context.Request.Query)
        {
            if (kv.Key.Equals(Model.PageKey, StringComparison.OrdinalIgnoreCase) || kv.Key.Equals(Model.SizeKey, StringComparison.OrdinalIgnoreCase)) { continue; }
            foreach (var v in kv.Value)
            {
                <input type="hidden" name="@kv.Key" value="@v" />
            }
        }
        <input type="hidden" name="@Model.PageKey" value="1" />
        <select class="au-select au-select--sm au-pager__size" name="@Model.SizeKey" aria-label="Rows per page">
            @foreach (var size in PagerModel.Sizes)
            {
                <option value="@size" selected="@(size == Model.PageSize)">@size</option>
            }
        </select>
        @if (Model.Page > 1)
        {
            <a class="au-icon-btn au-pager__btn" href="@Context.Request.WithQuery((Model.PageKey, (Model.Page - 1).ToString()))" aria-label="Previous page"><span class="au-icon" aria-hidden="true">chevron_left</span></a>
        }
        else
        {
            <span class="au-icon-btn au-pager__btn is-disabled" aria-disabled="true"><span class="au-icon" aria-hidden="true">chevron_left</span></span>
        }
        <span class="au-pager__page">@Model.Page / @Model.PageCount</span>
        @if (Model.Page < Model.PageCount)
        {
            <a class="au-icon-btn au-pager__btn" href="@Context.Request.WithQuery((Model.PageKey, (Model.Page + 1).ToString()))" aria-label="Next page"><span class="au-icon" aria-hidden="true">chevron_right</span></a>
        }
        else
        {
            <span class="au-icon-btn au-pager__btn is-disabled" aria-disabled="true"><span class="au-icon" aria-hidden="true">chevron_right</span></span>
        }
    </form>
</div>
```

`_SortLink.cshtml`:

```cshtml
@model SortLinkModel
@{
    var on = string.Equals(Model.Sort, Model.Key, StringComparison.OrdinalIgnoreCase);
    var next = on && Model.Dir == "asc" ? "desc" : "asc";
    var glyph = !on ? "unfold_more" : Model.Dir == "asc" ? "arrow_upward" : "arrow_downward";
}
<a class="au-th-sort @(on ? "is-sorted" : null)" href="@Context.Request.WithQuery((Model.SortKey, Model.Key), (Model.DirKey, next), (Model.PageKey, null))" aria-label="Sort by @Model.Label">@Model.Label<span class="au-icon" aria-hidden="true">@glyph</span></a>
```

`_EmptyState.cshtml`:

```cshtml
@model EmptyStateModel
<div class="au-empty">
    <span class="au-empty__icon"><span class="au-icon" aria-hidden="true">@Model.Icon</span></span>
    <span>@Model.Message</span>
    @if (Model.ActionHref is not null)
    {
        <a class="au-btn au-btn--outlined au-btn--sm" href="@Model.ActionHref">@Model.ActionLabel</a>
    }
</div>
```

`_ErrorPanel.cshtml`:

```cshtml
@model ErrorPanelModel
<div class="au-alert au-alert--error au-error-panel" role="alert">
    <span class="au-icon au-alert__icon" aria-hidden="true">error</span>
    <div class="au-alert__body">
        <div class="au-alert__title">@Model.Title</div>
        <div>@Model.Message</div>
        @if (Model.Hint is not null) { <div class="au-alert__hint"><b>What to check:</b> @Model.Hint</div> }
        @if (Model.Call is not null) { <div class="au-alert__call">@Model.Call</div> }
        @if (Model.Facts is { Count: > 0 } || Model.Stack is not null)
        {
            <details>
                <summary>Details</summary>
                <dl>
                    @foreach (var f in Model.Facts ?? Array.Empty<KeyValuePair<string, string>>())
                    {
                        <dt>@f.Key</dt><dd>@f.Value</dd>
                    }
                </dl>
                @if (Model.Stack is not null) { <pre>@Model.Stack</pre> }
            </details>
        }
    </div>
    <div class="au-alert__actions">
        <button type="button" class="au-icon-btn au-icon-btn--sm" data-au-copy="@Model.Report" data-au-copy-message="Error details copied" aria-label="Copy error details">
            <span class="au-icon" aria-hidden="true">content_copy</span>
        </button>
    </div>
</div>
```

Date-range filter (only where the page already filters by date; keep its existing `from`/`to` names):

```cshtml
<div class="au-row au-row--wrap" data-au-range>
    <div class="au-field" style="width:170px">
        <label class="au-label" for="range">Period</label>
        <select class="au-select" id="range" name="range" data-au-range-select asp-items="Model.RangeOptions"></select>
    </div>
    <div class="au-row" data-au-range-custom hidden>
        <div class="au-field"><label class="au-label" asp-for="From">From</label><input class="au-input" asp-for="From" type="datetime-local" /></div>
        <div class="au-field"><label class="au-label" asp-for="To">To</label><input class="au-input" asp-for="To" type="datetime-local" /></div>
        <button class="au-btn au-btn--outlined" type="submit">Apply</button>
    </div>
</div>
```

Presets: Today · Yesterday · This week · Last week · This month · Last month · Last 3 months ·
Custom…, resolved server-side to a half-open `[from, to)` in the display time zone. If an
endpoint caps the window (e.g. 31 days), hide presets that exceed it and clamp custom ranges —
never let the user pick a range the server will reject.

---

## 12. Definition of done (UI)

- [ ] `dotnet build` clean; the app runs; no console errors on the page.
- [ ] Page checked in **light and dark**, at desktop width and at 390px.
- [ ] Every form still posts and validates exactly as before (try one invalid and one valid submit).
- [ ] No raw colours in the page (`#`, `rgb(`) — tokens/classes only.
- [ ] List: server-paged, `_Pager`, empty state, sort links keep filters, filters live in the URL.
- [ ] Errors render through `_ErrorPanel`/toasts with a specific message; writes use PRG + toast.
- [ ] Destructive actions behind `data-au-confirm`.
- [ ] Icon-only buttons have `aria-label`; keyboard path works (Tab, Enter, Esc).
- [ ] Page is in `NavTree` (or deliberately reachable only from another page).
- [ ] Scope: capability declared if (and only if) the user approved clinic filtering for this page.
- [ ] Overview tile offered for new features; this guide updated if a rule or the kit changed.

---

## Appendices — the kit (verbatim; extract with §0.2, do not retype)

Appendix A is the full stylesheet (tokens → base → components → shell → client-facing extras such
as the sign-in screen and stacked tables). Appendix B is the Bootstrap 5 bridge. Appendix C is the
behaviour script. They are identical in the Admin and Client guides — keep them in sync if you
maintain both portals.

### Appendix A — `wwwroot/css/aurora.css`

<!-- au-file: wwwroot/css/aurora.css -->
````css
/* Aurora UI kit — aurora.css (tokens · base · components · shell · client extras).
   Ported from the Omni Support Portal (React/MUI) design system. Vendored: fix bugs here,
   keep page-specific styles elsewhere. Pair with aurora.js. */

/* ==========================================================================
   AURORA — design tokens. The ONLY place raw colours live.
   Components read var(--au-*) so light and dark stay in lockstep.
   Mode is chosen by <html data-theme="light|dark"> (stamped before first paint).
   ========================================================================== */
:root {
  color-scheme: light;

  /* --- type ------------------------------------------------------------- */
  --au-font-sans: 'Inter Variable', Inter, ui-sans-serif, system-ui, -apple-system,
    BlinkMacSystemFont, 'Segoe UI', sans-serif;
  --au-font-mono: 'JetBrains Mono Variable', 'JetBrains Mono', ui-monospace, SFMono-Regular,
    Menlo, Consolas, monospace;
  --au-font-icons: 'Material Symbols Outlined';

  /* --- brand (indigo → violet) ------------------------------------------- */
  --au-primary: #4f46e5;
  --au-primary-light: #818cf8;
  --au-primary-dark: #4338ca;
  --au-on-primary: #ffffff;
  --au-secondary: #7c3aed;
  --au-secondary-light: #a78bfa;
  --au-secondary-dark: #6d28d9;
  --au-gradient: linear-gradient(135deg, #6366f1 0%, #8b5cf6 55%, #a855f7 100%);
  --au-gradient-soft: linear-gradient(135deg, rgba(99, 102, 241, 0.08), rgba(168, 85, 247, 0.06));
  --au-brand-glow: rgba(109, 90, 232, 0.55);
  --au-brand-glow-strong: rgba(109, 90, 232, 0.7);

  /* --- semantic ---------------------------------------------------------- */
  --au-success: #059669;
  --au-success-dark: #047857;
  --au-on-success: #ffffff;
  --au-warning: #d97706;
  --au-warning-dark: #b45309;
  --au-on-warning: #ffffff;
  --au-error: #e11d48;
  --au-error-dark: #be123c;
  --au-on-error: #ffffff;
  --au-info: #0284c7;
  --au-info-dark: #0369a1;
  --au-on-info: #ffffff;
  /* toast fills (MUI "filled" alert) */
  --au-toast-success: var(--au-success);
  --au-toast-warning: var(--au-warning);
  --au-toast-error: var(--au-error);
  --au-toast-info: var(--au-info);
  --au-on-toast-success: #ffffff;
  --au-on-toast-warning: #ffffff;
  --au-on-toast-error: #ffffff;
  --au-on-toast-info: #ffffff;

  /* --- surfaces & text --------------------------------------------------- */
  --au-bg: #f2f5fa;
  --au-paper: #ffffff;
  --au-text: #0b1220;
  --au-text-2: #5b6b82; /* secondary */
  --au-text-3: #9aa8bc; /* disabled / hints */
  --au-divider: rgba(15, 23, 42, 0.08);
  --au-hover: rgba(0, 0, 0, 0.04);
  --au-selected: rgba(0, 0, 0, 0.08);
  --au-disabled: rgba(0, 0, 0, 0.26);
  --au-disabled-bg: rgba(0, 0, 0, 0.12);
  --au-input-bg: #ffffff;
  --au-input-border: #dbe3ee;
  --au-input-border-hover: #b9c5d6;
  --au-label-notch: #ffffff; /* solid colour of an input's fill, used to cut the label notch */
  --au-code-bg: #f6f8fc;
  --au-table-head-bg: #f7f9fc; /* must be SOLID (sticky header) */
  --au-glass: rgba(255, 255, 255, 0.72);
  --au-sheen: linear-gradient(180deg, rgba(255, 255, 255, 0.75) 0%, rgba(255, 255, 255, 0) 60%);
  --au-mix-bg: #ffffff; /* alert tint base   */
  --au-mix-fg: #000000; /* alert text base   */
  --au-hero-a: 12%;
  --au-hero-b: 8%;

  /* --- elevation --------------------------------------------------------- */
  --au-shadow-paper: 0 1px 2px rgba(15, 23, 42, 0.04), 0 12px 32px -18px rgba(15, 23, 42, 0.14);
  --au-shadow-pop: 0 4px 10px rgba(15, 23, 42, 0.06), 0 20px 44px -16px rgba(15, 23, 42, 0.24);
  --au-shadow-toast: 0 3px 5px -1px rgba(0, 0, 0, 0.2), 0 6px 10px 0 rgba(0, 0, 0, 0.14),
    0 1px 18px 0 rgba(0, 0, 0, 0.12);
  --au-backdrop: rgba(15, 23, 42, 0.4);
  --au-tooltip-bg: #0f172a;
  --au-focus-ring: 0 0 0 3px rgba(79, 70, 229, 0.16);
  --au-focus-ring-error: 0 0 0 3px rgba(225, 29, 72, 0.16);

  /* --- controls ---------------------------------------------------------- */
  --au-segment-bg: rgba(11, 18, 32, 0.045);
  --au-segment-selected: #ffffff;
  --au-segment-shadow: 0 1px 3px rgba(15, 23, 42, 0.12);
  --au-meter-track: rgba(11, 18, 32, 0.08);
  --au-select-chevron: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='%235b6b82'%3E%3Cpath d='M7 10l5 5 5-5z'/%3E%3C/svg%3E");
  --au-scroll-thumb: #c3cddc;
  --au-scroll-thumb-hover: #94a3b8;

  /* --- ambient aurora backdrop ------------------------------------------- */
  --au-aurora: radial-gradient(1000px 520px at 88% -12%, rgba(99, 102, 241, 0.09), transparent 62%),
    radial-gradient(820px 460px at -8% 28%, rgba(168, 85, 247, 0.06), transparent 60%),
    radial-gradient(900px 520px at 55% 112%, rgba(14, 165, 233, 0.05), transparent 62%);

  /* --- sidebar: deliberately DARK in BOTH modes -------------------------- */
  --au-side-bg: linear-gradient(180deg, #0b1020 0%, #0a0e1c 55%, #0b0f1f 100%);
  --au-side-bg-solid: #0b1020;
  --au-side-hover: rgba(148, 163, 184, 0.08);
  --au-side-text: #8b9bb5;
  --au-side-text-strong: #eef2f9;
  --au-side-section: #5d6d89;
  --au-side-active-bg: linear-gradient(90deg, rgba(99, 102, 241, 0.22) 0%, rgba(139, 92, 246, 0.1) 100%);
  --au-side-active-text: #e0e7ff;
  --au-side-active-icon: #a5b4fc;
  --au-side-border: rgba(148, 163, 184, 0.1);

  /* --- shape ------------------------------------------------------------- */
  --au-r-xs: 8px; /* chips, menu items, toggle buttons, tooltips */
  --au-r-sm: 10px; /* buttons, inputs, icon buttons, tables, default panel */
  --au-r-md: 12px; /* menus, popovers, alerts, accordions, toasts */
  --au-r-card: 14px; /* .au-card */
  --au-r-dialog: 18px; /* dialogs, auth card */
  --au-r-pill: 20px; /* sidebar rows, sidebar filter */
  --au-r-control: 25px; /* header pills: search, scope, environment */
  --au-r-panel: 30px; /* toolbars, stat tiles, status cards, dashboard panels */
  --au-r-hero: 40px; /* landing hero */

  /* --- motion ------------------------------------------------------------ */
  --au-ease-out: cubic-bezier(0.2, 0.8, 0.3, 1);
  --au-t-fast: 0.12s;
  --au-t: 0.15s;
  --au-t-slow: 0.26s;

  /* --- layout ------------------------------------------------------------ */
  --au-sidebar-w: 272px;
  --au-sidebar-w-collapsed: 76px;
  --au-header-h: 64px;
  --au-z-header: 10;
  --au-z-sidebar: 20;
  --au-z-toast: 1400;
}

:root[data-theme='dark'] {
  color-scheme: dark;

  --au-primary: #818cf8;
  --au-primary-light: #a5b4fc;
  --au-primary-dark: #6366f1;
  --au-on-primary: #ffffff;
  --au-secondary: #a78bfa;
  --au-secondary-light: #c4b5fd;
  --au-secondary-dark: #8b5cf6;
  --au-gradient-soft: linear-gradient(135deg, rgba(129, 140, 248, 0.14), rgba(168, 85, 247, 0.1));

  --au-success: #34d399;
  --au-success-dark: #10b981;
  --au-on-success: rgba(0, 0, 0, 0.87);
  --au-warning: #fbbf24;
  --au-warning-dark: #f59e0b;
  --au-on-warning: rgba(0, 0, 0, 0.87);
  --au-error: #fb7185;
  --au-error-dark: #f43f5e;
  --au-on-error: rgba(0, 0, 0, 0.87);
  --au-info: #38bdf8;
  --au-info-dark: #0ea5e9;
  --au-on-info: rgba(0, 0, 0, 0.87);
  --au-toast-success: var(--au-success-dark);
  --au-toast-warning: var(--au-warning-dark);
  --au-toast-error: var(--au-error-dark);
  --au-toast-info: var(--au-info-dark);
  --au-on-toast-success: rgba(0, 0, 0, 0.87);
  --au-on-toast-warning: rgba(0, 0, 0, 0.87);
  --au-on-toast-error: #ffffff;
  --au-on-toast-info: rgba(0, 0, 0, 0.87);

  --au-bg: #070b15;
  --au-paper: #0d1526;
  --au-text: #e7edf8;
  --au-text-2: #93a5c0;
  --au-text-3: #556685;
  --au-divider: rgba(148, 163, 184, 0.14);
  --au-hover: rgba(255, 255, 255, 0.08);
  --au-selected: rgba(255, 255, 255, 0.16);
  --au-disabled: rgba(255, 255, 255, 0.3);
  --au-disabled-bg: rgba(255, 255, 255, 0.12);
  --au-input-bg: rgba(148, 163, 184, 0.05);
  --au-input-border: rgba(148, 163, 184, 0.2);
  --au-input-border-hover: rgba(148, 163, 184, 0.36);
  --au-label-notch: #111a2b;
  --au-code-bg: #0a101d;
  --au-table-head-bg: #111a2b;
  --au-glass: rgba(9, 13, 24, 0.72);
  --au-sheen: linear-gradient(180deg, rgba(255, 255, 255, 0.05) 0%, rgba(255, 255, 255, 0.012) 42%,
      rgba(255, 255, 255, 0) 100%);
  --au-mix-bg: #000000;
  --au-mix-fg: #ffffff;
  --au-hero-a: 28%;
  --au-hero-b: 16%;

  --au-shadow-paper: 0 1px 2px rgba(0, 0, 0, 0.35), 0 16px 40px -22px rgba(0, 0, 0, 0.55);
  --au-shadow-pop: 0 4px 10px rgba(0, 0, 0, 0.35), 0 24px 48px -16px rgba(0, 0, 0, 0.6);
  --au-backdrop: rgba(2, 6, 16, 0.6);
  --au-tooltip-bg: #1c2740;
  --au-focus-ring: 0 0 0 3px rgba(129, 140, 248, 0.24);
  --au-focus-ring-error: 0 0 0 3px rgba(251, 113, 133, 0.24);

  --au-segment-bg: rgba(231, 237, 248, 0.08);
  --au-segment-selected: rgba(148, 163, 184, 0.16);
  --au-segment-shadow: 0 1px 3px rgba(0, 0, 0, 0.4);
  --au-meter-track: rgba(231, 237, 248, 0.1);
  --au-select-chevron: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='%2393a5c0'%3E%3Cpath d='M7 10l5 5 5-5z'/%3E%3C/svg%3E");
  --au-scroll-thumb: #33415a;
  --au-scroll-thumb-hover: #48597a;

  --au-aurora: radial-gradient(1000px 520px at 88% -12%, rgba(99, 102, 241, 0.16), transparent 62%),
    radial-gradient(820px 460px at -8% 28%, rgba(168, 85, 247, 0.1), transparent 60%),
    radial-gradient(900px 520px at 55% 112%, rgba(14, 165, 233, 0.08), transparent 62%);
}

/* Optional alternate brand ramp (teal → cyan). Put data-brand="teal" on <html>.
   Only brand tokens change; every other token is shared. */
:root[data-brand='teal'] {
  --au-primary: #0d9488;
  --au-primary-light: #2dd4bf;
  --au-primary-dark: #0f766e;
  --au-secondary: #0891b2;
  --au-secondary-light: #22d3ee;
  --au-secondary-dark: #0e7490;
  --au-gradient: linear-gradient(135deg, #0d9488 0%, #0891b2 55%, #06b6d4 100%);
  --au-gradient-soft: linear-gradient(135deg, rgba(13, 148, 136, 0.08), rgba(6, 182, 212, 0.06));
  --au-brand-glow: rgba(8, 145, 178, 0.5);
  --au-brand-glow-strong: rgba(8, 145, 178, 0.65);
  --au-focus-ring: 0 0 0 3px rgba(13, 148, 136, 0.16);
  --au-side-active-bg: linear-gradient(90deg, rgba(13, 148, 136, 0.24) 0%, rgba(6, 182, 212, 0.1) 100%);
  --au-side-active-text: #ccfbf1;
  --au-side-active-icon: #5eead4;
}
:root[data-brand='teal'][data-theme='dark'] {
  --au-primary: #2dd4bf;
  --au-primary-light: #5eead4;
  --au-primary-dark: #14b8a6;
  --au-on-primary: #052823;
  --au-secondary: #22d3ee;
  --au-secondary-light: #67e8f9;
  --au-secondary-dark: #06b6d4;
  --au-gradient-soft: linear-gradient(135deg, rgba(45, 212, 191, 0.14), rgba(34, 211, 238, 0.1));
  --au-focus-ring: 0 0 0 3px rgba(45, 212, 191, 0.24);
}

/* ==========================================================================
   AURORA — base: document, typography, ambient backdrop, motion, scrollbars
   ========================================================================== */
*,
*::before,
*::after {
  box-sizing: border-box;
}

html {
  font-size: 16px; /* rem base — do NOT shrink it (the Razor template sets 14px; remove that) */
  -webkit-text-size-adjust: 100%;
}

body {
  margin: 0;
  min-width: 320px;
  min-height: 100vh;
  font-family: var(--au-font-sans);
  font-size: 0.875rem;
  line-height: 1.5;
  color: var(--au-text);
  background-color: var(--au-bg);
  font-synthesis: none;
  text-rendering: optimizeLegibility;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
}

/* Ambient aurora backdrop — fixed, non-interactive, behind everything. */
body::before {
  content: '';
  position: fixed;
  inset: 0;
  z-index: -1;
  pointer-events: none;
  background: var(--au-aurora);
}

::selection {
  background: color-mix(in srgb, var(--au-primary) 24%, transparent);
}

button,
input,
textarea,
select {
  font: inherit;
  color: inherit;
}

a {
  color: inherit;
  text-decoration: none;
}

/* Links inside running text / tables that should look like links. */
.au-link {
  color: var(--au-primary);
  font-weight: 600;
  text-decoration: none;
}
.au-link:hover {
  text-decoration: underline;
}
/* Plain links inside page content (scaffolded "Edit | Details | Delete" etc.) */
.au-main a:not([class]) {
  color: var(--au-primary);
  font-weight: 500;
}
.au-main a:not([class]):hover {
  text-decoration: underline;
}

pre,
code,
kbd,
samp {
  font-family: var(--au-font-mono);
}
pre {
  margin: 0;
  white-space: pre-wrap;
  word-break: break-word;
}

img,
svg {
  vertical-align: middle;
}

/* --- headings (sizes/weights from the Aurora type scale) ----------------- */
h1,
.au-h1 {
  font-size: 1.6rem;
  font-weight: 800;
  letter-spacing: -0.02em;
  line-height: 1.2;
  margin: 0 0 0.5rem;
}
h2,
.au-h2 {
  font-size: 1.3rem;
  font-weight: 800;
  letter-spacing: -0.015em;
  line-height: 1.25;
  margin: 0 0 0.5rem;
}
h3,
.au-h3 {
  font-size: 1.05rem;
  font-weight: 750;
  letter-spacing: -0.01em;
  line-height: 1.3;
  margin: 0 0 0.5rem;
}
h4,
.au-h4 {
  font-size: 0.95rem;
  font-weight: 750;
  letter-spacing: -0.01em;
  margin: 0 0 0.5rem;
}
h5,
h6,
.au-h5 {
  font-size: 0.875rem;
  font-weight: 700;
  margin: 0 0 0.5rem;
}
p {
  margin: 0 0 0.75rem;
}

/* --- text utilities ------------------------------------------------------ */
.au-muted {
  color: var(--au-text-2);
}
.au-faint {
  color: var(--au-text-3);
}
.au-caption {
  font-size: 0.75rem;
  color: var(--au-text-2);
}
.au-eyebrow {
  display: block;
  font-size: 0.75rem;
  font-weight: 700;
  letter-spacing: 0.05em;
  text-transform: uppercase;
  color: var(--au-text-2);
}
.au-mono {
  font-family: var(--au-font-mono);
  font-size: 12.5px;
}
.au-num {
  font-variant-numeric: tabular-nums;
}
.au-strong {
  font-weight: 650;
}
.au-ellipsis {
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  min-width: 0;
}
.au-text-success {
  color: var(--au-success);
}
.au-text-warning {
  color: var(--au-warning);
}
.au-text-error {
  color: var(--au-error);
}
.au-text-info {
  color: var(--au-info);
}
.au-text-primary {
  color: var(--au-primary);
}
.au-gradient-text {
  background-image: var(--au-gradient);
  -webkit-background-clip: text;
  background-clip: text;
  -webkit-text-fill-color: transparent;
}

/* --- layout helpers ------------------------------------------------------ */
.au-stack {
  display: flex;
  flex-direction: column;
  gap: var(--au-gap, 16px);
}
.au-row {
  display: flex;
  align-items: center;
  gap: var(--au-gap, 8px);
  min-width: 0;
}
.au-row--wrap {
  flex-wrap: wrap;
}
.au-row--between {
  justify-content: space-between;
}
.au-spacer {
  flex: 1;
}
.au-grid {
  display: grid;
  gap: var(--au-gap, 16px);
}
.au-grid--2 {
  grid-template-columns: repeat(2, minmax(0, 1fr));
}
.au-grid--3 {
  grid-template-columns: repeat(3, minmax(0, 1fr));
}
.au-grid--4 {
  grid-template-columns: repeat(4, minmax(0, 1fr));
}
@media (max-width: 1100px) {
  .au-grid--4 {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }
  .au-grid--3 {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }
}
@media (max-width: 640px) {
  .au-grid--2,
  .au-grid--3 {
    grid-template-columns: minmax(0, 1fr);
  }
}
.au-sr-only {
  position: absolute !important;
  width: 1px;
  height: 1px;
  padding: 0;
  margin: -1px;
  overflow: hidden;
  clip: rect(0, 0, 0, 0);
  white-space: nowrap;
  border: 0;
}

/* --- icons: Material Symbols (same glyph family as @mui/icons-material) --
   <span class="au-icon" aria-hidden="true">search</span>                  */
.au-icon {
  font-family: var(--au-font-icons);
  font-weight: normal;
  font-style: normal;
  font-size: 20px;
  line-height: 1;
  letter-spacing: normal;
  text-transform: none;
  display: inline-block;
  width: 1em; /* if the font is missing, the ligature name is clipped, not spilled */
  height: 1em;
  overflow: hidden;
  white-space: nowrap;
  word-wrap: normal;
  direction: ltr;
  flex-shrink: 0;
  user-select: none;
  -webkit-font-smoothing: antialiased;
  font-feature-settings: 'liga';
  font-variation-settings: 'FILL' 0, 'wght' 400, 'GRAD' 0, 'opsz' 20;
}
.au-icon--fill {
  font-variation-settings: 'FILL' 1, 'wght' 400, 'GRAD' 0, 'opsz' 20;
}
.au-icon--xs {
  font-size: 14px;
}
.au-icon--sm {
  font-size: 16px;
}
.au-icon--md {
  font-size: 18px;
}
.au-icon--lg {
  font-size: 24px;
}

/* --- motion primitives --------------------------------------------------- */
@keyframes auPageIn {
  from {
    opacity: 0.3;
    transform: translateY(6px);
  }
  to {
    opacity: 1;
    transform: none;
  }
}
@keyframes auFadeSlideIn {
  from {
    opacity: 0;
    transform: translateY(4px);
  }
  to {
    opacity: 1;
    transform: none;
  }
}
@keyframes auSpin {
  to {
    transform: rotate(360deg);
  }
}
@keyframes auIndeterminate {
  0% {
    transform: translateX(-100%);
  }
  100% {
    transform: translateX(320%);
  }
}
@keyframes auPulse {
  0% {
    opacity: 1;
  }
  50% {
    opacity: 0.4;
  }
  100% {
    opacity: 1;
  }
}
@keyframes auSlideInRight {
  from {
    opacity: 0;
    transform: translateX(24px);
  }
  to {
    opacity: 1;
    transform: none;
  }
}
@keyframes auDialogIn {
  from {
    opacity: 0;
    transform: translateY(8px) scale(0.98);
  }
  to {
    opacity: 1;
    transform: none;
  }
}
@keyframes auDrawerIn {
  from {
    transform: translateX(100%);
  }
  to {
    transform: none;
  }
}
@keyframes auFadeIn {
  from {
    opacity: 0;
  }
  to {
    opacity: 1;
  }
}

.au-animate-in {
  animation: auFadeSlideIn 0.24s ease;
}
.au-spin {
  animation: auSpin 0.9s linear infinite;
}

@media (prefers-reduced-motion: reduce) {
  *,
  *::before,
  *::after {
    animation-duration: 0.001s !important;
    transition-duration: 0.001s !important;
  }
  /* looping indicators: stop outright instead of strobing */
  .au-spin,
  .au-spinner,
  .au-progress--indeterminate .au-progress__bar,
  .au-topbar-progress__bar,
  .au-skeleton {
    animation: none !important;
  }
}

/* --- refined thin scrollbars, theme-aware -------------------------------- */
* {
  scrollbar-width: thin;
  scrollbar-color: var(--au-scroll-thumb) transparent;
}
*::-webkit-scrollbar {
  width: 10px;
  height: 10px;
}
*::-webkit-scrollbar-track {
  background: transparent;
}
*::-webkit-scrollbar-thumb {
  background-color: var(--au-scroll-thumb);
  border-radius: 8px;
  border: 2px solid transparent;
  background-clip: content-box;
}
*::-webkit-scrollbar-thumb:hover {
  background-color: var(--au-scroll-thumb-hover);
}

/* ==========================================================================
   AURORA — components. Every colour comes from a token; never hard-code one.
   ========================================================================== */

/* --- surfaces ------------------------------------------------------------ */
/* Panel = MUI <Paper variant="outlined">: hairline border, soft shadow and a
   faint "glass top-light" (sheen) so surfaces catch the aurora backdrop. */
.au-panel {
  background-color: var(--au-paper);
  background-image: var(--au-sheen);
  border: 1px solid var(--au-divider);
  border-radius: var(--au-r-sm);
  box-shadow: var(--au-shadow-paper);
  min-width: 0;
}
.au-panel--round {
  border-radius: var(--au-r-panel);
}
.au-panel--pad {
  padding: 20px;
}
.au-panel--pad-sm {
  padding: 12px;
}
.au-panel__head {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  margin-bottom: 16px;
  min-width: 0;
}
.au-panel__title {
  margin: 0;
  font-size: 0.875rem;
  font-weight: 700;
  letter-spacing: 0;
}
.au-panel__link {
  font-size: 0.75rem;
  font-weight: 600;
  color: var(--au-primary);
  white-space: nowrap;
}
.au-panel__link:hover {
  text-decoration: underline;
}
.au-card {
  background-color: var(--au-paper);
  background-image: var(--au-sheen);
  border: 1px solid var(--au-divider);
  border-radius: var(--au-r-card);
  box-shadow: var(--au-shadow-paper);
  padding: 16px;
}
.au-divider {
  height: 1px;
  border: 0;
  margin: 16px 0;
  background: var(--au-divider);
}
.au-divider-label {
  display: flex;
  align-items: center;
  gap: 10px;
  margin: 16px 0 8px;
  font-size: 0.75rem;
  color: var(--au-text-2);
}
.au-divider-label::after {
  content: '';
  flex: 1;
  height: 1px;
  background: var(--au-divider);
}

/* --- buttons ------------------------------------------------------------- */
.au-btn {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  min-height: 36px;
  padding: 6px 14px;
  border: 1px solid transparent;
  border-radius: var(--au-r-sm);
  background: transparent;
  color: var(--au-text);
  font-size: 0.875rem;
  font-weight: 650;
  line-height: 1.5;
  letter-spacing: 0;
  text-transform: none;
  white-space: nowrap;
  text-decoration: none;
  cursor: pointer;
  user-select: none;
  transition: transform var(--au-t-fast) ease, box-shadow 0.18s ease, background-color 0.18s ease,
    border-color 0.18s ease, filter 0.18s ease, color 0.18s ease;
}
.au-btn:hover {
  background-color: var(--au-hover);
  text-decoration: none;
}
.au-btn:active {
  transform: scale(0.98);
}
.au-btn:focus-visible {
  outline: none;
  box-shadow: var(--au-focus-ring);
}
.au-btn .au-icon {
  font-size: 18px;
  margin-inline: -2px;
}
/* contained primary — the brand gradient CTA */
.au-btn--primary {
  color: var(--au-on-primary);
  background-color: var(--au-primary);
  background-image: var(--au-gradient);
  box-shadow: 0 2px 10px -2px var(--au-brand-glow);
}
.au-btn--primary:hover {
  background-color: var(--au-primary);
  filter: brightness(1.07);
  box-shadow: 0 4px 18px -4px var(--au-brand-glow-strong);
}
/* outlined neutral — secondary actions, Export, Refresh, date range */
.au-btn--outlined {
  border-color: var(--au-input-border);
  background-color: var(--au-paper);
}
.au-btn--outlined:hover {
  border-color: var(--au-input-border-hover);
  background-color: color-mix(in srgb, var(--au-text) 4%, var(--au-paper));
}
.au-btn--outlined-primary {
  border-color: color-mix(in srgb, var(--au-primary) 50%, transparent);
  color: var(--au-primary);
}
.au-btn--outlined-primary:hover {
  border-color: var(--au-primary);
  background-color: color-mix(in srgb, var(--au-primary) 4%, transparent);
}
/* text button */
.au-btn--text {
  padding-inline: 8px;
}
.au-btn--text-primary {
  padding-inline: 8px;
  color: var(--au-primary);
}
.au-btn--text-primary:hover {
  background-color: color-mix(in srgb, var(--au-primary) 6%, transparent);
}
/* semantic contained (destructive confirms etc.) */
.au-btn--danger {
  color: var(--au-on-error);
  background-color: var(--au-error);
}
.au-btn--danger:hover {
  background-color: var(--au-error-dark);
}
.au-btn--success {
  color: var(--au-on-success);
  background-color: var(--au-success);
}
.au-btn--success:hover {
  background-color: var(--au-success-dark);
}
.au-btn--warning {
  color: var(--au-on-warning);
  background-color: var(--au-warning);
}
.au-btn--warning:hover {
  background-color: var(--au-warning-dark);
}
.au-btn--danger-outlined {
  border-color: color-mix(in srgb, var(--au-error) 50%, transparent);
  color: var(--au-error);
}
.au-btn--danger-outlined:hover {
  border-color: var(--au-error);
  background-color: color-mix(in srgb, var(--au-error) 5%, transparent);
}
/* sizes */
.au-btn--sm {
  min-height: 30px;
  padding: 4px 10px;
  font-size: 0.8125rem;
}
.au-btn--lg {
  min-height: 42px;
  padding: 8px 22px;
  font-size: 0.9375rem;
}
.au-btn--block {
  display: flex;
  width: 100%;
}
/* disabled / busy */
.au-btn:disabled,
.au-btn.is-disabled,
.au-btn[aria-disabled='true'] {
  color: var(--au-disabled);
  background-image: none;
  box-shadow: none;
  filter: none;
  cursor: default;
  pointer-events: none;
}
.au-btn--primary:disabled,
.au-btn--primary.is-disabled,
.au-btn--danger:disabled,
.au-btn--success:disabled,
.au-btn--warning:disabled {
  background-color: var(--au-disabled-bg);
}
.au-btn--outlined:disabled,
.au-btn--outlined.is-disabled {
  border-color: var(--au-disabled-bg);
}
.au-btn[aria-busy='true'] {
  pointer-events: none;
  opacity: 0.85;
}
.au-btn[aria-busy='true']::before {
  content: '';
  width: 14px;
  height: 14px;
  border: 2px solid currentColor;
  border-right-color: transparent;
  border-radius: 50%;
  animation: auSpin 0.75s linear infinite;
}

/* --- icon buttons -------------------------------------------------------- */
.au-icon-btn {
  display: inline-grid;
  place-items: center;
  width: 36px;
  height: 36px;
  padding: 0;
  border: 0;
  border-radius: var(--au-r-sm);
  background: transparent;
  color: var(--au-text-2);
  cursor: pointer;
  flex-shrink: 0;
  transition: background-color var(--au-t) ease, color var(--au-t) ease,
    transform var(--au-t-fast) ease, border-color var(--au-t) ease, box-shadow var(--au-t) ease;
}
.au-icon-btn:hover {
  background-color: color-mix(in srgb, var(--au-text) 6%, transparent);
  color: var(--au-text);
}
.au-icon-btn:active {
  transform: scale(0.94);
}
.au-icon-btn:focus-visible {
  outline: none;
  box-shadow: var(--au-focus-ring);
}
.au-icon-btn:disabled,
.au-icon-btn.is-disabled {
  color: var(--au-disabled);
  pointer-events: none;
}
.au-icon-btn--bordered {
  border: 1px solid var(--au-divider);
  background-color: var(--au-paper);
}
.au-icon-btn--bordered:hover {
  background-color: var(--au-paper);
  color: var(--au-primary);
}
.au-icon-btn--primary:hover {
  color: var(--au-primary);
}
.au-icon-btn--sm {
  width: 28px;
  height: 28px;
  border-radius: var(--au-r-xs);
}
.au-icon-btn--sm .au-icon {
  font-size: 17px;
}
.au-icon-btn--xs {
  width: 20px;
  height: 20px;
  border-radius: 6px;
}
.au-icon-btn--xs .au-icon {
  font-size: 13px;
}

/* --- form controls ------------------------------------------------------- */
.au-input,
.au-select,
.au-textarea {
  display: block;
  width: 100%;
  min-height: 40px;
  padding: 7.5px 14px;
  border: 1px solid var(--au-input-border);
  border-radius: var(--au-r-sm);
  background-color: var(--au-input-bg);
  color: var(--au-text);
  font-size: 1rem;
  line-height: 1.5;
  outline: 0;
  appearance: none;
  transition: border-color var(--au-t) ease, box-shadow var(--au-t) ease;
}
.au-textarea {
  min-height: 96px;
  resize: vertical;
}
.au-input::placeholder,
.au-textarea::placeholder {
  color: color-mix(in srgb, var(--au-text) 42%, transparent);
  opacity: 1;
}
.au-input:hover,
.au-select:hover,
.au-textarea:hover {
  border-color: var(--au-input-border-hover);
}
.au-input:focus,
.au-select:focus,
.au-textarea:focus {
  border-color: var(--au-primary);
  box-shadow: var(--au-focus-ring);
}
.au-input:disabled,
.au-select:disabled,
.au-textarea:disabled,
.au-input[readonly] {
  background-color: var(--au-hover);
  color: var(--au-text-2);
  cursor: not-allowed;
}
.au-select {
  padding-right: 36px;
  background-image: var(--au-select-chevron);
  background-repeat: no-repeat;
  background-position: right 8px center;
  background-size: 22px;
  cursor: pointer;
}
.au-select[multiple],
.au-select[size] {
  background-image: none;
  padding-right: 14px;
}
.au-input--sm,
.au-select--sm {
  min-height: 34px;
  padding-top: 4px;
  padding-bottom: 4px;
  font-size: 0.875rem;
}
.au-input[type='datetime-local'],
.au-input[type='date'] {
  font-size: 0.9375rem;
}
:root[data-theme='dark'] .au-input::-webkit-calendar-picker-indicator {
  filter: invert(0.75);
}
.au-select option {
  background-color: var(--au-paper);
  color: var(--au-text);
}

/* Input with a leading/trailing icon */
.au-input-icon {
  position: relative;
  display: block;
  min-width: 0;
}
.au-input-icon > .au-icon {
  position: absolute;
  left: 12px;
  top: 50%;
  transform: translateY(-50%);
  color: var(--au-text-3);
  font-size: 20px;
  pointer-events: none;
}
.au-input-icon > .au-input {
  padding-left: 40px;
}

/* Field with a NOTCHED label (the MUI outlined look). The label sits on the
   top border; the bottom half of its background is the input fill, which
   hides the border line behind it on any surface. */
.au-field {
  position: relative;
  display: flex;
  flex-direction: column;
  min-width: 0;
}
.au-field > .au-label {
  position: absolute;
  top: 0;
  left: 10px;
  z-index: 1;
  max-width: calc(100% - 20px);
  transform: translateY(-50%);
  padding: 0 5px;
  border-radius: 4px;
  background: linear-gradient(to bottom, transparent 45%, var(--au-label-notch) 45%);
  color: var(--au-text-2);
  font-size: 0.75rem;
  font-weight: 500;
  line-height: 1.2;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  pointer-events: none;
  transition: color var(--au-t) ease;
}
.au-field:focus-within > .au-label {
  color: var(--au-primary);
}
.au-field.is-invalid > .au-label,
.au-field:has(.input-validation-error) > .au-label {
  color: var(--au-error);
}
/* Stacked label — long forms and customer-facing screens */
.au-form-field {
  display: flex;
  flex-direction: column;
  gap: 6px;
  min-width: 0;
}
.au-form-label {
  font-size: 0.8125rem;
  font-weight: 600;
  color: var(--au-text);
}
.au-form-label .au-required {
  color: var(--au-error);
  margin-left: 2px;
}
.au-help {
  font-size: 0.75rem;
  color: var(--au-text-2);
}
.au-form-grid {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 18px 16px;
}
.au-form-grid > .au-span-2 {
  grid-column: 1 / -1;
}
@media (max-width: 640px) {
  .au-form-grid {
    grid-template-columns: minmax(0, 1fr);
  }
}
.au-form-actions {
  display: flex;
  justify-content: flex-end;
  align-items: center;
  gap: 8px;
  margin-top: 20px;
}

/* Validation — matches ASP.NET unobtrusive validation class names */
.field-validation-error,
.au-field-error {
  display: block;
  margin-top: 4px;
  font-size: 0.75rem;
  font-weight: 500;
  color: var(--au-error);
}
.field-validation-valid {
  display: none;
}
.input-validation-error,
.au-input.is-invalid,
.au-select.is-invalid,
.au-textarea.is-invalid {
  border-color: var(--au-error) !important;
}
.input-validation-error:focus,
.au-input.is-invalid:focus {
  box-shadow: var(--au-focus-ring-error) !important;
}
.validation-summary-errors ul {
  margin: 0;
  padding-left: 18px;
}
.validation-summary-valid {
  display: none;
}

/* checkbox / radio — native controls, brand accent */
.au-check {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  cursor: pointer;
  font-size: 0.875rem;
}
/* :where() = zero specificity, so Bootstrap .form-check-input layout still wins */
:where(input[type='checkbox'], input[type='radio']) {
  accent-color: var(--au-primary);
  width: 16px;
  height: 16px;
  margin: 0;
  cursor: pointer;
}
/* switch: <input type="checkbox" class="au-switch" role="switch"> */
.au-switch {
  appearance: none;
  position: relative;
  width: 36px !important;
  height: 20px !important;
  margin: 0;
  border-radius: 999px;
  background: var(--au-disabled-bg);
  cursor: pointer;
  flex-shrink: 0;
  transition: background-color var(--au-t) ease;
}
.au-switch::after {
  content: '';
  position: absolute;
  top: 2px;
  left: 2px;
  width: 16px;
  height: 16px;
  border-radius: 50%;
  background: #ffffff;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.3);
  transition: transform 0.18s ease;
}
.au-switch:checked {
  background-color: var(--au-primary);
  background-image: var(--au-gradient);
}
.au-switch:checked::after {
  transform: translateX(16px);
}
.au-switch:focus-visible {
  outline: none;
  box-shadow: var(--au-focus-ring);
}

/* --- chips (status / badges) --------------------------------------------- */
.au-chip {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  max-width: 100%;
  height: 22px;
  padding: 0 8px;
  border: 1px solid transparent;
  border-radius: var(--au-r-xs);
  background-color: var(--au-selected);
  color: var(--au-text);
  font-size: 0.8125rem;
  font-weight: 600;
  line-height: 1;
  white-space: nowrap;
  vertical-align: middle;
}
.au-chip > span {
  overflow: hidden;
  text-overflow: ellipsis;
}
.au-chip .au-icon {
  font-size: 15px;
}
.au-chip--md {
  height: 28px;
  padding: 0 11px;
}
.au-chip--success {
  background-color: var(--au-success);
  color: var(--au-on-success);
}
.au-chip--warning {
  background-color: var(--au-warning);
  color: var(--au-on-warning);
}
.au-chip--error {
  background-color: var(--au-error);
  color: var(--au-on-error);
}
.au-chip--info {
  background-color: var(--au-info);
  color: var(--au-on-info);
}
.au-chip--primary {
  background-color: var(--au-primary);
  color: var(--au-on-primary);
}
/* outlined = quieter; neutral border unless a tone is set */
.au-chip--outlined {
  background-color: transparent;
  border-color: var(--au-input-border);
  color: var(--au-text);
}
.au-chip--outlined.au-chip--success {
  border-color: color-mix(in srgb, var(--au-success) 70%, transparent);
  color: var(--au-success);
}
.au-chip--outlined.au-chip--warning {
  border-color: color-mix(in srgb, var(--au-warning) 70%, transparent);
  color: var(--au-warning);
}
.au-chip--outlined.au-chip--error {
  border-color: color-mix(in srgb, var(--au-error) 70%, transparent);
  color: var(--au-error);
}
.au-chip--outlined.au-chip--info {
  border-color: color-mix(in srgb, var(--au-info) 70%, transparent);
  color: var(--au-info);
}
.au-chip--outlined.au-chip--primary {
  border-color: color-mix(in srgb, var(--au-primary) 70%, transparent);
  color: var(--au-primary);
}
/* soft = tinted pill (verdicts, counters) */
.au-chip--soft {
  --c: var(--au-text);
  border-color: color-mix(in srgb, var(--au-text) 12%, transparent);
  background-color: color-mix(in srgb, var(--c) 8%, transparent);
  color: var(--c);
  font-weight: 700;
}
.au-chip--soft.au-chip--success {
  --c: var(--au-success);
}
.au-chip--soft.au-chip--warning {
  --c: var(--au-warning);
}
.au-chip--soft.au-chip--error {
  --c: var(--au-error);
}
.au-chip--soft.au-chip--info {
  --c: var(--au-info);
}
.au-chip--soft.au-chip--primary {
  --c: var(--au-primary);
}
a.au-chip,
button.au-chip {
  cursor: pointer;
  transition: filter var(--au-t) ease, background-color var(--au-t) ease;
}
a.au-chip:hover,
button.au-chip:hover {
  filter: brightness(0.97);
  text-decoration: none;
}
.au-chip__delete {
  display: inline-grid;
  place-items: center;
  margin-right: -4px;
  padding: 0;
  border: 0;
  background: none;
  color: inherit;
  opacity: 0.7;
  cursor: pointer;
}
.au-chip__delete:hover {
  opacity: 1;
}
.au-dot {
  display: inline-block;
  width: 8px;
  height: 8px;
  border-radius: 50%;
  flex-shrink: 0;
  background: var(--au-text-3);
}
.au-dot--success {
  background: var(--au-success);
}
.au-dot--warning {
  background: var(--au-warning);
}
.au-dot--error {
  background: var(--au-error);
}
.au-dot--info {
  background: var(--au-info);
}

/* --- filter toolbar ------------------------------------------------------ */
.au-toolbar {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  justify-content: space-between;
  gap: 10px;
  margin-bottom: 16px;
  padding: 12px;
  background-color: var(--au-paper);
  background-image: var(--au-sheen);
  border: 1px solid var(--au-divider);
  border-radius: var(--au-r-panel);
  box-shadow: var(--au-shadow-paper);
}
.au-toolbar__filters {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 8px;
  min-width: 0;
}
.au-toolbar__actions {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 8px;
  flex-shrink: 0;
  margin-left: auto;
}
.au-toolbar__meta {
  font-size: 0.75rem;
  color: var(--au-text-2);
  white-space: nowrap;
}
@media (max-width: 900px) {
  .au-toolbar__filters {
    flex-direction: column;
    align-items: stretch;
    width: 100%;
  }
  .au-toolbar__filters > * {
    width: 100% !important;
  }
  .au-toolbar__actions {
    flex-shrink: 1;
    width: 100%;
    min-width: 0;
    margin-left: 0;
  }
}

/* --- data table ---------------------------------------------------------- */
.au-table-card {
  position: relative;
  overflow: hidden;
  background-color: var(--au-paper);
  background-image: var(--au-sheen);
  border: 1px solid var(--au-divider);
  border-radius: var(--au-r-sm);
  box-shadow: var(--au-shadow-paper);
}
.au-table-card__loading {
  height: 4px;
  overflow: hidden;
  background: transparent;
}
.au-table-card.is-loading .au-table-card__loading {
  background: color-mix(in srgb, var(--au-primary) 12%, transparent);
}
.au-table-card.is-loading .au-table-card__loading::before {
  content: '';
  display: block;
  width: 40%;
  height: 100%;
  border-radius: 999px;
  background-image: var(--au-gradient);
  animation: auIndeterminate 1.1s ease-in-out infinite;
}
.au-table-scroll {
  overflow: auto;
  max-height: var(--au-table-max-h, none);
}
.au-table {
  width: 100%;
  border-collapse: separate;
  border-spacing: 0;
  font-size: 0.875rem;
  font-variant-numeric: tabular-nums;
}
.au-table thead th {
  position: sticky;
  top: 0;
  z-index: 2;
  height: 44px;
  padding: 6px 10px;
  border-bottom: 1px solid var(--au-divider);
  background: var(--au-table-head-bg);
  color: var(--au-text-2);
  font-size: 11px;
  font-weight: 700;
  letter-spacing: 0.05em;
  text-align: left;
  text-transform: uppercase;
  white-space: nowrap;
  vertical-align: middle;
}
.au-table tbody td {
  height: 44px;
  max-width: var(--au-col-max, 420px);
  padding: 6px 10px;
  border-bottom: 1px solid var(--au-divider);
  vertical-align: middle;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  transition: background-color var(--au-t-fast) ease;
}
.au-table tbody tr:last-child td {
  border-bottom: 0;
}
.au-table .au-wrap,
.au-table td.au-wrap {
  white-space: normal;
  word-break: break-word;
}
.au-table .au-right {
  text-align: right;
}
.au-table .au-center {
  text-align: center;
}
.au-table .au-shrink {
  width: 1%;
}
.au-table tbody tr[data-au-href],
.au-table--clickable tbody tr {
  cursor: pointer;
}
.au-table tbody tr[data-au-href]:hover td,
.au-table--clickable tbody tr:hover td {
  background-color: color-mix(in srgb, var(--au-primary) 5%, transparent);
}
.au-table tbody tr.is-selected td {
  background-color: color-mix(in srgb, var(--au-primary) 7%, transparent);
}
.au-table tbody tr[data-au-href]:focus-visible {
  outline: 2px solid var(--au-primary);
  outline-offset: -2px;
}
/* sortable header link */
.au-th-sort {
  display: inline-flex;
  align-items: center;
  gap: 2px;
  color: inherit;
}
.au-th-sort .au-icon {
  display: inline-grid;
  place-items: center;
  width: 24px;
  height: 24px;
  border-radius: 50%;
  font-size: 16px;
  color: var(--au-text-2);
  transition: background-color var(--au-t) ease;
}
.au-th-sort:hover .au-icon {
  background-color: var(--au-hover);
  color: var(--au-text);
}
.au-th-sort.is-sorted .au-icon {
  color: var(--au-text);
}
/* two-line cell: code + subtitle */
.au-cell-2l {
  display: flex;
  flex-direction: column;
  line-height: 1.25;
  min-width: 0;
}
.au-cell-2l > :first-child {
  font-family: var(--au-font-mono);
  font-size: 12.5px;
  overflow: hidden;
  text-overflow: ellipsis;
}
.au-cell-2l > :last-child {
  font-size: 12px;
  color: var(--au-text-2);
  overflow: hidden;
  text-overflow: ellipsis;
}
.au-row-actions {
  display: inline-flex;
  align-items: center;
  gap: 2px;
}

/* pager (table footer) */
.au-pager {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  justify-content: space-between;
  gap: 8px;
  padding: 10px;
  border-top: 1px solid var(--au-divider);
}
.au-pager__count {
  color: var(--au-text-2);
  font-variant-numeric: tabular-nums;
}
.au-pager__count strong {
  color: var(--au-text);
  font-weight: 650;
}
.au-pager__nav {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-left: auto;
}
.au-pager__size {
  width: 96px;
}
.au-pager__btn {
  width: 34px;
  height: 34px;
  border: 1px solid var(--au-divider);
}
.au-pager__page {
  min-width: 70px;
  text-align: center;
  font-weight: 600;
  font-variant-numeric: tabular-nums;
}

/* empty state */
.au-empty {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 10px;
  min-height: 220px;
  padding: 40px 16px;
  text-align: center;
  color: var(--au-text-2);
  font-weight: 500;
}
.au-empty__icon {
  display: grid;
  place-items: center;
  width: 52px;
  height: 52px;
  border-radius: 50%;
  background: color-mix(in srgb, var(--au-text) 4.5%, transparent);
  color: var(--au-text-3);
}
.au-empty__icon .au-icon {
  font-size: 24px;
}

/* --- KPI stat tile -------------------------------------------------------- */
.au-stat {
  height: 100%;
  padding: 16px;
  background-color: var(--au-paper);
  background-image: var(--au-sheen);
  border: 1px solid var(--au-divider);
  border-radius: var(--au-r-panel);
  box-shadow: var(--au-shadow-paper);
}
.au-stat__value {
  font-size: 26px;
  font-weight: 800;
  line-height: 1.2;
  font-variant-numeric: tabular-nums;
  color: var(--au-text);
}
.au-stat__hint {
  font-size: 0.75rem;
  color: var(--au-text-2);
}

/* --- status board (one health card per area, coloured left rail) -------- */
.au-status-board__head {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  margin-bottom: 12px;
}
.au-status-grid {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 12px;
}
@media (min-width: 900px) {
  .au-status-grid {
    grid-template-columns: repeat(4, minmax(0, 1fr));
  }
}
.au-status {
  --au-level: var(--au-text-3);
  position: relative;
  display: block;
  height: 100%;
  overflow: hidden;
  padding: 14px 14px 14px 18px;
  color: inherit;
  text-decoration: none;
  background-color: var(--au-paper);
  background-image: var(--au-sheen);
  border: 1px solid var(--au-divider);
  border-radius: var(--au-r-panel);
  box-shadow: var(--au-shadow-paper);
  transition: border-color var(--au-t) ease, transform var(--au-t) ease, box-shadow var(--au-t) ease;
}
.au-status::before {
  content: '';
  position: absolute;
  left: 0;
  top: 0;
  bottom: 0;
  width: 4px;
  background: var(--au-level);
}
a.au-status:hover {
  border-color: var(--au-primary-light);
  transform: translateY(-2px);
  box-shadow: 0 12px 30px -22px color-mix(in srgb, var(--au-primary) 50%, transparent);
  text-decoration: none;
}
.au-status[data-level='crit'] {
  --au-level: var(--au-error);
}
.au-status[data-level='warn'] {
  --au-level: var(--au-warning);
}
.au-status[data-level='ok'] {
  --au-level: var(--au-success);
}
.au-status[data-level='idle']::before {
  opacity: 0.5;
}
.au-status__head {
  display: flex;
  align-items: center;
  gap: 6px;
  margin-bottom: 6px;
  min-width: 0;
  color: var(--au-text-2);
}
.au-status__head .au-icon {
  font-size: 16px;
}
.au-status__head .au-eyebrow {
  flex: 1;
  min-width: 0;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}
.au-status__head .au-dot {
  background: var(--au-level);
}
.au-status__metric {
  font-size: 22px;
  font-weight: 800;
  line-height: 1.2;
  font-variant-numeric: tabular-nums;
}
.au-status[data-level='crit'] .au-status__metric {
  color: var(--au-error);
}
.au-status__sub {
  display: block;
  font-size: 0.75rem;
  color: var(--au-text-2);
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

/* --- key/value fields ---------------------------------------------------- */
.au-kv {
  display: grid;
  gap: 14px;
}
.au-kv--2 {
  grid-template-columns: repeat(2, minmax(0, 1fr));
}
.au-kv--3 {
  grid-template-columns: repeat(3, minmax(0, 1fr));
}
@media (max-width: 640px) {
  .au-kv--2,
  .au-kv--3 {
    grid-template-columns: minmax(0, 1fr);
  }
}
.au-kv__item {
  min-width: 0;
}
.au-kv__value {
  font-size: 0.875rem;
  word-break: break-word;
}
.au-detail-rows {
  display: flex;
  flex-direction: column;
}
.au-detail-row {
  display: flex;
  gap: 8px;
  padding: 7px 0;
  border-bottom: 1px solid var(--au-divider);
}
.au-detail-row > dt,
.au-detail-row > .au-detail-row__label {
  min-width: 130px;
  margin: 0;
  color: var(--au-text-2);
}
.au-detail-row > dd,
.au-detail-row > .au-detail-row__value {
  flex: 1;
  margin: 0;
  word-break: break-word;
}
@media (max-width: 640px) {
  .au-detail-row {
    flex-direction: column;
    gap: 2px;
  }
}

/* --- copyable id, code block, kbd ---------------------------------------- */
.au-copy {
  display: inline-flex;
  align-items: center;
  gap: 2px;
  min-width: 0;
  max-width: 100%;
}
.au-copy > .au-mono {
  font-size: 12px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}
.au-code {
  margin: 0;
  padding: 10px 12px;
  max-height: 320px;
  overflow: auto;
  border: 1px solid var(--au-divider);
  border-radius: var(--au-r-md);
  background: var(--au-code-bg);
  color: var(--au-text);
  font-family: var(--au-font-mono);
  font-size: 12.5px;
  line-height: 1.55;
  white-space: pre-wrap;
  word-break: break-word;
}
.au-kbd {
  display: inline-block;
  padding: 0 6px;
  border: 1px solid var(--au-divider);
  border-radius: var(--au-r-xs);
  background: var(--au-bg);
  color: var(--au-text-2);
  font-family: var(--au-font-mono);
  font-size: 11px;
  font-weight: 600;
  line-height: 1.6;
  white-space: nowrap;
}

/* --- tabs (gradient indicator) ------------------------------------------- */
.au-tabs {
  display: flex;
  overflow-x: auto;
  border-bottom: 1px solid var(--au-divider);
  scrollbar-width: none;
}
.au-tabs::-webkit-scrollbar {
  display: none;
}
.au-tab {
  position: relative;
  display: inline-flex;
  align-items: center;
  gap: 6px;
  min-height: 44px;
  padding: 8px 16px;
  border: 0;
  background: none;
  color: var(--au-text-2);
  font-size: 0.875rem;
  font-weight: 650;
  white-space: nowrap;
  cursor: pointer;
  transition: color var(--au-t) ease;
}
.au-tab:hover {
  color: var(--au-text);
  text-decoration: none;
}
.au-tab .au-icon {
  font-size: 18px;
}
.au-tab.is-active,
.au-tab[aria-selected='true'] {
  color: var(--au-primary);
}
.au-tab.is-active::after,
.au-tab[aria-selected='true']::after {
  content: '';
  position: absolute;
  left: 0;
  right: 0;
  bottom: 0;
  height: 3px;
  border-radius: 3px;
  background-image: var(--au-gradient);
}
.au-tab__count {
  padding: 0 6px;
  border-radius: 999px;
  background: var(--au-selected);
  color: var(--au-text-2);
  font-size: 11px;
  font-weight: 700;
}

/* --- segmented toggle (MUI ToggleButtonGroup) ---------------------------- */
.au-segmented {
  display: inline-flex;
  gap: 2px;
  padding: 3px;
  border-radius: var(--au-r-sm);
  background: var(--au-segment-bg);
}
.au-segmented > a,
.au-segmented > button,
.au-segmented > label {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  padding: 4px 11px;
  border: 0;
  border-radius: var(--au-r-xs);
  background: transparent;
  color: var(--au-text-2);
  font-size: 0.8125rem;
  font-weight: 600;
  line-height: 1.6;
  white-space: nowrap;
  cursor: pointer;
  transition: background-color var(--au-t) ease, color var(--au-t) ease, box-shadow var(--au-t) ease;
}
.au-segmented > a:hover,
.au-segmented > button:hover,
.au-segmented > label:hover {
  color: var(--au-text);
  text-decoration: none;
}
.au-segmented > .is-active,
.au-segmented > [aria-pressed='true'],
.au-segmented > input:checked + label {
  background: var(--au-segment-selected);
  color: var(--au-primary);
  box-shadow: var(--au-segment-shadow);
}
.au-segmented > input {
  position: absolute;
  opacity: 0;
  pointer-events: none;
}

/* --- alerts --------------------------------------------------------------- */
.au-alert {
  --c: var(--au-info);
  display: flex;
  align-items: flex-start;
  gap: 12px;
  margin: 8px 0;
  padding: 6px 16px;
  border-radius: var(--au-r-md);
  background-color: color-mix(in srgb, var(--c) 10%, var(--au-mix-bg));
  color: color-mix(in srgb, var(--c) 40%, var(--au-mix-fg));
  font-weight: 500;
  line-height: 1.43;
}
.au-alert--success {
  --c: var(--au-success);
}
.au-alert--warning {
  --c: var(--au-warning);
}
.au-alert--error {
  --c: var(--au-error);
}
.au-alert__icon {
  padding-top: 7px;
  color: var(--c);
  font-size: 22px;
}
.au-alert__body {
  flex: 1;
  min-width: 0;
  padding: 8px 0;
  word-break: break-word;
}
.au-alert__title {
  margin-bottom: 2px;
  font-weight: 700;
}
.au-alert__actions {
  display: flex;
  align-items: center;
  gap: 2px;
  padding-top: 4px;
  margin-left: auto;
  margin-right: -8px;
}
.au-alert__actions .au-btn,
.au-alert__actions .au-icon-btn {
  color: inherit;
}

/* Error panel — the ONE way a failed call/operation is shown.
   Headline · cause · What to check · the failing call · <details> · Copy */
.au-error-panel .au-alert__hint {
  margin-top: 6px;
  font-size: 0.875rem;
  opacity: 0.9;
}
.au-error-panel .au-alert__call {
  margin-top: 6px;
  font-family: var(--au-font-mono);
  font-size: 0.75rem;
  opacity: 0.85;
  word-break: break-all;
}
.au-error-panel details {
  margin-top: 8px;
}
.au-error-panel summary {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  font-size: 0.8125rem;
  font-weight: 600;
  cursor: pointer;
  list-style: none;
}
.au-error-panel summary::-webkit-details-marker {
  display: none;
}
.au-error-panel summary::after {
  content: 'expand_more';
  font-family: var(--au-font-icons);
  font-size: 18px;
  transition: transform var(--au-t) ease;
}
.au-error-panel details[open] summary::after {
  transform: rotate(180deg);
}
.au-error-panel dl {
  display: grid;
  grid-template-columns: max-content minmax(0, 1fr);
  gap: 4px 16px;
  margin: 10px 0 0;
  font-size: 12.5px;
}
.au-error-panel dt {
  font-weight: 700;
  opacity: 0.85;
}
.au-error-panel dd {
  margin: 0;
  white-space: pre-wrap;
  word-break: break-word;
}
.au-error-panel pre {
  margin-top: 10px;
  padding: 8px;
  max-height: 240px;
  overflow: auto;
  border: 1px solid var(--au-divider);
  border-radius: var(--au-r-sm);
  background: var(--au-code-bg);
  color: var(--au-text);
  font-size: 11.5px;
  white-space: pre;
}

/* --- toasts (bottom-right, filled, auto-dismiss) ------------------------- */
.au-toasts {
  position: fixed;
  right: 24px;
  bottom: 24px;
  z-index: var(--au-z-toast);
  display: flex;
  flex-direction: column;
  gap: 8px;
  max-width: min(440px, calc(100vw - 48px));
  pointer-events: none;
}
.au-toast {
  --c: var(--au-toast-info);
  --on: var(--au-on-toast-info);
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 6px 8px 6px 16px;
  border-radius: var(--au-r-md);
  background: var(--c);
  color: var(--on);
  box-shadow: var(--au-shadow-toast);
  font-weight: 500;
  line-height: 1.43;
  word-break: break-word;
  pointer-events: auto;
  animation: auSlideInRight 0.22s var(--au-ease-out);
}
.au-toast--success {
  --c: var(--au-toast-success);
  --on: var(--au-on-toast-success);
}
.au-toast--warning {
  --c: var(--au-toast-warning);
  --on: var(--au-on-toast-warning);
}
.au-toast--error {
  --c: var(--au-toast-error);
  --on: var(--au-on-toast-error);
}
.au-toast__msg {
  flex: 1;
  padding: 8px 0;
}
.au-toast .au-icon-btn {
  color: inherit;
}
.au-toast .au-icon-btn:hover {
  background-color: rgba(255, 255, 255, 0.16);
}
.au-toast.is-leaving {
  opacity: 0;
  transform: translateX(24px);
  transition: opacity 0.2s ease, transform 0.2s ease;
}

/* --- dialog (native <dialog>) -------------------------------------------- */
dialog.au-dialog {
  width: calc(100% - 32px);
  max-width: 444px;
  max-height: calc(100% - 64px);
  padding: 0;
  border: 1px solid var(--au-divider);
  border-radius: var(--au-r-dialog);
  background: var(--au-paper);
  color: var(--au-text);
  box-shadow: var(--au-shadow-pop);
  overflow: auto;
}
dialog.au-dialog--sm {
  max-width: 600px;
}
dialog.au-dialog--md {
  max-width: 900px;
}
dialog.au-dialog--lg {
  max-width: 1200px;
}
dialog.au-dialog[open] {
  animation: auDialogIn 0.2s var(--au-ease-out);
}
dialog.au-dialog::backdrop,
dialog.au-drawer::backdrop,
dialog.au-palette::backdrop {
  background: var(--au-backdrop);
  backdrop-filter: blur(5px);
  animation: auFadeIn 0.2s ease;
}
.au-dialog__title {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  margin: 0;
  padding: 16px 24px;
  font-size: 1.02rem;
  font-weight: 750;
  letter-spacing: -0.01em;
}
.au-dialog__content {
  padding: 0 24px 20px;
}
.au-dialog__text {
  margin: 0;
  color: var(--au-text-2);
  font-size: 1rem;
}
.au-dialog__actions {
  display: flex;
  justify-content: flex-end;
  align-items: center;
  gap: 8px;
  padding: 8px 24px 16px;
}

/* --- drawer (right-side detail panel; native <dialog>) ------------------- */
dialog.au-drawer {
  position: fixed;
  inset: 0 0 0 auto;
  width: 460px;
  max-width: 100%;
  height: 100%;
  max-height: 100%;
  margin: 0;
  padding: 0;
  border: 0;
  border-left: 1px solid var(--au-divider);
  background: var(--au-paper);
  color: var(--au-text);
  box-shadow: var(--au-shadow-pop);
  overflow: auto;
}
dialog.au-drawer--wide {
  width: 720px;
}
dialog.au-drawer[open] {
  animation: auDrawerIn 0.26s var(--au-ease-out);
}
.au-drawer__head {
  position: sticky;
  top: 0;
  z-index: 1;
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  padding: 16px 20px 8px;
  background: var(--au-paper);
}
.au-drawer__title {
  margin: 0;
  font-size: 1rem;
  font-weight: 700;
}
.au-drawer__body {
  padding: 8px 20px 24px;
}
.au-drawer__chips {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
  margin-bottom: 16px;
}

/* --- menus / dropdowns (<details class="au-dropdown">) ------------------- */
.au-dropdown {
  position: relative;
  display: inline-block;
}
.au-dropdown > summary {
  list-style: none;
  cursor: pointer;
}
.au-dropdown > summary::-webkit-details-marker {
  display: none;
}
.au-menu {
  position: absolute;
  z-index: 30;
  top: calc(100% + 6px);
  min-width: 200px;
  padding: 6px;
  border: 1px solid var(--au-divider);
  border-radius: var(--au-r-md);
  background-color: color-mix(in srgb, var(--au-paper) 92%, transparent);
  backdrop-filter: blur(14px);
  box-shadow: var(--au-shadow-pop);
  animation: auFadeSlideIn 0.16s ease;
}
.au-menu--end {
  right: 0;
}
.au-menu__caption {
  display: block;
  padding: 6px 10px 4px;
  color: var(--au-text-3);
  font-size: 0.75rem;
  font-weight: 700;
  letter-spacing: 0.04em;
  text-transform: uppercase;
}
.au-menu__item {
  display: flex;
  align-items: center;
  gap: 10px;
  width: 100%;
  padding: 6px 10px;
  border: 0;
  border-radius: var(--au-r-xs);
  background: none;
  color: var(--au-text);
  font-size: 0.875rem;
  text-align: left;
  cursor: pointer;
}
.au-menu__item:hover {
  background-color: var(--au-hover);
  text-decoration: none;
}
.au-menu__item.is-selected {
  background-color: color-mix(in srgb, var(--au-primary) 10%, transparent);
}
.au-menu__item .au-icon {
  color: var(--au-text-2);
  font-size: 18px;
}
.au-menu__item--danger {
  color: var(--au-error);
}
.au-menu__item--danger .au-icon {
  color: inherit;
}
.au-menu__sep {
  height: 1px;
  margin: 6px 0;
  background: var(--au-divider);
}

/* --- tooltip: <button data-au-tip="Copy"> (CSS only) --------------------- */
[data-au-tip] {
  position: relative;
}
[data-au-tip]::after {
  content: attr(data-au-tip);
  position: absolute;
  z-index: 50;
  left: 50%;
  top: calc(100% + 6px);
  transform: translate(-50%, -2px);
  padding: 6px 10px;
  border-radius: var(--au-r-xs);
  background: var(--au-tooltip-bg);
  color: #ffffff;
  box-shadow: 0 6px 20px -6px rgba(0, 0, 0, 0.45);
  font-family: var(--au-font-sans);
  font-size: 12px;
  font-weight: 500;
  line-height: 1.4;
  letter-spacing: 0;
  text-transform: none;
  white-space: nowrap;
  opacity: 0;
  pointer-events: none;
  transition: opacity var(--au-t) ease 0.25s, transform var(--au-t) ease 0.25s;
}
[data-au-tip]:hover::after,
[data-au-tip]:focus-visible::after {
  opacity: 1;
  transform: translate(-50%, 0);
}
[data-au-tip-pos='right']::after {
  left: calc(100% + 8px);
  top: 50%;
  transform: translate(-2px, -50%);
}
[data-au-tip-pos='right']:hover::after,
[data-au-tip-pos='right']:focus-visible::after {
  transform: translate(0, -50%);
}
[data-au-tip-pos='left']::after {
  left: auto;
  right: 0;
  transform: translate(0, -2px);
}
[data-au-tip-pos='left']:hover::after {
  transform: none;
}

/* --- progress, meters, loading ------------------------------------------- */
.au-progress {
  height: 4px;
  overflow: hidden;
  border-radius: 999px;
  background: color-mix(in srgb, var(--au-primary) 12%, transparent);
}
.au-progress__bar {
  width: var(--value, 0%);
  height: 100%;
  border-radius: 999px;
  background-image: var(--au-gradient);
  transition: width 0.4s ease;
}
.au-progress--indeterminate .au-progress__bar {
  width: 40%;
  animation: auIndeterminate 1.1s ease-in-out infinite;
}
/* channel/usage meter */
.au-meter {
  height: 6px;
  overflow: hidden;
  border-radius: 999px;
  background: var(--au-meter-track);
}
.au-meter__fill {
  width: var(--value, 0%);
  height: 100%;
  border-radius: 999px;
  background-image: var(--au-gradient);
  transition: width 0.4s ease;
}
.au-meter--success .au-meter__fill {
  background: var(--au-success);
}
.au-meter--warning .au-meter__fill {
  background: var(--au-warning);
}
.au-meter--error .au-meter__fill {
  background: var(--au-error);
}
.au-skeleton {
  display: block;
  border-radius: var(--au-r-sm);
  background-color: color-mix(in srgb, var(--au-text) 6%, transparent);
  animation: auPulse 1.5s ease-in-out 0.5s infinite;
}
.au-skeleton--text {
  height: 0.9em;
  margin: 0.2em 0;
  border-radius: 4px;
}
.au-spinner {
  display: inline-block;
  width: 18px;
  height: 18px;
  border: 2px solid var(--au-primary);
  border-right-color: transparent;
  border-radius: 50%;
  animation: auSpin 0.75s linear infinite;
  vertical-align: middle;
}

/* --- page-level patterns ------------------------------------------------- */
/* Back header for detail pages: ← Title / subtitle */
.au-back-header {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-bottom: 16px;
  min-width: 0;
}
.au-back-header__title {
  margin: 0;
  font-size: 1.5rem;
  font-weight: 700;
  letter-spacing: -0.015em;
  line-height: 1.25;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}
.au-back-header__sub {
  display: block;
  font-size: 0.75rem;
  color: var(--au-text-2);
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}
/* Landing hero — a lit glass header */
.au-hero {
  position: relative;
  display: flex;
  align-items: center;
  gap: 16px;
  overflow: hidden;
  padding: 24px;
  border: 1px solid var(--au-divider);
  border-radius: var(--au-r-hero);
  box-shadow: var(--au-shadow-paper);
  background-color: var(--au-paper);
  background-image: radial-gradient(680px 220px at 88% -40%,
      color-mix(in srgb, var(--au-primary) var(--au-hero-a), transparent), transparent 70%),
    radial-gradient(520px 220px at 8% 140%,
      color-mix(in srgb, var(--au-secondary) var(--au-hero-b), transparent), transparent 68%);
}
.au-hero__mark {
  flex-shrink: 0;
  line-height: 0;
  filter: drop-shadow(0 10px 24px color-mix(in srgb, var(--au-primary) 50%, transparent));
}
.au-hero__title {
  margin: 0;
  font-size: 1.3rem;
  font-weight: 800;
  letter-spacing: -0.02em;
}
.au-hero__sub {
  margin: 0;
  color: var(--au-text-2);
  font-size: 1rem;
}
@media (max-width: 640px) {
  .au-hero {
    padding: 20px;
    border-radius: var(--au-r-panel);
  }
}
/* 404 / error page */
.au-fullpage-msg {
  display: grid;
  grid-template-columns: minmax(0, 1fr); /* the column may not grow past the screen to fit a long id */
  place-items: center;
  padding: 80px 16px;
}
.au-fullpage-msg__inner {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 16px;
  width: 100%; /* a grid item sizes to its content: without this a long request id overflows a phone */
  max-width: 560px;
  text-align: center;
}
.au-fullpage-msg__code {
  font-size: 72px;
  font-weight: 800;
  line-height: 1;
  background-image: var(--au-gradient);
  -webkit-background-clip: text;
  background-clip: text;
  -webkit-text-fill-color: transparent;
}
/* soft brand-tinted icon tile (feature cards, list avatars) */
.au-icon-tile {
  display: grid;
  place-items: center;
  width: 28px;
  height: 28px;
  border-radius: var(--au-r-xs);
  background: color-mix(in srgb, var(--tile, var(--au-primary)) 12%, transparent);
  color: var(--tile, var(--au-primary));
  flex-shrink: 0;
}
:root[data-theme='dark'] .au-icon-tile {
  background: color-mix(in srgb, var(--tile, var(--au-primary)) 18%, transparent);
}
.au-icon-tile .au-icon {
  font-size: 18px;
}
.au-avatar {
  display: grid;
  place-items: center;
  width: 30px;
  height: 30px;
  border-radius: 50%;
  background-image: var(--au-gradient);
  color: #ffffff;
  font-size: 12px;
  font-weight: 700;
  flex-shrink: 0;
}

/* ==========================================================================
   AURORA — app shell: dark sidebar · glass sticky header · section tabs ·
   main column. Collapsed rail = <html class="au-side-collapsed">.
   Mobile (<900px) = off-canvas sidebar, <html class="au-side-open">.
   ========================================================================== */
.au-app {
  display: flex;
  min-height: 100vh;
}

/* --- sidebar ------------------------------------------------------------- */
.au-sidebar {
  position: sticky;
  top: 0;
  z-index: var(--au-z-sidebar);
  display: flex;
  flex-direction: column;
  flex-shrink: 0;
  align-self: flex-start;
  width: var(--au-sidebar-w);
  height: 100vh;
  overflow: hidden;
  background: var(--au-side-bg);
  border-right: 1px solid var(--au-side-border);
  color: var(--au-side-text);
  transition: width 0.2s ease, transform 0.22s var(--au-ease-out);
}
.au-brand {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 20px 20px 12px;
  color: inherit;
  text-decoration: none;
}
.au-brand:hover {
  text-decoration: none;
}
.au-brand__mark {
  flex-shrink: 0;
  line-height: 0;
  filter: drop-shadow(0 6px 14px rgba(0, 0, 0, 0.45));
}
.au-brand__text {
  min-width: 0;
}
.au-brand__name {
  display: block;
  color: var(--au-side-text-strong);
  font-size: 15px;
  font-weight: 800;
  line-height: 1.15;
  letter-spacing: -0.01em;
  white-space: nowrap;
}
.au-brand__sub {
  display: block;
  color: var(--au-side-text);
  font-size: 11px;
  white-space: nowrap;
}
.au-side-rule {
  margin: 0 20px 4px;
  border-bottom: 1px solid var(--au-side-border);
}
.au-side-filter {
  display: flex;
  align-items: center;
  gap: 6px;
  margin: 4px 16px 4px;
  padding: 3.2px 8px;
  border: 1px solid var(--au-side-border);
  border-radius: var(--au-r-pill);
  background: var(--au-side-hover);
}
.au-side-filter .au-icon {
  font-size: 16px;
  color: var(--au-side-section);
}
.au-side-filter input {
  flex: 1;
  min-width: 0;
  padding: 4px 0 5px;
  border: 0;
  outline: 0;
  background: transparent;
  color: var(--au-side-text-strong);
  font-size: 13px;
  line-height: 1.4375;
}
.au-side-filter input::placeholder {
  color: var(--au-side-section);
  opacity: 1;
}
.au-side-filter button {
  display: none;
  padding: 2px;
  border: 0;
  border-radius: 6px;
  background: none;
  color: var(--au-side-section);
  cursor: pointer;
}
.au-side-filter.has-value button {
  display: inline-grid;
}
.au-side-filter button:hover {
  color: var(--au-side-text-strong);
}
.au-side-scroll {
  flex: 1;
  overflow-y: auto;
  padding: 4px 0 8px;
  scrollbar-color: #33415a transparent;
}
.au-side-scroll::-webkit-scrollbar-thumb {
  background-color: #33415a;
}
/* section divider: tiny uppercase caption + hairline */
.au-side-section {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 18px 20px 6px;
  color: var(--au-side-section);
  font-size: 10px;
  font-weight: 750;
  letter-spacing: 0.12em;
  text-transform: uppercase;
  white-space: nowrap;
  user-select: none;
}
.au-side-section::after {
  content: '';
  flex: 1;
  height: 1px;
  background: var(--au-side-border);
}
.au-side-section .au-icon {
  font-size: 12px;
}
/* a leaf row (link) */
.au-nav-item {
  position: relative;
  display: flex;
  align-items: center;
  margin: 0 8px;
  padding: 8.8px 32px 8.8px 10px;
  border-radius: var(--au-r-pill);
  color: var(--au-side-text);
  font-size: 13px;
  font-weight: 500;
  line-height: 1.5;
  text-decoration: none;
  transition: background-color var(--au-t) ease, color var(--au-t) ease, transform var(--au-t) ease;
}
.au-nav-item .au-icon {
  margin-right: 10px;
  font-size: 20px;
  color: inherit;
}
.au-nav-item__label {
  min-width: 0;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}
.au-nav-item::before {
  content: '';
  position: absolute;
  left: 0;
  top: 50%;
  width: 3px;
  height: 0;
  border-radius: 999px;
  background-image: var(--au-gradient);
  transform: translateY(-50%);
  transition: height 0.18s ease;
}
.au-nav-item:hover {
  background-color: var(--au-side-hover);
  color: var(--au-side-text-strong);
  transform: translateX(2px);
  text-decoration: none;
}
.au-nav-item.is-active {
  background: var(--au-side-active-bg);
  color: var(--au-side-active-text);
  font-weight: 700;
}
.au-nav-item.is-active .au-icon {
  color: var(--au-side-active-icon);
}
.au-nav-item.is-active::before {
  height: 16px;
}
.au-nav-item:focus-visible {
  outline: 2px solid var(--au-side-active-icon);
  outline-offset: -2px;
}
/* favourite star, revealed on hover */
.au-nav-row {
  position: relative;
}
.au-nav-star {
  position: absolute;
  right: 12px;
  top: 50%;
  display: grid;
  place-items: center;
  width: 22px;
  height: 22px;
  padding: 0;
  border: 0;
  border-radius: 6px;
  background: none;
  color: var(--au-side-section);
  opacity: 0;
  transform: translateY(-50%);
  cursor: pointer;
  transition: opacity var(--au-t) ease, color var(--au-t) ease;
}
.au-nav-star .au-icon {
  font-size: 15px;
}
.au-nav-row:hover .au-nav-star,
.au-nav-star:focus-visible,
.au-nav-star.is-on {
  opacity: 1;
}
.au-nav-star:hover,
.au-nav-star.is-on {
  color: var(--au-primary-light);
}
.au-nav-star.is-on .au-icon {
  font-variation-settings: 'FILL' 1, 'wght' 400, 'GRAD' 0, 'opsz' 20;
}
/* an expandable group (<details>) */
.au-nav-group {
  margin-bottom: 6px;
}
.au-nav-group--nested {
  margin-bottom: 2px;
}
.au-nav-group > summary {
  display: flex;
  align-items: center;
  margin: 0 8px;
  padding: 8.8px 8px 8.8px 10px;
  border-radius: var(--au-r-pill);
  color: var(--au-side-section);
  font-size: 11px;
  font-weight: 750;
  letter-spacing: 0.08em;
  text-transform: uppercase;
  line-height: 1.5;
  list-style: none;
  cursor: pointer;
  transition: background-color var(--au-t) ease, color var(--au-t) ease;
}
.au-nav-group--nested > summary {
  padding-top: 8px;
  padding-bottom: 8px;
  color: var(--au-side-text);
  font-size: 12.5px;
  font-weight: 650;
  letter-spacing: 0.01em;
  text-transform: none;
}
.au-nav-group > summary::-webkit-details-marker {
  display: none;
}
.au-nav-group > summary:hover {
  background-color: var(--au-side-hover);
  color: var(--au-side-text-strong);
}
.au-nav-group > summary .au-icon:first-child {
  margin-right: 10px;
  font-size: 20px;
}
.au-nav-group > summary .au-nav-item__label {
  flex: 1;
}
.au-nav-group > summary .au-nav-chevron {
  font-size: 18px;
  transition: transform 0.2s ease;
}
.au-nav-group[open] > summary .au-nav-chevron {
  transform: rotate(180deg);
}
.au-nav-children {
  margin: 2px 0 0 18px;
  padding-left: 2px;
  border-left: 1px solid var(--au-side-border);
}
.au-nav-empty {
  padding: 16px 20px;
  color: var(--au-side-section);
  font-size: 12.5px;
}
/* identity footer */
.au-side-footer {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 18px 20px;
  border-top: 1px solid var(--au-side-border);
}
.au-side-footer__text {
  min-width: 0;
}
.au-side-footer__name {
  display: block;
  color: var(--au-side-text-strong);
  font-size: 12.5px;
  font-weight: 650;
  line-height: 1.2;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}
.au-side-footer__email {
  display: block;
  color: var(--au-side-text);
  font-size: 11px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}
/* filtering mode: flat list of matching leaves */
.au-sidebar.is-filtering .au-side-section,
.au-sidebar.is-filtering .au-nav-group > summary,
.au-sidebar.is-filtering [data-au-favorites] {
  display: none;
}
.au-sidebar.is-filtering .au-nav-children {
  display: block !important;
  margin: 0;
  padding: 0;
  border: 0;
}
.au-sidebar.is-filtering .au-nav-row.is-filtered-out {
  display: none;
}
/* child views of a collapsed group: only listed while filtering */
.au-nav-row--search-only {
  display: none;
}
.au-sidebar.is-filtering .au-nav-row--search-only:not(.is-filtered-out) {
  display: block;
}

/* --- collapsed icon rail ------------------------------------------------- */
@media (min-width: 900px) {
  html.au-side-collapsed .au-sidebar {
    width: var(--au-sidebar-w-collapsed);
  }
  html.au-side-collapsed .au-brand {
    justify-content: center;
    padding-inline: 0;
  }
  html.au-side-collapsed .au-brand__text,
  html.au-side-collapsed .au-side-filter,
  html.au-side-collapsed .au-side-rule,
  html.au-side-collapsed .au-side-section,
  html.au-side-collapsed [data-au-favorites],
  html.au-side-collapsed .au-nav-item__label,
  html.au-side-collapsed .au-nav-chevron,
  html.au-side-collapsed .au-nav-star,
  html.au-side-collapsed .au-nav-children,
  html.au-side-collapsed .au-side-footer__text {
    display: none !important;
  }
  html.au-side-collapsed .au-side-scroll {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 4px;
    padding-top: 8px;
  }
  html.au-side-collapsed .au-nav-item,
  html.au-side-collapsed .au-nav-group > summary {
    justify-content: center;
    width: 42px;
    height: 42px;
    margin: 0;
    padding: 0;
  }
  html.au-side-collapsed .au-nav-item::before {
    display: none;
  }
  html.au-side-collapsed .au-nav-item:hover {
    transform: none;
  }
  html.au-side-collapsed .au-nav-item .au-icon,
  html.au-side-collapsed .au-nav-group > summary .au-icon:first-child {
    margin-right: 0;
  }
  html.au-side-collapsed .au-nav-group {
    margin: 0;
  }
  html.au-side-collapsed .au-side-footer {
    justify-content: center;
    padding: 12px 0;
  }
}

/* --- main column + glass header ------------------------------------------ */
.au-main-col {
  display: flex;
  flex: 1;
  flex-direction: column;
  min-width: 0;
}
.au-header {
  position: sticky;
  top: 0;
  z-index: var(--au-z-header);
}
.au-header__bar {
  position: relative;
  z-index: 2; /* its dropdowns must paint over the glass tab strip below */
  display: flex;
  align-items: center;
  gap: 12px;
  min-height: var(--au-header-h);
  padding: 10px 24px;
  border-bottom: 1px solid var(--au-divider);
  background: var(--au-glass);
  backdrop-filter: blur(16px) saturate(1.5);
  -webkit-backdrop-filter: blur(16px) saturate(1.5);
}
.au-header__title-wrap {
  min-width: 0;
  animation: auFadeSlideIn 0.24s ease;
}
.au-header__title {
  margin: 0;
  font-size: 1.05rem;
  font-weight: 750;
  letter-spacing: -0.01em;
  line-height: 1.3;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}
.au-header__tools {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-left: auto;
}
/* breadcrumb: ancestors are sibling-switcher dropdowns */
.au-crumbs {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  row-gap: 2px;
  margin-bottom: 1px;
}
.au-crumbs__sep {
  margin: 0 2px;
  color: var(--au-text-3);
  font-size: 12px;
}
.au-crumb {
  display: inline-flex;
  align-items: center;
  gap: 2px;
  padding: 0 4px;
  border: 0;
  border-radius: 6px;
  background: none;
  color: var(--au-text-2);
  font-size: 12px;
  line-height: 1.3;
  cursor: pointer;
}
.au-crumb .au-icon {
  font-size: 14px;
}
.au-crumb:hover {
  background: var(--au-hover);
  color: var(--au-text);
  text-decoration: none;
}
span.au-crumb {
  cursor: default;
}
span.au-crumb:hover {
  background: none;
  color: var(--au-text-2);
}
/* header pill controls */
.au-search-btn {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  padding: 6px 10px;
  border: 1px solid var(--au-divider);
  border-radius: var(--au-r-control);
  background: var(--au-paper);
  color: var(--au-text-2);
  font-size: 0.875rem;
  font-weight: 500;
  cursor: pointer;
  transition: border-color var(--au-t) ease, box-shadow var(--au-t) ease;
}
.au-search-btn .au-icon {
  font-size: 18px;
}
.au-search-btn:hover {
  border-color: var(--au-primary-light);
  box-shadow: 0 0 0 3px color-mix(in srgb, var(--au-primary) 12%, transparent);
}
.au-pill-btn {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  padding: 6px 10px;
  border: 1px solid var(--au-divider);
  border-radius: var(--au-r-control);
  background: var(--au-paper);
  color: var(--au-text-2);
  font-size: 13px;
  font-weight: 700;
  letter-spacing: 0.01em;
  cursor: pointer;
  transition: border-color var(--au-t) ease, box-shadow var(--au-t) ease;
}
.au-pill-btn .au-icon {
  font-size: 17px;
}
.au-pill-btn:hover {
  border-color: var(--au-primary-light);
  box-shadow: 0 0 0 3px color-mix(in srgb, var(--au-primary) 12%, transparent);
}
.au-pill-btn--danger {
  border-color: var(--au-error);
  background: color-mix(in srgb, var(--au-error) 10%, var(--au-paper));
  color: var(--au-error);
}
/* the global scope selector (one entity: e.g. Clinic) */
.au-scope {
  display: flex;
  align-items: center;
  gap: 4px;
  padding: 4px;
  border: 1px solid var(--au-divider);
  border-radius: var(--au-r-control);
  background: var(--au-bg);
}
.au-combobox {
  position: relative;
  width: 230px;
}
.au-combobox__input {
  width: 100%;
  min-height: 36px;
  padding: 6px 30px 6px 36px;
  border: 0;
  border-radius: var(--au-r-pill);
  background: var(--au-paper);
  color: var(--au-text);
  font-size: 0.9375rem;
  outline: 0;
  text-overflow: ellipsis;
}
.au-combobox__input:focus {
  box-shadow: var(--au-focus-ring);
}
.au-combobox__input::placeholder {
  color: var(--au-text);
  opacity: 1;
}
.au-combobox > .au-icon {
  position: absolute;
  left: 10px;
  top: 50%;
  transform: translateY(-50%);
  color: var(--au-text-2);
  font-size: 20px;
  pointer-events: none;
}
.au-combobox__caret {
  position: absolute;
  right: 6px;
  top: 50%;
  transform: translateY(-50%);
  color: var(--au-text-2);
  pointer-events: none;
}
.au-combobox__list {
  position: absolute;
  top: calc(100% + 8px);
  right: 0;
  left: 0;
  z-index: 40;
  max-height: 340px;
  min-width: 260px;
  overflow-y: auto;
  margin: 0;
  padding: 6px;
  list-style: none;
  border: 1px solid var(--au-divider);
  border-radius: var(--au-r-md);
  background-color: color-mix(in srgb, var(--au-paper) 92%, transparent);
  backdrop-filter: blur(14px);
  box-shadow: var(--au-shadow-pop);
}
.au-combobox__list[hidden] {
  display: none;
}
.au-combobox__opt {
  display: flex;
  flex-direction: column;
  gap: 2px;
  padding: 6px 10px;
  border-radius: var(--au-r-xs);
  cursor: pointer;
}
.au-combobox__opt[aria-selected='true'],
.au-combobox__opt:hover {
  background: var(--au-hover);
}
.au-combobox__opt.is-current {
  background: color-mix(in srgb, var(--au-primary) 10%, transparent);
}
.au-combobox__opt-name {
  display: flex;
  align-items: center;
  gap: 8px;
  min-width: 0;
  font-size: 0.875rem;
}
.au-combobox__opt-name > span:first-child {
  flex: 1;
  min-width: 0;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}
.au-combobox__opt--all .au-combobox__opt-name {
  color: var(--au-text-2);
  font-style: italic;
}
.au-combobox__opt-sub {
  font-size: 11px;
  color: var(--au-text-2);
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}
.au-combobox__status {
  padding: 8px 10px;
  color: var(--au-text-2);
  font-size: 0.8125rem;
}
/* active-scope chip in the header */
.au-scope-chip {
  max-width: 260px;
  height: 24px;
  border-color: color-mix(in srgb, var(--au-primary) 70%, transparent);
  color: var(--au-primary);
}
/* section tabs strip (sibling views of the current page) */
.au-section-tabs {
  position: relative;
  z-index: 1;
  padding: 0 16px;
  border-bottom: 1px solid var(--au-divider);
  background: var(--au-glass);
  backdrop-filter: blur(16px) saturate(1.5);
  -webkit-backdrop-filter: blur(16px) saturate(1.5);
}
.au-section-tabs .au-tabs {
  border-bottom: 0;
}
/* one global in-flight signal under the header */
.au-topbar-progress {
  height: 2px;
  overflow: hidden;
  opacity: 0;
  transition: opacity 0.2s ease;
}
.au-topbar-progress.is-active {
  opacity: 1;
}
.au-topbar-progress__bar {
  width: 40%;
  height: 100%;
  background-image: var(--au-gradient);
}
.au-topbar-progress.is-active .au-topbar-progress__bar {
  animation: auIndeterminate 1.1s ease-in-out infinite;
}
/* standing production strip (only if the app can point at production) */
.au-prod-banner {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 6px;
  padding: 3px 16px;
  border-bottom: 1px solid var(--au-error);
  background: color-mix(in srgb, var(--au-error) 14%, var(--au-bg));
  color: var(--au-error);
  font-size: 12px;
  font-weight: 700;
  letter-spacing: 0.04em;
}
.au-prod-banner .au-icon {
  font-size: 15px;
}
/* routed content */
.au-main {
  flex: 1;
  min-width: 0;
  padding: 24px;
}
.au-main--narrow {
  max-width: 1160px;
}

/* --- command palette (Ctrl/⌘+K) ------------------------------------------ */
dialog.au-palette {
  width: calc(100% - 32px);
  max-width: 620px;
  margin-top: 12vh;
  padding: 0;
  border: 1px solid var(--au-divider);
  border-radius: var(--au-r-dialog);
  background: var(--au-paper);
  color: var(--au-text);
  box-shadow: var(--au-shadow-pop);
  overflow: hidden;
}
dialog.au-palette[open] {
  animation: auDialogIn 0.18s var(--au-ease-out);
}
.au-palette__search {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 14px 16px;
  border-bottom: 1px solid var(--au-divider);
}
.au-palette__search .au-icon {
  color: var(--au-text-2);
}
.au-palette__search input {
  flex: 1;
  min-width: 0;
  border: 0;
  outline: 0;
  background: transparent;
  font-size: 1rem;
}
.au-palette__list {
  max-height: 52vh;
  overflow-y: auto;
  margin: 0;
  padding: 6px;
  list-style: none;
}
.au-palette__group {
  padding: 8px 10px 4px;
  color: var(--au-text-3);
  font-size: 11px;
  font-weight: 700;
  letter-spacing: 0.06em;
  text-transform: uppercase;
}
.au-palette__item {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 8px 10px;
  border-radius: var(--au-r-sm);
  color: var(--au-text);
  cursor: pointer;
}
.au-palette__item .au-icon {
  color: var(--au-text-2);
}
.au-palette__item[aria-selected='true'] {
  background: color-mix(in srgb, var(--au-primary) 10%, transparent);
}
.au-palette__item[aria-selected='true'] .au-icon {
  color: var(--au-primary);
}
.au-palette__trail {
  margin-left: auto;
  color: var(--au-text-3);
  font-size: 12px;
  white-space: nowrap;
}
.au-palette__foot {
  display: flex;
  gap: 14px;
  padding: 8px 16px;
  border-top: 1px solid var(--au-divider);
  color: var(--au-text-3);
  font-size: 12px;
}

/* --- responsive: off-canvas sidebar under 900px -------------------------- */
.au-scrim {
  display: none;
}
@media (max-width: 899.98px) {
  .au-sidebar {
    position: fixed;
    left: 0;
    top: 0;
    width: var(--au-sidebar-w);
    transform: translateX(-100%);
  }
  html.au-side-open .au-sidebar {
    transform: none;
    box-shadow: var(--au-shadow-pop);
  }
  html.au-side-open .au-scrim {
    display: block;
    position: fixed;
    inset: 0;
    z-index: calc(var(--au-z-sidebar) - 1);
    background: var(--au-backdrop);
    backdrop-filter: blur(3px);
  }
  .au-header__bar {
    flex-wrap: wrap;
    row-gap: 8px;
    padding: 10px 16px;
  }
  /* with a scope selector, tools drop to their own row and it takes the width */
  .au-header__tools:has(.au-scope) {
    order: 3;
    width: 100%;
    margin-left: 0;
    gap: 8px;
    justify-content: flex-end;
  }
  .au-header__tools .au-scope {
    flex: 1;
    min-width: 0;
  }
  .au-header__tools .au-combobox {
    width: 100%;
  }
  .au-main {
    padding: 16px;
  }
  .au-hide-mobile {
    display: none !important;
  }
}
@media (min-width: 900px) {
  .au-show-mobile {
    display: none !important;
  }
}
@media (max-width: 1199.98px) {
  .au-hide-md {
    display: none !important;
  }
}

/* ==========================================================================
   AURORA — client-portal additions (customer-facing screens).
   ========================================================================== */

/* Optional LIGHT sidebar: <nav class="au-sidebar au-sidebar--light">.
   The sidebar reads only --au-side-* tokens, so a variant is a token swap. */
.au-sidebar--light {
  --au-side-bg: var(--au-paper);
  --au-side-hover: var(--au-hover);
  --au-side-text: var(--au-text-2);
  --au-side-text-strong: var(--au-text);
  --au-side-section: var(--au-text-3);
  --au-side-active-bg: var(--au-gradient-soft);
  --au-side-active-text: var(--au-primary);
  --au-side-active-icon: var(--au-primary);
  --au-side-border: var(--au-divider);
  background-image: var(--au-sheen);
}
.au-sidebar--light .au-brand__mark {
  filter: drop-shadow(0 6px 14px color-mix(in srgb, var(--au-primary) 35%, transparent));
}

/* Page head inside content: title + one-line purpose + primary action */
.au-page-head {
  display: flex;
  flex-wrap: wrap;
  align-items: flex-end;
  justify-content: space-between;
  gap: 12px 16px;
  margin-bottom: 20px;
}
.au-page-head__title {
  margin: 0;
  font-size: 1.3rem;
  font-weight: 800;
  letter-spacing: -0.015em;
}
.au-page-head__sub {
  margin: 2px 0 0;
  color: var(--au-text-2);
  font-size: 0.9375rem;
}
.au-page-head__actions {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
}

/* User menu in the header: <details class="au-dropdown au-user-menu"> */
.au-user-menu > summary {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  padding: 3px 8px 3px 3px;
  border: 1px solid var(--au-divider);
  border-radius: 999px;
  background: var(--au-paper);
  color: var(--au-text);
  font-size: 0.875rem;
  font-weight: 600;
  transition: border-color var(--au-t) ease, box-shadow var(--au-t) ease;
}
.au-user-menu > summary:hover {
  border-color: var(--au-primary-light);
  box-shadow: 0 0 0 3px color-mix(in srgb, var(--au-primary) 12%, transparent);
}
.au-user-menu > summary .au-icon {
  color: var(--au-text-2);
  font-size: 18px;
}
.au-user-menu .au-menu {
  min-width: 230px;
}
.au-user-menu__who {
  display: block;
  padding: 8px 10px 10px;
  border-bottom: 1px solid var(--au-divider);
  margin-bottom: 6px;
}
.au-user-menu__who strong {
  display: block;
  font-size: 0.875rem;
}
.au-user-menu__who span {
  color: var(--au-text-2);
  font-size: 0.75rem;
}

/* Sign-in / sign-up / reset: a lit glass card on the aurora backdrop */
.au-auth {
  position: relative;
  display: grid;
  place-items: center;
  min-height: 100vh;
  padding: 32px 16px;
  overflow: hidden;
}
.au-auth::before {
  content: '';
  position: absolute;
  width: 560px;
  height: 560px;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -62%);
  border-radius: 50%;
  background: radial-gradient(closest-side, color-mix(in srgb, var(--au-primary) 18%, transparent), transparent);
  pointer-events: none;
}
.au-auth__card {
  position: relative;
  width: 100%;
  max-width: 420px;
  padding: 32px;
  border: 1px solid var(--au-divider);
  border-radius: var(--au-r-dialog);
  background-color: var(--au-paper);
  background-image: var(--au-sheen);
  box-shadow: var(--au-shadow-pop);
  animation: auDialogIn 0.3s var(--au-ease-out);
}
.au-auth__brand {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 12px;
  margin-bottom: 24px;
  text-align: center;
}
.au-auth__brand .au-brand__mark {
  filter: drop-shadow(0 10px 24px color-mix(in srgb, var(--au-primary) 45%, transparent));
}
.au-auth__title {
  margin: 0;
  font-size: 1.3rem;
  font-weight: 800;
  letter-spacing: -0.02em;
}
.au-auth__sub {
  margin: 0;
  color: var(--au-text-2);
}
.au-auth__form {
  display: flex;
  flex-direction: column;
  gap: 16px;
}
.au-auth__row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 8px;
  font-size: 0.8125rem;
}
.au-auth__foot {
  margin-top: 20px;
  color: var(--au-text-2);
  font-size: 0.8125rem;
  text-align: center;
}
.au-auth__legal {
  position: relative;
  margin-top: 16px;
  color: var(--au-text-3);
  font-size: 0.75rem;
  text-align: center;
}
@media (max-width: 480px) {
  .au-auth__card {
    padding: 24px 20px;
  }
}

/* Quick-action card (client home): icon tile + title + one line + arrow */
.au-action-card {
  display: flex;
  align-items: flex-start;
  gap: 14px;
  height: 100%;
  padding: 18px;
  color: inherit;
  text-decoration: none;
  background-color: var(--au-paper);
  background-image: var(--au-sheen);
  border: 1px solid var(--au-divider);
  border-radius: var(--au-r-panel);
  box-shadow: var(--au-shadow-paper);
  transition: border-color var(--au-t) ease, transform var(--au-t) ease, box-shadow var(--au-t) ease;
}
.au-action-card:hover {
  border-color: var(--au-primary-light);
  transform: translateY(-2px);
  box-shadow: 0 12px 30px -22px color-mix(in srgb, var(--au-primary) 50%, transparent);
  text-decoration: none;
}
.au-action-card .au-icon-tile {
  width: 40px;
  height: 40px;
  border-radius: var(--au-r-md);
}
.au-action-card .au-icon-tile .au-icon {
  font-size: 22px;
}
.au-action-card__title {
  display: block;
  margin-bottom: 2px;
  font-weight: 700;
}
.au-action-card__text {
  display: block;
  color: var(--au-text-2);
  font-size: 0.8125rem;
}
.au-action-card__arrow {
  margin-left: auto;
  color: var(--au-text-3);
  transition: transform var(--au-t) ease, color var(--au-t) ease;
}
.au-action-card:hover .au-action-card__arrow {
  color: var(--au-primary);
  transform: translateX(3px);
}

/* Stepper for multi-step flows: <ol class="au-steps"><li class="is-done|is-current"> */
.au-steps {
  display: flex;
  gap: 8px;
  margin: 0 0 20px;
  padding: 0;
  list-style: none;
  counter-reset: au-step;
}
.au-steps > li {
  display: flex;
  flex: 1;
  align-items: center;
  gap: 8px;
  min-width: 0;
  color: var(--au-text-2);
  font-size: 0.8125rem;
  font-weight: 600;
  counter-increment: au-step;
}
.au-steps > li::before {
  content: counter(au-step);
  display: grid;
  place-items: center;
  flex-shrink: 0;
  width: 26px;
  height: 26px;
  border: 1px solid var(--au-input-border);
  border-radius: 50%;
  background: var(--au-paper);
  font-size: 12px;
  font-weight: 700;
}
.au-steps > li:not(:last-child)::after {
  content: '';
  flex: 1;
  height: 2px;
  border-radius: 2px;
  background: var(--au-divider);
}
.au-steps > li.is-current {
  color: var(--au-text);
}
.au-steps > li.is-current::before {
  border-color: transparent;
  background-image: var(--au-gradient);
  color: #ffffff;
  box-shadow: 0 2px 10px -2px var(--au-brand-glow);
}
.au-steps > li.is-done::before {
  content: '\2713';
  border-color: transparent;
  background: var(--au-success);
  color: var(--au-on-success);
}
.au-steps > li.is-done::after {
  background: var(--au-success);
}
@media (max-width: 640px) {
  .au-steps > li:not(.is-current) span {
    display: none;
  }
}

/* Responsive table: under 640px every row becomes a card.
   <table class="au-table au-table--stack"> + <td data-label="Status"> */
@media (max-width: 640px) {
  .au-table--stack thead {
    display: none;
  }
  .au-table--stack,
  .au-table--stack tbody,
  .au-table--stack tr,
  .au-table--stack td {
    display: block;
    width: 100%;
  }
  .au-table--stack tbody tr {
    padding: 10px 14px;
    border-bottom: 1px solid var(--au-divider);
  }
  .au-table--stack tbody tr:last-child {
    border-bottom: 0;
  }
  .au-table--stack tbody td {
    display: flex;
    justify-content: space-between;
    gap: 12px;
    height: auto;
    max-width: none;
    padding: 4px 0;
    border: 0;
    white-space: normal;
    text-align: right;
  }
  .au-table--stack tbody td::before {
    content: attr(data-label);
    color: var(--au-text-2);
    font-size: 0.75rem;
    font-weight: 700;
    letter-spacing: 0.05em;
    text-align: left;
    text-transform: uppercase;
  }
  .au-table--stack tbody td:not([data-label])::before {
    content: none;
  }
}
````

### Appendix B — `wwwroot/css/aurora-bootstrap.css` (only if the project uses Bootstrap 5)

<!-- au-file: wwwroot/css/aurora-bootstrap.css -->
````css
/* ==========================================================================
   AURORA — Bootstrap 5 bridge (aurora-bootstrap.css).
   ONLY for a project that already uses Bootstrap. Load it AFTER bootstrap.css
   and aurora.css. It restyles the Bootstrap classes existing pages already use,
   so they take the Aurora look with NO markup changes. Delete sections you don't
   use. Works with 5.1–5.3 (sets both the --bs-* vars and the real properties).
   ========================================================================== */
:root,
[data-bs-theme] {
  --bs-body-font-family: var(--au-font-sans);
  --bs-body-font-size: 0.875rem;
  --bs-body-line-height: 1.5;
  --bs-body-color: var(--au-text);
  --bs-body-bg: var(--au-bg);
  --bs-emphasis-color: var(--au-text);
  --bs-secondary-color: var(--au-text-2);
  --bs-tertiary-color: var(--au-text-3);
  --bs-secondary-bg: var(--au-paper);
  --bs-tertiary-bg: var(--au-table-head-bg);
  --bs-heading-color: inherit;
  --bs-border-color: var(--au-divider);
  --bs-border-color-translucent: var(--au-divider);
  --bs-border-radius: var(--au-r-sm);
  --bs-border-radius-sm: var(--au-r-xs);
  --bs-border-radius-lg: var(--au-r-md);
  --bs-border-radius-xl: var(--au-r-dialog);
  --bs-box-shadow: var(--au-shadow-pop);
  --bs-box-shadow-sm: var(--au-shadow-paper);
  --bs-focus-ring-color: color-mix(in srgb, var(--au-primary) 24%, transparent);
  --bs-code-color: var(--au-text);
  --bs-font-monospace: var(--au-font-mono);
}

/* --- buttons ------------------------------------------------------------- */
.btn {
  --bs-btn-padding-x: 14px;
  --bs-btn-padding-y: 6px;
  --bs-btn-font-size: 0.875rem;
  --bs-btn-font-weight: 650;
  --bs-btn-line-height: 1.5;
  --bs-btn-border-radius: var(--au-r-sm);
  --bs-btn-focus-box-shadow: var(--au-focus-ring);
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  min-height: 36px;
  padding: 6px 14px;
  border-radius: var(--au-r-sm);
  font-size: 0.875rem;
  font-weight: 650;
  line-height: 1.5;
  transition: transform var(--au-t-fast) ease, box-shadow 0.18s ease, background-color 0.18s ease,
    border-color 0.18s ease, filter 0.18s ease, color 0.18s ease;
}
.btn:active {
  transform: scale(0.98);
}
.btn:focus-visible {
  outline: 0;
  box-shadow: var(--au-focus-ring);
}
.btn-sm {
  min-height: 30px;
  padding: 4px 10px;
  font-size: 0.8125rem;
  border-radius: var(--au-r-sm);
}
.btn-lg {
  min-height: 42px;
  padding: 8px 22px;
  font-size: 0.9375rem;
  border-radius: var(--au-r-sm);
}
/* primary = brand gradient CTA */
.btn-primary {
  --bs-btn-color: var(--au-on-primary);
  --bs-btn-bg: var(--au-primary);
  --bs-btn-border-color: transparent;
  --bs-btn-hover-color: var(--au-on-primary);
  --bs-btn-hover-bg: var(--au-primary);
  --bs-btn-hover-border-color: transparent;
  --bs-btn-active-color: var(--au-on-primary);
  --bs-btn-active-bg: var(--au-primary-dark);
  --bs-btn-active-border-color: transparent;
  --bs-btn-disabled-color: var(--au-disabled);
  --bs-btn-disabled-bg: var(--au-disabled-bg);
  --bs-btn-disabled-border-color: transparent;
  color: var(--au-on-primary);
  background-color: var(--au-primary);
  background-image: var(--au-gradient);
  border-color: transparent;
  box-shadow: 0 2px 10px -2px var(--au-brand-glow);
}
.btn.btn-primary:hover,
.btn.btn-primary:focus-visible {
  color: var(--au-on-primary);
  background-color: var(--au-primary);
  border-color: transparent;
  filter: brightness(1.07);
  box-shadow: 0 4px 18px -4px var(--au-brand-glow-strong);
}
.btn.btn-primary:disabled,
.btn.btn-primary.disabled {
  color: var(--au-disabled);
  background-color: var(--au-disabled-bg);
  background-image: none;
  box-shadow: none;
}
/* neutral outlined = secondary actions */
.btn-secondary,
.btn-outline-secondary,
.btn-light,
.btn-outline-light,
.btn-outline-dark,
.btn-default {
  --bs-btn-color: var(--au-text);
  --bs-btn-bg: var(--au-paper);
  --bs-btn-border-color: var(--au-input-border);
  --bs-btn-hover-color: var(--au-text);
  --bs-btn-hover-bg: color-mix(in srgb, var(--au-text) 4%, var(--au-paper));
  --bs-btn-hover-border-color: var(--au-input-border-hover);
  --bs-btn-active-color: var(--au-text);
  --bs-btn-active-bg: var(--au-selected);
  --bs-btn-active-border-color: var(--au-input-border-hover);
  --bs-btn-disabled-color: var(--au-disabled);
  --bs-btn-disabled-bg: transparent;
  --bs-btn-disabled-border-color: var(--au-disabled-bg);
  color: var(--au-text);
  background-color: var(--au-paper);
  border-color: var(--au-input-border);
}
.btn.btn-secondary:hover,
.btn.btn-outline-secondary:hover,
.btn.btn-light:hover,
.btn.btn-outline-light:hover,
.btn.btn-outline-dark:hover,
.btn.btn-default:hover {
  color: var(--au-text);
  background-color: color-mix(in srgb, var(--au-text) 4%, var(--au-paper));
  border-color: var(--au-input-border-hover);
}
.btn-outline-primary {
  --bs-btn-color: var(--au-primary);
  --bs-btn-border-color: color-mix(in srgb, var(--au-primary) 50%, transparent);
  --bs-btn-hover-color: var(--au-primary);
  --bs-btn-hover-bg: color-mix(in srgb, var(--au-primary) 5%, transparent);
  --bs-btn-hover-border-color: var(--au-primary);
  --bs-btn-active-color: var(--au-primary);
  --bs-btn-active-bg: color-mix(in srgb, var(--au-primary) 10%, transparent);
  --bs-btn-active-border-color: var(--au-primary);
  color: var(--au-primary);
  background-color: transparent;
  border-color: color-mix(in srgb, var(--au-primary) 50%, transparent);
}
.btn.btn-outline-primary:hover {
  color: var(--au-primary);
  background-color: color-mix(in srgb, var(--au-primary) 5%, transparent);
  border-color: var(--au-primary);
}
/* semantic contained */
.btn-danger,
.btn-success,
.btn-warning,
.btn-info {
  --bs-btn-border-color: transparent;
  --bs-btn-hover-border-color: transparent;
  --bs-btn-active-border-color: transparent;
  border-color: transparent;
}
.btn-danger {
  --bs-btn-color: var(--au-on-error);
  --bs-btn-bg: var(--au-error);
  --bs-btn-hover-color: var(--au-on-error);
  --bs-btn-hover-bg: var(--au-error-dark);
  --bs-btn-active-color: var(--au-on-error);
  --bs-btn-active-bg: var(--au-error-dark);
  color: var(--au-on-error);
  background-color: var(--au-error);
}
.btn.btn-danger:hover {
  color: var(--au-on-error);
  background-color: var(--au-error-dark);
}
.btn-success {
  --bs-btn-color: var(--au-on-success);
  --bs-btn-bg: var(--au-success);
  --bs-btn-hover-color: var(--au-on-success);
  --bs-btn-hover-bg: var(--au-success-dark);
  --bs-btn-active-bg: var(--au-success-dark);
  color: var(--au-on-success);
  background-color: var(--au-success);
}
.btn.btn-success:hover {
  color: var(--au-on-success);
  background-color: var(--au-success-dark);
}
.btn-warning {
  --bs-btn-color: var(--au-on-warning);
  --bs-btn-bg: var(--au-warning);
  --bs-btn-hover-color: var(--au-on-warning);
  --bs-btn-hover-bg: var(--au-warning-dark);
  --bs-btn-active-bg: var(--au-warning-dark);
  color: var(--au-on-warning);
  background-color: var(--au-warning);
}
.btn.btn-warning:hover {
  color: var(--au-on-warning);
  background-color: var(--au-warning-dark);
}
.btn-info {
  --bs-btn-color: var(--au-on-info);
  --bs-btn-bg: var(--au-info);
  --bs-btn-hover-color: var(--au-on-info);
  --bs-btn-hover-bg: var(--au-info-dark);
  --bs-btn-active-bg: var(--au-info-dark);
  color: var(--au-on-info);
  background-color: var(--au-info);
}
.btn.btn-info:hover {
  color: var(--au-on-info);
  background-color: var(--au-info-dark);
}
.btn-outline-danger {
  --bs-btn-color: var(--au-error);
  --bs-btn-border-color: color-mix(in srgb, var(--au-error) 50%, transparent);
  --bs-btn-hover-color: var(--au-error);
  --bs-btn-hover-bg: color-mix(in srgb, var(--au-error) 5%, transparent);
  --bs-btn-hover-border-color: var(--au-error);
  --bs-btn-active-color: var(--au-error);
  --bs-btn-active-bg: color-mix(in srgb, var(--au-error) 10%, transparent);
  color: var(--au-error);
  background-color: transparent;
  border-color: color-mix(in srgb, var(--au-error) 50%, transparent);
}
.btn.btn-outline-danger:hover {
  color: var(--au-error);
  background-color: color-mix(in srgb, var(--au-error) 5%, transparent);
  border-color: var(--au-error);
}
.btn-outline-success,
.btn-outline-warning,
.btn-outline-info {
  background-color: transparent;
}
.btn-outline-success {
  --bs-btn-color: var(--au-success);
  --bs-btn-border-color: color-mix(in srgb, var(--au-success) 50%, transparent);
  --bs-btn-hover-color: var(--au-success);
  --bs-btn-hover-bg: color-mix(in srgb, var(--au-success) 6%, transparent);
  --bs-btn-hover-border-color: var(--au-success);
  color: var(--au-success);
  border-color: color-mix(in srgb, var(--au-success) 50%, transparent);
}
.btn-outline-warning {
  --bs-btn-color: var(--au-warning);
  --bs-btn-border-color: color-mix(in srgb, var(--au-warning) 50%, transparent);
  --bs-btn-hover-color: var(--au-warning);
  --bs-btn-hover-bg: color-mix(in srgb, var(--au-warning) 6%, transparent);
  --bs-btn-hover-border-color: var(--au-warning);
  color: var(--au-warning);
  border-color: color-mix(in srgb, var(--au-warning) 50%, transparent);
}
.btn-outline-info {
  --bs-btn-color: var(--au-info);
  --bs-btn-border-color: color-mix(in srgb, var(--au-info) 50%, transparent);
  --bs-btn-hover-color: var(--au-info);
  --bs-btn-hover-bg: color-mix(in srgb, var(--au-info) 6%, transparent);
  --bs-btn-hover-border-color: var(--au-info);
  color: var(--au-info);
  border-color: color-mix(in srgb, var(--au-info) 50%, transparent);
}
.btn-link {
  --bs-btn-color: var(--au-primary);
  --bs-btn-hover-color: var(--au-primary-dark);
  color: var(--au-primary);
  text-decoration: none;
}
.btn.btn-link:hover {
  color: var(--au-primary);
  background-color: color-mix(in srgb, var(--au-primary) 6%, transparent);
  text-decoration: none;
}
:root[data-theme='dark'] .btn-close {
  filter: invert(1) grayscale(100%) brightness(200%);
}

/* --- forms --------------------------------------------------------------- */
.form-control,
.form-select {
  min-height: 40px;
  padding: 7.5px 14px;
  color: var(--au-text);
  background-color: var(--au-input-bg);
  border: 1px solid var(--au-input-border);
  border-radius: var(--au-r-sm);
  font-size: 1rem;
  line-height: 1.5;
  transition: border-color var(--au-t) ease, box-shadow var(--au-t) ease;
}
.form-select {
  padding-right: 36px;
  background-image: var(--au-select-chevron);
  background-repeat: no-repeat;
  background-position: right 8px center;
  background-size: 22px;
}
.form-select[multiple],
.form-select[size]:not([size='1']) {
  background-image: none;
  padding-right: 14px;
}
.form-control::placeholder {
  color: color-mix(in srgb, var(--au-text) 42%, transparent);
  opacity: 1;
}
.form-control:hover,
.form-select:hover {
  border-color: var(--au-input-border-hover);
}
.form-control:focus,
.form-select:focus {
  color: var(--au-text);
  background-color: var(--au-input-bg);
  border-color: var(--au-primary);
  box-shadow: var(--au-focus-ring);
  outline: 0;
}
.form-control:disabled,
.form-select:disabled,
.form-control[readonly] {
  color: var(--au-text-2);
  background-color: var(--au-hover);
}
.form-control-sm,
.form-select-sm {
  min-height: 34px;
  padding-top: 4px;
  padding-bottom: 4px;
  font-size: 0.875rem;
  border-radius: var(--au-r-sm);
}
textarea.form-control {
  min-height: 96px;
}
.form-label,
.col-form-label,
.control-label {
  margin-bottom: 6px;
  color: var(--au-text);
  font-size: 0.8125rem;
  font-weight: 600;
}
.form-text,
.help-block {
  color: var(--au-text-2);
  font-size: 0.75rem;
}
.form-check-input {
  border-color: var(--au-input-border);
  background-color: var(--au-input-bg);
}
.form-check-input:checked {
  background-color: var(--au-primary);
  border-color: var(--au-primary);
}
.form-check-input:focus {
  border-color: var(--au-primary);
  box-shadow: var(--au-focus-ring);
}
.form-check-label {
  font-size: 0.875rem;
}
.input-group-text {
  color: var(--au-text-2);
  background-color: var(--au-table-head-bg);
  border-color: var(--au-input-border);
  border-radius: var(--au-r-sm);
}
.form-floating > label {
  color: var(--au-text-2);
}
.form-floating > .form-control:focus ~ label::after,
.form-floating > .form-control:not(:placeholder-shown) ~ label::after,
.form-floating > .form-select ~ label::after {
  background-color: transparent !important;
}
.is-invalid,
.form-control.is-invalid,
.form-select.is-invalid {
  border-color: var(--au-error) !important;
  background-image: none;
}
.is-invalid:focus {
  box-shadow: var(--au-focus-ring-error) !important;
}
.is-valid,
.form-control.is-valid {
  background-image: none;
}
.invalid-feedback,
.text-danger.field-validation-error {
  color: var(--au-error);
  font-size: 0.75rem;
  font-weight: 500;
}
/* ASP.NET validation summary (<div asp-validation-summary> + .text-danger) */
.validation-summary-errors {
  margin: 8px 0 16px;
  padding: 10px 16px;
  border-radius: var(--au-r-md);
  background-color: color-mix(in srgb, var(--au-error) 10%, var(--au-mix-bg));
  color: color-mix(in srgb, var(--au-error) 40%, var(--au-mix-fg)) !important;
  font-weight: 500;
}

/* --- cards ---------------------------------------------------------------- */
.card {
  --bs-card-bg: var(--au-paper);
  --bs-card-color: var(--au-text);
  --bs-card-border-color: var(--au-divider);
  --bs-card-border-radius: var(--au-r-card);
  --bs-card-inner-border-radius: calc(var(--au-r-card) - 1px);
  --bs-card-cap-bg: transparent;
  --bs-card-spacer-x: 20px;
  --bs-card-spacer-y: 20px;
  color: var(--au-text);
  background-color: var(--au-paper);
  background-image: var(--au-sheen);
  border: 1px solid var(--au-divider);
  border-radius: var(--au-r-card);
  box-shadow: var(--au-shadow-paper);
}
.card-header,
.card-footer {
  padding: 14px 20px;
  background-color: transparent;
  border-color: var(--au-divider);
}
.card-header {
  font-size: 0.875rem;
  font-weight: 700;
}
.card-body {
  padding: 20px;
}
.card-title {
  font-size: 0.875rem;
  font-weight: 700;
}
.card-subtitle,
.card-text {
  color: var(--au-text-2);
}

/* --- tables ---------------------------------------------------------------
   Wrap a page's <table class="table"> in <div class="au-table-card"> (markup
   only) to get the card + rounded clip; the rules below do the rest. */
.table {
  --bs-table-bg: transparent;
  --bs-table-color: var(--au-text);
  --bs-table-border-color: var(--au-divider);
  --bs-table-striped-bg: color-mix(in srgb, var(--au-text) 2.5%, transparent);
  --bs-table-striped-color: var(--au-text);
  --bs-table-hover-bg: color-mix(in srgb, var(--au-primary) 5%, transparent);
  --bs-table-hover-color: var(--au-text);
  --bs-table-active-bg: color-mix(in srgb, var(--au-primary) 7%, transparent);
  --bs-table-active-color: var(--au-text);
  margin-bottom: 0;
  color: var(--au-text);
  border-color: var(--au-divider);
  font-size: 0.875rem;
  font-variant-numeric: tabular-nums;
  vertical-align: middle;
}
.table > :not(caption) > * > * {
  height: 44px;
  padding: 6px 10px;
  color: var(--au-text);
  border-bottom-color: var(--au-divider);
}
.table > thead > tr > th,
.table > thead > tr > td {
  position: sticky;
  top: 0;
  z-index: 2;
  background-color: var(--au-table-head-bg);
  color: var(--au-text-2);
  font-size: 11px;
  font-weight: 700;
  letter-spacing: 0.05em;
  text-transform: uppercase;
  white-space: nowrap;
}
.table > thead > tr > th a {
  color: inherit;
  text-decoration: none;
}
.table > tbody > tr:last-child > * {
  border-bottom-width: 0;
}
.table-light,
.table-dark,
.thead-light,
.thead-dark,
.table > thead.table-light > tr > th,
.table > thead.table-dark > tr > th {
  --bs-table-bg: var(--au-table-head-bg);
  --bs-table-color: var(--au-text-2);
  background-color: var(--au-table-head-bg);
  color: var(--au-text-2);
}
.table-bordered > :not(caption) > * > * {
  border-color: var(--au-divider);
}
.table-responsive {
  border-radius: inherit;
}
.au-table-card > .table,
.au-table-card .table-responsive > .table {
  margin: 0;
}

/* --- badges -> chips --------------------------------------------------------- */
.badge {
  display: inline-flex;
  align-items: center;
  height: 22px;
  padding: 0 8px;
  border-radius: var(--au-r-xs);
  font-size: 0.8125rem;
  font-weight: 600;
  line-height: 1;
}
.badge.rounded-pill {
  border-radius: 999px;
}
.badge.bg-primary,
.badge.text-bg-primary {
  color: var(--au-on-primary) !important;
  background-color: var(--au-primary) !important;
}
.badge.bg-success,
.badge.text-bg-success {
  color: var(--au-on-success) !important;
  background-color: var(--au-success) !important;
}
.badge.bg-warning,
.badge.text-bg-warning {
  color: var(--au-on-warning) !important;
  background-color: var(--au-warning) !important;
}
.badge.bg-danger,
.badge.text-bg-danger {
  color: var(--au-on-error) !important;
  background-color: var(--au-error) !important;
}
.badge.bg-info,
.badge.text-bg-info {
  color: var(--au-on-info) !important;
  background-color: var(--au-info) !important;
}
.badge.bg-secondary,
.badge.text-bg-secondary,
.badge.bg-dark,
.badge.text-bg-dark {
  color: var(--au-text) !important;
  background-color: var(--au-selected) !important;
}
.badge.bg-light,
.badge.text-bg-light {
  color: var(--au-text) !important;
  background-color: transparent !important;
  border: 1px solid var(--au-input-border);
}

/* --- alerts ---------------------------------------------------------------- */
.alert {
  --c: var(--au-info);
  padding: 14px 16px;
  border: 0;
  border-radius: var(--au-r-md);
  background-color: color-mix(in srgb, var(--c) 10%, var(--au-mix-bg));
  color: color-mix(in srgb, var(--c) 40%, var(--au-mix-fg));
  font-weight: 500;
}
.alert-primary {
  --c: var(--au-primary);
}
.alert-success {
  --c: var(--au-success);
}
.alert-warning {
  --c: var(--au-warning);
}
.alert-danger {
  --c: var(--au-error);
}
.alert-secondary,
.alert-light,
.alert-dark {
  --c: var(--au-text-2);
}
.alert .alert-link {
  color: inherit;
  font-weight: 700;
}
.alert-heading {
  color: inherit;
  font-weight: 700;
}

/* --- modals ---------------------------------------------------------------- */
.modal-content {
  color: var(--au-text);
  background-color: var(--au-paper);
  border: 1px solid var(--au-divider);
  border-radius: var(--au-r-dialog);
  box-shadow: var(--au-shadow-pop);
}
.modal-header {
  padding: 16px 24px;
  border-bottom: 0;
}
.modal-title {
  font-size: 1.02rem;
  font-weight: 750;
  letter-spacing: -0.01em;
}
.modal-body {
  padding: 0 24px 20px;
}
.modal-footer {
  gap: 8px;
  padding: 8px 24px 16px;
  border-top: 0;
}
.modal-footer > * {
  margin: 0;
}
.modal-backdrop {
  background-color: var(--au-backdrop);
  backdrop-filter: blur(5px);
}
.modal-backdrop.show {
  opacity: 1;
}

/* --- dropdowns ------------------------------------------------------------- */
.dropdown-menu {
  --bs-dropdown-bg: var(--au-paper);
  --bs-dropdown-color: var(--au-text);
  --bs-dropdown-border-color: var(--au-divider);
  --bs-dropdown-link-color: var(--au-text);
  --bs-dropdown-link-hover-color: var(--au-text);
  --bs-dropdown-link-hover-bg: var(--au-hover);
  --bs-dropdown-link-active-color: var(--au-text);
  --bs-dropdown-link-active-bg: color-mix(in srgb, var(--au-primary) 10%, transparent);
  padding: 6px;
  color: var(--au-text);
  background-color: color-mix(in srgb, var(--au-paper) 92%, transparent);
  backdrop-filter: blur(14px);
  border: 1px solid var(--au-divider);
  border-radius: var(--au-r-md);
  box-shadow: var(--au-shadow-pop);
  font-size: 0.875rem;
}
.dropdown-item {
  padding: 6px 10px;
  color: var(--au-text);
  border-radius: var(--au-r-xs);
}
.dropdown-item:hover,
.dropdown-item:focus {
  color: var(--au-text);
  background-color: var(--au-hover);
}
.dropdown-item.active,
.dropdown-item:active {
  color: var(--au-text);
  background-color: color-mix(in srgb, var(--au-primary) 10%, transparent);
}
.dropdown-divider {
  border-color: var(--au-divider);
}
.dropdown-header {
  color: var(--au-text-3);
  font-size: 0.75rem;
  font-weight: 700;
  letter-spacing: 0.04em;
  text-transform: uppercase;
}

/* --- nav tabs / pills ------------------------------------------------------ */
.nav-tabs {
  gap: 0;
  border-bottom: 1px solid var(--au-divider);
}
.nav-tabs .nav-link {
  position: relative;
  min-height: 44px;
  margin-bottom: 0;
  padding: 8px 16px;
  color: var(--au-text-2);
  background: none;
  border: 0;
  border-radius: 0;
  font-weight: 650;
}
.nav-tabs .nav-link:hover,
.nav-tabs .nav-link:focus {
  color: var(--au-text);
  border: 0;
  isolation: auto;
}
.nav-tabs .nav-link.active,
.nav-tabs .nav-item.show .nav-link {
  color: var(--au-primary);
  background: none;
  border: 0;
}
.nav-tabs .nav-link.active::after {
  content: '';
  position: absolute;
  left: 0;
  right: 0;
  bottom: 0;
  height: 3px;
  border-radius: 3px;
  background-image: var(--au-gradient);
}
.nav-pills {
  display: inline-flex;
  gap: 2px;
  padding: 3px;
  border-radius: var(--au-r-sm);
  background: var(--au-segment-bg);
}
.nav-pills .nav-link {
  padding: 4px 11px;
  color: var(--au-text-2);
  border-radius: var(--au-r-xs);
  font-size: 0.8125rem;
  font-weight: 600;
}
.nav-pills .nav-link.active,
.nav-pills .show > .nav-link {
  color: var(--au-primary);
  background: var(--au-segment-selected);
  box-shadow: var(--au-segment-shadow);
}

/* --- pagination ------------------------------------------------------------ */
.pagination {
  gap: 6px;
  margin: 0;
}
.page-link {
  display: grid;
  place-items: center;
  min-width: 34px;
  height: 34px;
  padding: 0 8px;
  color: var(--au-text);
  background-color: transparent;
  border: 1px solid var(--au-divider);
  border-radius: var(--au-r-sm) !important;
  font-size: 0.875rem;
  font-weight: 600;
  font-variant-numeric: tabular-nums;
}
.page-link:hover {
  color: var(--au-text);
  background-color: var(--au-hover);
  border-color: var(--au-divider);
}
.page-link:focus {
  box-shadow: var(--au-focus-ring);
}
.page-item.active .page-link,
.active > .page-link {
  color: var(--au-primary);
  background-color: color-mix(in srgb, var(--au-primary) 10%, transparent);
  border-color: color-mix(in srgb, var(--au-primary) 40%, transparent);
}
.page-item.disabled .page-link,
.disabled > .page-link {
  color: var(--au-disabled);
  background-color: transparent;
  border-color: var(--au-divider);
}

/* --- breadcrumb, list group, accordion, misc ------------------------------- */
.breadcrumb {
  margin: 0;
  font-size: 12px;
}
.breadcrumb-item a {
  color: var(--au-text-2);
  text-decoration: none;
}
.breadcrumb-item a:hover {
  color: var(--au-text);
}
.breadcrumb-item.active {
  color: var(--au-text);
}
.breadcrumb-item + .breadcrumb-item::before {
  content: '\203A';
  color: var(--au-text-3);
}
.list-group-item {
  color: var(--au-text);
  background-color: var(--au-paper);
  border-color: var(--au-divider);
}
.list-group-item-action:hover,
.list-group-item-action:focus {
  color: var(--au-text);
  background-color: var(--au-hover);
}
.list-group-item.active {
  color: var(--au-primary);
  background-color: color-mix(in srgb, var(--au-primary) 10%, var(--au-paper));
  border-color: var(--au-divider);
}
.accordion-item {
  color: var(--au-text);
  background-color: var(--au-paper);
  border: 1px solid var(--au-divider);
}
.accordion-item:first-of-type {
  border-top-left-radius: var(--au-r-md);
  border-top-right-radius: var(--au-r-md);
}
.accordion-item:last-of-type {
  border-bottom-left-radius: var(--au-r-md);
  border-bottom-right-radius: var(--au-r-md);
}
.accordion-button {
  color: var(--au-text);
  background-color: var(--au-paper);
  font-weight: 650;
}
.accordion-button:not(.collapsed) {
  color: var(--au-primary);
  background-image: var(--au-gradient-soft);
  box-shadow: none;
}
.accordion-button:focus {
  box-shadow: var(--au-focus-ring);
}
.progress {
  height: 6px;
  border-radius: 999px;
  background-color: color-mix(in srgb, var(--au-primary) 12%, transparent);
}
.progress-bar {
  background-color: var(--au-primary);
  background-image: var(--au-gradient);
  border-radius: 999px;
}
.tooltip {
  --bs-tooltip-bg: var(--au-tooltip-bg);
  --bs-tooltip-color: #ffffff;
  --bs-tooltip-font-size: 12px;
  --bs-tooltip-border-radius: var(--au-r-xs);
  --bs-tooltip-padding-x: 10px;
  --bs-tooltip-padding-y: 6px;
}
.toast {
  color: var(--au-text);
  background-color: var(--au-paper);
  border: 1px solid var(--au-divider);
  border-radius: var(--au-r-md);
  box-shadow: var(--au-shadow-pop);
}
.offcanvas {
  color: var(--au-text);
  background-color: var(--au-paper);
  border-color: var(--au-divider);
  box-shadow: var(--au-shadow-pop);
}
.spinner-border,
.spinner-grow {
  color: var(--au-primary);
}

/* --- utility re-mapping (so dark mode stays correct) ----------------------- */
.text-muted,
.text-secondary,
.text-body-secondary {
  color: var(--au-text-2) !important;
}
.text-primary {
  color: var(--au-primary) !important;
}
.text-success {
  color: var(--au-success) !important;
}
.text-warning {
  color: var(--au-warning) !important;
}
.text-danger {
  color: var(--au-error) !important;
}
.text-info {
  color: var(--au-info) !important;
}
.text-dark,
.text-body,
.text-black {
  color: var(--au-text) !important;
}
.bg-white,
.bg-light,
.bg-body,
.bg-body-tertiary {
  background-color: var(--au-paper) !important;
  color: var(--au-text);
}
.border,
.border-top,
.border-bottom,
.border-start,
.border-end {
  border-color: var(--au-divider) !important;
}
.shadow-sm {
  box-shadow: var(--au-shadow-paper) !important;
}
.shadow,
.shadow-lg {
  box-shadow: var(--au-shadow-pop) !important;
}
.rounded {
  border-radius: var(--au-r-sm) !important;
}
hr {
  color: var(--au-divider);
  opacity: 1;
}
````

### Appendix C — `wwwroot/js/aurora.js`

<!-- au-file: wwwroot/js/aurora.js -->
````js
/* ==========================================================================
   AURORA — UI behaviours. Vanilla JS, no dependencies, no build step.
   Every feature is OPT-IN by markup (data-au-* attributes); a page that does
   not use a feature is untouched. Never put business logic here.
   Public API: window.Aurora.{theme, toast, confirm, copy, progress, dialog}
   ========================================================================== */
(function () {
  'use strict';

  var root = document.documentElement;
  var APP = root.getAttribute('data-au-app') || 'aurora';
  var KEY = {
    theme: APP + '.themeMode',
    collapsed: APP + '.sidebarCollapsed',
    favs: APP + '.nav.favorites',
    recent: APP + '.nav.recent',
  };
  var reduceMotion = !!(window.matchMedia && window.matchMedia('(prefers-reduced-motion: reduce)').matches);
  var desktop = window.matchMedia ? window.matchMedia('(min-width: 900px)') : { matches: true };

  // --- tiny helpers --------------------------------------------------------
  function $(sel, ctx) {
    return (ctx || document).querySelector(sel);
  }
  function $$(sel, ctx) {
    return Array.prototype.slice.call((ctx || document).querySelectorAll(sel));
  }
  function store(key, value) {
    try {
      if (value === undefined) return localStorage.getItem(key);
      if (value === null) localStorage.removeItem(key);
      else localStorage.setItem(key, value);
    } catch (e) {
      /* private mode / blocked storage: degrade silently */
    }
    return null;
  }
  function readJson(key, fallback) {
    try {
      var raw = store(key);
      return raw ? JSON.parse(raw) : fallback;
    } catch (e) {
      return fallback;
    }
  }
  function esc(s) {
    return String(s == null ? '' : s).replace(/[&<>"']/g, function (c) {
      return { '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c];
    });
  }
  function icon(name, cls) {
    return '<span class="au-icon' + (cls ? ' ' + cls : '') + '" aria-hidden="true">' + esc(name) + '</span>';
  }
  function isTyping(el) {
    return !!el && (el.isContentEditable || /^(INPUT|TEXTAREA|SELECT)$/.test(el.tagName));
  }
  function debounce(fn, ms) {
    var t;
    return function () {
      var args = arguments,
        self = this;
      clearTimeout(t);
      t = setTimeout(function () {
        fn.apply(self, args);
      }, ms);
    };
  }

  // A click whose target is the <dialog> itself AND lands outside its box is a
  // backdrop click (a click on empty space inside the dialog also targets it).
  function isBackdropClick(e, dlg) {
    if (e.target !== dlg) return false;
    var r = dlg.getBoundingClientRect();
    return e.clientX < r.left || e.clientX > r.right || e.clientY < r.top || e.clientY > r.bottom;
  }

  // --- theme (light / dark) -------------------------------------------------
  var theme = {
    get: function () {
      return root.getAttribute('data-theme') === 'dark' ? 'dark' : 'light';
    },
    set: function (mode) {
      root.setAttribute('data-theme', mode);
      root.setAttribute('data-bs-theme', mode); // Bootstrap 5.3+ follows along
      store(KEY.theme, mode);
      $$('[data-au-theme-toggle]').forEach(syncThemeToggle);
      document.dispatchEvent(new CustomEvent('au:themechange', { detail: { mode: mode } }));
    },
    toggle: function () {
      theme.set(theme.get() === 'dark' ? 'light' : 'dark');
    },
  };
  function syncThemeToggle(btn) {
    var dark = theme.get() === 'dark';
    var label = dark ? 'Switch to light mode' : 'Switch to dark mode';
    btn.setAttribute('aria-label', label);
    if (btn.hasAttribute('data-au-tip')) btn.setAttribute('data-au-tip', label);
    var glyph = btn.querySelector('.au-icon');
    if (glyph) {
      glyph.textContent = dark ? 'light_mode' : 'dark_mode';
      glyph.classList.remove('au-animate-in');
      void glyph.offsetWidth; // restart the animation
      glyph.classList.add('au-animate-in');
    }
  }

  // --- top progress bar (navigation / submit in flight) ---------------------
  var progress = {
    el: null,
    start: function () {
      if (progress.el) progress.el.classList.add('is-active');
    },
    stop: function () {
      if (progress.el) progress.el.classList.remove('is-active');
    },
  };

  // --- toasts ----------------------------------------------------------------
  var TOAST_MS = 4500;
  var TOAST_MAX = 4;
  var TOAST_ICON = { success: 'check_circle', error: 'error', warning: 'warning', info: 'info' };
  function toastHost() {
    var host = $('.au-toasts');
    if (!host) {
      host = document.createElement('div');
      host.className = 'au-toasts';
      host.setAttribute('aria-live', 'polite');
      document.body.appendChild(host);
    }
    return host;
  }
  function showToast(severity, message) {
    if (!message) return;
    var sev = TOAST_ICON[severity] ? severity : 'info';
    var host = toastHost();
    while (host.children.length >= TOAST_MAX) host.removeChild(host.firstElementChild);
    var text = String(message);
    if (text.length > 300) text = text.slice(0, 297) + '…'; // the full error lives in the page panel
    var el = document.createElement('div');
    el.className = 'au-toast au-toast--' + sev;
    el.setAttribute('role', sev === 'error' ? 'alert' : 'status');
    el.innerHTML =
      icon(TOAST_ICON[sev]) +
      '<div class="au-toast__msg">' +
      esc(text) +
      '</div>' +
      '<button type="button" class="au-icon-btn au-icon-btn--sm" aria-label="Dismiss">' +
      icon('close') +
      '</button>';
    var dismiss = function () {
      if (!el.parentNode) return;
      el.classList.add('is-leaving');
      setTimeout(function () {
        if (el.parentNode) el.parentNode.removeChild(el);
      }, 200);
    };
    el.querySelector('button').addEventListener('click', dismiss);
    host.appendChild(el);
    setTimeout(dismiss, TOAST_MS);
  }
  var toast = {
    success: function (m) {
      showToast('success', m);
    },
    error: function (m) {
      showToast('error', m);
    },
    warning: function (m) {
      showToast('warning', m);
    },
    info: function (m) {
      showToast('info', m);
    },
  };

  // --- clipboard (falls back to execCommand on plain-http origins) ----------
  function copy(text, container) {
    if (navigator.clipboard && window.isSecureContext) return navigator.clipboard.writeText(text);
    return new Promise(function (resolve, reject) {
      var host = container || document.body; // inside a <dialog>, pass the dialog
      var ta = document.createElement('textarea');
      ta.value = text;
      ta.setAttribute('readonly', '');
      ta.style.position = 'fixed';
      ta.style.opacity = '0';
      host.appendChild(ta);
      ta.select();
      var ok = false;
      try {
        ok = document.execCommand('copy');
      } catch (e) {
        ok = false;
      }
      host.removeChild(ta);
      if (ok) resolve();
      else reject(new Error('copy failed'));
    });
  }

  // --- dialogs / confirm ----------------------------------------------------
  var dialog = {
    open: function (target) {
      var d = typeof target === 'string' ? $(target) : target;
      if (d && typeof d.showModal === 'function' && !d.open) d.showModal();
      return d;
    },
    close: function (target) {
      var d = typeof target === 'string' ? $(target) : target;
      if (d && d.open) d.close();
    },
  };
  var confirmEl = null;
  function confirmDialog(opts) {
    opts = opts || {};
    if (!confirmEl) {
      confirmEl = document.createElement('dialog');
      confirmEl.className = 'au-dialog';
      confirmEl.setAttribute('aria-labelledby', 'au-confirm-title');
      confirmEl.innerHTML =
        '<h2 class="au-dialog__title" id="au-confirm-title"></h2>' +
        '<div class="au-dialog__content"><div class="au-dialog__text"></div></div>' +
        '<div class="au-dialog__actions">' +
        '<button type="button" class="au-btn" data-au-cancel>Cancel</button>' +
        '<button type="button" class="au-btn" data-au-ok>Confirm</button>' +
        '</div>';
      document.body.appendChild(confirmEl);
      confirmEl.addEventListener('click', function (e) {
        if (isBackdropClick(e, confirmEl)) confirmEl.close('cancel');
      });
    }
    var tone = opts.tone || 'primary';
    confirmEl.querySelector('.au-dialog__title').textContent = opts.title || 'Are you sure?';
    var text = confirmEl.querySelector('.au-dialog__text');
    if (opts.html) text.innerHTML = opts.html;
    else text.textContent = opts.message || '';
    var ok = confirmEl.querySelector('[data-au-ok]');
    ok.className = 'au-btn au-btn--' + tone;
    ok.textContent = opts.confirmLabel || 'Confirm';
    confirmEl.querySelector('[data-au-cancel]').textContent = opts.cancelLabel || 'Cancel';
    return new Promise(function (resolve) {
      function done(result) {
        ok.removeEventListener('click', onOk);
        cancel.removeEventListener('click', onCancel);
        confirmEl.removeEventListener('close', onClose);
        resolve(result);
      }
      var cancel = confirmEl.querySelector('[data-au-cancel]');
      function onOk() {
        confirmEl.close('ok');
      }
      function onCancel() {
        confirmEl.close('cancel');
      }
      function onClose() {
        done(confirmEl.returnValue === 'ok');
      }
      ok.addEventListener('click', onOk);
      cancel.addEventListener('click', onCancel);
      confirmEl.addEventListener('close', onClose);
      confirmEl.returnValue = '';
      confirmEl.showModal();
      (tone === 'danger' ? cancel : ok).focus(); // destructive: default focus is Cancel
    });
  }
  function confirmOptions(el, fallback) {
    var src = el && el.hasAttribute('data-au-confirm') ? el : fallback;
    return {
      message: src.getAttribute('data-au-confirm'),
      title: src.getAttribute('data-au-confirm-title') || undefined,
      confirmLabel: src.getAttribute('data-au-confirm-label') || undefined,
      tone: src.getAttribute('data-au-confirm-tone') || undefined,
    };
  }

  // --- sidebar: collapse rail, mobile drawer, filter, favourites, recent ----
  function initSidebar() {
    var sidebar = $('.au-sidebar');
    $$('[data-au-sidebar-toggle]').forEach(function (btn) {
      btn.addEventListener('click', function () {
        if (desktop.matches) {
          var next = !root.classList.contains('au-side-collapsed');
          root.classList.toggle('au-side-collapsed', next);
          store(KEY.collapsed, next ? '1' : '0');
        } else {
          root.classList.toggle('au-side-open');
        }
      });
    });
    var scrim = $('.au-scrim');
    if (scrim)
      scrim.addEventListener('click', function () {
        root.classList.remove('au-side-open');
      });
    if (!sidebar) return;

    // Collapsed rail: a group click expands the rail instead of toggling.
    sidebar.addEventListener('click', function (e) {
      var summary = e.target.closest('.au-nav-group > summary');
      if (summary && desktop.matches && root.classList.contains('au-side-collapsed')) {
        e.preventDefault();
        root.classList.remove('au-side-collapsed');
        store(KEY.collapsed, '0');
        summary.parentElement.open = true;
      }
    });

    initNavFilter(sidebar);
    initFavorites(sidebar);
    recordVisit(sidebar);
  }

  function initNavFilter(sidebar) {
    var box = $('[data-au-nav-filter]', sidebar);
    if (!box) return;
    var input = box.querySelector('input');
    var clear = box.querySelector('button');
    var empty = $('.au-nav-empty', sidebar);
    var tree = $('[data-au-nav-tree]', sidebar) || sidebar;
    var filtering = false;
    function apply() {
      var raw = input.value.trim();
      var q = raw.toLowerCase();
      var now = q.length > 0;
      if (now !== filtering) {
        // a closed <details> hides its children no matter what CSS says,
        // so open every group while filtering and restore afterwards
        $$('details.au-nav-group', tree).forEach(function (d) {
          if (now) {
            d.setAttribute('data-au-was-open', d.open ? '1' : '0');
            d.open = true;
          } else {
            d.open = d.getAttribute('data-au-was-open') === '1';
            d.removeAttribute('data-au-was-open');
          }
        });
        filtering = now;
      }
      box.classList.toggle('has-value', now);
      sidebar.classList.toggle('is-filtering', now);
      var shown = 0;
      $$('.au-nav-row', tree).forEach(function (row) {
        var link = row.querySelector('.au-nav-item');
        var hay = ((link.getAttribute('data-trail') || '') + ' ' + link.textContent).toLowerCase();
        var match = !now || hay.indexOf(q) !== -1;
        row.classList.toggle('is-filtered-out', !match);
        if (now && match) shown++;
      });
      if (empty) {
        empty.hidden = !(now && shown === 0);
        empty.textContent = 'No pages match “' + raw + '”.';
      }
    }
    input.addEventListener('input', apply);
    input.addEventListener('keydown', function (e) {
      if (e.key === 'Escape') {
        input.value = '';
        apply();
      }
    });
    if (clear)
      clear.addEventListener('click', function () {
        input.value = '';
        apply();
        input.focus();
      });
  }

  function initFavorites(sidebar) {
    var host = $('[data-au-favorites]', sidebar);
    var tree = $('[data-au-nav-tree]', sidebar) || sidebar;
    function render() {
      var favs = readJson(KEY.favs, []);
      var byPath = {};
      // skip the filter-only child rows: they share their group row's href
      $$('.au-nav-row:not(.au-nav-row--search-only) > .au-nav-item', tree).forEach(function (a) {
        byPath[a.getAttribute('href')] = a;
      });
      $$('.au-nav-star', sidebar).forEach(function (b) {
        var on = favs.some(function (f) {
          return f.path === b.getAttribute('data-path');
        });
        b.classList.toggle('is-on', on);
        b.setAttribute('aria-pressed', on ? 'true' : 'false');
        var label = b.getAttribute('data-label') || '';
        b.setAttribute('aria-label', (on ? 'Unpin ' : 'Pin ') + label);
        b.title = on ? 'Unpin from favorites' : 'Pin to favorites';
      });
      if (!host) return;
      var rows = favs
        .filter(function (f) {
          return byPath[f.path];
        })
        .map(function (f) {
          var row = byPath[f.path].closest('.au-nav-row').cloneNode(true);
          row.classList.remove('is-filtered-out');
          var star = row.querySelector('.au-nav-star');
          if (star) star.classList.add('is-on');
          return row;
        });
      host.innerHTML = '';
      if (!rows.length) return;
      var label = document.createElement('div');
      label.className = 'au-side-section';
      label.innerHTML = icon('star', 'au-icon--fill') + 'Favorites';
      host.appendChild(label);
      rows.forEach(function (r) {
        host.appendChild(r);
      });
    }
    sidebar.addEventListener('click', function (e) {
      var star = e.target.closest('.au-nav-star');
      if (!star) return;
      e.preventDefault();
      e.stopPropagation();
      var path = star.getAttribute('data-path');
      var favs = readJson(KEY.favs, []);
      var exists = favs.some(function (f) {
        return f.path === path;
      });
      favs = exists
        ? favs.filter(function (f) {
            return f.path !== path;
          })
        : favs.concat([{ path: path, label: star.getAttribute('data-label') || path }]);
      store(KEY.favs, JSON.stringify(favs));
      render();
    });
    render();
  }

  function recordVisit(sidebar) {
    var active = $('[data-au-nav-tree] .au-nav-row:not(.au-nav-row--search-only) > .au-nav-item.is-active', sidebar);
    if (!active) return;
    var path = active.getAttribute('href');
    var label = (active.querySelector('.au-nav-item__label') || active).textContent.trim();
    var recent = readJson(KEY.recent, []).filter(function (r) {
      return r.path !== path;
    });
    recent.unshift({ path: path, label: label, at: Date.now() });
    store(KEY.recent, JSON.stringify(recent.slice(0, 8)));
  }

  // --- command palette (Ctrl/⌘+K quick navigation over the nav tree) -------
  function initPalette() {
    var dlg = $('dialog.au-palette');
    var dataEl = $('#au-nav-data');
    if (!dlg || !dataEl) return;
    var pages = [];
    try {
      pages = JSON.parse(dataEl.textContent || '[]');
    } catch (e) {
      pages = [];
    }
    var input = dlg.querySelector('input');
    var list = dlg.querySelector('.au-palette__list');
    var results = [];
    var index = 0;

    function byPath(path) {
      for (var i = 0; i < pages.length; i++) if (pages[i].path === path) return pages[i];
      return null;
    }
    function compute() {
      var q = input.value.trim().toLowerCase();
      var groups = [];
      if (!q) {
        var favs = readJson(KEY.favs, []).map(function (f) {
          return byPath(f.path);
        }).filter(Boolean);
        var recent = readJson(KEY.recent, []).map(function (r) {
          return byPath(r.path);
        }).filter(Boolean);
        if (favs.length) groups.push({ title: 'Favorites', items: favs });
        if (recent.length) groups.push({ title: 'Recent', items: recent.slice(0, 6) });
        groups.push({ title: 'All pages', items: pages });
      } else {
        var terms = q.split(/\s+/);
        groups.push({
          title: 'Pages',
          items: pages
            .filter(function (p) {
              var hay = ((p.trail || []).join(' ') + ' ' + p.label).toLowerCase();
              return terms.every(function (t) {
                return hay.indexOf(t) !== -1;
              });
            })
            .slice(0, 40),
        });
      }
      return groups;
    }
    function render() {
      var groups = compute();
      results = [];
      var html = '';
      groups.forEach(function (g) {
        if (!g.items.length) return;
        html += '<li class="au-palette__group" role="presentation">' + esc(g.title) + '</li>';
        g.items.forEach(function (p) {
          var i = results.length;
          results.push(p);
          html +=
            '<li class="au-palette__item" role="option" id="au-pal-' +
            i +
            '" data-i="' +
            i +
            '" aria-selected="' +
            (i === index) +
            '">' +
            icon(p.icon || 'article') +
            '<span>' +
            esc(p.label) +
            '</span><span class="au-palette__trail">' +
            esc((p.trail || []).join(' › ')) +
            '</span></li>';
        });
      });
      if (!results.length) html = '<li class="au-palette__group">No pages match.</li>';
      list.innerHTML = html;
      var sel = list.querySelector('[aria-selected="true"]');
      if (sel) sel.scrollIntoView({ block: 'nearest' });
    }
    function go(i) {
      var p = results[i];
      if (!p) return;
      dlg.close();
      progress.start();
      window.location.href = p.path;
    }
    function open() {
      input.value = '';
      index = 0;
      render();
      dlg.showModal();
      input.focus();
    }
    input.addEventListener('input', function () {
      index = 0;
      render();
    });
    input.addEventListener('keydown', function (e) {
      if (e.key === 'ArrowDown' || e.key === 'ArrowUp') {
        e.preventDefault();
        if (!results.length) return;
        index = (index + (e.key === 'ArrowDown' ? 1 : -1) + results.length) % results.length;
        render();
      } else if (e.key === 'Enter') {
        e.preventDefault();
        go(index);
      }
    });
    list.addEventListener('click', function (e) {
      var item = e.target.closest('.au-palette__item');
      if (item) go(Number(item.getAttribute('data-i')));
    });
    dlg.addEventListener('click', function (e) {
      if (isBackdropClick(e, dlg)) dlg.close();
    });
    document.addEventListener('keydown', function (e) {
      if ((e.ctrlKey || e.metaKey) && !e.altKey && e.key.toLowerCase() === 'k') {
        e.preventDefault();
        if (!dlg.open) open();
      }
    });
    $$('[data-au-palette-open]').forEach(function (b) {
      b.addEventListener('click', open);
    });
  }

  // --- global scope selector (one entity, e.g. Clinic) ----------------------
  // Server contract: GET {endpoint}&q=&skip=&take= → { items:[{id,name,isActive,sub}], hasMore }
  // Picking sets ?{param}=<id> (or ?{param}= for "All") and reloads; the server
  // persists the choice (cookie) and every scope-aware page reads it.
  function initScope() {
    $$('[data-au-scope]').forEach(function (host) {
      var input = host.querySelector('.au-combobox__input');
      var list = host.querySelector('.au-combobox__list');
      if (!input || !list) return;
      var endpoint = host.getAttribute('data-endpoint');
      var param = host.getAttribute('data-param') || 'scope';
      var allLabel = host.getAttribute('data-all-label') || 'All';
      var currentId = host.getAttribute('data-current-id') || '';
      var currentName = host.getAttribute('data-current-name') || '';
      var resetParams = (host.getAttribute('data-reset-params') || 'page').split(',');
      var PAGE = 25;
      var items = [];
      var hasMore = false;
      var loading = false;
      var q = '';
      var active = -1;
      var seq = 0;

      // keep the active scope in the URL so a copied link reproduces the view
      if (currentId) {
        var here = new URL(window.location.href);
        if (!here.searchParams.has(param)) {
          here.searchParams.set(param, currentId);
          history.replaceState(history.state, '', here.toString());
        }
      }

      function options() {
        return [{ id: '', name: allLabel, isActive: true, all: true }].concat(items);
      }
      function render(status) {
        var opts = options();
        var html = opts
          .map(function (o, i) {
            var cls = 'au-combobox__opt' + (o.all ? ' au-combobox__opt--all' : '') + (o.id === currentId ? ' is-current' : '');
            return (
              '<li class="' + cls + '" role="option" id="' + list.id + '-' + i + '" data-i="' + i + '" aria-selected="' + (i === active) + '">' +
              '<span class="au-combobox__opt-name"><span>' + esc(o.name || 'Unnamed') + '</span>' +
              (!o.all && o.isActive === false ? '<span class="au-chip au-chip--outlined" style="height:18px;font-size:10px">inactive</span>' : '') +
              '</span>' +
              (o.sub ? '<span class="au-combobox__opt-sub">' + esc(o.sub) + '</span>' : '') +
              '</li>'
            );
          })
          .join('');
        if (status) html += '<li class="au-combobox__status" role="presentation">' + esc(status) + '</li>';
        list.innerHTML = html;
        input.setAttribute('aria-activedescendant', active >= 0 ? list.id + '-' + active : '');
      }
      function load(reset) {
        if (!endpoint || (loading && !reset)) return;
        if (reset) {
          items = [];
          hasMore = false;
        }
        loading = true;
        var my = ++seq;
        render('Loading…');
        var url = endpoint + (endpoint.indexOf('?') === -1 ? '?' : '&') + 'q=' + encodeURIComponent(q) + '&skip=' + items.length + '&take=' + PAGE;
        fetch(url, { headers: { Accept: 'application/json', 'X-Requested-With': 'XMLHttpRequest' }, credentials: 'same-origin' })
          .then(function (r) {
            if (!r.ok) throw new Error('HTTP ' + r.status);
            return r.json();
          })
          .then(function (data) {
            if (my !== seq) return;
            items = items.concat(data.items || []);
            hasMore = !!data.hasMore;
            loading = false;
            render(items.length ? null : q ? 'No matches' : null);
          })
          .catch(function (err) {
            if (my !== seq) return;
            loading = false;
            render('Could not load the list (' + err.message + ')');
          });
      }
      function openList() {
        if (!list.hidden) return;
        list.hidden = false;
        input.setAttribute('aria-expanded', 'true');
        active = -1;
        q = '';
        load(true);
      }
      function closeList(restore) {
        list.hidden = true;
        input.setAttribute('aria-expanded', 'false');
        if (restore) input.value = currentName;
      }
      function pick(o) {
        closeList(false);
        if ((o.id || '') === currentId) {
          input.value = currentName;
          return;
        }
        var url = new URL(window.location.href);
        url.searchParams.set(param, o.id || ''); // empty = explicit "All" (server clears)
        resetParams.forEach(function (p) {
          if (p) url.searchParams.delete(p.trim());
        });
        progress.start();
        window.location.assign(url.toString());
      }
      var search = debounce(function () {
        q = input.value.trim();
        active = -1;
        load(true);
      }, 300);

      input.addEventListener('focus', function () {
        input.select();
        openList();
      });
      input.addEventListener('click', openList);
      input.addEventListener('input', function () {
        if (list.hidden) openList();
        search();
      });
      input.addEventListener('keydown', function (e) {
        var count = options().length;
        if (e.key === 'ArrowDown' || e.key === 'ArrowUp') {
          e.preventDefault();
          openList();
          active = (active + (e.key === 'ArrowDown' ? 1 : -1) + count) % count;
          render();
          var el = document.getElementById(list.id + '-' + active);
          if (el) el.scrollIntoView({ block: 'nearest' });
        } else if (e.key === 'Enter') {
          if (!list.hidden && active >= 0) {
            e.preventDefault();
            pick(options()[active]);
          }
        } else if (e.key === 'Escape') {
          closeList(true);
        }
      });
      input.addEventListener('blur', function () {
        setTimeout(function () {
          if (!host.contains(document.activeElement)) closeList(true);
        }, 150);
      });
      list.addEventListener('mousedown', function (e) {
        e.preventDefault(); // keep focus in the input
        var li = e.target.closest('.au-combobox__opt');
        if (li) pick(options()[Number(li.getAttribute('data-i'))]);
      });
      list.addEventListener('scroll', function () {
        if (hasMore && !loading && list.scrollTop + list.clientHeight >= list.scrollHeight - 48) load(false);
      });
    });
  }

  // --- forms: confirm gate, busy state, auto-submit filters, date range -----
  function initForms() {
    // Bubble phase on document: jQuery-unobtrusive validation runs first on the
    // form and stops an invalid submit, so we never confirm an invalid form.
    document.addEventListener('submit', function (e) {
      if (e.defaultPrevented) return;
      var form = e.target;
      if (form.method === 'dialog') return; // closes a <dialog>; not a navigation, no busy state
      var submitter = e.submitter || null;
      var gate = (submitter && submitter.hasAttribute('data-au-confirm')) || form.hasAttribute('data-au-confirm');
      if (gate && !form.__auConfirmed) {
        e.preventDefault();
        confirmDialog(confirmOptions(submitter, form)).then(function (ok) {
          if (!ok) return;
          form.__auConfirmed = true;
          // requestSubmit(submitter) keeps the submitter's name/value + formaction
          // (Razor's asp-page-handler on a button depends on it)
          if (form.requestSubmit) form.requestSubmit(submitter || undefined);
          else form.submit();
        });
        return;
      }
      form.__auConfirmed = false;
      if (form.target === '_blank' || form.hasAttribute('data-au-no-busy')) return;
      // NEVER disable the submitter here: a disabled button is dropped from the
      // form data and the page handler would not be selected.
      if (submitter && submitter.classList.contains('au-btn')) submitter.setAttribute('aria-busy', 'true');
      progress.start();
    });

    document.addEventListener('change', function (e) {
      var el = e.target;
      var form = el.form;
      if (!form || !form.hasAttribute('data-au-autosubmit')) return;
      if (el.hasAttribute('data-au-range-select') && el.value === 'custom') return; // wait for Apply
      if (el.closest('[data-au-range-custom]')) return;
      var isText = el.tagName === 'TEXTAREA' || (el.tagName === 'INPUT' && /^(text|search|email|tel|url|number)$/i.test(el.type));
      if (isText) return; // text filters submit on Enter, never on every keystroke
      resetPage(form);
      if (form.requestSubmit) form.requestSubmit();
      else form.submit();
    });

    $$('[data-au-range]').forEach(function (range) {
      var select = range.querySelector('[data-au-range-select]');
      var custom = range.querySelector('[data-au-range-custom]');
      if (!select || !custom) return;
      var sync = function () {
        custom.hidden = select.value !== 'custom';
      };
      select.addEventListener('change', sync);
      sync();
    });
  }
  function resetPage(form) {
    var name = form.getAttribute('data-au-page-param') || 'page';
    var first = form.getAttribute('data-au-page-first') || '1';
    var input = form.querySelector('[name="' + name + '"]');
    if (input) input.value = first;
  }

  // --- clicks: dialogs, drawers, copy, row links, confirm links, refresh ----
  function initClicks() {
    document.addEventListener('click', function (e) {
      var t = e.target;

      var opener = t.closest('[data-au-open]');
      if (opener) {
        e.preventDefault();
        dialog.open(opener.getAttribute('data-au-open'));
        return;
      }
      var closer = t.closest('[data-au-close]');
      if (closer) {
        var d = closer.closest('dialog');
        if (d) d.close();
        return;
      }
      // click on the backdrop of a dismissable dialog / drawer
      if (t.tagName === 'DIALOG' && (t.classList.contains('au-drawer') || t.hasAttribute('data-au-dismissable')) && isBackdropClick(e, t)) {
        t.close();
        return;
      }

      var copyBtn = t.closest('[data-au-copy]');
      if (copyBtn) {
        e.preventDefault();
        e.stopPropagation();
        var value = copyBtn.getAttribute('data-au-copy');
        var src = copyBtn.getAttribute('data-au-copy-target');
        if (src) {
          var srcEl = $(src);
          value = srcEl ? srcEl.textContent : '';
        }
        copy(value || '', copyBtn.closest('dialog') || undefined).then(
          function () {
            toast.success(copyBtn.getAttribute('data-au-copy-message') || 'Copied');
          },
          function () {
            toast.error('Could not copy to clipboard');
          }
        );
        return;
      }

      var refresh = t.closest('[data-au-refresh]');
      if (refresh) {
        e.preventDefault();
        progress.start();
        window.location.reload();
        return;
      }

      var link = t.closest('a[data-au-confirm]');
      if (link) {
        e.preventDefault();
        confirmDialog(confirmOptions(link, link)).then(function (ok) {
          if (ok) {
            progress.start();
            window.location.href = link.href;
          }
        });
        return;
      }

      // row → drawer (partial HTML) or row → page
      var row = t.closest('[data-au-drawer-url], [data-au-href]');
      if (row && !t.closest('a, button, input, select, textarea, label, summary, [data-au-stop]')) {
        if (row.hasAttribute('data-au-drawer-url')) {
          e.preventDefault();
          openRemoteDrawer(row.getAttribute('data-au-drawer-url'), row.getAttribute('data-au-drawer') || '#au-drawer');
          return;
        }
        var href = row.getAttribute('data-au-href');
        if (e.ctrlKey || e.metaKey || e.button === 1) window.open(href, '_blank');
        else {
          progress.start();
          window.location.href = href;
        }
        return;
      }

      // ordinary same-tab navigation → show the progress strip
      var a = t.closest('a[href]');
      if (
        a &&
        !e.defaultPrevented &&
        !e.ctrlKey &&
        !e.metaKey &&
        !e.shiftKey &&
        (!a.target || a.target === '_self') &&
        !a.hasAttribute('download') &&
        a.origin === window.location.origin &&
        !(a.pathname === window.location.pathname && a.search === window.location.search && a.hash)
      ) {
        progress.start();
      }
    });

    // keyboard: Enter on a focused clickable row
    document.addEventListener('keydown', function (e) {
      if (e.key !== 'Enter') return;
      var row = e.target.closest && e.target.closest('tr[data-au-href], tr[data-au-drawer-url]');
      if (row && e.target === row) row.click();
    });
  }

  function openRemoteDrawer(url, selector) {
    var drawer = $(selector);
    if (!drawer) return;
    var body = drawer.querySelector('[data-au-drawer-body]') || drawer;
    body.innerHTML =
      '<span class="au-skeleton au-skeleton--text" style="width:60%"></span>' +
      '<span class="au-skeleton" style="height:120px;margin-top:16px"></span>';
    dialog.open(drawer);
    fetch(url, { headers: { 'X-Requested-With': 'XMLHttpRequest' }, credentials: 'same-origin' })
      .then(function (r) {
        return r.text().then(function (html) {
          if (!r.ok) throw { status: r.status, html: html };
          return html;
        });
      })
      .then(function (html) {
        body.innerHTML = html; // a server-rendered partial (already HTML-encoded by Razor)
      })
      .catch(function (err) {
        body.innerHTML =
          '<div class="au-alert au-alert--error">' + icon('error', 'au-alert__icon') +
          '<div class="au-alert__body"><div class="au-alert__title">Could not load the details</div>' +
          esc(err && err.status ? 'The server answered HTTP ' + err.status + ' for ' + url : 'The request did not reach the server: ' + url) +
          '</div></div>';
      });
  }

  // --- dropdowns (<details class="au-dropdown">): one open at a time --------
  function initDropdowns() {
    document.addEventListener(
      'toggle',
      function (e) {
        var d = e.target;
        if (!d.classList || !d.classList.contains('au-dropdown') || !d.open) return;
        $$('details.au-dropdown[open]').forEach(function (o) {
          if (o !== d) o.open = false;
        });
      },
      true
    );
    document.addEventListener('click', function (e) {
      $$('details.au-dropdown[open]').forEach(function (d) {
        if (!d.contains(e.target) || e.target.closest('.au-menu__item')) d.open = false;
      });
    });
    document.addEventListener('keydown', function (e) {
      if (e.key !== 'Escape') return;
      $$('details.au-dropdown[open]').forEach(function (d) {
        d.open = false;
        var s = d.querySelector('summary');
        if (s) s.focus();
      });
      root.classList.remove('au-side-open');
    });
  }

  // --- boot -------------------------------------------------------------------
  function boot() {
    progress.el = $('.au-topbar-progress');
    $$('[data-au-theme-toggle]').forEach(function (btn) {
      syncThemeToggle(btn);
      btn.addEventListener('click', theme.toggle);
    });
    initSidebar();
    initPalette();
    initScope();
    initForms();
    initClicks();
    initDropdowns();

    // server-rendered toasts (e.g. from TempData after Post-Redirect-Get)
    $$('[data-au-toast]').forEach(function (el) {
      showToast(el.getAttribute('data-au-toast'), el.textContent.trim());
      el.parentNode.removeChild(el);
    });

    // keep the active section tab visible on narrow screens
    var tab = $('.au-section-tabs .au-tab.is-active');
    if (tab && tab.scrollIntoView) tab.scrollIntoView({ block: 'nearest', inline: 'center' });

    // soft page-enter motion (visual only)
    var main = $('.au-main');
    if (main && main.animate && !reduceMotion) {
      main.animate(
        [
          { opacity: 0.3, transform: 'translateY(6px)' },
          { opacity: 1, transform: 'none' },
        ],
        { duration: 260, easing: 'cubic-bezier(0.2, 0.8, 0.3, 1)' }
      );
    }
  }

  // back/forward-cache restore: clear the busy state the page was frozen with
  window.addEventListener('pageshow', function (e) {
    if (!e.persisted) return;
    progress.stop();
    $$('[aria-busy="true"]').forEach(function (b) {
      b.removeAttribute('aria-busy');
    });
  });

  window.Aurora = { theme: theme, toast: toast, confirm: confirmDialog, copy: copy, progress: progress, dialog: dialog };

  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', boot);
  else boot();
})();
````

