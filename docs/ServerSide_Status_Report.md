# SmartCart Pro — Server-Side Status Audit & Gap Report

**Scope:** `SmartCartPro/` (ASP.NET Core 9 MVC) unless noted  
**Date:** 2026-10-19  
**Method:** Read-only code + SQL inspection. No app run, no build, no DB execution.  
**Prior report compared:** `docs/ServerSide_Implementation_Report.md` (claims Phase 1–8 “Complete” in several places — this audit revises that with evidence).  
**Recent commits:** `8513bb6`…`3977a96` (Phase 1–8), plus later validation/common-model/comments work on branch.

---

## 1. Summary

### Overall completion (planned checklist items)

| Phase | Approx. Complete | Partial | Missing / Unverified | Rough % Complete* |
|-------|------------------|---------|----------------------|-------------------|
| 1 Platform / Super Admin | 5 | 4 | 1 | **50%** |
| 2 Company foundation | 7 | 2 | 1 | **70%** |
| 3 Product configuration | 6 | 3 | 1 | **60%** |
| 4 Inventory | 6 | 3 | 0 | **67%** |
| 5 Purchasing | 4 | 2 | 2 | **50%** |
| 6 POS & Billing | 7 | 3 | 2 | **58%** |
| 7 Online & others | 4 | 4 | 4 | **33%** |
| 8 Cross-cutting | 4 | 3 | 0 | **57%** |
| **Weighted overall** | | | | **~54%** |

\*Complete = SQL + models + repo + service (where used) + controller + auth wired. UI absence alone does not demote Complete.

### Top 10 critical gaps

1. **Multi-company DB provisioning** — not implemented in SmartCartPro (single install DB). `platform/` sketch exists separately; not wired to SmartCartPro registration.
2. **First Company Admin auto-create** on company bootstrap — missing (`user_invitations` table in V004; no invitation controller in SmartCartPro).
3. **Company + default store not one transaction** — `CompanyRepository` can leave company without store.
4. **Inter-store transfer non-atomic** — `StockRepository` runs two `ApplyMovementAsync` calls (two transactions).
5. **`pos.allow_negative_stock` unused** — hard block in `StockRepository.ApplyMovementAsync` always.
6. **Purchase return** — no debit-note / vendor return flow.
7. **POS hold / resume** — no park-sale.
8. **Over-receipt guard weak** — GRN UI sets `AlreadyReceivedQty = 0` in `PurchaseController.Receive` GET.
9. **CartController / AdminController** — no `[Authorize]` (security).
10. **FilterController uses `[ApiController]`** — violates plain-MVC rule.

---

## 2. Status matrix — planned scope

### Phase 1 — Platform / Super Admin

| # | Feature | Status | Evidence | Missing parts | Remarks |
|---|---------|--------|----------|---------------|---------|
| 1.1 | Platform Super Admin identity + isolation | **Partial** | Seed `02_Seed/004_PlatformSuperAdmin.sql`; claim helpers `Helpers/AuthClaims.cs`; policy `PlatformSuperAdmin` in DI; `LicenseGateFilter` bypass | Separate platform identity DB / multi-tenant Super Admin app not in SmartCartPro | Super Admin is a **user in the same company DB**, not a separate platform DB |
| 1.2 | Company register / list / edit / details | **Partial** | `CompanyController`, `CompanyService`, `CompanyRepository`, `CompanyModels.cs`, `001_Company.sql` | No company **list** of many tenants; single-company install model | Edit/create via `Save` on shared view |
| 1.3 | Company DB provisioning from SQL | **Missing** | — | No provisioning service that creates a new DB per company | `platform/` has tenancy sketches; SmartCartPro uses one connection |
| 1.4 | Default store auto-created on registration | **Partial** | Store create path in `CompanyService.CreateStoreAsync` / bootstrap flows | Not proven atomic with company create | Depends on 1.2 bootstrap path |
| 1.5 | Max stores + Super Admin-only create | **Complete** | `StoreController.Create` `[Authorize(Policy=PlatformSuperAdmin)]`; `CompanyService.CreateStoreAsync` checks license `MaxStores` + `CanCreate` | — | Admin edit allowed via `Edit` without create policy |
| 1.6 | License start/expiry/status/suspend/activate | **Complete** | `LicenseController` Save/Suspend/Activate; `V004__LicenseAndInstallation.sql`; `CommonModel.LicenseStatus`; seed `006_DevTrialLicense.sql` | No dedicated “renew” action (save extends dates) | Status int in view; TEXT in DB |
| 1.7 | Module templates + per-company selection | **Complete** | Seed `005_ModulePresets.sql`; `ModuleController` Index/Save/ApplyPreset; V004 tables | “Custom” is free selection, not a named preset row | Basic/Premium seeds present |
| 1.8 | First Company Admin + hashed password + Admin role | **Partial** | Roles seed; `EmployeeController.Create` + password hash in user service | No auto-create on company register; invitation tables unused by controllers | Manual employee create only |
| 1.9 | Registration failure = no half company | **Partial** | EF `SaveChanges` per operation | No explicit compensating transaction across company+store+admin | Risk of orphan company |
| 1.10 | Platform audit log | **Partial** | `audit_logs` `033_AuditLogs.sql`; writes from company/store/license services; `AuditController` | Not a separate “platform” log store | Same DB audit table |

