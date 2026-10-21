# Admin Module — Company

## Module Overview
- **Area:** ADMIN
- **Module Name:** Company
- **Current Status:** `IMPLEMENTED` (server + AJAX UI). Written without a build or a browser run; the first build and click-through still has to confirm it (see Notes).

## Purpose & Responsibilities
One company row per client database: display name, legal name, tax/registration, email, phone and address. Platform Super Admin creates the row when none exists; a company administrator with `settings.manage` edits it.

## Implemented Features
- `Company/Index` form (`InCompanyUpdateView`): identity, tax/reg, contact, address, currency, time zone and locale.
- `Company/Save` (POST, AJAX): create when bootstrapping, otherwise update. Success reloads with a toast.
- Read-only license summary card for the platform Super Admin, with a link to `License/Index`. Other users do not see that card.
- Empty state when no company exists and the signed-in user is not a platform Super Admin.

## Routes & Permissions
Permission `settings.manage` on every action (platform Super Admin bypasses). No `ModuleAccess` attribute — the sidebar still hides the item when SETTINGS is off.

| Method | Route | Action | Notes |
|---|---|---|---|
| GET | `/Company` | `Index` | bootstrap form when no company and the actor is platform Super Admin |
| POST | `/Company/Save` | `Save` | create or update; `{ id, redirectUrl }` |

## Filters / Search / Sorting
- N/A (single-row profile).

## Data
- Entity (repository only): `Company` → `company`.
- Models: `Models/CompanyModels.cs` — `InCompanyUpdateView`, `InputCompanyCreate` / `OutputCompanyCreate`, `InputCompanyUpdate` / `OutputCompanyUpdate`, `InputCompanySelect` / `OutputCompanySelect`.

## Database Dependencies
- Table: `company` (`001_Company.sql`). One active row per installation. Unique `companycode`.

## API & Data Access
- `CompanyController` → `ICompanyService` / `CompanyService` → `ICompanyRepository` / `CompanyRepository`.
- License summary uses `ICompanyService.GetLicenseAsync`. Failures throw typed `AppException`s (`ForbiddenException`, `ConflictException`, `ResourceNotFoundException`).

## UI Rules & Conventions
- View: `Views/Company/Index.cshtml`. JS: `wwwroot/js/admin/company.js` on `sc-ajax.js`.
- Form: `novalidate`, `[data-sc-form-summary]`, `asp-validation-for`, Save via `SmartCartAjax.submitForm`.

## Business Rules
- Platform Super Admin (`CanCreate`) may create the first company. A second create is a 409.
- Company admin may only update the existing row.
- Audit (`Company` entity): Create, Update.

## Dependencies & Related Modules
- `stores.md`, `license.md`, `settings.md`, `super-admin.md`, `audit.md`.

## Notes
- Currency / time zone / locale on this form are company master fields, not the `settings` table keys of the same idea.
- Not built or run when this was written: compile and click-through checks are still open.
