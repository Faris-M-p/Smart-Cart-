# Admin Module — Settings

## Module Overview
- **Area:** ADMIN
- **Module Name:** Settings
- **Current Status:** `IMPLEMENTED` (server + AJAX UI). Written without a build or a browser run; the first build and click-through still has to confirm it (see Notes).

## Purpose & Responsibilities
Edit the known configuration keys of the `settings` table. Company scope holds the defaults; store scope lets a store override the keys that allow it. Only the keys of the built-in catalog can be saved.

## Implemented Features
- `Setting/Index` shows the keys grouped by purpose (**Point of sale**, **Localisation**). Each row has the control, a plain-language description, the key, the value in force with its source (Store override / Company value / Built-in default), the built-in default, and how many stores override it (company tab).
- Two tabs: **Company defaults** and **Store: <name>** (the store the user is signed in to). A user without a store (platform Super Admin) sees an explanation panel on the store tab instead of an error.
- `Setting/Save` (POST, AJAX): validates every posted value, writes only what changed, redirects back to the same tab with a toast ("Settings saved." or "No changes to save.").
- Store tab: "Use company setting" (or an empty field) removes the store override so the company value applies again.
- Unsaved-changes hint and a warning when leaving the page with edits.

## Routes & Permissions
Module `SETTINGS`, permission `settings.manage` for both actions.

| Method | Route | Action | Notes |
|---|---|---|---|
| GET | `/Setting?scope=company\|store` | `Index` | `scope` defaults to `company`; `store` uses the store from the sign-in claims |
| POST | `/Setting/Save` | `Save` | form fields `Scope` and `Values[<setting.key>]` |

## Known Keys
| Key | Control | Scope | Default | Rules |
|---|---|---|---|---|
| `pos.invoice_prefix` | text (max 20) | company + store override | `INV` | optional (empty = no prefix); letters, digits and `. _ / -` |
| `pos.allow_negative_stock` | Allowed / Blocked | company + store override | `false` (Blocked) | stored as `true` / `false`; read by `StockRepository` for POS sales |
| `default_tax_code` | select of active taxes | company + store override | none | optional; an already saved value that is no longer active stays selectable |
| `locale.currency` | text with suggestions | company only | `INR` | required; 3 letters, stored upper-case |
| `locale.timezone` | text with suggestions | company only | `Asia/Kolkata` | required; suggested list or any IANA id the server knows |

## Data
- Entity: `Setting` → `settings` (repository only). Columns used: `setting_scope` (`company` | `store`), `fk_store`, `settingkey`, `settingvalue`, `valuetype` (`string` | `bool`), `description`.
- Models: `Models/SettingModels.cs` — `InputSettingList` / `OutputSettingList` (stored rows), `InputSettingSelect` / `OutputSettingSelect` (page model with groups and items), `InSettingUpsertView` / `InputSettingUpsert` / `OutputSettingUpsert`.

## Database Dependencies
- `ck_settings_store_scope` requires `fk_store` for store rows. The unique constraint does not de-duplicate company rows (NULL store), so reads use the lowest `id_setting` when duplicates exist.
- The seed `003_DefaultSettings.sql` inserts the company defaults; a key without a row falls back to the built-in default of the catalog.

## API & Data Access
- `SettingController` → `ISettingService` / `SettingService` → `ISettingRepository` / `SettingRepository`. The service no longer touches the DbContext.
- Other modules: `ISettingService.GetValueAsync(key, scope, fkStore)` (exact row, no fallback; signature unchanged) and the new `GetEffectiveValueAsync(key, fkStore)` (store override → company value → built-in default).

## UI Rules & Conventions
- View: `Views/Setting/Index.cshtml`. JS: `wwwroot/js/admin/setting.js` on top of `sc-ajax.js`.
- Field names are `Values[<setting.key>]`; server errors use the same key, so messages appear under the right control.
- The store tab lists only the keys that allow an override.

## Business Rules
- Unknown keys, or a store-scope save of a company-only key, are rejected ("One or more settings are not recognised. Reload the page and try again.").
- Store scope always uses the store of the signed-in user (claim); a posted store id is never trusted. Store scope without a store is refused.
- Company scope: a key without a saved value shows its built-in default; posting that default back is not a change. An empty optional value is stored as an empty string.
- Store scope: an empty value deletes the override row (settings has no cancel column). `StockRepository` reads the store row first and treats an empty value as `false`, so a cleared override must never remain as a row.
- Audit (`IAuditService`, entity `Setting`): one `Update` entry per save with old → new values (truncated to 900 characters).

## Dependencies & Related Modules
- `pos.md` / `inventory.md` (negative stock), `tax.md` (default tax list), `stores.md`, `audit.md`.

## Notes
- Consumed keys: `pos.allow_negative_stock` (stock movements), `pos.invoice_prefix` (SALE document numbers), `default_tax_code` (POS tax fallback), `locale.timezone` (report/dashboard business day), `locale.currency` (report headers).
- The seeded key `inventory.track_variants` is not part of the catalog, so it is not editable here.
- A store override cannot be an explicit empty value (empty means "use the company value").
- Not built or run when this was written: compile and click-through checks are still open.