### Phase 2 — Company foundation

| # | Feature | Status | Evidence | Missing parts | Remarks |
|---|---------|--------|----------|---------------|---------|
| 2.1 | Login / logout / change password | **Complete** | `AuthController` Login, Logout, ChangePassword; OTP verify | — | Cookie JWT pattern |
| 2.2 | Reset password | **Missing** | `AuthController.ForgotPassword` / `ResetPassword` return `View()` only (L218–226); no POST handlers, no token service | Email send, token persist, password update | Views exist (`Views/Auth/ForgotPassword.cshtml`, `ResetPassword.cshtml`) — UI only |
| 2.3 | Company context (connection per company) | **Missing** in SmartCartPro | Single `SmartCartDbContext` connection | Tenant resolver | Exists as sketch under `platform/` |
| 2.4 | Store context / switch / user–store access | **Complete** | `009_UserStoreAccess.sql`; `StoreController.SetCurrentStore`; `IAuthService.SwitchStoreAsync` | — | |
| 2.5 | Store edit by Company Admin | **Complete** | `StoreController.Edit` `[Authorize]` without SuperAdmin-only | — | Create still Super Admin only |
| 2.6 | Users/employees CRUD, activate, store assign | **Complete** | `EmployeeController`, `UserModels.cs`, user repo/service | — | |
| 2.7 | Roles CRUD + permissions + mapping | **Complete** | `RoleController`; seeds `001`/`002`; `RolePermission` entities | — | |
| 2.8 | Filters: Permission, ModuleAccess, License, Store, Platform | **Partial** | `RequirePermissionAttribute`, `ModuleAccessAttribute`, `LicenseGateFilter` | Dedicated StoreAccess filter not universal on all actions; Platform = policy attribute | Store often via claim `FkStore` |
| 2.9 | Dashboard summary | **Complete** | `DashboardController`, `DashboardModels`, dashboard service | Low-stock count may use `<= 0` vs reorder (verify in service) | |
| 2.10 | Company settings | **Complete** | `SettingController`, `032_Settings.sql`, known keys | — | |

### Phase 3 — Product configuration

| # | Feature | Status | Evidence | Missing parts | Remarks |
|---|---------|--------|----------|---------------|---------|
| 3.1 | Category / SubCategory / Brand / Unit / Tax | **Complete** | `CatalogController` + `CatalogModels` + tables 010–014 | — | |
| 3.2 | Variant + variant **values** | **Partial** | `016_ProductVariants.sql`; `ProductController` Variants CRUD | No attribute/value dimension tables; free-text variant name | |
| 3.3 | Product CRUD | **Complete** | `ProductController`, `015_Products.sql` | — | |
| 3.4 | SKU per product/variant + combination generation | **Partial** | `Sku` / `SkuCode` fields on product & variant | No server-side combinatorial SKU generator | Manual variants only |
| 3.5 | SKU uniqueness, prices, reorder | **Partial** | Fields + validation on In…View; DB unique on variant barcode | Company-wide SKU uniqueness enforcement Unverified in repo | |
| 3.6 | Lookups | **Complete** | Catalog `*Lookup` actions; supplier lookup | — | |

### Phase 4 — Inventory

