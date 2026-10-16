# Admin Module — Employees

## Module Overview
- **Area:** ADMIN
- **Module Name:** Employees
- **Current Status:** `UI SHELL`

## Purpose & Responsibilities
Manage users who access SmartCartPro.

## Implemented Features
- `Admin/Employees` sample list

## Filters / Search / Sorting
- Planned

## Data
- Table: `users`; links: `user_roles`, `user_store_access`

## Database Dependencies
- Soft cancel; unique username/email

## API & Data Access
- `AdminController.Employees`
- View: `Views/Admin/Employees.cshtml`

## Business Rules
- Do not store plain-text passwords
- Store access via `user_store_access`

## Dependencies & Related Modules
- `auth.md`, `roles.md`, `stores.md`