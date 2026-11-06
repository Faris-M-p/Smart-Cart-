# Feature-level modules — architecture audit

Read-only. Evidence is a file path plus a type, method, or SQL object. Items not confirmed in source are marked **Unverified**. No code is proposed.

## 1. Summary

Modules are a flat catalog of nine codes. A company install turns a module on by inserting one row in `installation_modules`. There is no parent, menu key, or feature key. “Core” is not a column: `ModuleService` treats every module in the Basic preset as locked on. Custom is not stored; the Modules page labels the current set Custom when it does not match Basic or Premium.

**Feasibility: Feasible with moderate changes.** Screens, permissions, and the sidebar already exist as leaves. The database cannot store Module → Menu → Feature, and the server gate (`ModuleAccess`) only knows a module code. A third level needs new stored flags, a check on each leaf action, menu conditions, a new Modules page, and preset rows at that grain. Business controllers do not have to be rewritten to add the gate.

Top gaps:

1. No sub-level column or table (`modules`, `module_preset_items`, `installation_modules`).
2. One module attribute covers a whole controller, including every JSON action.
3. The sidebar is hard-coded and only hides whole groups (or a few items by permission).
4. Company, Stores, Audit, License, Modules, Home, and Cart have no `[ModuleAccess]`.
5. Enabled modules are read from the database on each request (scoped, in-memory for that request only). There is no shared module cache to invalidate.

## 2. Module data model

### 2.1 Tables

| Table | Source | Columns | Keys |
|---|---|---|---|
| `modules` | `SmartCartPro/Database-Postgres/01_Tables/005_Modules.sql` lines 5–18 | `id_module` INT identity, `modulecode` TEXT NOT NULL, `modulename` TEXT NOT NULL, `displayorder` INT NOT NULL DEFAULT 0, `isactive` BOOLEAN NOT NULL DEFAULT TRUE, `createdat` TIMESTAMP NOT NULL DEFAULT NOW(), `updatedat` TIMESTAMP NULL, `cancelled` BOOLEAN NOT NULL DEFAULT FALSE, `cancelledon` TIMESTAMP NULL, `cancelledreason` TEXT NULL | PK `id_module`. UNIQUE `uq_modules_modulecode` (`modulecode`). No FK. No parent, no `is_core`. |
| `permissions` | `01_Tables/006_Permissions.sql` lines 6–23 | `id_permission` INT identity, `fk_module` INT NOT NULL, `permissioncode` TEXT NOT NULL, `permissionname` TEXT NOT NULL, `displayorder` INT NOT NULL DEFAULT 0, `isactive` BOOLEAN, `createdat`, `updatedat`, `cancelled`, `cancelledon`, `cancelledreason` | PK `id_permission`. UNIQUE `uq_permissions_permissioncode`. FK `fk_permissions_module` → `modules(id_module)`. Index `ix_permissions_fk_module`. |
| `role_permissions` | `01_Tables/007_RolePermissions.sql` lines 6–17 | `id_rolepermission` INT identity, `fk_role` INT NOT NULL, `fk_permission` INT NOT NULL, `createdat`, `updatedat` | PK `id_rolepermission`. UNIQUE (`fk_role`, `fk_permission`). FK to `roles` and `permissions`. |
| `license` | `03_Patches/V004__LicenseAndInstallation.sql` lines 11–32 | `id_license`, `licensekey`, `license_type` DEFAULT `'standard'`, `status` DEFAULT `'active'`, `startdate`, `expirydate`, `maxstores` DEFAULT 1, `notes`, audit columns | PK `id_license`. UNIQUE `licensekey`. CHECKs on status (`active`,`suspended`,`expired`,`trial`), dates, `maxstores >= 1`. No module columns. |
| `installation_config` | V004 lines 39–55 | `id_config`, `fk_license` NULL, `theme`, `store_mode`, `onboarding_step`, `onboarding_complete`, `activated_at`, timestamps | PK `id_config`. FK `fk_installation_license` → `license`. This links the install to a license. It does not store modules. |
| `module_presets` | V004 lines 63–71 | `id_preset`, `presetcode`, `presetname`, `description`, `displayorder`, `createdat` | PK `id_preset`. UNIQUE `uq_module_presets_code`. |
| `module_preset_items` | V004 lines 76–85 | `id_preset_item`, `fk_preset`, `fk_module` | PK `id_preset_item`. FK to `module_presets` and `modules`. UNIQUE (`fk_preset`, `fk_module`). No feature column. |
| `installation_modules` | V004 lines 93–100 | `id_inst_module`, `fk_module`, `createdat` | PK `id_inst_module`. FK `fk_inst_modules_module` → `modules`. UNIQUE (`fk_module`). No `fk_company`, no `fk_license`, no feature column. |

`user_invitations` is also created in V004 (lines 105–131). It assigns a role and a store. It does not assign modules.

Entity map: `SmartCartPro/Entities/ModuleEntities.cs` (`InstallationModule` comment at line 93: final enabled list for this installation).

### 2.2 Seeded module codes

`02_Seed/002_DefaultPermissions.sql` lines 6–15. No core flag in the insert.

| Code | Name | `displayorder` |
|---|---|---|
| DASHBOARD | Dashboard | 10 |
| PRODUCTS | Products | 20 |
| INVENTORY | Inventory | 30 |
| PURCHASES | Purchases | 40 |
| POS | Point of Sale | 50 |
| CUSTOMERS | Customers | 60 |
| REPORTS | Reports | 70 |
| SETTINGS | Settings | 80 |
| USERS | Users & Roles | 90 |

### 2.3 Presets and Custom

Stored as rows, not hard-coded lists, in `02_Seed/005_ModulePresets.sql`:

- `BASIC` / Basic — description “Core modules: Dashboard, POS, Products, Inventory”. Items: `DASHBOARD`, `PRODUCTS`, `POS`, `INVENTORY` (lines 14–36).
- `PREMIUM` / Premium — “All supported modules including Reports and User Management”. Items: all nine codes (lines 43–52).

