# Admin Module — Super Admin (Platform)

## Module Overview
- **Area:** ADMIN (platform management within the same MVC host)
- **Module Name:** Super Admin
- **Current Status:** `PARTIALLY IMPLEMENTED` — shared login + email OTP auth done; company/license/package wizard still **PLANNED**
- **Source:** Product functional description held for analysis (2026-04-16). This file is the working spec + gap analysis against SmartCartPro as it exists today.

## Purpose & Responsibilities

Platform Super Admin manages **company onboarding**, **packages / menu groups**, **feature enablement**, **licenses** (create / renew / expiry / suspend), **store limits**, **storefront theme assignment**, and **privileged audit**. Super Admin also uses all normal Admin business features without a duplicate app.

Normal company Admin and Store Admin must **never** see or call Super Admin platform menus, accounts, or APIs.

## Architecture constraints (locked by product)

| Requirement | Decision |
|-------------|----------|
| Host | Same ASP.NET Core MVC Admin app (`SmartCartPro`) — no separate Super Admin website |
| Users | Reuse `users` — **no** `SuperAdminUsers` table |
| Auth | Same `/Auth/Login` as store admin; Super Admin → email OTP (`/Auth/VerifyOtp`); SMTP in `SmtpSettings` |
| Schema | SQL scripts only (`Database-Postgres/`) — **no** EF migrations |
| Layers | Controller → Service → Repository (`*/Interfaces/`); thin controllers; typed exceptions via `AppExceptionHandler` |
| Secrets | No hardcoded Super Admin password in source; one-time secure seed/bootstrap |

## Gap analysis vs current SmartCartPro

### What already exists (reuse)

| Area | Today | Reuse for Super Admin |
|------|--------|------------------------|
| Auth | Real JWT cookie (`smartcartpro.auth.token`), `PasswordHasher`, claims `id_user` / `fk_store` / `fk_company` | Extend with Super Admin policy + OTP step. **Auth has no Register** — company create is Super Admin wizard only |
| Users | `users` | Same table; mark Super Admin with a **protected** flag/role normal admins cannot assign |
| Roles / permissions | `roles`, `modules`, `permissions`, `role_permissions`, `user_roles` | Company RBAC remains; add **separate** platform authorization policy |
| Company / store | `company`, `store`, `user_store_access` | Company + store creation already patterned in Auth bootstrap transaction |
| Modules catalog | Seeded `DASHBOARD`, `PRODUCTS`, `INVENTORY`, `PURCHASES`, `POS`, `CUSTOMERS`, `REPORTS`, `SETTINGS`, `USERS` | Map package features to these codes (+ add e.g. `ECOMMERCE`, `PLATFORM` when needed) |
| Audit | `audit_logs` (`fk_user`, entity, action, details) | Extend usage for license/package privileged actions; may need richer columns later |
| Settings | `settings` with scopes `platform` / `company` / `store` | Platform settings + theme key candidates |
| UI shell | Metis `_Layout` sidebar (Catalog, Inventory, Sales, Reports, Administration) | Add **Platform Management** nav only when Super Admin policy passes |
| Controllers | `Auth`, `Home`, `Catalog`, `Inventory`, `Sales`, `Customers`, `Reports`, `Admin`, `Filter` | Reuse business controllers; new Super Admin controllers for platform only |
| Exception handling | `AppExceptionHandler` + `Exceptions/AppExceptions.cs` | Throw typed exceptions from Super Admin services |

### Critical naming collision

Seeded role **`SUPER_ADMIN`** today means **full company admin** inside a client company (`02_Seed/001_DefaultRoles.sql`), **not** platform Super Admin.

**Required policy when implementing:** introduce a distinct platform identity, e.g.:

- Role/code `PLATFORM_SUPER_ADMIN`, **or**
- Boolean / protected column on `users` (e.g. `is_platform_super_admin`) that company `users.manage` cannot set

Do **not** overload company `SUPER_ADMIN` for platform access.

### Critical tenancy mismatch

| Current DB assumption | Super Admin product need |
|-----------------------|--------------------------|
| Comments / docs: **one company per client database**; Auth register blocked if any company exists | Many companies, licenses, packages, store limits in one platform Admin |

