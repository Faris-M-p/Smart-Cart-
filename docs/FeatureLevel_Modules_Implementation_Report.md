# Feature-level modules — implementation report

Branch: `Pro_12_Modules_restructure`.

The module switch stays the nine existing codes. Under each module, `module_features` stores groups and leaves. Only leaves are saved in `installation_features`. A module is on when at least one of its leaves is on. `[ModuleAccess]` is unchanged and still runs. `[FeatureAccess]` is an extra gate on optional screens.

## 1. Scripts and run order

**Existing database**

1. Apply `SmartCartPro/Database-Postgres/03_Patches/V012__FeatureLevelModules.sql` (idempotent).
2. Record `V012` in `patch_history` when the patch is applied outside `ApplyPatches.bat`.

Applied on `smartcart_pro` during this work. `patch_history` has `V012` / success.

**Fresh database**

1. `BuildDatabase.bat` creates tables. `01_Tables/005_Modules.sql` now creates `modules.description` and `modules.is_core`.
2. Seeds `001`–`004`, then `02_Seed/007_ModuleFeatures.sql`, which runs the same V012 script.
3. Later patches, including V012 again, are idempotent.

`005_ModulePresets.sql` is not in the build bat. Preset tables are created by V004 and again, if missing, by V012.

Do not edit older patch files. No EF migration.

## 2. Feature catalog

54 leaves. Groups (not stored as enabled): `REPORTS.SALES`, `REPORTS.STOCK`, `REPORTS.PURCHASE`, `REPORTS.PARTIES`. `REPORTS.TAX.GST` and `REPORTS.PROFIT.MARGIN` sit directly under Reports. License and Modules are not in the tree.

Core modules (`modules.is_core`): DASHBOARD, PRODUCTS, SETTINGS, USERS. Their core leaves cannot be stored off.