There is no `CUSTOM` row. `Views/Module/Index.cshtml` lines 4–13 set `currentPreset = "CUSTOM"` and overwrite it only when the enabled id set equals a preset’s id set. The Custom button is `disabled` (lines 47–49). The page text (line 52) says Custom is whatever is ticked and saved.

“Core” is computed in `ModuleService.CoreModuleIds` (`Services/ModuleService.cs` lines 93–108): the Basic preset’s module ids, or, if that preset is missing, the `DASHBOARD` module only. `AssignModulesAsync` (lines 67–73) adds those ids back if the posted list omitted them. Descriptions shown on the page are a hard-coded dictionary in the same class (lines 9–20), not a database column.

### 2.4 Where the enabled set lives

`installation_modules.fk_module` only. `ModuleRepository.GetEnabledModuleIdsAsync` / `GetEnabledModuleCodesAsync` / `SaveModuleAssignmentAsync` (`Repositories/ModuleRepository.cs` lines 47–82). Save deletes every existing row and inserts the new set. It does not read or write `license` or `installation_config`.

`company` is documented as one company per client database (`01_Tables/001_Company.sql` lines 1–3). Module lookup does not filter by `fk_company`.

### 2.5 Existing sub-level

**None** for a feature or menu. Closest related fields:

- `modules.displayorder` — sort only.
- `permissions.fk_module` — permission belongs to a module, not a page.
- `module_preset_items.fk_module` — preset membership at module grain.
- `OutputModuleSelect.IsCore` — filled in memory by `ModuleService`, not a column (`ModuleService.cs` lines 36–42).

## 3. Enforcement map

### 3.1 `ModuleAccessAttribute`

`SmartCartPro/Filters/ModuleAccessAttribute.cs`.

- One attribute, class or method, `AllowMultiple = false` (line 14). Several codes on that attribute mean **any** of them (lines 10–11, loop 42–49).
- Reads modules by calling `ICurrentUserAccess.ModuleEnabledAsync` (line 45). That method (`Services/CurrentUserAccess.cs` lines 71–97):
  - signed-out → false;
  - platform Super Admin (`is_platform_super_admin` claim) → **true for every code**, no database read;
  - company role `ADMIN` → **true for `USERS` and `SETTINGS` only**, even when those rows are absent from `installation_modules` (lines 9–16, 83–88);
  - otherwise loads enabled codes once per request via `IModuleService.GetEnabledModuleCodesAsync` → `installation_modules` join `modules` (`ModuleRepository.cs` lines 52–55). Not from claims.
- Denied JSON/AJAX (`ExceptionErrorMapper.WantsJsonResponse`): HTTP 403 body `CommonResponseModel.Failure("This module is not enabled for your company.", statusCode: 403)` (lines 54–62).
- Denied normal request: `ForbidResult()` (line 66). No dedicated “module off” view in this filter.
- Registered as scoped: `Extensions/ServiceCollectionExtensions.cs` line 125 `AddScoped<ICurrentUserAccess, CurrentUserAccess>`.

### 3.2 Every `[ModuleAccess]`

All are **class-level** unless noted. Method-level: only `DashboardController.Snapshot`.

| Controller | Code(s) | Evidence |
|---|---|---|
| `CatalogController` | PRODUCTS | `Controllers/CatalogController.cs` line 13 |
| `ProductController` | PRODUCTS | line 12 |
| `ProductReviewController` | PRODUCTS | line 16 |
| `FilterController` | PRODUCTS | line 15 |
| `InventoryController` | INVENTORY | line 13 |
| `SupplierController` | PURCHASES | line 12 |
| `PurchaseController` | PURCHASES | line 12 |
| `SalesController` | POS | line 13 |
| `OnlineOrderController` | POS | line 17 |
| `CustomersController` | CUSTOMERS **or** POS | line 12 |
| `ReportsController` | REPORTS | `Controllers/ReportsController.cs` line 13 (partial `ReportsController.Suite.cs` inherits it) |
| `AuditController` | SETTINGS | line 16 |
| `SettingController` | SETTINGS | line 17 |
| `EmployeeController` | USERS | line 15 |
| `RoleController` | USERS | line 16 |
| `DashboardController.Snapshot` only | DASHBOARD | lines 22–23 |

`HomeController.Index` (the Dashboard menu target) has **no** `[ModuleAccess]`.

### 3.3 Controllers / actions with no module check

| Place | What protects it | Evidence |
|---|---|---|
| `HomeController` (`Index`, `Error`, `StatusCodePage`, `LicenseBlocked`, `NotFoundPage`) | `Error` is `[AllowAnonymous]`. `Index` comment says every authenticated user may open it; `dashboard.view` is not required. | `Controllers/HomeController.cs` lines 29–32, 33, 67–68 |
| `AuthController` | Anonymous login/OTP/forgot/reset; `[Authorize]` on change-password and logout | `Controllers/AuthController.cs` lines 190, 202, 313, 326 |
| `CompanyController` | `[Authorize]` + `[RequirePermission("settings.manage")]` on Index and Save. No module attribute. | lines 10, 25, 40 |
| `StoreController` | `[Authorize]`. `[RequirePermission("settings.manage")]` on Index (31), Edit GET (102), Edit POST (121), SetActive (157). **No** permission attribute on Create GET (45), Create POST (58), or `SetCurrentStore` (178). No module attribute. | grep of `RequirePermission` in that file |
| `LicenseController` | `[Authorize(Policy = "PlatformSuperAdmin")]` only | lines 9–10 |
| `ModuleController` | `[Authorize(Policy = "PlatformSuperAdmin")]` only | lines 10–11 |
| `CartController` | `[Authorize]` only. Cart and wishlist JSON. | lines 14–15; actions at 39, 52, 70, 87, 100 |
| `ProductController.Search`, `FindByBarcode` | Class `[ModuleAccess("PRODUCTS")]` applies. **No** `[RequirePermission]` on these two actions (attributes at lines 339 and 357 are `[HttpGet]` only). | `ProductController.cs` 339–368 |