`FUTURE.md` already lists a **platform multi-tenant registry DB** as deferred.

**Open architecture choice (must decide before coding):**

1. **Platform registry DB** (preferred for true SaaS): Super Admin + packages + licenses + company registry live in a platform database; each client DB remains company-scoped operational data.  
2. **Single expanded DB**: allow many `company` rows + license/package tables in the same PostgreSQL database; Auth “first company register” becomes Super-Admin-only.

Document the chosen option in `DATABASE.md` / `ARCHITECTURE.md` at implementation time. Until then, treat multi-company platform tables as **missing**.

### What does not exist yet (must add via SQL)

- License tables (key, status, start/expiry, history)
- Package / menu-group templates + package↔feature maps
- Company-specific feature entitlements
- Store-limit enforcement fields (per company/license)
- Storefront theme registry / assignment (React storefront theme ≠ Metis admin theme)
- OTP challenge storage (hashed OTP, expiry, attempt counters) for `/platform-access`
- Protected Super Admin identifier (column and/or system role) hidden from employee UI
- `RequirePermission` / feature-entitlement middleware (permissions seeded; **not** enforced end-to-end yet)
- Separate Super Admin login route + OTP UI
- React storefront + dedicated Web API host as described in the product brief — **not present in this repo today** (Admin MVC only; USER storefront deferred). Enforce e-commerce entitlements when those surfaces exist; until then, enforce on Admin APIs / future USER modules.

### Current Auth vs required Super Admin login

| Step | Current `/Auth/Login` | Required `/platform-access` |
|------|------------------------|-----------------------------|
| Password verify | Yes | Yes |
| Store claim `fk_store` | Yes (preferred `user_store_access`) | Platform session may be company-agnostic or “no store” until impersonating/managing a company |
| OTP email | No | Required (6-digit, 5 min, hashed, single-use, rate limits) |
| Public register | First-company bootstrap only | **Forbidden** for Super Admin |

## Functional description (held product scope)

### Authentication

1. Dedicated route `/platform-access` (name finalizable).
2. Verify user + password hash + protected Super Admin flag + active status.
3. Generate cryptographically secure 6-digit OTP; store **protected representation only**; email OTP.
4. Verify OTP (5-minute validity, attempt/resend/login rate limits, single-use).
5. Establish Super Admin session (JWT cookie or distinct cookie name — decide at implement time; must not grant platform claims to normal Admin).
6. Redirect to Super Admin Dashboard.
7. All Super Admin endpoints: server-side authorization + audit. Hidden menus are not security.

### Dashboard (live DB stats — no hardcoded KPIs)

- Total companies; active / expired / expiring-soon / suspended licenses
- Total stores; total allowed stores across licenses
- Recent license activities + quick actions (register company, renew, packages, menus)

### Company registration wizard (one transaction)

1. Company details (validate uniqueness against schema)
2. Select package (Basic / E-commerce / Premium templates — **editable**, not hardcoded forever)
3. Review/configure menus/features (map to real `modules` / routes)
4. Store limit (server-enforced on store create)
5. Theme assignment (customer React storefront only)
6. License config + unique secure license key
7. Review → create company + package + features + theme + license + audit in one transaction

### Package management

CRUD packages, defaults (features, store limit, duration), activate/deactivate, list assigned companies. **Explicit policy** required: package edit must not silently rewrite existing company entitlements unless Super Admin chooses “apply to existing”.

### License management

Searchable list; renew / change store limit / change package / suspend / reactivate / revoke; history with before/after, actor, timestamp, reason. Define renewal rules for already-expired licenses.

### Expiry enforcement (server-side)

- Active: entitled ops  
- Expiring soon: warn + notify  
- Expired: agreed read-only / block writes (sales, stock mutations, etc.)  
- Suspended/revoked: configured restrictions  

Also define post-expiry policy for refunds, reports, export. UI hide alone is insufficient.

### Feature / menu visibility

- Super Admin: all business menus + Platform Management menus  
- Company Admin: only company-enabled modules + role permissions  
- Store Admin: role + `user_store_access` + store limit  