| # | Feature | Status | Evidence | Missing parts | Remarks |
|---|---------|--------|----------|---------------|---------|
| 4.1 | Store stock per product/variant | **Complete** | `021_Inventory.sql`; list via `InventoryController` | — | Not separate “SKU” table |
| 4.2 | Movement ledger | **Complete** | `022_StockMovements.sql`; written in `ApplyMovementAsync` | — | |
| 4.3 | Central stock update + transaction | **Complete** | `IStockRepository.ApplyMovementAsync`; `BeginTransactionAsync` | Transfer uses **two** calls | Single-movement path OK |
| 4.4 | Opening stock | **Complete** | `InventoryController.Opening`; StockService | Service takes View model (arch note) | |
| 4.5 | Adjustment + reason | **Complete** | `Adjust` + Direction add/remove | No approval workflow | |
| 4.6 | Transfer between stores | **Partial** | `TransferStockAsync` | Not one shared DB transaction for out+in | Data-consistency risk |
| 4.7 | Low-stock list | **Complete** | Inventory / Reports low-stock | — | |
| 4.8 | Stock history | **Complete** | `InventoryController` history action + movements | — | |
| 4.9 | Negative stock rule | **Partial** | Hard reject `newQty < 0` in `StockRepository` L70–77 | Setting `pos.allow_negative_stock` not read | |

### Phase 5 — Purchasing

| # | Feature | Status | Evidence | Missing parts | Remarks |
|---|---------|--------|----------|---------------|---------|
| 5.1 | Supplier CRUD | **Complete** | `SupplierController`, `018_Suppliers.sql` | Dedicated deactivate endpoint Unverified (IsActive on form) | |
| 5.2 | Purchase order header/items/statuses/totals | **Complete** | `PurchaseController`, `023`/`024`, V005 statuses | — | |
| 5.3 | GRN partial + over-receipt block | **Partial** | `Receive` POST; stock via `ApplyMovementAsync` | Server over-receipt vs already-received **weak** (`AlreadyReceivedQty=0` in GET) | |
| 5.4 | Purchase return | **Missing** | — | Tables/controllers/services | |
| 5.5 | Document numbering concurrency-safe | **Complete** | `DocumentNumberRepository` + `041_InvoiceSequences.sql` (`FOR UPDATE` pattern per prior audit) | — | Used for sales/PO/GRN as wired |

### Phase 6 — POS & Billing

| # | Feature | Status | Evidence | Missing parts | Remarks |
|---|---------|--------|----------|---------------|---------|
| 6.1 | Customer walk-in + phone search | **Complete** | `CustomersController`, `SearchByPhone` | — | |
| 6.2 | Sale/invoice server totals + stock + TX | **Complete** | `SalesController.CreateSale`; `SaleRepository` + stock movements | — | |
| 6.3 | Double-submit / idempotency | **Partial** | `ClientRef` on `InSaleCreateView` | Implementation via Notes field (fragile) | |
| 6.4 | Item/bill discount + role max | **Partial** | Discount fields on lines | Role-based max discount **not** enforced server-side | |
| 6.5 | Split payments | **Complete** | `InPaymentView` list; `028_Payments.sql` | — | |
| 6.6 | Hold / resume bill | **Missing** | — | No hold tables/actions | |
| 6.7 | Sale return / refund qty guard | **Complete** | `SalesController.Returns`; return tables 029/030 | — | |
| 6.8 | Invoice cancel + stock reverse | **Complete** | `CancelSale`; permission `pos.cancel` V006 | — | |
| 6.9 | Invoice print data | **Partial** | `InvoiceJson` / details | Dedicated print DTO Unverified | JSON usable by UI |
| 6.10 | Day close / cash session | **Complete** | `OpenSession`/`CloseSession`/`DayClose`; `027_CashSessions.sql` | — | |

### Phase 7 — Online sales & others

