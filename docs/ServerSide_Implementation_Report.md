# SmartCart Pro — Server-Side Implementation Report

**Branch:** `Pro_3`  
**Host:** `SmartCartPro` (ASP.NET Core 9 MVC)  
**Date:** 2026-10-19  
**Scope:** Server only — no `Views/**`, `wwwroot/**`, or `SmartCart.Customer` changes  

---

## 1. Audit matrix

| Module | Before | After | Evidence |
|--------|--------|-------|----------|
| Company | Complete | Complete | `CompanyController`, `ICompanyService`, `CompanyModels.cs` |
| Store | Complete | Complete | `StoreController` (+ `SetCurrentStore`), `StoreModels.cs` |
| License / Modules | Partial | Complete | `LicenseController`, `ModuleController`, `LicenseModels.cs`, `ModuleModels.cs`, seed `006` |
| Auth / Platform claim | Partial | Partial→Complete (core) | JWT cookie + OTP; Forgot/Reset still stubs |
| Users / Employees | Missing | Complete | `EmployeeController`, `UserModels.cs`, `UserRepository` |
| Roles / Permissions | Missing | Complete | `RoleController`, filters `RequirePermission`, seed permissions |
| Category / Subcategory / Brand / Unit / Tax | Partial/Missing | Complete | `CatalogController`, `CatalogModels.cs` |
| Product / Variant / SKU | Partial | Complete | `ProductController`, `ProductModels.cs` |
| Inventory / Movements | Missing | Complete | `InventoryController`, `StockRepository.ApplyMovementAsync`, `InventoryModels.cs` |
| Purchasing / Supplier / PO / GRN | Missing | Complete | `SupplierController`, `PurchaseController`, V005 |
| POS / Sales / Customers / Returns | Missing | Complete | `SalesController`, `CustomersController`, `SaleModels.cs` |
| Online orders | Missing | Complete | `OnlineOrderController`, stock on confirm/cancel, V007 |
| Cart / Wishlist | Missing | Complete (JSON services) | `CartController` JSON, `CartModels.cs` |
| Reviews | Missing | Complete | `ProductReviewController` + purchase/7-day rules, V007 |
| Reports | Partial | Complete | `ReportsController` JSON actions, `ReportService` |
| Settings | Missing | Complete | `SettingController`, `SettingModels.cs` |
| Audit log | Partial (write only) | Complete | `AuditController` list + JSON |
| ModuleAccess / License / Permission filters | Missing | Complete | `Filters/*`, global `LicenseGateFilter` |
| Document numbering | Missing | Complete | `DocumentNumberRepository` + `invoice_sequences` |
| Campaigns | Missing | **Skipped** | No `campaigns` table |
| Multi-company DB provisioning | Missing | **Skipped / proposed** | Single DB per install today; see §8 |
| Customer React / new API project | N/A | **Not created** | Per rules; cart/wishlist services reusable |

---

## 2. SQL scripts (run order)

### Create (`01_Tables` via `create.sql` / V001)
Existing numbered tables `001`–`043` (company through auth OTP). No rewrite.

### Seeds (`02_Seed`) — apply after tables
| File | Purpose |
|------|---------|
| `001_DefaultRoles.sql` | Default roles |
| `002_DefaultPermissions.sql` | Modules + permissions + SUPER_ADMIN grants |
| `003_DefaultSettings.sql` | Default settings keys |
| `004_PlatformSuperAdmin.sql` | Dev platform Super Admin user |
| `005_ModulePresets.sql` | Basic / Premium presets |
| **`006_DevTrialLicense.sql`** | **New** — idempotent trial license (1 yr, max stores 5) |

### Patches (`03_Patches`) — apply in order
| Patch | Purpose |
|-------|---------|
| V001 | Initial schema |
| V002 | Company/store relationship notes |
| V003 | Platform Super Admin + OTP |
| V004 | License + installation modules + presets |
| **V005** | Purchase status CHECK extension |
| **V006** | `pos.cancel` permission |
| **V007** | `product_reviews.fk_onlineorderitem` + unique index |

Do **not** run EF migrations / `EnsureCreated`. Use project `ApplyPatches.bat` (or equivalent) manually.

---

## 3. Files created / modified (by area)

### Cross-cutting
- `Filters/ModuleAccessAttribute.cs`, `LicenseGateFilter.cs`, `RequirePermissionAttribute.cs`
- `Repositories/DocumentNumberRepository.cs`, `Services/DocumentNumberService.cs`
- `Program.cs`, `Extensions/ServiceCollectionExtensions.cs`, `Data/SmartCartDbContext.cs`

### Phase 1–2
- Controllers: `License`, `Module`, `Employee`, `Role`, `Dashboard`
- Models: `ModuleModels`, `UserModels`, `RoleModels`, `DashboardModels`
- Entities: `Permission`, `RolePermission`, `InvoiceSequence`

