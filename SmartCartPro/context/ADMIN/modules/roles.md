# Admin Module — Roles & permissions

## Module Overview
- **Area:** ADMIN
- **Module Name:** Roles
- **Current Status:** `UI SHELL` (seed data exists)

## Purpose & Responsibilities
Roles, modules, permissions, role_permissions.

## Implemented Features
- `Admin/Roles` sample list matching seeded role codes
- Seed scripts: `02_Seed/001_DefaultRoles.sql`, `002_DefaultPermissions.sql`

## Filters / Search / Sorting
- N/A

## Data
- `roles`, `modules`, `permissions`, `role_permissions`, `user_roles`

## Database Dependencies
- Seeded system roles (`SUPER_ADMIN`, `CASHIER`, …)

## API & Data Access
- `AdminController.Roles`
- View: `Views/Admin/Roles.cshtml`

## Business Rules
- System roles should not be casually deleted
- Permissions enforced server-side when auth is live

## Dependencies & Related Modules
- `employee.md`, `auth.md`