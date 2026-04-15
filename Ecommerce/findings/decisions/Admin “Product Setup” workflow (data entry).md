Phase 1: Admin “Product Setup” workflow (data entry)
1) Master data
Brands, Categories, SubCategories, Variants, VariantValues (you already completed)
2) Product create/update
Insert into Products
3) SKU create/update
Insert into ProductVariants
Insert rows into ProductVariantAttributes (one per axis)
Insert default image(s) into ProductImages (optional but recommended)
4) Stock entry (this is the missing piece you asked)
When a SKU is created, ensure a row exists in Stock:
FK_VariantProduct (unique), QuantityAvailable, QuantityReserved
Admin screen to set/adjust stock per SKU
simplest: “Set Available Qty”
better: “Adjust + / -” (audit later)
Result: Admin can create Product + SKUs + Stock, without Purchases/Suppliers.

Phase 2: User-side catalog APIs (read-only)
Product list endpoint
returns: ProductName, Slug, Brand, Category/SubCategory, StartingPrice = MIN(SellingPrice) from active SKUs, InStock = any SKU stock > 0
Product detail endpoint
returns: product info + all SKUs with:
VariantLabel, SellingPrice, MRP, Combination
QuantityAvailable from Stock
default image from ProductImages
Result: User can browse products and see available variants/prices/stock.

Phase 3: User pages
Shop page (list + filters + search)
Product detail page (variant picker + price + stock)
Phase 4: Cart (after user can view)
Cart stores FK_VariantProduct (SKU id)
On add/update: validate QuantityAvailable - QuantityReserved
Immediate next step (what we implement next)
Implement Admin SKU Stock screen using your Stock table:

Add API: GetStockBySku, SetStock, AdjustStock
Show stock column on SKU grid
Auto-create Stock row when SKU is created
If you confirm this is the next module, tell me: do you want “Set Stock” only, or “Adjust (+/-) Stock” too?