| # | Feature | Status | Evidence | Missing parts | Remarks |
|---|---------|--------|----------|---------------|---------|
| 7.1 | SmartCart.API / customer endpoints | **Partial** | **Not** in SmartCartPro. Exists separately: `platform/src/SmartCart.API` (`StorefrontConfigController`, `LicenseController`); scaffold only | Cart/checkout/order APIs | Unwired to SmartCartPro; `[ApiController]` in platform (separate solution) |
| 7.2 | Online orders admin list/details/status | **Complete** | `OnlineOrderController`; tables 034/035; V007 | COD field missing | Status machine present |
| 7.3 | Stock deduct / cancel restore | **Complete** | `OrderRepository` calls `ApplyMovementAsync` on confirm/cancel | Deduction **point** = confirm (evidence) | |
| 7.4 | Cart / wishlist vs stock model | **Partial** | `CartController`, tables 036–039; services | No `[Authorize]`; stock reservation Unverified | |
| 7.5 | Reviews rules | **Complete** | `ProductReviewController` + service rules (purchase, 7-day, hide) V007 | — | |
| 7.6 | Campaigns / promotions | **Missing** | Explicitly deferred historically | Tables + module | |
| 7.7 | Reports suite | **Partial** | `ReportsController` + `ReportService`: SalesByDate, StockValuation, LowStock, TaxSummary, TopSelling | Purchase report; movement report | TaxSummary groups by store with `TaxCode="TOTAL"` placeholder (`ReportService.cs` L167–175) — not per tax rate |
| 7.8 | Audit log viewer | **Complete** | `AuditController` Index + ListJson | — | |

### Phase 8 — Cross-cutting

| # | Feature | Status | Evidence | Missing parts | Remarks |
|---|---------|--------|----------|---------------|---------|
| 8.1 | Common JSON + helpers | **Complete** | `CommonResponseModel` in `Models/CommonModel.cs`; `CommonHelpers.SuccessResponse` / `ValidateModelState` | — | |
| 8.2 | AJAX exception + JSON 401/403 | **Partial** | `AppExceptionHandler` + `ExceptionErrorMapper.WantsJsonResponse` | Permission `ForbidResult` may still be HTML | |
| 8.3 | Company / store isolation | **Partial** | Store via claims + user_store_access | Multi-company connection isolation Missing | Single-DB install |
| 8.4 | Logging | **Complete** | `AppExceptionHandler` `LogError` structured | — | |

---

## 3. Advanced features matrix

| Feature | Status | Evidence | Missing | Schema ready? |
|---------|--------|----------|---------|---------------|
| Barcode on product/variant | **Partial** | `Product.Barcode`, variant `barcode` + unique; `FindByBarcode` | Auto EAN gen; multi-barcode; scale barcodes; label print API | **Needs tables** for multi-barcode / label jobs |
| POS barcode lookup | **Complete** | `ProductController.FindByBarcode` | — | Yes |
| HSN / GST split / inclusive | **Missing** | Tax is single `TaxRate` | HSN, CGST/SGST/IGST | **Needs columns/tables** |
| MRP ≤ selling enforcement | **Partial** | MRP + SellingPrice fields | Server rule Unverified | Yes (rule only) |
| Price lists / store pricing / history | **Missing** | — | — | **Needs tables** |
| Batch + expiry + FEFO | **Missing** | — | — | **Needs tables** |
| Unit conversion / multi-unit sell | **Missing** | Unit master only | Conversion factors | **Needs tables** |
| Stock take / variance | **Missing** | — | — | **Needs tables** |
| Reorder suggestions | **Missing** | ReorderLevel field only | Velocity engine | Mostly Yes |
| Damaged/expired write-off | **Partial** | Adjustment can reduce | Dedicated type/reason codes | Mostly Yes |
| Valuation FIFO/WAC | **Missing** | StockValuation report uses cost snapshot | Method engine | **Needs design** |
| Offline idempotent POS | **Partial** | ClientRef | Robust store | Needs stronger unique key |
| Cashier shift | **Complete** | Cash sessions | — | Yes |
| Credit sale / outstanding | **Missing** | — | — | **Needs tables** |
| Loyalty / vouchers / schemes | **Missing** | — | — | **Needs tables** |
| Credit note returns | **Partial** | Sales return exists | Credit-note doc type | Partial |
| Round-off / e-invoice fields | **Missing** | — | — | **Needs columns** |
| Supplier payments / ageing | **Missing** | — | — | **Needs tables** |
| Purchase price history | **Missing** | — | — | **Needs tables** |
| Auto PO from reorder | **Missing** | — | — | Needs suggestions first |
| Import/export Excel | **Missing** | — | — | App feature |
| Notifications SMS/WhatsApp | **Missing** | — | — | External + tables |
| Per-company backup strategy | **Missing** | — | Ops runbook | Ops |
| Multi-language / multi-currency | **Partial** | Currency/locale settings | Full i18n | Partial |

