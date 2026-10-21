# Admin Module — Stores

## Module Overview
- **Area:** ADMIN
- **Module Name:** Stores
- **Current Status:** `IMPLEMENTED` (server + AJAX UI). Written without a build or a browser run; the first build and click-through still has to confirm it (see Notes).

## Purpose & Responsibilities
Store master for the company: list, create (platform only), edit, activate/deactivate, and the header store switcher for users with more than one accessible store. Exactly one default store. No hard delete.

## Implemented Features
- `Store/Index` list: search (name/code/city), status filter, sorting, paging, Default badge, license usage subtitle (`x of y` from `license.maxstores`), Edit, Activate / Deactivate (`data-sc-post`).
- `Store/Create` (platform Super Admin): full-page form. MaxStores enforced. First store is always default.
- `Store/Edit`: code, name, contact, address, default and active flags.
- `Store/SetActive` (POST): activate/deactivate from the list. Deactivating the current default throws `ConflictException`.
- `Store/SetCurrentStore` (POST, signed-in): verifies `user_store_access`, re-issues the JWT cookie via `IAuthService.SwitchStoreAsync` + `SetAuthCookie`, returns `SuccessResponse` (no redirect).
- Header `StoreSwitcher` view component: nothing for platform Super Admin; current-store badge when the user has one store; dropdown when they have more than one (`data-sc-post` `Store/SetCurrentStore`, `data-sc-after=reload`).

## Routes & Permissions
`settings.manage` for list/edit/activate. Create is `PlatformSuperAdmin` policy. SetCurrentStore is `[Authorize]` only (any signed-in user with access to that store).

| Method | Route | Action | Notes |
|---|---|---|---|
| GET | `/Store` | `Index` | query: `Search`, `ActiveOnly`, `SortBy`, `SortDesc`, `Page`, `PageSize` |
| GET / POST | `/Store/Create` | `Create` | platform Super Admin; POST `{ id, redirectUrl }` |
| GET / POST | `/Store/Edit/{id}` | `Edit` | POST `{ id, redirectUrl }` |
| POST | `/Store/SetActive` | `SetActive` | `id`, `isActive` |
| POST | `/Store/SetCurrentStore` | `SetCurrentStore` | `storeId`; `{ storeId, storeName }` |

## Filters / Search / Sorting
- Search: store name, code, city (contains).
- Status: all / active / inactive.
- Sort: `storename` (default, default-store first), `storecode`, `city`, `isdefault`, `isactive`. Page size 10 / 25 / 50 / 100.

## Data
- Entity (repository only): `Store` → `store`. Access rows: `UserStoreAccess` → `user_store_access`.
- Models: `Models/StoreModels.cs` — create/update/select/list, `InputStoreStatusUpdate` / `OutputStoreStatusUpdate`, `InputStoreAccessibleList` / `OutputStoreAccessibleList`.

## Database Dependencies
- `002_Store.sql`: unique store code per company; partial unique one default among non-cancelled rows; default store must stay active.
- License `maxstores` caps active stores.

## API & Data Access
- `StoreController` → `ICompanyService` / `CompanyService` → `ICompanyRepository` / `CompanyRepository`.
- `ListAccessibleStoresAsync` on service and repo (no Entities leave the repo).
- `StoreSwitcherViewComponent` → `ICompanyService.ListAccessibleStoresAsync`. View: `Views/Shared/Components/StoreSwitcher/Default.cshtml` (the only Shared file this module adds). `_Layout` already invokes the component; this module does not edit the layout.
- Default promotion uses a transaction and two `SaveChanges` passes so the partial unique index is respected.

## UI Rules & Conventions
- Views: `Views/Store/Index.cshtml`, `Create.cshtml`, `Edit.cshtml`, `_StoreCreateForm.cshtml`, `_StoreUpdateForm.cshtml`.
- JS: `wwwroot/js/admin/store.js` on `sc-ajax.js`. List toggles are declarative (`data-sc-post` + confirm).
- Switcher is a `<details>` dropdown (no `window.bootstrap`).

## Business Rules
- Create is platform-only. Active count must be below MaxStores; an active license (or trial) that is not expired is required.
- Exactly one default. Un-flagging or deactivating the current default without promoting another is `ConflictException`.
- Promoting a store to default unsets the previous default first, then sets the new one.
- Activate is refused when it would exceed MaxStores.
- Store switcher and SetCurrentStore require an active, non-cancelled `user_store_access` row (same rule as `AuthRepository.GetStoreAccessAsync`).
- Audit (`Store` entity): Create, Update, Activate, Deactivate.
- Soft deactivate only — no hard delete / cancel from the UI.

## Dependencies & Related Modules
- `company.md`, `license.md`, `employee.md` (store access picker), `auth.md` (JWT `fk_store` claim), `settings.md`, `audit.md`.

## Notes
- Sample `Admin/Stores` UI is gone; this module owns `StoreController` only.
- `SwitchStoreAsync` already checks `user_store_access` and `store.isactive`. SetCurrentStore checks access again through `ListAccessibleStoresAsync` before calling it.
- Remember-me on switch is taken from the JWT `remember_me` claim set at sign-in, so a session cookie stays a session cookie.
- Not built or run when this was written: compile and click-through checks are still open.
