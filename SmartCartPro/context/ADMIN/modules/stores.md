# Admin Module — Stores

## Module Overview
- **Area:** ADMIN
- **Module Name:** Stores
- **Current Status:** `IMPLEMENTED` (server + AJAX UI). Written without a build or a browser run; the first build and click-through still has to confirm it (see Notes).

## Purpose & Responsibilities
Store master for the company: list, create (platform only), edit, activate/deactivate, and the header store switcher. Super Admin sees every active store; other users see stores they can access. Exactly one default store. No hard delete.

## Implemented Features
- `Store/Index` list: search (name/code/city), status filter, sorting, paging, Default badge, license usage subtitle (`x of y` from `license.maxstores`), Edit, Activate / Deactivate (`data-sc-post`).
- `Store/Create` (platform Super Admin): full-page form. MaxStores enforced. First store is always default.
- `Store/Edit`: code, name, contact, address, default and active flags.
- `Store/SetActive` (POST): activate/deactivate from the list. Deactivating the current default throws `ConflictException`.
- `Store/SetCurrentStore` (POST, signed-in): Super Admin may enter any active store; other users need `user_store_access`. Re-issues the JWT cookie via `IAuthService.SwitchStoreAsync` + `SetAuthCookie`, returns `SuccessResponse` (no redirect).
- Header `StoreSwitcher` view component: Super Admin lists all active stores; other users list accessible stores. Current-store badge when there is one store; dropdown when there is more than one (`data-sc-post` `Store/SetCurrentStore`, `data-sc-after=reload`).

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
- Create is platform-only. Super Admin is not blocked by a missing license. MaxStores applies only when a license row exists.
- Exactly one default. Un-flagging or deactivating the current default without promoting another is `ConflictException`.
- Promoting a store to default unsets the previous default first, then sets the new one.
- Activate is refused when it would exceed MaxStores.
- Store switcher and SetCurrentStore: Super Admin uses any active store; other users need an active, non-cancelled `user_store_access` row.
- Audit (`Store` entity): Create, Update, Activate, Deactivate.
- Soft deactivate only — no hard delete / cancel from the UI.

## Dependencies & Related Modules
- `company.md`, `license.md`, `employee.md` (store access picker), `auth.md` (JWT `fk_store` claim), `settings.md`, `audit.md`.

## Notes
- Sample `Admin/Stores` UI is gone; this module owns `StoreController` only.
- `SwitchStoreAsync` uses any active store for Super Admin, and `user_store_access` for everyone else. SetCurrentStore checks the same list through `ListAccessibleStoresAsync` before calling it. Creating a store switches Super Admin into that store.
- Remember-me on switch is taken from the JWT `remember_me` claim set at sign-in, so a session cookie stays a session cookie.
- Not built or run when this was written: compile and click-through checks are still open.