---

## 4. Rule compliance

| Rule | Compliant? | Violations | Fix needed |
|------|------------|------------|------------|
| No `[ApiController]` / separate API controllers | **No** | `Controllers/FilterController.cs` L11 `[ApiController]` | Remove attribute; keep MVC JSON |
| No Admin/SuperAdmin in **controller names** | **No** | `AdminController.cs` | Rename/redirect to Employee/Role/Setting or deprecate shell |
| Entities never outside repositories | **Mostly** | Unverified leaks rare; platform API may use DbContext directly (outside SmartCartPro) | Keep SmartCartPro clean |
| In / Input / Output naming | **Yes** | — | Continue |
| One model file per module | **Yes** | — | Continue |
| POST AJAX → CommonResponseModel (no TempData redirect) | **Partial** | `AuthController` many `TempData`+`RedirectToAction`; `PurchaseController.Edit` GET guard uses TempData+Redirect | Auth is View/Redirect by design (exception); fix purchase guard to JSON if AJAX |
| No EF migrations / EnsureCreated | **Yes** | `SmartCartDbContext` comments forbid | Keep |
| One DB per company; no CompanyId on company-owned rows | **Partial** | `store.fk_company` exists (needed for single-DB multi-store under one company) | Accept for current single-tenant install; no `CompanyId` on products/sales |
| Super Admin only creates stores + limit | **Yes** | `StoreController.Create` policy; `CompanyService` MaxStores | — |
| Super Admin hidden from company user lists | **Yes** | `UserRepository.cs` L18 `.Where(u => !u.Cancelled && !u.IsPlatformSuperAdmin)`; same filter on Get/Update/Activate | — |
| Auth + permission + module + license + store on actions | **Partial** | `CartController`, `AdminController` no `[Authorize]`; store filter not global | Add auth + module where needed |
| Parameterized SQL / hashed passwords / anti-forgery | **Mostly** | EF parameterized; anti-forgery on mutating POSTs; Cart JSON lacks antiforgery by design | Harden Cart |
| Stock only via central method + movement | **Yes** (single path) | Transfer = two central calls | Wrap transfer in one TX |
| Server recalculation prices/tax/discount | **Partial** | Sale repo recalculates; discount **cap** missing | Enforce max discount |

---

## 5. SQL scripts review

### Run order (intended) — `SmartCartPro/Database-Postgres/`

**01_Tables** (via `create.sql`):  
`001_Company` → `002_Store` → `003_Users` → `004_Roles` → `005_Modules` → `006_Permissions` → `007_RolePermissions` → `008_UserRoles` → `009_UserStoreAccess` → `010_Units` → `011_Taxes` → `012_Categories` → `013_SubCategories` → `014_Brands` → `015_Products` → `016_ProductVariants` → `017_ProductImages` → `018_Suppliers` → `019_Customers` → `020_CustomerAddresses` → `021_Inventory` → `022_StockMovements` → `023_Purchases` → `024_PurchaseDetails` → `025_Sales` → `026_SaleDetails` → `027_CashSessions` → `028_Payments` → `029_SalesReturns` → `030_SalesReturnDetails` → `031_Expenses` → `032_Settings` → `033_AuditLogs` → `034_OnlineOrders` → `035_OnlineOrderItems` → `036_Carts` → `037_CartItems` → `038_Wishlists` → `039_WishlistItems` → `040_ProductReviews` → `041_InvoiceSequences` → `042_PatchHistory` → `043_AuthOtpChallenges`.

**02_Seed:** `001_DefaultRoles` → `002_DefaultPermissions` → `003_DefaultSettings` → `004_PlatformSuperAdmin` → `005_ModulePresets` → `006_DevTrialLicense`.

**03_Patches:** `V001__InitialSchema` → `V002__CompanyStoreRelationships` → `V003__PlatformSuperAdminOtp` → `V004__LicenseAndInstallation` → `V005__PurchaseStatusExtension` → `V006__PosCancelPermission` → `V007__Phase7OnlineOrders`.

Plus `DatabaseVersion.sql` (if present at DB root).

### Notes