### Phase 3–6
- Controllers: `Catalog`, `Product`, `Inventory` (rewritten), `Supplier`, `Purchase`, `Customers`, `Sales` (rewritten)
- Models: `Catalog`, `Product`, `Inventory`, `Supplier`, `Purchase`, `Customer`, `Sale`
- Stock engine: `StockRepository.ApplyMovementAsync` (sole stock writer)
- Entities: Brand, Unit, Tax, ProductVariant, StockMovement, Supplier, Purchase*, Customer, Sale*, Payment, CashSession, SalesReturn*

### Phase 7
- Controllers: `OnlineOrder`, `ProductReview`, `Cart` (JSON), `Setting`, `Audit`, `Reports` (expanded)
- Models: `OnlineOrder`, `ProductReview`, `Cart`, `Report`, `Setting` (+ Audit list models)
- Patch V007; context docs under `context/ADMIN/modules/`

**Not touched:** `Views/**`, `wwwroot/**`, `SmartCart.Customer`, platform React.

---

## 4. Controller action list (UI driver)

| Controller | Action | Verb | Returns | Module / Permission |
|------------|--------|------|---------|---------------------|
| **Auth** | Login, VerifyOtp, ResendOtp, ChangePassword, Logout | GET/POST | View / Redirect | Anonymous / Auth |
| **Company** | Index, Save | GET/POST | View `Index` | Auth; create gated by platform claim |
| **Store** | Index, Create, Edit, SetCurrentStore | GET/POST | View / Redirect | Create: `PlatformSuperAdmin` |
| **License** | Index, Save, Suspend, Activate | GET/POST | View | `PlatformSuperAdmin` |
| **Module** | Index, Save, ApplyPreset | GET/POST | View | `PlatformSuperAdmin` |
| **Employee** | Index, Create, Edit, Deactivate, ResetPassword, AssignStores, AssignRole | GET/POST | View | `users.manage` |
| **Role** | Index, Create, Edit, Delete | GET/POST | View | `users.manage` / PSA |
| **Dashboard** | Summary | GET | JSON | Auth |
| **Catalog** | Categories/SubCategories/Brands/Units/Taxes CRUD + Lookups | GET/POST | View / JSON | `PRODUCTS` + products.* |
| **Product** | Index, Create, Edit, Variants, FindByBarcode | GET/POST | View / JSON | `PRODUCTS` |
| **Inventory** | Opening, Adjust, Transfer, History, LowStock | GET/POST | View | `INVENTORY` |
| **Supplier** | Index, Create, Edit | GET/POST | View | `PURCHASES` |
| **Purchase** | Index, Create, Edit, Approve, Receive (GRN), Cancel | GET/POST | View | `PURCHASES` |
| **Customers** | Index, Create, Edit, SearchByPhone | GET/POST | View / JSON | `POS` / customers.* |
| **Sales** | Pos, CreateSale, Invoices, Cancel, Returns, Session*, DayClose, InvoiceJson, CashSummaryJson | GET/POST | View / JSON | `POS` + pos.sell / pos.return / pos.cancel |
| **OnlineOrder** | Index, Details, UpdateStatus | GET/POST | View | `POS` |
| **ProductReview** | Index, Hide | GET/POST | View | Admin review moderation |
| **Cart** | GetCart, UpsertItem, ClearCart, GetWishlist, ToggleWishlist | GET/POST | JSON | Reusable storefront |
| **Reports** | SalesByDate, StockValuation, LowStock, TaxSummary, TopSelling (+ legacy shells) | GET | JSON / View | `REPORTS` |
| **Setting** | Index, Save | GET/POST | View | `SETTINGS` |
| **Audit** | Index, ListJson | GET | View / JSON | `settings.manage` |
| **Admin** | Employees, Roles, Settings | GET | Legacy shells | Prefer Employee/Role/Setting controllers |
| **Filter** | GetFilterData | GET | JSON | Existing dropdown API |

---

## 5. Views required (for UI task)

Actions return `View(model)` even when `.cshtml` is missing. Needed views/partials:

| View path | Model type |
|-----------|------------|
| `Views/License/Index.cshtml` | `InLicenseUpdateView` |
| `Views/Module/Index.cshtml` | `InModuleAssignView` (or Output list + assign form) |
| `Views/Employee/Index|Create|Edit.cshtml` (+ assign stores/role) | `InUserCreateView` / `InUserUpdateView` / `OutputUserList` |
| `Views/Role/Index|Create|Edit.cshtml` | Role In/Input views |
| `Views/Catalog/Categories|CreateCategory|EditCategory.cshtml` (+ Sub/Brand/Unit/Tax) | Catalog In*View / Output*List |
| `Views/Product/Index|Create|Edit.cshtml` (+ variants) | Product models |
| `Views/Inventory/Opening|Adjust|Transfer|History|LowStock.cshtml` | Inventory models |
| `Views/Supplier/*`, `Views/Purchase/*` | Supplier / Purchase models |
| `Views/Customers/*` | Customer models |
| `Views/Sales/Pos|Invoices|Returns|…` | Sale models / Output |
| `Views/OnlineOrder/Index|Details.cshtml` | Online order models |
| `Views/ProductReview/Index.cshtml` | Review list output |
| `Views/Setting/Index.cshtml` | Setting models |
| `Views/Audit/Index.cshtml` | `OutputAuditLogList` |
| Existing Company/Store views | Already present |

