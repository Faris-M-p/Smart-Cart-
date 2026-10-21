# Admin Module — Modules

## Module Overview
- **Area:** ADMIN
- **Module Name:** Modules
- **Current Status:** `IMPLEMENTED` (server + AJAX UI). Written without a build or a browser run; the first build and click-through still has to confirm it (see Notes).

## Purpose & Responsibilities
Choose which catalog modules are enabled for this installation (`installation_modules`). Platform Super Admin only. Sidebar visibility uses `Access.ModuleEnabledAsync`; `[ModuleAccess]` on other controllers enforces the same list.

## Implemented Features
- `Module/Index`: checkbox list with descriptions, Core badge, Basic / Premium preset buttons (from `005_ModulePresets.sql`) and a Custom indicator when the saved set matches neither preset.
- `Module/Save`: posts `EnabledModuleIds`. Core modules (the Basic preset: DASHBOARD, PRODUCTS, POS, INVENTORY) are always included even if the form omitted them.
- `Module/ApplyPreset`: applies Basic or Premium immediately and saves.

## Routes & Permissions
Class-level `[Authorize(Policy = "PlatformSuperAdmin")]`.

| Method | Route | Action | Notes |
|---|---|---|---|
| GET | `/Module` | `Index` | |
| POST | `/Module/Save` | `Save` | `{ enabledCount, redirectUrl }` |
| POST | `/Module/ApplyPreset` | `ApplyPreset` | `SelectedPresetCode`; `{ enabledCount, presetCode, redirectUrl }` |

## Filters / Search / Sorting
- N/A. Modules are ordered by `displayorder` then name.

## Data
- Entities (repository only): `Module`, `ModulePreset`, `ModulePresetItem`, `InstallationModule`.
- Models: `Models/ModuleModels.cs` — `OutputModuleSelect` (`Description`, `IsCore`, `IsEnabled`), `OutputModuleList`, `OutputModulePresetSelect`, `InModuleAssignView`, `InputModuleAssign` / `OutputModuleAssign`.
- Descriptions are filled in `ModuleService` (the `modules` table has no description column).

## Database Dependencies
- `005_Modules.sql` catalog. `V004` installation tables. Seed `005_ModulePresets.sql`: BASIC (DASHBOARD, PRODUCTS, POS, INVENTORY) and PREMIUM (all nine). There is no CUSTOM row — Custom is “whatever is saved that is not a preset”.

## API & Data Access
- `ModuleController` → `IModuleService` / `ModuleService` → `IModuleRepository` / `ModuleRepository`.
- Existing members kept: `GetModuleListAsync`, `AssignModulesAsync`, `GetEnabledModuleCodesAsync` (used by `CurrentUserAccess` and `ModuleAccessFilter`).
- Audit (`Module` entity): Update on save/preset.

## UI Rules & Conventions
- View: `Views/Module/Index.cshtml`. JS: `wwwroot/js/admin/module.js`.
- Core checkboxes are disabled; a hidden `EnabledModuleIds` input still posts them.
- Preset buttons are `data-sc-post` + confirm (`data-sc-after=reload`).

## Business Rules
- Unknown module ids in the post are dropped. Core ids from the BASIC preset are forced on.
- Platform Super Admin bypasses module gates everywhere else; this screen still requires the platform policy.

## Dependencies & Related Modules
- `super-admin.md`, `roles.md` (permission catalog sits under modules), `auth.md`, every module gated by `ModuleAccess`.

## Notes
- SETTINGS / USERS can be turned off; Company/Store actions use `settings.manage` without `[ModuleAccess]`, so those URLs still work if someone has the permission.
- Not built or run when this was written: compile and click-through checks are still open.