| Topic | Finding |
|-------|---------|
| Idempotency | Tables/seeds/patches generally `IF NOT EXISTS` / guarded inserts |
| Tables with little/no app code | `031_Expenses.sql`; `user_invitations` (V004); expense CRUD Missing |
| Code without tables | Campaigns / hold-bill / purchase-return / multi-barcode — correctly Missing |
| `CompanyId` / `fk_company` | `store.fk_company` (`002_Store.sql` L17); installation/license tables in V004. No `fk_company` on products/sales/inventory (correct for single-company DB) |
| EF migrations | None under SmartCartPro (correct) |
| Document numbers | `041_InvoiceSequences.sql` + `DocumentNumberRepository.NextAsync` uses `FOR UPDATE` |

---

## 6. Security findings

| Finding | Evidence | Severity |
|---------|----------|----------|
| `CartController` unauthenticated | No `[Authorize]` on class/actions | **High** |
| `AdminController` unauthenticated shell views | No `[Authorize]` | **Medium** (empty shells) |
| `[ApiController]` on Filter | `FilterController.cs` | **Medium** (rule + behavior) |
| HTML Forbid vs JSON for AJAX | Permission filter | **Medium** |
| Super Admin in same user table | Seed + roles | **Low** if list queries filter; verify |
| No unparameterized raw SQL found in repos | EF LINQ | OK |
| Passwords hashed on create | Employee create path | OK (ForgotPassword incomplete) |

---

## 7. Data-consistency risks

| Risk | Evidence | Impact |
|------|----------|--------|
| Transfer out commits before transfer in | `StockRepository` sequential `ApplyMovementAsync` | Lost stock if second fails |
| Company without store | Separate saves | Broken install |
| GRN over-receive | `AlreadyReceivedQty = 0` in Receive GET | Double stock in |
| Idempotency via Notes | Sale `ClientRef` | Collisions / lost notes |
| Negative stock setting ignored | Always block `< 0` | Setting lies to admin |

---

## 8. Controller action inventory

Views under `SmartCartPro/Views/` (39 cshtml). “Has view?” = matching view folder/action exists for primary pages (server Complete does not require views).

| Controller | Action | Verb | Returns | Permission | Module | Has view? |
|------------|--------|------|---------|------------|--------|-----------|
| AdminController | Employees / Roles / Settings | GET | View | **None** | — | Yes (shells) |
| AuthController | Login | GET/POST | View / Redirect | Anonymous | — | Yes |
| AuthController | VerifyOtp | GET/POST | View / Redirect | Anonymous | — | Yes |
| AuthController | ChangePassword | GET/POST | View / Redirect | Authorize | — | Yes |
| AuthController | ForgotPassword / ResetPassword | GET | View only | Anonymous | — | Yes (no POST) |
| AuthController | Logout / LogoutGet | POST/GET | Redirect | Authorize | — | — |
| HomeController | Index / Error / StatusCodePage / NotFoundPage | GET | View | Mixed | — | Yes |
| CompanyController | Index | GET | View | Authorize | — | Yes |
| CompanyController | Save | POST | JSON | Authorize | — | — |
| StoreController | Index / Edit | GET | View | Authorize | — | Yes |
| StoreController | Create | GET/POST | View / JSON | PlatformSuperAdmin | — | Yes |
| StoreController | Edit | POST | JSON | Authorize | — | — |
| StoreController | SetCurrentStore | POST | Redirect/JSON | Authorize | — | — |
| LicenseController | Index / Save / Suspend / Activate | GET/POST | View / JSON | PlatformSuperAdmin | — | **No** dedicated Views/License |
| ModuleController | Index / Save / ApplyPreset | GET/POST | View / JSON | PlatformSuperAdmin | — | **No** Views/Module |
| EmployeeController | Index / Create / Edit / Activate / Deactivate / … | GET/POST | View / JSON | Authorize; some PlatformSuperAdmin | — | Via Admin/Employees shell |
| RoleController | Index / Create / Edit / Delete | GET/POST | View / JSON | PlatformSuperAdmin | — | Via Admin/Roles shell |
| CatalogController | Categories…Taxes CRUD + Lookups | GET/POST | View / JSON | products.view/manage | PRODUCTS | Partial (Categories, Brands) |
| ProductController | Index / Create / Edit / Variants / FindByBarcode | GET/POST | View / JSON | products.* | PRODUCTS | Partial (Products, Variants) |
| InventoryController | Index / Opening / Adjust / Transfer / History / LowStock | GET/POST | View / JSON | inventory.* | INVENTORY | Partial (Stock) |
| SupplierController | Index / Create / Edit / Lookup | GET/POST | View / JSON | purchasing.* | PURCHASES | Via Inventory/Suppliers |
| PurchaseController | Index / Create / Edit / Receive / Cancel / … | GET/POST | View / JSON (+ TempData on Edit guard) | purchasing.* | PURCHASES | Via Inventory/Purchases |
| CustomersController | Index / Create / Edit / SearchByPhone | GET/POST | View / JSON | pos.* | POS | Yes Index |
| SalesController | Pos / CreateSale / Invoices / Cancel / Returns / Session / DayClose / … | GET/POST | View / JSON | pos.* | POS | Yes (Pos, Invoices, Returns, OnlineOrders) |
| OnlineOrderController | Index / Details / UpdateStatus (+ Json variants) | GET/POST | View / JSON | pos.sell (class) | — | Partial (Sales/OnlineOrders) |
| CartController | GetCart / UpsertItem / ClearCart / GetWishlist / ToggleWishlist | GET/POST | JSON | **None** | — | No |
| ProductReviewController | Index / ListJson / Hide / Approve / HideJson | GET/POST | View / JSON | products.manage | PRODUCTS | **No** Views/ProductReview |
| SettingController | Index / Save | GET/POST | View / JSON | settings.manage | SETTINGS | Via Admin/Settings |
| AuditController | Index / ListJson | GET | View / JSON | settings.manage | — | **No** Views/Audit |
| ReportsController | Sales / Products / Customers (shells) + SalesByDate / StockValuation / LowStock / TaxSummary / TopSelling | GET | View / JSON | reports.view | REPORTS | Partial shells |
| DashboardController | (summary endpoints) | GET | JSON/View | Authorize | — | Unverified / minimal |
| FilterController | (filter helpers) | GET | JSON | — | — | N/A (`[ApiController]`) |