`LicenseGateFilter` (`Filters/LicenseGateFilter.cs`) is global and is **not** a module check. Suspended license → 403. Expired or missing license blocks POST/PUT/DELETE and allows GET/HEAD. Super Admin bypasses. License status uses `IMemoryCache` key `CompanyService.LicenseStatusCacheKey` (`"smartcart:license:status"`), TTL referenced as `CacheTtl` in the filter (lines 99–107). Cleared in `CompanyService` line 273 on license save. That cache does not hold modules.

Other module reads outside the attribute:

- `CurrentUserAccess.ModuleEnabledAsync` — used by the sidebar and by `HomeController.Index` (lines 50–57).
- `ProductService` product details: stock block only if `INVENTORY` is enabled and `inventory.view` (`Services/ProductService.cs` lines 62–70).
- `DashboardService.BuildQuickActions` (`Services/DashboardService.cs` lines 172–191): each shortcut needs the matching permission **and** module flag passed from Home.

No other service grep hit for `ModuleEnabledAsync`.

### 3.4 Caching

Enabled modules: **no `IMemoryCache`**. `CurrentUserAccess._enabledModules` is filled once per scoped request (lines 90–94) and discarded with the request. `AssignModulesAsync` does not remove a cache entry. The next request reads `installation_modules` again. License cache (above) is separate and is not cleared by a module save.

## 4. Permissions vs modules

### 4.1 Permission table and keys

Structure: section 2.1 (`permissions`, `role_permissions`).

Seeded in `002_DefaultPermissions.sql` lines 23–38, plus `03_Patches/V006__PosCancelPermission.sql` lines 8–10:

| Module code | Permission code | Name |
|---|---|---|
| DASHBOARD | `dashboard.view` | View dashboard |
| PRODUCTS | `products.view` | View products |
| PRODUCTS | `products.manage` | Manage products |
| INVENTORY | `inventory.view` | View inventory |
| INVENTORY | `inventory.adjust` | Adjust stock |
| PURCHASES | `purchases.view` | View purchases |
| PURCHASES | `purchases.manage` | Manage purchases |
| POS | `pos.sell` | Create POS sales |
| POS | `pos.return` | Process sales returns |
| POS | `pos.cancel` | Cancel POS invoices (V006) |
| CUSTOMERS | `customers.view` | View customers |
| CUSTOMERS | `customers.manage` | Manage customers |
| REPORTS | `reports.view` | View reports |
| REPORTS | `reports.profit` | View cost and profit on reports |
| SETTINGS | `settings.manage` | Manage settings |
| USERS | `users.manage` | Manage users and roles |

`002` lines 47–56 grant every non-cancelled permission to roles `ADMIN` and `SUPER_ADMIN`. V006 grants `pos.cancel` to `ADMIN` (and `SUPER_ADMIN` in the same patch’s second insert). What V010 grants to `STORE_MANAGER` / `VIEWER` is **not re-read for this audit**; do not treat the summary memory as evidence here.

### 4.2 `RequirePermissionAttribute`

`Filters/RequirePermissionAttribute.cs` lines 11–16 and filter from line 29. Several codes on **one** attribute mean any of them. Two attributes on the same action mean all of them (class comment, and `ReportsController` class `reports.view` plus method `reports.profit`). Platform Super Admin passes. Permissions come from `IUserService.GetUserPermissionsAsync` for the user id claim, cached on the scoped `CurrentUserAccess` for that request (`CurrentUserAccess.cs` lines 99–116). JSON denial uses the same `WantsJsonResponse` path as other filters; the exact permission failure message is in that filter (**not quoted here beyond the attribute file’s stated any/all rule** — the denial body was not re-opened line by line in this pass). Menu checks use `CanAsync` / `CanAnyAsync`, which Super Admin short-circuits to true (lines 49–51).

### 4.3 Link between permission and module

- Database: `permissions.fk_module` → `modules.id_module` (`006_Permissions.sql` lines 19–20).
- Naming: permission code starts with a short name of the module (`products.`, `pos.`, `reports.`). POS does not use a `pos.` module code; the module code is `POS`.
- Runtime: they are separate gates. A controller can require both `[ModuleAccess]` and `[RequirePermission]`. Turning a module off does not delete or hide `role_permissions` rows. **Unverified** whether the Roles UI hides permissions whose module is off — not read in this pass.

### 4.4 Permissions below module level

Yes, but only a few actions, not per page:

- `inventory.adjust` on Opening, Adjust, Transfer (`InventoryController` lines 62, 93, 136, 167, 207, 235). View actions use `inventory.view`.
- `pos.cancel` on `SalesController.CancelSale` (line 193). `pos.return` on return actions (lines 223–295). `pos.sell` on POS, session, online orders.
- `reports.profit` on `ProfitMargin`, `ProfitMarginData`, `ProfitMarginExport`, `ProfitMarginPrint` only (`ReportsController.Suite.cs` lines 137–146), **in addition to** class-level `reports.view`.
- `customers.view` **or** `pos.sell` on `CustomersController.Search` (line 129); `customers.manage` **or** `pos.sell` on `QuickCreate` (line 142).
- Invoices: `pos.sell` **or** `reports.view` (`SalesController` lines 123, 137, 153, 172).

No permission exists per report other than `reports.profit`, and none per catalog screen (categories vs products vs reviews all use `products.view` / `products.manage`).

## 5. Menu / sidebar

Built in `Views/Shared/_Layout.cshtml` lines 79–338. Hard-coded markup. Not a table, not a ViewComponent. Flags at lines 23–33:

- `showCatalog` = `ModuleEnabledAsync("PRODUCTS")` && `CanAsync("products.view")`
- `showPurchases` = `ModuleEnabledAsync("PURCHASES")` && (`CanAsync("purchases.view")` || `CanAsync("purchases.manage")`) — condition continues on lines 24–25
- `showInventory` = `ModuleEnabledAsync("INVENTORY")` && (`inventory.view` || `inventory.adjust`) — lines 26–27
- `showPos` = `ModuleEnabledAsync("POS")` && (`pos.sell` || `pos.return` || `pos.cancel`) — lines 28–29
- `showCustomers` = CUSTOMERS module && `customers.view`
- `showReports` = REPORTS module && `reports.view`
- `showUsers` = USERS module && `users.manage`
- `showSettings` = SETTINGS module && `settings.manage`
- `isPlatform` = `AuthClaims.IsPlatformSuperAdminUser(User)` at line 5 (used at lines 69, 279, 323)

`ModuleEnabledAsync` already returns true for Super Admin, and `CanAsync` does too, so Super Admin sees every group those flags control. License and Modules are an extra `@if (isPlatform)` (lines 323–335).

Dashboard link (lines 83–86) has **no** module or permission condition. It goes to `Home/Index`.

### Current tree (as rendered)

| Group | Item | Shown when | Controller / action |
|---|---|---|---|
| (top) | Dashboard | always | `Home/Index` |
| Catalog | Categories | `showCatalog` | `Catalog/Categories` |
| Catalog | Sub-categories | `showCatalog` | `Catalog/SubCategories` |
| Catalog | Brands | `showCatalog` | `Catalog/Brands` |
| Catalog | Units | `showCatalog` | `Catalog/Units` |
| Catalog | Taxes | `showCatalog` | `Catalog/Taxes` |
| Catalog | Products | `showCatalog` | `Product/Index` |
| Catalog | SKU / Variants | `showCatalog` | `Product/Variants` |
| Catalog | Reviews | `showCatalog` && `products.manage` | `ProductReview/Index` |
| Purchase & stock | Suppliers | `showPurchases` | `Supplier/Index` |
| Purchase & stock | Purchases | `showPurchases` | `Purchase/Index` |
| Purchase & stock | Stock | `showInventory` | `Inventory/Stock` |
| Purchase & stock | Low stock | `showInventory` | `Inventory/LowStock` |
| Purchase & stock | Stock history | `showInventory` | `Inventory/History` |
| Purchase & stock | Opening stock | `showInventory` && `inventory.adjust` | `Inventory/Opening` |
| Purchase & stock | Adjust stock | same | `Inventory/Adjust` |
| Purchase & stock | Transfer | same | `Inventory/Transfer` |
| Sales | POS Billing | group `showPos` \|\| `showCustomers`, item `pos.sell` | `Sales/Pos` |
| Sales | Cash session | `pos.sell` | `Sales/Session` |
| Sales | Invoices | `pos.sell` or `reports.view` | `Sales/Invoices` |
| Sales | Returns | `pos.return` | `Sales/Returns` |
| Sales | Online orders | `pos.sell` | `OnlineOrder/Index` |
| Sales | Customers | `showCustomers` | `Customers/Index` |
| Reports | All reports | `showReports` | `Reports/Index` |
| Administration | Employees | `showUsers` | `Employee/Index` |
| Administration | Roles | `showUsers` | `Role/Index` |
| Administration | Company | `showSettings` | `Company/Index` |
| Administration | Stores | `showSettings` | `Store/Index` |
| Administration | Settings | `showSettings` | `Setting/Index` |
| Administration | Audit log | `showSettings` | `Audit/Index` |
| Administration | License | `isPlatform` | `License/Index` |
| Administration | Modules | `isPlatform` | `Module/Index` |

Individual reports are not sidebar items. They are opened from `Views/Reports/Index.cshtml` (report hub). **Unverified in this pass:** the exact card list on that view; the action list below is from the controller.

## 6. Current Modules page

`ModuleController` (`Controllers/ModuleController.cs` line 10): policy `PlatformSuperAdmin` on the class. No `[ModuleAccess]`.

| Action | Verb | Auth | Input | Output | What it saves |
|---|---|---|---|---|---|
| `Index` line 24 | GET | class policy | none | `InModuleAssignView` (`AllModules`, `Presets`, `EnabledModuleIds`) from `GetModuleListAsync` | nothing |
| `Save` line 42 | POST + antiforgery | class policy | `InModuleAssignView.EnabledModuleIds` mapped to a named `InputModuleAssign` (lines 44–48) | JSON via `CommonHelpers.SuccessResponse` | `AssignModulesAsync` → replace `installation_modules` |
| `ApplyPreset` line 61 | POST + antiforgery | class policy | `InModuleAssignView.SelectedPresetCode` | JSON success | Looks up the preset, sets `EnabledModuleIds` to `preset.ModuleIds`, then the same `AssignModulesAsync` |

`AssignModulesAsync` (`ModuleService.cs` lines 56–87) drops unknown ids, forces core (Basic) ids on, then saves. Apply Preset therefore **replaces** the previous set with the preset plus any core ids the preset already contains. The confirm text on the button says current custom toggles will be replaced (`Views/Module/Index.cshtml` line 40).

View: `Views/Module/Index.cshtml` (this path exists; there is not a separate missing view). One checkbox per module. Core rows are `checked disabled` plus a hidden `EnabledModuleIds` input (lines 80–85). Optional rows post `EnabledModuleIds` only when checked. Script: `wwwroot/js/admin/module.js` — submits `#moduleForm` through `SmartCartAjax.submitForm`. Preset buttons use `data-sc-post` (declarative AJAX), `data-sc-after="reload"`.

