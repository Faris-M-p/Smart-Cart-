# Admin Module — Company & stores

## Module Overview
- **Area:** ADMIN
- **Module Name:** Stores
- **Current Status:** `UI SHELL` (schema + EF entities exist)

## Purpose & Responsibilities
Company profile and store list; default store invariant.

## Implemented Features
- `Admin/Stores` sample company + stores cards
- Entities: `Company`, `Store`

## Filters / Search / Sorting
- N/A

## Data
- `company`, `store` (`fk_company`, `isdefault`)

## Database Dependencies
- Partial unique one default store; store code unique per company

## API & Data Access
- `AdminController.Stores`
- View: `Views/Admin/Stores.cshtml`

## Business Rules
- Create company + first default store in one transaction
- Promote another default before cancelling the current default
- Operational data uses `fk_store`

## Dependencies & Related Modules
- `settings.md`, `employee.md`, all store-scoped modules