| Code | Name | Core | Actions |
|---|---|---|---|
| DASHBOARD.OVERVIEW | Dashboard overview | yes | `Home.Index` (signed-in landing; no feature attribute) |
| DASHBOARD.METRICS | Store metrics | yes | Dashboard KPIs, `Dashboard.Snapshot` |
| DASHBOARD.SHORTCUTS | Quick shortcuts | yes | `DashboardService.BuildQuickActions` |
| PRODUCTS.CATEGORIES | Categories | yes | Catalog category actions |
| PRODUCTS.SUBCATEGORIES | Sub-categories | yes | Catalog sub-category actions |
| PRODUCTS.BRANDS | Brands | no | Brand pages, `SetBrandActive`, `BrandLookup` |
| PRODUCTS.UNITS | Units | yes | Catalog unit actions |
| PRODUCTS.TAXES | Taxes | yes | Catalog tax actions |
| PRODUCTS.PRODUCTS | Products | yes | Product list/create/edit/details/active, `Search`, `FindByBarcode`, filter data |
| PRODUCTS.VARIANTS | SKU / Variants | no | `Variants`, `CreateVariant`, `EditVariant`, `SetVariantActive` |
| PRODUCTS.REVIEWS | Reviews | no | `ProductReview` (class) |
| INVENTORY.STOCK | Stock overview | yes | `Inventory.Index`, `Stock`, `ProductPicker` |
| INVENTORY.LOW_STOCK | Low stock | no | `Inventory.LowStock` |
| INVENTORY.HISTORY | Stock history | no | `Inventory.History` |
| INVENTORY.OPENING | Opening stock | no | `Inventory.Opening` GET/POST |
| INVENTORY.ADJUST | Adjust stock | no | `Inventory.Adjust` GET/POST |
| INVENTORY.TRANSFER | Transfer | no | `Inventory.Transfer` GET/POST |
| PURCHASES.SUPPLIERS | Suppliers | yes | `Supplier` |
| PURCHASES.ORDERS | Purchase orders | yes | Purchase list/create/edit/details/print/cancel/lookup |
| PURCHASES.RECEIVING | Purchase receiving | no | `Purchase.Receive` GET/POST |
| POS.BILLING | POS billing | yes | `Sales.Pos`, `CreateSale`, `ProductByBarcode`, `ProductSearch` |
| POS.CASH_SESSION | Cash session | no | `Session`, `OpenSession`, `CloseSession`, `DayClose`, `CashSummaryJson` |
| POS.INVOICES | Invoices | yes | Invoice list/details/print/json, `CancelSale` |
| POS.RETURNS | Returns | no | `Returns`, `Return` GET/POST, `ReturnDetails` |
| POS.ONLINE_ORDERS | Online orders | no | `OnlineOrder` (class) |
| CUSTOMERS.MANAGE | Customers | yes | `Customers.Index/Create/Edit/SetActive` |
| REPORTS.HUB | Reports hub | yes | `Reports.Index` (module + `reports.view` only) |
| REPORTS.SALES.BILL_WISE | Sales bill-wise | no | Suite bill-wise + old `Sales`, `SalesCsv` |
| REPORTS.SALES.ITEM | Item-wise sales | no | Suite item + old `Products`, `ProductsCsv` |
| REPORTS.SALES.CATEGORY | Category-wise sales | no | Suite category |
| REPORTS.SALES.RETURNS | Sales returns | no | Suite sales returns |
| REPORTS.SALES.DAY_CLOSE | Day close / cashier | no | Suite day close |
| REPORTS.SALES.DISCOUNT | Discount report | no | Suite discount |
| REPORTS.SALES.CANCELLED | Cancelled bills | no | Suite cancelled bills |
| REPORTS.SALES.HOURLY | Hourly sales | no | Suite hourly |
| REPORTS.STOCK.VALUATION | Stock valuation | no | Suite current stock + old `StockValuation`, `StockValuationCsv` |
| REPORTS.STOCK.ITEM_LEDGER | Item ledger | no | Suite item ledger |
| REPORTS.STOCK.STOCK_LEDGER | Stock ledger | no | Suite stock ledger |
| REPORTS.STOCK.MOVEMENT | Stock inward/outward | no | Suite movement |
| REPORTS.STOCK.NON_MOVING | Non-moving items | no | Suite non-moving |
| REPORTS.STOCK.LOW_STOCK | Low stock / reorder | no | Suite low stock |
| REPORTS.STOCK.ADJUSTMENTS | Adjustments & transfers | no | Suite adjustments |
| REPORTS.PURCHASE.BILLS | Purchase bills | no | Suite purchase bills + old `Purchases`, `PurchasesCsv` |
| REPORTS.PURCHASE.ANALYSIS | Supplier / item purchase | no | Suite purchase analysis |
| REPORTS.TAX.GST | GST summary | no | Suite GST + old `Tax`, `TaxCsv` |
| REPORTS.PROFIT.MARGIN | Profit / margin | no | Suite profit (still also `reports.profit`) |
| REPORTS.PARTIES.CUSTOMERS | Customer report | no | Old `Customers`, `CustomersCsv` |
| REPORTS.PARTIES.MASTERS | Master lists | no | Suite master lists |
| SETTINGS.SETTINGS | Settings | yes | `Setting` |
| SETTINGS.COMPANY | Company | yes | `Company` |
| SETTINGS.STORES | Stores | yes | Store list/create/edit/active |
| SETTINGS.AUDIT | Audit log | no | `Audit` (class) |
| USERS.EMPLOYEES | Employees | yes | `Employee` |
| USERS.ROLES | Roles | yes | `Role` |

`*` on a suite report means page, `Data`, `Export`, and `Print`. No catalog action name was missing. Store-wise stock, product images, barcode scan, discounts, and payment processing are not features. `Customers.Search` and `QuickCreate` stay available when POS is on, even if CUSTOMERS is off.

**Presets**