Audit: **yes**. `ModuleService.AssignModulesAsync` lines 78–85 calls `IAuditService.WriteLogAsync` with `EntityName = "Module"`, `EntityId = null`, `Action = "Update"`, details = enabled count. Both Save and Apply Preset go through this method, so both are logged. The log does not list which codes changed.

## 7. Feature inventory

Legend: **Protected** means a `[ModuleAccess]` that covers the action (class or method) **and** a `[RequirePermission]` on that action, unless the cell says otherwise. Menu = a sidebar item exists for that screen.

| Module | Menu | Feature | Exists? | Controller.Action | View | JSON / AJAX | Module code | Permission | Protected server-side? | Menu item? |
|---|---|---|---|---|---|---|---|---|---|---|
| Dashboard | Dashboard | Overview | Partial | `Home.Index` renders the page. `Dashboard.Snapshot` is the poll. | `Views/Home/Index.cshtml` | `Dashboard/Snapshot` GET JSON | none on Home; DASHBOARD on Snapshot | `dashboard.view` gates KPIs in `DashboardService` line 46, not the page. Snapshot requires it. | Snapshot yes. Home page no. | Yes → Home |
| Dashboard | Dashboard | Store metrics | Partial | Same `Home.Index` when `HasDashboardView` and a store is selected (`DashboardService.cs` 46–70): KPIs, trends, payments, top products, invoices, low stock. | same | Snapshot | same | `dashboard.view` | metrics loaded only with that permission; endpoint protected | same item |
| Dashboard | Dashboard | Quick shortcuts | Yes | `DashboardService.BuildQuickActions` 172–191 and `SetupActions` 194–200 | same | none | each shortcut checks its own module flag | each shortcut checks its permission | built in the service, not a separate action | same item |
| Catalog | Categories | List / create / edit | Yes | `Catalog.Categories`, `CreateCategory`, `EditCategory`, `SetCategoryActive`, `CategoryLookup` | `Views/Catalog/` (standard MVC; individual files not opened) | `CategoryLookup` | PRODUCTS | view / manage | Yes (class module + method permission) | Yes |
| Catalog | Sub-categories | List / create / edit | Yes | `SubCategories`, `CreateSubCategory`, `EditSubCategory`, `SetSubCategoryActive`, `SubCategoryLookup` | Catalog views | `SubCategoryLookup` | PRODUCTS | view / manage | Yes | Yes |
| Catalog | Brands | List / create / edit | Yes | `Brands`, `CreateBrand`, `EditBrand`, `SetBrandActive`, `BrandLookup` | Catalog views | `BrandLookup` | PRODUCTS | view / manage | Yes | Yes |
| Catalog | Units | List / create / edit | Yes | `Units`, `CreateUnit`, `EditUnit`, `SetUnitActive`, `UnitLookup` | Catalog views | `UnitLookup` | PRODUCTS | view / manage | Yes | Yes |
| Catalog | Taxes | List / create / edit | Yes | `Taxes`, `CreateTax`, `EditTax`, `SetTaxActive`, `TaxLookup` | Catalog views | `TaxLookup` | PRODUCTS | view / manage | Yes | Yes |
| Catalog | Products | Product list | Yes | `Product.Index` | Product views | — | PRODUCTS | `products.view` | Yes | Yes |
| Catalog | Products | Create | Yes | `Product.Create` GET/POST | Product create view | form POST | PRODUCTS | `products.manage` | Yes | via Products |
| Catalog | Products | Edit | Yes | `Product.Edit` GET/POST, `SetActive` | Product edit view | `SetActive` | PRODUCTS | `products.manage` | Yes | via Products |
| Catalog | Products | Details | Yes | `Product.Details` | Product details view | — | PRODUCTS | `products.view` | Yes | via Products |
| Catalog | Products | Images | No (UI) | No controller action. Table `product_images` exists (`01_Tables/017_ProductImages.sql`). | none found | none | — | — | — | No |
| Catalog | Products | Pricing | Partial | No separate pricing action. Price fields are on product create/edit (not re-opened field by field). | product forms | — | PRODUCTS | `products.manage` | Yes, as part of product save | No separate item |
| Catalog | SKU / Variants | Variant list | Yes | `Product.Variants` | variant list view | — | PRODUCTS | `products.view` | Yes | Yes |
| Catalog | SKU / Variants | Create/edit variants | Yes | `CreateVariant`, `EditVariant`, `SetVariantActive` | variant forms | `SetVariantActive` | PRODUCTS | `products.manage` | Yes | via SKU item |
| Catalog | SKU / Variants | SKU management | Partial | SKU is a field on the variant (`product_variants.skucode` — column not re-opened; entity used by variant actions). No standalone SKU screen. | variant forms | — | PRODUCTS | manage | Yes as part of variant save | No |
| Catalog | SKU / Variants | Barcode management | Partial | Barcode on product/variant save, plus `Product.FindByBarcode` and `Sales.ProductByBarcode`. No barcode admin screen. | variant/product forms, POS | `FindByBarcode` has module, **no** permission | PRODUCTS / POS | manage on save; barcode lookup see note | Save yes. Lookup module only. | No |
| Catalog | SKU / Variants | Variant pricing | Partial | `additionalprice` on the variant form (entity field; form not re-opened). No separate price list. | variant forms | — | PRODUCTS | `products.manage` | Yes as part of variant save | No |
| Catalog | Reviews | Review list | Yes | `ProductReview.Index`, `ListJson` | review views | `ListJson` | PRODUCTS | `products.manage` (list included) | Yes | Yes, only if `products.manage` |
| Catalog | Reviews | Approve/reject | Partial | `Approve`, `Hide`, `HideJson`. No action named Reject. Hide is the negative path. | review views | `HideJson` | PRODUCTS | `products.manage` | Yes | same item |
| Purchase & stock | Suppliers | List | Yes | `Supplier.Index` | supplier views | — | PURCHASES | `purchases.view` | Yes | Yes |
| Purchase & stock | Suppliers | Create/edit | Yes | `Create`, `Edit`, `SetActive`, `Lookup` | supplier forms | `Lookup`, `SetActive` | PURCHASES | `purchases.manage` (Lookup is view — line 145) | Yes | via Suppliers |
| Purchase & stock | Purchases | List | Yes | `Purchase.Index` | purchase views | — | PURCHASES | `purchases.view` | Yes | Yes |
| Purchase & stock | Purchases | Create | Yes | `Purchase.Create` GET/POST | create view | POST body JSON | PURCHASES | `purchases.manage` | Yes | via Purchases |
| Purchase & stock | Purchases | Receiving | Yes | `Receive` GET/POST | receive view | POST body | PURCHASES | `purchases.manage` | Yes | No separate item |
| Purchase & stock | Purchases | History | Partial | `Details`, `Print`, `Edit`, `Cancel`. No action named History. | details/print | `ProductLookup` | PURCHASES | view on details/print; manage on edit/cancel | Yes | No |
| Purchase & stock | Stock | Overview | Yes | `Inventory.Stock` (and `Index` redirects — line 34) | stock view | — | INVENTORY | `inventory.view` | Yes | Yes |
| Purchase & stock | Stock | Store-wise | Partial | Stock is loaded for the actor’s store. No “all stores” stock action found on `InventoryController`. | stock view | `ProductPicker` | INVENTORY | `inventory.view` on `ProductPicker` (line 318) | Yes | No |
| Purchase & stock | Low stock | Low stock screen | Yes | `Inventory.LowStock` | low stock view | — | INVENTORY | `inventory.view` | Yes | Yes |
| Purchase & stock | Stock history | History | Yes | `Inventory.History` | history view | — | INVENTORY | `inventory.view` | Yes | Yes |
| Purchase & stock | Opening stock | Opening | Yes | `Inventory.Opening` GET/POST | opening view | — | INVENTORY | `inventory.adjust` | Yes | Yes |
| Purchase & stock | Adjust stock | Adjust | Yes | `Inventory.Adjust` GET/POST | adjust view | — | INVENTORY | `inventory.adjust` | Yes | Yes |
| Purchase & stock | Transfer | Transfer | Yes | `Inventory.Transfer` GET/POST | transfer view | POST body | INVENTORY | `inventory.adjust` | Yes | Yes |
| Sales | POS Billing | New sale | Yes | `Sales.Pos`, `CreateSale` | POS view | `CreateSale` | POS | `pos.sell` | Yes | Yes |
| Sales | POS Billing | Barcode scanning | Partial | `Sales.ProductByBarcode`, `ProductSearch` (no separate scan page) | POS | both JSON | POS | `pos.sell` | Yes | No |
| Sales | POS Billing | Discounts | Partial | Discount is part of `CreateSale` / sale lines. No discount screen. | POS | `CreateSale` | POS | `pos.sell` | Yes as part of sale | No |
| Sales | POS Billing | Payment processing | Partial | Payments are part of `CreateSale`. No standalone payment controller. | POS | `CreateSale` | POS | `pos.sell` | Yes as part of sale | No |
| Sales | Cash session | Open | Yes | `Sales.OpenSession` | session view | POST | POS | `pos.sell` | Yes | Yes (Cash session) |
| Sales | Cash session | Close | Yes | `Sales.CloseSession` | session view | POST | POS | `pos.sell` | Yes | same |
| Sales | Cash session | History | Partial | `Sales.Session` lists sessions. `DayClose`, `CashSummaryJson` are extra. | session view | `CashSummaryJson` | POS | `pos.sell` | Yes | same |
| Sales | Invoices | List / details / print / cancel | Yes | `Invoices`, `InvoiceDetails`, `InvoicePrint`, `InvoiceJson`, `CancelSale` | invoice views | `InvoiceJson` | POS | list/detail/print: `pos.sell` **or** `reports.view`. Cancel: `pos.cancel` | Yes | Yes |
| Sales | Returns | List / create / details | Yes | `Returns`, `Return`, `ReturnDetails` | return views | — | POS | `pos.return` | Yes | Yes |
| Sales | Online orders | List | Yes | `OnlineOrder.Index`, `ListJson` | online order views | `ListJson` | POS | `pos.sell` | Yes | Yes |
| Sales | Online orders | Details | Yes | `Details`, `DetailsJson` | details view | `DetailsJson` | POS | `pos.sell` | Yes | via Online orders |
| Sales | Online orders | Status management | Yes | `UpdateStatus`, `UpdateStatusJson` | details | `UpdateStatusJson` | POS | `pos.sell` | Yes | same |
| Sales | Customers | List | Yes | `Customers.Index` | customer views | `Search` | CUSTOMERS **or** POS (class) | `customers.view`; Search is view **or** `pos.sell` | Yes | Yes |
| Sales | Customers | Details | Partial | No `Details` action. `Edit` GET is the record screen. `Create`, `Edit` POST, `SetActive`, `QuickCreate`. | customer edit | `QuickCreate` | same | `customers.manage` (QuickCreate also allows `pos.sell`) | Yes | via Customers |

