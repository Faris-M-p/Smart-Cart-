# Admin Module — Audit

## Module Name & Area
- **Area:** ADMIN
- **Module Name:** Audit log
- **Current Status:** `IMPLEMENTED` — read-only viewer + existing write API

## Purpose & Responsibilities
Company-scoped, read-only trail of admin actions. Services keep calling `IAuditService.WriteLogAsync(InputAuditLogCreate)` (source-compatible). This module only adds list/detail members.

## Implemented Features
- Index defaults to the last 7 UTC days when From/To are omitted
- Filters: entity name, entity id, action, user id, store id, date range, page size
- Details JSON modal: fetch one row, pretty-print `details` with `JSON.parse` / `JSON.stringify`, then `.text()` into `<pre>` (never `Html.Raw`)
- Timestamps via `TimeDisplay.Iso` + `data-sc-utc`
- JSON list/detail return `CommonResponseModel`

## Filters / Search / Sorting
- `EntityName`, `EntityId`, `Action`, `FkUser`, `FkStore`, `From`, `To`, `Page`, `PageSize`
- Ordered by `CreatedAt` descending
- Inclusive calendar `To` (end-of-day) in the repository

## Data
- **Write (unchanged):** `InputAuditLogCreate` (`ActorUserId`, `EntityName`, `EntityId`, `Action`, `Details`)
- **Read:** `InputAuditLogList`, `InputAuditLogSelect`, `OutputAuditLogItem`, `OutputAuditLogList`
- Entity `AuditLog` stays in the repository

## Database Dependencies
- `audit_logs` — `fk_store` nullable, **no `fk_company`**. Company scope: rows whose store belongs to the caller’s company, plus rows with null `fk_store`. Platform Super Admin with no company claim sees all rows.

## API & Data Access
| Method | Route | Permission | Description |
|---|---|---|---|
| GET | `/Audit` | settings.manage | Paged list (default last 7 days) |
| GET | `/Audit/ListJson` | settings.manage | AJAX envelope list |
| GET | `/Audit/Details/{id}` | settings.manage | AJAX envelope single row |

Write (not HTTP): `IAuditService.WriteLogAsync` / `IAuditRepository.WriteLogAsync` — **do not rename or remove**. Callers: `RoleService`, `UserService`, `SettingService`, `ModuleService`.

Controller: `[ModuleAccess("SETTINGS")]` + `[RequirePermission("settings.manage")]`. Read-only (no POST).

## UI Rules & Conventions
- `Views/Audit/Index.cshtml`, `wwwroot/js/audit/audit.js`
- Modal shown with Bootstrap CSS classes (`window.bootstrap` is not global)
- Details payload always inserted with `.text()`

## Business Rules
- Read-only UI; no edit/delete of audit rows
- Company-scoped for company users; Super Admin without company sees the full table
- Default window is last 7 days UTC

## Dependencies & Related Modules
- `settings.md`, `roles.md`, `employee.md`

## Notes
- Adding `fk_company` to `audit_logs` would tighten multi-company isolation (orchestrator / schema). Until then, null-store rows are visible to every company admin on the installation.