JSON-only: `CartController`, many report/dashboard endpoints — no Razor required.

---

## 6. Business rules implemented

| Rule | Implementation |
|------|----------------|
| Super Admin creates stores; admin edits | `StoreController` + `PlatformSuperAdmin` policy; license `MaxStores` |
| License expired → read-only; suspended → blocked | Global `LicenseGateFilter` |
| Module entitlement | `[ModuleAccess("…")]` vs `installation_modules` |
| Permission per action | `[RequirePermission("…")]` |
| Stock only via central method | `IStockRepository.ApplyMovementAsync` — movement + inventory same TX; **no negative stock** |
| PO / GRN / Invoice numbers | `IDocumentNumberService` + `invoice_sequences` (`FOR UPDATE`) |
| GRN partial receive | Purchase receive path; status partially_received / received |
| POS totals | Recalculated server-side; stock `sale` per line; cancel reverses; returns `return_in` |
| Idempotency | Sale `ClientRef` (where modeled) |
| Online confirm/cancel stock | Confirm → `sale`; cancel after confirm → `return_in` |
| Reviews | Purchased only; one per order item; edit/delete ≤ 7 days; admin hide |
| Super Admin hidden from employee lists | User queries exclude `is_platform_super_admin` |
| Passwords | Identity `PasswordHasher`; never logged |
| Anti-forgery | `[ValidateAntiForgeryToken]` on POST form actions |

---

## 7. Skipped / blocked

| Item | Why |
|------|-----|
| All Razor UI / menus / theme | Explicitly out of scope |
| New Web API / SmartCart.API expansion | Forbidden; cart/wishlist exposed as MVC JSON |
| Campaigns / promotions | No table in schema |
| Multi-DB company provisioning | Current model = one company DB per connection string; see §8 |
| Forgot/Reset password POST | Still GET stubs |
| Hold bill UI storage | No dedicated hold table; skipped |
| Variant attribute matrix (Size×Colour generator) | SKU/variant CRUD exists; combinatorial generator not built |
| AdminController shells | Superseded by Employee/Role/Setting; left as legacy View() |

---

## 8. Open questions / decisions needed

1. **Platform registry / multi-company provisioning** — Propose separate `smartcart_platform` DB (companies, connection strings, licenses) and provisioning job that creates company DB + runs scripts. Confirm before implementing.
2. **Negative stock** — Currently blocked. Setting key `allow_negative_stock` can override if you want it wired into `ApplyMovementAsync`.
3. **Online stock deduction point** — Implemented on **Confirm**. Prefer Placed vs Packed instead?
4. **OnlineOrder ModuleAccess** — Currently `"POS"`. Add dedicated `ORDERS` module in seed?
5. **Customers ModuleAccess** — Controllers use `POS`; seed has `CUSTOMERS` module — align UI/permissions?
6. **Purchase return to supplier** — Movement type exists; dedicated return document flow may need polish.
7. **AuthService still uses `User` entity** for Identity hasher — acceptable exception or move hashing into AuthRepository only?

---

## 9. Manual test steps (after UI)

1. **Seed/patch** — Apply seeds 001–006 and patches V001–V007 on a fresh DB. Login as platform Super Admin.
2. **License/Modules** — `/License` save trial; `/Module` enable PRODUCTS, INVENTORY, PURCHASES, POS, etc.
3. **Company/Store** — Save company; create store (Super Admin); edit as normal admin; `SetCurrentStore`.
4. **Employees/Roles** — Create employee, assign role + store; confirm Super Admin not listed.
5. **Catalog/Product** — Create category → product → variant; barcode search JSON.
6. **Stock** — Opening stock; adjust; transfer (2 stores); low stock list; verify movement rows.
7. **Purchase** — Supplier → PO approve → partial GRN → stock increased; cancel PO.
8. **POS** — Open cash session; sell; invoice JSON; return; cancel invoice; day close.
9. **Online** — Place order (via service/status); Confirm deducts stock; Cancel restores.
10. **Review** — Create review for purchased item; hide as admin; reject second review same order item.
11. **Reports/Settings/Audit** — Hit JSON report endpoints; change a setting; list audit.
12. **License gate** — Suspend license → POS create blocked; expire → GET ok, POST blocked.

---

## 10. Commits on `Pro_3`

| Commit | Message |
|--------|---------|
| `8513bb6` | Phase 1: License, modules, gates and document numbering server side. |
| `3e613c4` | Phase 2: Employees, roles, permissions, dashboard and store switch server side. |
| `8d7f07f` | Phase 3: Catalog and product configuration server side. |
| `6f24909` | Phase 4: Inventory stock movements and adjustments server side. |
| `dbc7d95` | Phase 5: Supplier, purchase order and GRN server side. |
| `4d193ac` | Phase 6: POS sales, customers, returns and cash sessions server side. |
| `0b7972a` | Phase 7: Online orders, reviews, cart, reports, settings and audit server side. |
| *(this)* | Phase 8: Server-side implementation report. |

**Build:** `dotnet build SmartCartPro/SmartCartPro.csproj` → **0 errors, 0 warnings** (verified after Phase 7).
