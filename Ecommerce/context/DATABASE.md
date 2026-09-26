# SmartCart — Database

**Technology:** Microsoft SQL Server. Database name from scripts/config: `SmartCart`.

**How scripts run**

- Full create: `Ecommerce/Database/RunDatabase.bat` → `Database.sql` (creates DB if needed, tables via `01_Tables/Create.sql` + Patch includes, seed, all procedure Patch.sql files).
- Incremental: `RunPatch.bat` → `Database-Patch.sql` → `01_Tables/Patch.sql` (IF EXISTS / COL_LENGTH only) → `05_Seed/Patch.sql` → procedure patches (`CREATE OR ALTER`).

Connection string key: `Ecommerse`.

## Database Diagram

The current database structure is represented in: [`context/database.dbml`](file:///d:/Faris/Work%20Area/Smart-Cart-/Ecommerce/context/database.dbml)

This DBML file is the database diagram source for SmartCart and can be opened or imported directly into [dbdiagram.io](https://dbdiagram.io).

### Database Structure Summary
- **Total Tables Documented:** 33 physical tables
- **Major Table Groups:**
  1. **Identity & Auth (6 tables):** `Users`, `AdminUsers`, `UserRoles`, `Modules`, `Permissions`, `UserRolePermissions`
  2. **Catalog & Attributes (9 tables):** `Category`, `SubCategory`, `Brand`, `Products`, `ProductVariants`, `Variants`, `VariantValues`, `ProductVariantAttributes`, `ProductStatus`
  3. **Media (4 tables):** `ProductMedia`, `SkuMedia`, `ProductImages`, `ProductVariantImages`
  4. **Purchasing & Stock (4 tables):** `Supplier`, `Purchase`, `PurchaseDetail`, `Stock`
  5. **Storefront Bags (4 tables):** `Cart`, `CartItems`, `Wishlist`, `WishlistItems`
  6. **Order Fulfillment & Payment (4 tables):** `Orders`, `OrderItems`, `Payments`, `Shipping`
  7. **Auxiliary & Logs (2 tables):** `Ratings`, `AuditLogs`
- **Main User-Side Database Entities:** `Users`, `Products`, `ProductVariants`, `Cart`, `CartItems`, `Wishlist`, `WishlistItems`, `Orders`, `OrderItems`, `Payments`, `Shipping`
- **Main Admin-Side Database Entities:** `AdminUsers`, `UserRoles`, `Modules`, `Permissions`, `UserRolePermissions`, `Category`, `SubCategory`, `Brand`, `Products`, `ProductVariants`, `ProductMedia`, `SkuMedia`, `Supplier`, `Purchase`, `PurchaseDetail`, `Stock`, `Orders`, `OrderItems`
- **Important Relationships:** 36 foreign keys linking catalog hierarchy, SKU attributes, purchase stock batches, storefront bags, and customer orders.

## Access from the app

| Consumer | Mechanism |
| --- | --- |
| Storefront + admin orders | Dapper, stored procedures |
| Most admin modules | EF Core mapped in `EcommerceDbContext` |
| Images | Files on disk; URLs in media tables |

EF does **not** map `Users`, `Orders`, `Cart`, `Payments`, `Shipping`, `Ratings`. Those are SP/Dapper (or unused).

## Naming conventions

- Prefer `ID_{Entity}` / `FK_{Entity}` on newer tables.
- Legacy: `Orders.OrderId`, `Users.UserId`, `CartId`, `PaymentId`, `ShippingId`.
- Soft delete columns: `Cancelled`, often `CancelledOn`, `CancelledReason`.
- `SellOnline` BIT NOT NULL default 0 on `Products` and `ProductVariants`.

## Tables (from `01_Tables` scripts)

### Identity

**Users** (legacy PK `UserId`): `UserName`, `FullName`, `PasswordHash`, `Email`, `PhoneNumber`, `IsAdmin`, timestamps, cancelled. Unique email index added in `Users_AddFullName.sql`. Used by storefront auth. `IsAdmin` is **not** how the admin panel authenticates.

**AdminUsers** PK `ID_AdminUser`: `FK_UserRole`, `UserName` unique, `PasswordHash`, `FullName`, `Email`, `PhoneNumber`, `ProfileImageUrl`, `IsActive`, cancelled.

**UserRoles** PK `ID_UserRole`. **Modules** PK `ID_Module`. **Permissions** PK `ID_Permission`, `FK_Module`, `PermissionCode`. **UserRolePermissions** PK `ID_UserRolePermission`, unique (`FK_UserRole`, `FK_Permission`).

### Catalog

**Category** (script file `Categories.sql`) PK `ID_Category`: Name, Description, IsActive, ImageUrl, cancelled.

**SubCategory** PK `ID_SubCategory`: Name, Description, `FK_Category`, IsActive, ImageUrl, cancelled.

**Brand** PK `ID_Brand`: BrandName, Description, IsActive, ImageUrl, cancelled.

**Products** PK `ID_Product`: `FK_SubCategory`, `FK_Brand` null, Name, Slug unique, Description, IsActive, **SellOnline**, timestamps, cancelled.

**ProductVariants** PK `ID_ProductVariant`: `FK_Product`, SKU unique, Barcode unique null, VariantLabel, Description, MRP, SellingPrice, UnitOfMeasure, UnitValue, IsDefault, MaxOrderQty, IsActive, **SellOnline**, cancelled.

**Variants** PK `ID_Variant` (axis, e.g. Size). **VariantValues** PK `ID_VariantValue`, `FK_Variant`.

**ProductVariantAttributes** PK `ID_ProductVariantAttribute`: `FK_ProductVariant`, `FK_Variant`, `FK_VariantValue`; unique (`FK_ProductVariant`, `FK_Variant`).

### Media

**ProductMedia** PK `ID_ProductMedia`: `FK_Product`, MediaType Image|Video, MediaUrl, DisplayOrder, IsPrimary. Index on (`FK_Product`, `DisplayOrder`). **Live product media.**

**SkuMedia** PK `ID_SkuMedia`: `FK_ProductSKU` → ProductVariants, MediaType Image only, MediaUrl, DisplayOrder, IsPrimary. Patch can copy from `ProductVariantImages`.

**ProductImages** (legacy): `ProductImageId`, `ProductId`, ImageUrl, cancelled. Still created on full install. **Not** the admin product uploader target.

**ProductVariantImages**: `ID_ProductVariantImage`, `FK_ProductVariant`, ImageUrl, DisplayOrder, IsPrimary. Still used by some SKU EF code; storefront order/cart SPs read **SkuMedia** then ProductMedia.

### Purchasing and stock

**Supplier** PK `ID_Supplier`: Name, CompanyName, Email, Phone, State, District, City, Address, Pincode, Description, IsActive, cancelled.

**Purchase** PK `ID_Purchase`: `FK_Supplier`, PurchaseDate, InvoiceNumber, TotalAmount, Notes, EnterBy, cancelled.

**PurchaseDetail** PK `ID_PurchaseDetail` (script `PurchaseDetails.sql`): purchase line, `FK_ProductVariant`, qty/price fields as in that script.

**Stock** PK `ID_Stock`: `FK_PurchaseDetail`, `FK_ProductVariant`, Quantity, CreatedOn, EnterBy, cancelled. Available qty = SUM of non-cancelled Quantity per SKU. Inventory adjust may insert `FK_PurchaseDetail = 0` (FK to PurchaseDetail is declared NOT NULL in create script — **runtime adjust depends on DB allowing 0 without a matching purchase detail**; treat constraint vs app insert as **NOT CONFIRMED** if a patch loosened the FK).

### Storefront bags

**Cart** PK `CartId`: `UserId`, `SessionKey` null, CreatedAt. **WishList** similar + cancelled. C# cart/wishlist APIs pass **UserId only**.

**CartItems** PK `CartItemId`: CartId, ProductId, ProductVariantId, Quantity, Price.

**WishlistItems** PK `WishlistItemId`: WishlistId, ProductId, cancelled.

### Orders

**Orders** PK `OrderId`: UserId → Users, OrderDate, TotalAmount, OrderStatus, ShippingAddress, PaymentMethod, cancelled + reason. Patch adds OrderNumber, ReceiverName, Phone, AddressLine, City, Pincode.

**OrderItems** PK `ID_OrderItem`: `FK_Order` → Orders.OrderId, `FK_Product`, `FK_ProductVariant`, snapshot names/SKU/prices.

**Payments** PK `PaymentId`: OrderId, PaymentDate, PaymentAmount, PaymentStatus, PaymentMethod, cancelled. Written by `PlaceOrder` / updated on deliver.

**Shipping** PK `ShippingId`: OrderId, ShippingAddress, ShippingDate, EstimatedDeliveryDate, ShippingStatus, cancelled.

### Present in create scripts, little or no app use

| Table | Status |
| --- | --- |
| Ratings | Table + unused shop DTO fields. No rating UI/API. |
| AuditLogs | Created. No app writes found. |
| ProductStatus | Created. Not used by current product module. |
| StoreSettings | **Removed** from scripts; Patch drops leftover table/procs if present. |

## Relationships (core)

```
Category 1—n SubCategory 1—n Products n—1 Brand
Products 1—n ProductVariants 1—n ProductVariantAttributes n—1 Variants / VariantValues
ProductVariants 1—n Stock (via FK_ProductVariant)
Purchase 1—n PurchaseDetail → ProductVariant; Stock.FK_PurchaseDetail
Users 1—n Orders 1—n OrderItems → Product + ProductVariant
Users 1—n Cart 1—n CartItems
AdminUsers n—1 UserRoles 1—n UserRolePermissions n—1 Permissions n—1 Modules
```

## Stored procedures (in repo and wired from C# unless noted)

### Users / Shop (`02_Users/01_Shop`)

`GetProducts`, `GetProductDetails`, `GetShopFilters`. **`GetProductDetailsById` exists, not called from C#.**

### Users / Cart

`GetCart`, `GetBagCounts`, `AddCartItem`, `UpdateCartItem`, `RemoveCartItem`, `ClearCart`.

### Users / Wishlist

`GetWishlist`, `GetWishlistStatus`, `ToggleWishlist`, `RemoveWishlistItem`.

### Users / Auth

`RegisterUser`, `GetUserByEmail`, `GetUserById`.

### Users / Orders

`GetCheckoutPreview`, `PlaceOrder`, `GetOrder`, `GetOrders`, `CancelOrder`.

### Admin / Orders (used)

`GetAdminOrders`, `GetAdminOrder`, `AdminConfirmOrder`, `AdminUpdateOrderStatus`, `AdminDeliverOrder`, `AdminCancelOrder`.

### Admin / other folders (`01_Admin/01`–`11`, `03_Common/01_Stock`)

Scripts such as `ProProductListSelect`, `ProCategoryListSelect`, `ProEmployeeListSelect`, `ProProductVariantStockSelect`, `ProAdminAuthSelectByUserName` **exist on disk**. Current admin repositories for those modules use **EF**, not these names. Treat as legacy/optional DB objects unless a caller is added.

## Functions

No user-defined SQL functions were found under `Database/`.

## Indexes (from scripts)

- `UQ_Products_Slug`, unique SKU/Barcode on ProductVariants
- `IX_ProductMedia_FK_Product`, `IX_SkuMedia_FK_ProductSKU`
- `IX_ProductVariantImages_FK_ProductVariant`
- `UX_Users_Email` (patch)
- Unique AdminUsers.UserName, UserRolePermissions pair, PermissionCode, ModuleName

## Database rules encoded in SPs (not only C#)

- Shop/cart/place require product+SKU active, not cancelled, **SellOnline = 1**
- PlaceOrder: COD only; UPI returns “available soon”; address/phone/pincode format checks; stock must cover qty; order number `SC` + padded id
- Admin: Placed/Pending → Confirmed; Confirmed → Shipped (status update only); Confirmed or Shipped → Delivered (payment collected); cancel only Placed/Pending/Confirmed (not after Shipped); customer cancel more restricted (Placed/Pending in customer SP — see USER orders module)

## Seed (`05_Seed`)

Modules, Permissions, UserRoles, UserRolePermissions (Admin role gets all remaining permissions), AdminUsers (dev admin), optional SuperMarketCatalog / DevSampleCatalog.

**Billing** module and permissions are seeded. **Customers** and **Ratings** appear only as permission-tree group labels in C# if those modules exist in DB; they are **not** seeded in `Modules.sql`.