**Security hotspots in inventory:** `AdminController` (no auth), `CartController` (no auth), `FilterController` (`[ApiController]` at L11).

---

## 9. Recommended new functionality (commercial supermarket)

| Feature | Reason | Priority | Effort |
|---------|--------|----------|--------|
| Multi-barcode + label print payload | Real POS receiving & shelf labels | **Must** | M |
| GST/HSN + tax split on invoice | India compliance | **Must** | L |
| Hold/resume bill | Queue at counter | **Must** | M |
| Purchase return / debit note | Vendor returns | **Must** | M |
| Atomic stock transfer + GRN received-qty truth | Prevent inventory corruption | **Must** | S |
| Role max discount enforcement | Shrinkage / fraud | **Must** | S |
| Batch/expiry + near-expiry | Grocery perishables | **Should** | L |
| Stock take with variance | Period close | **Should** | L |
| Credit customer + receipts | Kirana / B2B mix | **Should** | L |
| Loyalty + simple schemes | Retention | **Should** | L |
| Import products/stock CSV | Onboarding | **Should** | M |
| Supplier AP ageing | Finance | **Could** | L |
| Weighing-scale barcodes | Fresh produce | **Could** | M |
| E-invoice field readiness | India B2B | **Could** | M |

---

## 10. Pending task list (commit-ready)

Ordered: security & consistency → planned gaps → compliance → advanced Must.

