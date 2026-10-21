# Admin Module — License

## Module Overview
- **Area:** ADMIN
- **Module Name:** License
- **Current Status:** `IMPLEMENTED` (server + AJAX UI). Written without a build or a browser run; the first build and click-through still has to confirm it (see Notes).

## Purpose & Responsibilities
One installation license row: plan, status, start/expiry, MaxStores, notes. Platform Super Admin only. The global `LicenseGateFilter` reads this row (5-minute memory cache) to block suspended or expired tenants.

## Implemented Features
- `License/Index`: plan, status badge, dates, days left, store usage vs MaxStores, active employee count (informational — the schema has no MaxUsers), Save form, Suspend / Activate with confirm + written reason.
- Help card documents `LicenseGateFilter`: Super Admin bypass; Suspended → 403 all requests; Expired or missing license → GET allowed, writes blocked; anonymous auth pages stay reachable.
- Save: expiry on or after start; MaxStores must be ≥ current active store count. Saving clears the license status cache used by the gate.

## Routes & Permissions
Class-level `[Authorize(Policy = "PlatformSuperAdmin")]`.

| Method | Route | Action | Notes |
|---|---|---|---|
| GET | `/License` | `Index` | empty form defaults when no row |
| POST | `/License/Save` | `Save` | `{ id, redirectUrl }` |
| POST | `/License/Suspend` | `Suspend` | `Reason` required; appended to notes |
| POST | `/License/Activate` | `Activate` | `Reason` required; appended to notes |

## Filters / Search / Sorting
- N/A.

## Data
- Entity (repository only): `License` → `license`. Status TEXT in DB (`active` / `suspended` / `expired` / `trial`) mapped in the repository to `CommonModel.LicenseStatus` int.
- Models: `Models/LicenseModels.cs` — `InLicenseUpdateView`, `InputLicenseUpdate` / `OutputLicenseUpdate`, `InputLicenseSelect` / `OutputLicenseSelect` (includes `ActiveStoreCount`, `ActiveUserCount`, `DaysRemaining`, `IsExpired`), `InLicenseStatusView` / `InputLicenseStatusUpdate` / `OutputLicenseStatusUpdate`.

## Database Dependencies
- `V004__LicenseAndInstallation.sql`: `ck_license_dates` (`expirydate >= startdate`), `ck_license_maxstores` (≥ 1), unique `licensekey`. No `maxusers` column.

## API & Data Access
- `LicenseController` → `ICompanyService` / `CompanyService` → `ICompanyRepository` / `CompanyRepository`.
- `LicenseGateFilter` (not owned by this module) calls `GetLicenseAsync` and caches `smartcart:license:status`. `CompanyService.UpdateLicenseAsync` removes that key.

## UI Rules & Conventions
- View: `Views/License/Index.cshtml`. JS: `wwwroot/js/admin/license.js`.
- Dates are calendar dates (shown as-is, not `TimeDisplay`).
- Suspend/Activate reason is posted as `Reason` after `SmartCartAjax.confirm` (falls back to `window.confirm` because `window.bootstrap` is not global).

## Business Rules
- Status enum must be defined. Expiry ≥ start. MaxStores ≥ active stores.
- Suspended status blocks all non–super-admin traffic. Expired dates block writes even if status is Active until expiry is extended.
- Audit (`License` entity): Update (including suspend/activate).

## Dependencies & Related Modules
- `company.md` (summary card), `stores.md` (MaxStores), `super-admin.md`, `auth.md`.

## Notes
- Active user count is displayed only; there is no user cap in SQL.
- Not built or run when this was written: compile and click-through checks are still open.
