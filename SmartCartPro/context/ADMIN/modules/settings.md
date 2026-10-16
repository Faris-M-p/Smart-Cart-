# Admin Module — Settings

## Module Overview
- **Area:** ADMIN
- **Module Name:** Settings
- **Current Status:** `UI SHELL` (seed keys exist)

## Purpose & Responsibilities
Company/store/platform settings (`settings` table).

## Implemented Features
- `Admin/Settings` form shell
- Seed: `02_Seed/003_DefaultSettings.sql`

## Filters / Search / Sorting
- N/A

## Data
- Table: `settings` (`setting_scope`, optional `fk_store`, `settingkey`)

## Database Dependencies
- CHECK: store scope requires `fk_store`

## API & Data Access
- `AdminController.Settings`
- View: `Views/Admin/Settings.cshtml`

## Business Rules
- Do not put secrets in settings without encryption strategy

## Dependencies & Related Modules
- `stores.md`, `pos.md`