### Reports that exist in code

Class gate for every row: `[ModuleAccess("REPORTS")]` and `[RequirePermission("reports.view")]` (`ReportsController.cs` lines 13–14). Sidebar: one item, “All reports”. Hub cards are not separate menu rows.

Each suite report has four actions: page, `*Data` (JSON), `*Export` (Excel), `*Print`. Permission is `reports.view` except Profit margin, which also requires `reports.profit`.

| Feature | Exists? | Actions (`ReportsController` / `.Suite.cs`) | View | Permission beyond `reports.view` |
|---|---|---|---|---|
| Sales (old) | Yes | `Sales`, `SalesCsv` | `Views/Reports/Sales.cshtml` (MVC convention; not opened) | none |
| Products (old) | Yes | `Products`, `ProductsCsv` | Products view | none |
| Stock Valuation (old) | Yes | `StockValuation`, `StockValuationCsv` | `Views/Reports/StockValuation.cshtml` | `ReportService.GetStockValuationAsync` sets `ShowCost = input.CanSeeProfit` (line 183) and zeros `UnitCost`, `StockValue`, and `TotalStockValue` when that flag is false (lines 190–214). |
| Tax (old) | Yes | `Tax`, `TaxCsv` | Tax view | none |
| Purchases (old) | Yes | `Purchases`, `PurchasesCsv` | Purchases view | none |
| Customers (old) | Yes | `Customers`, `CustomersCsv` | Customers view | none |
| Low stock report (new) | Yes | `LowStockReport` + Data/Export/Print | `Views/Reports/LowStockReport.cshtml` | none |
| Sales bill-wise | Yes | `SalesBillWise` + Data/Export/Print | matching view | none |
| Item sales | Yes | `ItemSales` + Data/Export/Print | matching view | none |
| Category sales | Yes | `CategorySales` + Data/Export/Print | matching view | none |
| Sales returns report | Yes | `SalesReturns` + Data/Export/Print | matching view | none |
| Day close report | Yes | `DayClose` + Data/Export/Print | matching view | none. Distinct from `Sales.DayClose`. |
| Current stock | Yes | `CurrentStock` + Data/Export/Print | matching view | none |
| Item ledger | Yes | `ItemLedger` + Data/Export/Print | matching view | none |
| Stock ledger | Yes | `StockLedger` + Data/Export/Print | matching view | none |
| Stock movement | Yes | `StockMovement` + Data/Export/Print | matching view | none |
| Non-moving | Yes | `NonMoving` + Data/Export/Print | matching view | none |
| Purchase bills | Yes | `PurchaseBills` + Data/Export/Print | matching view | none |
| Purchase analysis | Yes | `PurchaseAnalysis` + Data/Export/Print | matching view | none |
| GST summary | Yes | `GstSummary` + Data/Export/Print | matching view | none |
| Profit margin | Yes | `ProfitMargin` + Data/Export/Print | matching view | **also** `reports.profit` |
| Discount report | Yes | `DiscountReport` + Data/Export/Print | matching view | none |
| Cancelled bills | Yes | `CancelledBills` + Data/Export/Print | matching view | none |
| Stock adjustments report | Yes | `StockAdjustments` + Data/Export/Print | matching view | none |
| Hourly sales | Yes | `HourlySales` + Data/Export/Print | matching view | none |
| Master lists | Yes | `MasterLists` + Data/Export/Print | matching view | none |
| Reports hub | Yes | `Reports.Index` | `Views/Reports/Index.cshtml` | `reports.view` |