Enforce UI filter **and** server-side feature checks. E-commerce off must eventually block React storefront + APIs, not only menus.

### Super Admin navigation (target)

**Platform Management:** Dashboard, Companies, Register Company, Packages, Menu/Feature config, Licenses, License History, Themes, Store Limits, Audit Logs, Platform Settings.

**Business Management:** reuse existing Admin sidebar modules (Dashboard, Catalog, Inventory, Sales, Employees, Roles, Reports, Stores, Settings, …) without duplicating controllers.

### Acceptance criteria (summary)

Secure OTP Super Admin login; Super Admin invisible in normal employee screens; wizard register; package defaults; licenses CRUD/renew/suspend; server-side expiry; entitlement menus; store limits; audit; live dashboard; SQL seed/patch; preserve existing architecture.

## Proposed implementation phases (when authorized)

1. **AuthZ foundation** — platform Super Admin flag/role (distinct from company `SUPER_ADMIN`); `/platform-access` + OTP; policies; hide from employee UI  
2. **Packages & features** — tables + seed Basic/E-commerce/Premium; map to `modules`  
3. **Company registration wizard** — transactional create  
4. **Licenses** — generate/manage/history  
5. **Theme + store-limit** — registry + enforcement on store create  
6. **Dashboard + audit** — live aggregates  
7. **SQL patches/seed + manual test cases** — no app auto-launch in AI sessions unless asked  

Do **not** start these phases until the user explicitly says to implement.

## Tentative schema additions (analysis only — not created)

Names illustrative; finalize against `DATABASE.md` conventions (`id_*`, `fk_*`, soft cancel):

- `users` — protected platform Super Admin marker (or dedicated system role not assignable via Admin UI)
- `packages` / `package_features`
- `company_features` (or license-bound entitlements)
- `licenses` / `license_history`
- `storefront_themes` + assignment on company/license
- `auth_otp_challenges` (hashed code, expires, attempts)
- Possibly extend `audit_logs` for platform entity types

Resolve **platform registry DB vs single DB** before writing DDL.

## Data / API targets (planned)

| Layer | Planned |
|-------|---------|
| Controllers | e.g. `PlatformAuthController`, `PlatformDashboardController`, `PlatformCompaniesController`, `PlatformLicensesController`, `PlatformPackagesController` (final names TBD) |
| Services | OTP, license key generation, entitlement checks, wizard orchestration |
| Repositories | EF LINQ against new tables; no business helpers in repos |
| Routes | `/platform-access`, `/Platform/...` (TBD) |
| Policies | `RequirePlatformSuperAdmin` separate from company permission codes |

## UI Rules & Conventions

- Metis Admin chrome for Super Admin pages
- Platform nav section only for platform Super Admin
- Customer themes ≠ Metis dark/light admin theme
- Controllers stay endpoint-thin; cookie/OTP/license logic in services

## Business Rules

- No public Super Admin registration
- Normal Admin cannot grant platform Super Admin
- Store count ≤ license max (server-side)
- License/feature checks on mutating business operations
- Privileged actions audited
- Package template changes do not silently rewrite live entitlements without explicit policy

## Dependencies & Related Modules

- [`auth.md`](auth.md) — employee auth (reuse hasher/JWT patterns; separate platform login)
- [`roles.md`](roles.md) — company RBAC; do not conflate with platform Super Admin
- [`stores.md`](stores.md) — store limit enforcement
- [`employee.md`](employee.md) — must hide platform Super Admin accounts
- [`settings.md`](settings.md) — platform settings / theme keys
- Root: `FUTURE.md`, `DATABASE.md`, `ARCHITECTURE.md`

## Notes

- Spec mentions React customer app + ASP.NET Web API — those are **out of this repo’s current footprint**. Hold e-commerce entitlement enforcement for when USER/API surfaces exist; Admin-side entitlement still applies.
- `ADMIN.md` Auth section may lag; see `auth.md` for current auth status.
- **Hold status:** analysis + context only. No controllers, tables, or OTP code until implementation is requested.