- Basic (22 leaves): core leaves of DASHBOARD, PRODUCTS, INVENTORY, POS, SETTINGS, USERS, plus `PRODUCTS.BRANDS`, `INVENTORY.LOW_STOCK`, `INVENTORY.OPENING`, `INVENTORY.ADJUST`, `POS.CASH_SESSION`, `POS.RETURNS`.
- Premium: all 54 leaves.
- `module_preset_items` includes SETTINGS and USERS for Basic, and all nine modules for Premium.

## 3. Files by phase

**Database**

- `Database-Postgres/03_Patches/V012__FeatureLevelModules.sql`
- `Database-Postgres/02_Seed/007_ModuleFeatures.sql`
- `Database-Postgres/01_Tables/005_Modules.sql` (`description`, `is_core` on create)
- `Database-Postgres/BuildDatabase.bat` (runs 007)

**Server**

- `Entities/ModuleEntities.cs`, `Data/SmartCartDbContext.cs`
- `Models/ModuleModels.cs`
- `Repositories/ModuleRepository.cs`, `Repositories/Interfaces/IModuleRepository.cs`
- `Services/ModuleService.cs`, `Services/Interfaces/IModuleService.cs`
- `Services/CurrentUserAccess.cs`, `Services/Interfaces/ICurrentUserAccess.cs`
- `Filters/FeatureAccessAttribute.cs`
- `Helpers/ReportFeatureMap.cs`
- `Controllers/ModuleController.cs`

**Feature attributes**

- `CatalogController`, `ProductController`, `ProductReviewController`
- `InventoryController`, `PurchaseController`, `SalesController`, `OnlineOrderController`
- `ReportsController`, `ReportsController.Suite.cs`
- `AuditController`

**Security gaps**

- `CompanyController` — `[ModuleAccess("SETTINGS")]`
- `StoreController` — SETTINGS on list/create/edit/active; `settings.manage` on Create; `SetCurrentStore` still checks store access and is not module-gated
- `ProductController.Search` / `FindByBarcode` — `products.view` or `pos.sell`
- `CustomersController` — list/create/edit/active require CUSTOMERS; Search and QuickCreate still allow CUSTOMERS or POS

**Modules page**

- `Views/Module/Index.cshtml`
- `wwwroot/js/admin/module.js`
- `wwwroot/css/modules.css`

**Menus and screens**

- `Views/Shared/_Layout.cshtml`
- `Models/DashboardModels.cs`, `Services/DashboardService.cs`, `Controllers/HomeController.cs`, `Controllers/DashboardController.cs`, `Views/Home/Index.cshtml`
- `Views/Reports/Index.cshtml`, `Services/ReportService.Suite.cs` (customer report card), `Views/Reports/Products.cshtml`
- Inventory tabs, stock, history; purchase index/details; invoice details; POS; product create/edit/list/details

## 4. Where a feature flag is checked

- **Attribute:** every optional action in the catalog above. Denied JSON is HTTP 403, message “This feature is not enabled for your company.” Denied pages use the same forbid result as `[ModuleAccess]`.
- **`CurrentUserAccess.FeatureEnabledAsync`:** one `OutputFeatureGate` per request. Super Admin is always allowed. Company Admin (`ADMIN`) is allowed the core leaves of USERS and SETTINGS only (`SETTINGS.AUDIT` is not included). A leaf counts only when its module is on. If the feature table is empty, checks fall back to the module code prefix so a deploy before the patch does not 403 every optional screen. A missing `is_core` column still requires V012.
- **Sidebar:** one flag set at the top of `_Layout.cshtml` for Brands, Variants, Reviews, low stock, history, opening, adjust, transfer, cash session, returns, online orders, audit, plus core items used by the menu. A group header is hidden when none of its links are visible. Reports also require `REPORTS.HUB`.
- **Report hub:** `Reports.Index` keeps a card only when `ReportFeatureMap` says the feature is on. Empty sections disappear. No cards shows “No reports are enabled for this company.” Suite Show/Print drop links to disabled report actions.
- **Dashboard:** shortcuts use the matching feature (`INVENTORY.LOW_STOCK` for Low stock, `PURCHASES.ORDERS` for New purchase, `POS.BILLING` for Open POS, `REPORTS.HUB` for Reports). Sales, payments, invoices, trend, and top products need POS billing. Low stock needs `INVENTORY.LOW_STOCK`. Pending purchases need `PURCHASES.ORDERS`. Online orders need `POS.ONLINE_ORDERS`. Open cash sessions need `POS.CASH_SESSION`. `Dashboard.Snapshot` clears the same numbers.
- **Other links:** purchase Receive (`PURCHASES.RECEIVING`), invoice Return (`POS.RETURNS`), POS Sessions and the open-session form (`POS.CASH_SESSION`), product brand field and brand filter (`PRODUCTS.BRANDS`), variant links (`PRODUCTS.VARIANTS`), inventory tabs and stock buttons, product-report Low stock link. Edit product posts the current brand in a hidden field when the dropdown is hidden.