All of these share one module code (`REPORTS`) and, except profit, one permission (`reports.view`). A disabled “Stock Valuation only” flag cannot be expressed today.

### Administration

| Feature | Exists? | Actions | Module attribute | Permission | Menu |
|---|---|---|---|---|---|
| Employees | Yes | `Employee` Index, Create, Edit, SetActive, ResetPassword | USERS | `users.manage` | Yes |
| Roles | Yes | `Role` Index, Create, Edit, SetActive, Delete | USERS | `users.manage` | Yes |
| Company | Yes | `Company` Index, Save | **none** | `settings.manage` | Yes, if `showSettings` |
| Stores | Yes | `Store` Index, Create, Edit, SetActive, `SetCurrentStore` | **none** | `settings.manage` on Index/Edit/SetActive only | Yes, if `showSettings` |
| Settings | Yes | `Setting` Index, Save | SETTINGS | `settings.manage` | Yes |
| Audit log | Yes | `Audit` Index, ListJson, Details | SETTINGS | `settings.manage` | Yes |
| License | Yes | `License` Index, Save, Suspend, Activate | **none** | Platform Super Admin policy | Yes, Super Admin only |
| Modules | Yes | `Module` Index, Save, ApplyPreset | **none** | Platform Super Admin policy | Yes, Super Admin only |

Company Admin bypass (`CurrentUserAccess` lines 83–88) makes `USERS` and `SETTINGS` look enabled for role `ADMIN` even when the preset left them off. License and Modules stay Super Admin only.

### Existing screens or actions not in the target tree

- `FilterController.GetFilterData` — shared catalog dropdowns. Module PRODUCTS, permission `products.view`.
- `Product.Search`, `Product.FindByBarcode` — pickers. Module only on the barcode/search pair (no permission attribute).
- `Sales.DayClose`, `Sales.CashSummaryJson` — session print/JSON beside the cash-session screen.
- `Store.SetCurrentStore` — store switcher. No module, no permission attribute.
- `CartController` — `GetCart`, `UpsertItem`, `ClearCart`, `GetWishlist`, `ToggleWishlist`. Authorize only.
- `AuthController` — login, OTP, forgot/reset, change password, logout.
- `HomeController.Error`, `StatusCodePage`, `LicenseBlocked`, `NotFoundPage`.
- `Inventory.Index` — entry beside `Stock`.
- `product_images` table with no screen.
- `01_Tables/031_Expenses.sql` exists. No `ExpenseController` in `Controllers/`. No expense menu item.

## 8. Cross-cutting facts

1. **Claims.** `AuthJwtTokenService.CreateToken` (`Services/AuthJwtTokenService.cs` lines 42–58) writes: `sub`, name identifier, name (full name), email, `id_user`, `fk_store`, `fk_company`, `store_code`, `store_name`, `must_change_password`, `is_platform_super_admin`, `remember_me`, `session_version`, `username`. **No module list and no feature list.** Nine module codes would be small; a feature tree in the JWT is not present, so size is not a current problem. Permissions are also not claims; they are loaded per request.