| ID | Title | Phase | Depends | Files/tables (indicative) | Acceptance |
|----|-------|-------|---------|---------------------------|------------|
| SC-SRV-001 | Add `[Authorize]` (+ module) to `CartController` | 8 | — | `CartController.cs` | Unauthenticated calls → 401 |
| SC-SRV-002 | Secure or remove `AdminController` shells | 8 | — | `AdminController.cs` | No anonymous admin pages |
| SC-SRV-003 | Remove `[ApiController]` from `FilterController` | 8 | — | `FilterController.cs` | Still returns JSON; rule clean |
| SC-SRV-004 | Single DB transaction for stock transfer | 4 | — | `StockRepository.cs` | Out+in commit/rollback together |
| SC-SRV-005 | GRN respect already-received qty; block over-receipt | 5 | — | `PurchaseController`, `PurchaseRepository` | Second GRN cannot exceed ordered |
| SC-SRV-006 | Honor `pos.allow_negative_stock` in `ApplyMovementAsync` | 4 | — | `StockRepository`, settings | Setting true allows negative; false blocks |
| SC-SRV-007 | Atomic company bootstrap (company+store[+admin]) | 1 | — | `CompanyRepository`/`CompanyService` | Failure leaves zero orphans |
| SC-SRV-008 | JSON Forbid/Unauthorized for AJAX permission denials | 8 | — | Permission filter / cookie events | AJAX gets CommonResponseModel |
| SC-SRV-009 | Enforce role-based max discount on sale lines | 6 | — | `SaleRepository`/`SaleService` | Over-cap → ValidationException |
| SC-SRV-010 | Proper sale idempotency key (not Notes) | 6 | — | sales table + SaleRepository | Unique ClientRef per store |
| SC-SRV-011 | Purchase return (debit note) + stock out | 5 | SC-SRV-004 | New models/SQL/controller | Partial return qty capped |
| SC-SRV-012 | POS hold / resume | 6 | — | New table + SalesController | Hold list + resume to cart |
| SC-SRV-013 | First Company Admin invite/create on bootstrap | 1 | SC-SRV-007 | invitations + EmployeeService | Admin can log in after register |
| SC-SRV-014 | Implement Forgot/Reset password (POST + token + hash update) | 2 | — | AuthController, AuthService, mail/OTP tables | Token single-use; password hashed |
| SC-SRV-015 | Variant attribute model OR document free-text-only | 3 | — | SQL + Product | Explicit product decision |
| SC-SRV-016 | Reports: purchase + stock movement + real tax split | 7 | — | ReportService/Controller | Non-placeholder TaxSummary |
| SC-SRV-017 | Campaigns module (or formal deferral in product scope) | 7 | — | — | Either shipped or out-of-scope signed |
| SC-SRV-018 | Multi-barcode + label print DTO | Adv | — | SQL + Product | POS + print consumers |
| SC-SRV-019 | GST/HSN fields + invoice tax lines | Adv | — | products/taxes/sale_details | Invoice shows CGST/SGST |
| SC-SRV-020 | Wire or deprecate `platform/src/SmartCart.API` vs SmartCartPro online path | 7 | Decision Q5 | platform API + OnlineOrderController | Single documented online architecture |

---

## 11. Open questions (decisions needed)

1. **Negative stock:** Always forbid, or honor `pos.allow_negative_stock`?
2. **Online stock deduction:** Keep on **confirm**, or move to pack/ship/deliver?
3. **Inventory valuation:** FIFO vs weighted average vs last cost?
4. **Barcode format:** Internal Code128 vs EAN-13 company range?
5. **Multi-tenant:** Stay single-DB-per-install (SmartCartPro today) or invest in `platform/` multi-DB provisioning?
6. **Variants:** Free-text variants forever, or attribute×value matrix with auto SKUs?
7. **Auth POST redirects:** Keep View/Redirect for Auth only, or migrate Auth to JSON too?
8. **Campaigns:** In MVP or explicitly out of scope for v1?

---

## Appendix A — Evidence anchors (high-signal)

| Area | Path |
|------|------|
| Central stock | `Repositories/StockRepository.cs` → `ApplyMovementAsync` |
| License/modules SQL | `Database-Postgres/03_Patches/V004__LicenseAndInstallation.sql` |
| Common JSON | `Models/CommonModel.cs` → `CommonResponseModel` |
| Exception JSON | `Middleware/AppExceptionHandler.cs`, `Helpers/ExceptionErrorMapper.cs` |
| Document numbers | `Repositories/DocumentNumberRepository.cs` |
| Phase commits | `git log`: `8513bb6` … `3977a96` |
| Prior optimistic report | `docs/ServerSide_Implementation_Report.md` |

## Appendix B — What the prior implementation report overstated

Compared to `docs/ServerSide_Implementation_Report.md`, this audit marks **Partial/Missing** where that report said Complete for: multi-company provisioning (skipped ≠ done), SKU combination generation, purchase return, hold/resume, campaigns, full report suite, negative-stock setting behavior, and several compliance items (`[ApiController]`, unprotected Cart).

---

*End of report. Audit task created only `docs/ServerSide_Status_Report.md`. Other dirty/staged files in the working tree (if any) are from prior work, not this audit.*