Save replaces `installation_features` and `installation_modules` in one transaction. Audit `Details` lists added and removed feature codes.

## 5. Verification

| Check | Result |
|---|---|
| `dotnet build` | 0 errors (existing nullable warnings only) |
| V012 on `smartcart_pro` | Applied. 9 modules, 54 leaves, every previously installed module has all of its leaves in `installation_features` (3+8+6+3+5+1+22+4+2). |
| Core flags | DASHBOARD, PRODUCTS, SETTINGS, USERS are `is_core`. |
| Presets | Basic 22 leaves, Premium 54. |
| `installation_modules` after save | Rebuilt from enabled leaves in the same transaction as the feature rows. |
| Fresh-install path | 007 runs V012 from `BuildDatabase.bat`. Not executed as a second empty database in this session. |
| Reports only valuation / Reviews off | Rules are in `ModuleService.Normalize` and `[FeatureAccess]`. Not clicked through a signed-in browser in this session. |
| Modules page save, discard, presets, toggle, unsaved warning | Implemented in `module.js`. Not exercised in the browser here. |
| Super Admin / Company Admin | Super Admin short-circuits feature checks. Company Admin bypass is limited to core USERS and SETTINGS leaves in `GetFeatureGateAsync`. |
| N+1 | Enabled codes load once per request in `CurrentUserAccess`. |

The process already listening on port 5008 is an older build. This session did not stop it and did not sign in, so sidebar, hub, dashboard, and the Modules page were not clicked in the browser. Restart the app from this branch and sign in as Super Admin to confirm the tree.

## 6. Behaviour users can notice

- After V012, every module that was already on keeps every leaf on, so menus match the previous install.
- V012 also inserts DASHBOARD, PRODUCTS, SETTINGS, and USERS into `installation_modules` if they were missing. A role that already has `settings.manage` or `users.manage` can see those screens even if those modules had been switched off. Company Admin already saw them through the role bypass.
- Customer list, create, edit, and activate now require the CUSTOMERS module. POS customer search and quick-create still work with POS alone.
- Invoices in the Sales menu need `POS.INVOICES` as well as `pos.sell` or `reports.view`. With POS off, the invoice link is hidden.
- Company and store maintenance require SETTINGS. Store create still requires Platform Super Admin, and now also `settings.manage` (Super Admin bypasses the permission).
- Product search and barcode lookup require `products.view` or `pos.sell`.
- If cash sessions are off and no session is open, POS shows a message instead of posting `OpenSession`.
- The Modules page no longer saves a preset on click. Basic and Premium fill the tree; Save writes it. `ApplyPreset` still saves immediately for older callers.
- Turning an optional module on from the tree enables its core leaves only. Optional leaves stay as they were. Clear Optional removes optional leaves and leaves core leaves on, so Inventory, Purchases, POS, Customers, and Reports stay on through their core screens.

## Open points

- The live site on port 5008 must be restarted from this build before the new page is visible.
- Roles and permissions were not changed.
- Cart and Wishlist were not changed.