2. **Which company.** One company per client database (`001_Company.sql` header). Enabled modules are the rows in `installation_modules` for that database. `fk_company` on the token is not used by `ModuleRepository`. The license row is a sibling (`installation_config.fk_license`), not a parent of the module rows.

3. **403 in the browser.** `wwwroot/js/sc-ajax.js` lines 353–357: HTTP 403 reads `responseJSON.message`, or the fallback “You do not have permission to perform this action.”, and shows a danger toast. The module filter’s message (“This module is not enabled for your company.”) is what `res.message` carries for AJAX. A normal (non-AJAX) denial is `ForbidResult()` with no toast in this script.

4. **Dashboard widgets.** The home page itself is not behind the DASHBOARD module. `DashboardService.GetHomeAsync` returns setup/welcome without store KPIs when there is no store or no `dashboard.view` (lines 31–58). When both exist, KPIs are loaded with **no further per-widget module check** (lines 61–70): low stock and sales facts render for anyone with `dashboard.view`. Shortcuts **are** module-gated (`BuildQuickActions` 175–189). The live refresh endpoint `Dashboard.Snapshot` **is** behind `[ModuleAccess("DASHBOARD")]`.

## 9. Feasibility verdict

**Feasible with moderate changes.**

The product already has the leaves (controllers, views, a few finer permissions, a hard-coded menu). It does not have a place to store “Reports on, Stock Valuation off” or “Catalog on, Reviews off”. `installation_modules` is a set of module ids. `ModuleAccess` accepts only those nine codes. Presets point at the same ids. Core is “whatever Basic contains”, computed in C#, and Custom is a label.

What would have to change, by layer:

- **Database.** A stored child of a module (menu/feature/report) with its own code, parent, sort, and core/optional flag, plus a table of which children this install has turned on. Preset items would need to point at those children (or both levels). `modules` alone cannot represent the tree: it has no parent and no feature key.
- **Enforcement.** A check finer than `[ModuleAccess("REPORTS")]`, applied per action (page, JSON, export, print). Class-level module attributes would otherwise keep a disabled report callable. Super Admin bypass and the company-Admin bypass for USERS/SETTINGS need an explicit decision so they do not silently turn every new child on.
- **Menu.** `_Layout.cshtml` conditions are per group, plus a few permission checks. Each optional item (Reviews, one report, Transfer, …) would need its own flag. The report hub would need the same flags, because reports are not in the sidebar.
- **Modules page.** `Module/Index` is one checkbox per module, with Basic modules locked. It would need three levels, locked core rows, and a Custom state that is still “whatever was saved” unless Custom becomes a real preset.
- **Presets.** Basic and Premium are module id lists. They would need child rows. Apply Preset already wipes `installation_modules` and writes the preset; the same replace behaviour would have to include children. Core forcing in `AssignModulesAsync` would have to force core children, not only Basic modules.
- **Dashboard.** Shortcuts already combine permission and module. They do not know a feature. Store KPI widgets do not check INVENTORY or POS before showing low stock and sales.
- **Caching.** Nothing to clear today. If a shared cache is added later, module save must drop it. The license cache must stay separate.

## 10. Risks

- Menu hide is not the server gate. Company, Stores, and Audit disappear from the menu when SETTINGS is off, but `CompanyController` and `StoreController` have no `[ModuleAccess]`. A user with `settings.manage` can still open the URL. (Company Admin is treated as SETTINGS-on anyway.)
- `Product.Search` and `Product.FindByBarcode` are covered by PRODUCTS but not by a permission. Any signed-in user who passes the module gate can call them.
- `Store.SetCurrentStore` and Store Create have no permission attribute and no module attribute.
- `CartController` is only `[Authorize]`.
- All report actions, including Data, Export, and Print, share `reports.view`. Hiding one card on the hub would leave its URL working until each action is gated.
- `CustomersController` passes if **either** CUSTOMERS or POS is on. Disabling Customers while POS stays on leaves customer URLs open (Search and QuickCreate also allow `pos.sell` on purpose).
- Invoices stay available with `reports.view` even when the user lacks `pos.sell`, as long as the Sales group is rendered (`showPos` or `showCustomers`).
- Home dashboard KPIs ignore module flags. DASHBOARD off blocks `Snapshot` (refresh) but not the first `Home/Index` render for a user with `dashboard.view`.
- Reviews are hidden without `products.manage`, and the controller also requires `products.manage` for the list. That pair matches. A future “Reviews off” flag would still need the controller, because the permission alone is shared with every other catalog manage screen.
- Preset apply replaces the whole assignment and then forces Basic modules back on. A custom set that tried to drop POS would be overwritten on save.
- Module audit log stores a count, not the before/after codes.
- HTML module denial is a bare `ForbidResult`. Only AJAX shows the module message.

## 11. Open questions

1. Should a disabled feature also disappear from the Roles permission list, or do permissions stay as they are and only the module tree changes?
2. Is Core exactly the current Basic set (Dashboard, Products, POS, Inventory), or a smaller locked set (Dashboard only)?
3. Should company Admin keep the hard bypass that turns USERS and SETTINGS on even when the preset left them off?
4. Should Super Admin bypass every feature flag (as they bypass every module today) or be able to turn a feature off for themselves too?
5. Are presets editable by Super Admin, or still fixed seed data (Basic / Premium) plus an unsaved Custom label?
6. When a parent module is turned off, are its features stored as off, or deleted until the parent is turned on again?
7. Should old report actions (`Sales`, `Products`, `StockValuation`, `Tax`, `Purchases`, `Customers`) be features in the tree, or only the new suite reports?
8. Is “store-wise stock” one screen for the current store (what exists) or a cross-store screen (which does not exist)?
9. Should dashboard KPIs (sales, low stock) hide when POS or INVENTORY is off, matching the shortcuts?
10. Cart and wishlist have no module. Are they in this tree or out of scope?
