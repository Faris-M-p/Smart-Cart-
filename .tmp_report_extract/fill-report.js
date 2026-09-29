/**
 * Fill SmartCart_Project_Report.docx from verified project facts.
 * Preserve screenshot/diagram placeholders and personal SC-INFO/SC-CERT fields.
 */
const fs = require("fs");
const path = require("path");
const { execFileSync } = require("child_process");

const WORK = path.resolve(__dirname);
const UNZIPPED = path.join(WORK, "unzipped");
const DOC_XML = path.join(UNZIPPED, "word", "document.xml");
const OUT_DOCX = path.resolve(__dirname, "..", "Ecommerce", "SmartCart_Project_Report.docx");
const OUT_TMP = path.join(WORK, "SmartCart_Project_Report_filled.docx");

const EM = "\u2014"; // em dash used in template placeholders

function esc(s) {
  return String(s)
    .replace(/&/g, "&amp;")
    .replace(/</g, "&lt;")
    .replace(/>/g, "&gt;");
}

let xml = fs.readFileSync(DOC_XML, "utf8");
const original = xml;

function count(str, needle) {
  return str.split(needle).length - 1;
}

/** Replace full text of any <w:t>...</w:t> that contains the marker (exact contiguous). */
function replaceWtContaining(marker, newText) {
  const re = new RegExp(
    `(<w:t(?:\\s[^>]*)?>)([^<]*${marker.replace(/[.*+?^${}()|[\]\\]/g, "\\$&")}[^<]*)(</w:t>)`,
    "g"
  );
  let n = 0;
  xml = xml.replace(re, (full, open, _old, close) => {
    n++;
    return open + esc(newText) + close;
  });
  if (n === 0) console.warn("MISS marker:", marker);
  else console.log("OK", marker, "x" + n);
}

/** Replace exact substring if present (may span... but usually in one w:t). */
function replaceAll(from, to) {
  const n = count(xml, from);
  if (n === 0) {
    console.warn("MISS text:", from.slice(0, 70));
    return;
  }
  xml = xml.split(from).join(to);
  console.log("REPL", n, from.slice(0, 50));
}

// ---------------------------------------------------------------------------
// Tech corrections (PostgreSQL is the live stack)
// ---------------------------------------------------------------------------
replaceAll(
  "ASP.NET Core, SQL Server, Bootstrap and related technologies",
  "ASP.NET Core, PostgreSQL, Bootstrap and related technologies"
);
replaceAll(
  "Keywords: E-Commerce, Supermarket, ASP.NET Core MVC, SQL Server, Stored Procedures, Dapper, Inventory Management, Role-Based Access Control, JWT.",
  "Keywords: E-Commerce, Supermarket, ASP.NET Core MVC, PostgreSQL, Stored Procedures, Dapper, Entity Framework Core, Inventory Management, Role-Based Access Control, JWT."
);
replaceAll(
  "Relational database (SQL Server during development) with stored procedures",
  "Relational database (PostgreSQL) with stored procedures for storefront flows"
);
replaceAll(
  "Microsoft SQL Server (LocalDB / Express) during development",
  "PostgreSQL (local development; connection key PostgresConnection)"
);
replaceAll(
  "Dapper 2.1.35, Microsoft.Data.SqlClient, EF Core SQL Server 8.0.11",
  "Dapper 2.1.35, Npgsql 8.0.6, Npgsql.EntityFrameworkCore.PostgreSQL 8.0.11"
);
replaceAll(
  ".NET SDK, SQL Server Express/LocalDB, Node.js, open-source libraries",
  ".NET SDK, PostgreSQL, Node.js, open-source libraries"
);
replaceAll(
  "ASP.NET Core, SQL Server, Bootstrap) available",
  "ASP.NET Core, PostgreSQL, Bootstrap) available"
);
replaceAll(
  "SQL Server (LocalDB in default configuration)",
  "PostgreSQL (default local configuration via PostgresConnection)"
);
replaceAll(
  "[3] Microsoft. SQL Server technical documentation. https://learn.microsoft.com/sql/sql-server",
  "[3] PostgreSQL Global Development Group. PostgreSQL documentation. https://www.postgresql.org/docs/"
);

// Connection string key — try curly/straight quotes
for (const q of ['\u201c', '"', "&quot;"]) {
  const from = `connection string key in configuration is ${q}Ecommerse${q === "&quot;" ? "&quot;" : q === "\u201c" ? "\u201d" : '"'}`;
  // simpler patterns:
}
replaceAll(
  "configuration is \u201cEcommerse\u201d",
  "configuration is \u201cPostgresConnection\u201d"
);
replaceAll(
  'configuration is "Ecommerse"',
  'configuration is "PostgresConnection"'
);
replaceAll(
  "configuration is &quot;Ecommerse&quot;",
  "configuration is &quot;PostgresConnection&quot;"
);
replaceAll(
  "connection string key \u201cEcommerse\u201d",
  "connection string key \u201cPostgresConnection\u201d"
);
replaceAll(
  "connection string key &quot;Ecommerse&quot;",
  "connection string key PostgresConnection"
);

// Section 7.2 paragraph (long) — replace SQL Server wording inside the w:t
replaceAll(
  "During development, Microsoft SQL Server (LocalDB/Express) is used, accessed through Microsoft.Data.SqlClient, Dapper and EF Core\u2019s SQL Server provider.",
  "The current implementation uses PostgreSQL, accessed through Npgsql, Dapper and EF Core\u2019s PostgreSQL provider."
);
replaceAll(
  "During development, Microsoft SQL Server (LocalDB/Express) is used, accessed through Microsoft.Data.SqlClient, Dapper and EF Core's SQL Server provider.",
  "The current implementation uses PostgreSQL, accessed through Npgsql, Dapper and EF Core's PostgreSQL provider."
);

replaceAll(
  "Customer operations and admin order processing use stored procedures executed through Dapper, where business rules such as stock availability, online-sale eligibility and order-status transitions are enforced in the database. Other administrative modules use Entity Framework Core without migrations; the schema is created and updated through versioned build and patch scripts.",
  "Customer storefront operations (shop, cart, wishlist, auth, checkout and customer orders) use PostgreSQL stored procedures executed through Dapper, where business rules such as stock availability, online-sale eligibility and payment-method checks are enforced in the database. Administrative modules, including admin order processing, use Entity Framework Core (Npgsql) without migrations; the schema is created and updated through versioned PostgreSQL build and patch scripts under Database-Postgres."
);

replaceAll(
  "Storefront repositories depend on IDataAccessDapper and call stored procedures. Admin catalogue, supplier, purchase, stock, employee, role and admin-login repositories depend on EcommerceDbContext. The admin order repository is an exception and uses Dapper with dedicated procedures.",
  "Storefront repositories depend on IDataAccessDapper and call PostgreSQL stored procedures. Admin catalogue, supplier, purchase, stock, employee, role, admin-login, homepage, POS sales and admin-order repositories depend on EcommerceDbContext (EF Core)."
);

replaceAll(
  "A full build script creates the database, tables, seed data and procedures; an incremental patch script applies changes safely to an existing database using existence checks (IF OBJECT_ID, IF COL_LENGTH) and CREATE OR ALTER for procedures.",
  "A full PostgreSQL build script (Database-Postgres/Database.sql via psql) creates tables, seed data and procedures; patch scripts apply incremental changes with CREATE OR REPLACE for procedures."
);

replaceAll(
  "DataAccessDapper \u2192 SQL Server (GetProducts)",
  "DataAccessDapper \u2192 PostgreSQL (get_products)"
);
replaceAll(
  "\u2192 SQL Server database; side components",
  "\u2192 PostgreSQL database; side components"
);
replaceAll(
  "EF Core EcommerceDbContext) \u2192 SQL Server database",
  "EF Core EcommerceDbContext) \u2192 PostgreSQL database"
);

// Remaining generic "Microsoft SQL Server" / "SQL Server" in body (careful)
replaceAll("Microsoft SQL Server", "PostgreSQL");
console.log("Remaining SQL Server:", count(xml, "SQL Server"));

// ---------------------------------------------------------------------------
// Placeholder fills keyed by SC-* code (full w:t content replaced)
// ---------------------------------------------------------------------------
const ph = (code, title) => `[${code}]  PLACEHOLDER ${EM} ${title}`;

const fillsByMarker = {
  [ph("SC-DATA-001", "CONFIRM DATABASE ENGINE FOR SUBMISSION")]:
    "CONFIRMED FROM PROJECT: The running application uses PostgreSQL. Program.cs registers UseNpgsql with connection string PostgresConnection; Ecommerce.csproj references Npgsql and Npgsql.EntityFrameworkCore.PostgreSQL; health check postgres_dapper is registered. Schema and procedures for the current stack are maintained under Ecommerce/Database-Postgres. Legacy SQL Server scripts under Ecommerce/Database/ remain for historical reference but are not wired in Program.cs.",

  [ph("SC-TABLE-003", "CONFIRM TABLE COUNT")]:
    "CONFIRMED FROM PROJECT: Database-Postgres/01_Tables defines 42 physical tables (users, useraddresses, category, subcategory, brand, products, productvariants, variants, variantvalues, productvariantattributes, productmedia, skumedia, productimages, productvariantimages, productstatus, supplier, purchase, purchasedetail, stock, cart, cartitems, wishlist, wishlistitems, orders, orderitems, payments, shipping, sales, salesdetail, salesreturn, salesreturndetail, adminusers, userroles, modules, permissions, userrolepermissions, homepage_banners, homepage_banner_categories, homepage_categories, homepage_products, ratings, auditlogs). Live use focuses on identity, catalogue, media, purchasing/stock, shopping bags, orders, POS sales and homepage; ratings, auditlogs, productstatus and legacy productimages have little or no active UI wiring.",

  [ph("SC-DATA-004", "COLUMN-LEVEL DETAIL " + EM + " USER / AUTH TABLES")]:
    "users (PK id_user): username, fullname, passwordhash, email (unique ux_users_email), phonenumber, isadmin, createdat, updatedat, cancelled…. useraddresses (PK addressid): userid→users, addresstype, receivername, phone, addressline, city, pincode, latitude, longitude, isdefault, createdat, cancelled…. adminusers (PK id_adminuser): fk_userrole→userroles, username unique, passwordhash, fullname, email, phonenumber, profileimageurl, isactive, cancelled…. userroles; modules; permissions; userrolepermissions (unique role+permission). Seed includes Billing.* permissions without a Billing controller.",

  [ph("SC-DATA-005", "COLUMN-LEVEL DETAIL " + EM + " CATALOGUE TABLES")]:
    "category (PK id_category): name, description, isactive, imageurl, cancelled…. subcategory (PK id_subcategory, fk_category). brand (PK id_brand, brandname…). products (PK id_product): fk_subcategory, fk_brand, name, slug unique, description, isactive, sellonline, cancelled…. productvariants (PK id_productvariant): fk_product, sku unique, barcode unique nullable, variantlabel, mrp, sellingprice, maxorderqty, isactive, sellonline…. variants / variantvalues / productvariantattributes. Media: productmedia, skumedia. supplier for purchasing.",

  [ph("SC-DATA-006", "COLUMN-LEVEL DETAIL " + EM + " CART / WISHLIST TABLES")]:
    "cart (PK id_cart): fk_user, sessionkey nullable UUID, createdat. cartitems (PK id_cartitem): fk_cart, fk_product, fk_productvariant, quantity, price. wishlist (PK id_wishlist): fk_user, sessionkey, cancelled…. wishlistitems (PK id_wishlistitem): fk_wishlist, fk_product, cancelled…. Storefront APIs require logged-in UserId; guest/session merge is not implemented.",

  [ph("SC-DATA-007", "COLUMN-LEVEL DETAIL " + EM + " ORDER TABLES")]:
    "orders (PK id_order): fk_user, orderdate, totalamount, orderstatus, shippingaddress, paymentmethod, cancelled…, ordernumber, receivername, phone, addressline, city, pincode. orderitems (PK id_orderitem): fk_order, fk_product, fk_productvariant, productname, variantlabel, sku, unitprice, quantity, linetotal. payments and shipping link to the order as defined in payments.sql and shipping.sql. place_order writes these for COD and deducts stock.",

  [ph("SC-DATA-008", "VERIFY STOCK ADJUSTMENT REFERENCE")]:
    "VERIFIED FROM CODE: InventoryRepository.AdjustStockAsync inserts stock with fk_purchasedetail = 0, fk_productvariant = SKU, quantity = AdjustBy. Purchases insert stock batches via purchasedetail. Available qty = SUM(non-cancelled stock.quantity) per SKU. Runtime FK behaviour for fk_purchasedetail = 0: [TO BE FILLED - NOT CONFIRMED FROM PROJECT].",

  [ph("SC-DATA-003", "FRONTEND ASSET INVENTORY")]:
    "Storefront: Razor views (Home, Shop, Cart, Wishlist, Checkout, Orders, Account); CSS storefront/shop-listing/shop-bag; JS shop-listing.js, shop-details.js, shop-checkout.js, shop-bag.js. Admin: Mantis Bootstrap under wwwroot/Admin (SCSS via AspNetCore.SassCompiler + npm Bootstrap); toast/empty-state scripts; views/Admin/*. Uploads: wwwroot/uploads/.",

  [ph("SC-DATA-009", "UNVERIFIED SECURITY SETTINGS")]:
    "VERIFIED: JWT Bearer (issuer SmartCart, audiences SmartCartAdmin/SmartCartUser, lifetime, key length ≥32, ClockSkew=Zero); cookie smartcart.user.token (HttpOnly); smartcart.admin.token (HttpOnly=false); UseHttpsRedirection; UseHsts outside Development; AdminAuthMiddleware; RequirePermission; Identity password hasher; SafeReturnUrl guard. Token expiry JwtSettings.ExpiryMinutes=60. Anti-forgery/account lockout: [TO BE FILLED - NOT CONFIRMED FROM PROJECT].",

  [ph("SC-DATA-010", "COLUMN-LEVEL DATA DICTIONARY")]:
    "Source of truth: Ecommerce/Database-Postgres/01_Tables/*.sql (42 tables). Principal PKs: users.id_user, adminusers.id_adminuser, category.id_category, products.id_product, productvariants.id_productvariant, stock.id_stock, orders.id_order, cart.id_cart, wishlist.id_wishlist. Soft-delete cancelled/cancelledon/cancelledreason on most masters. sellonline on products and productvariants (default false).",

  [ph("SC-DATA-002", "ACTUAL DEVELOPMENT MACHINE SPECIFICATION")]:
    "[TO BE FILLED - NOT CONFIRMED FROM PROJECT] — record CPU, RAM, OS and disk of the development/demonstration machine.",

  [ph("SC-DATA-012", "COST ESTIMATE (OPTIONAL)")]:
    "Optional cost estimate: [TO BE FILLED - NOT CONFIRMED FROM PROJECT]. .NET SDK, PostgreSQL, Node.js and open-source libraries have no development licence fee.",

  [ph("SC-DATA-013", "PERT TIME ESTIMATES (OPTIONAL)")]:
    "PERT time estimates: [TO BE FILLED - NOT CONFIRMED FROM PROJECT] (use personal schedule / SC-INFO-009).",

  [ph("SC-DATA-014", "SAMPLE DATABASE TRACE LOG ENTRY (OPTIONAL)")]:
    "Optional redacted sample from Logs/DatabaseTrace.txt: [TO BE FILLED - NOT CONFIRMED FROM PROJECT].",

  [ph("SC-DATA-015", "TEST ENVIRONMENT")]:
    "ASP.NET Core 8 MVC Ecommerce against PostgreSQL database smartcart (PostgresConnection). Browser(s) used: [TO BE FILLED - NOT CONFIRMED FROM PROJECT].",

  [ph("SC-DATA-011", "PERFORMANCE OBSERVATIONS (OPTIONAL)")]:
    "Performance figures: [TO BE FILLED - NOT CONFIRMED FROM PROJECT]. Do not invent numbers.",

  [ph("SC-TEXT-002", "CONFIRM SEARCH REQUIREMENT")]:
    "CONFIRMED FROM PROJECT: Free-text search is IMPLEMENTED with category/brand/price filters. shop-listing.js posts searchName to /Shop/GetProducts → get_products. Treat FR-B2 as IMPLEMENTED.",

  [ph("SC-TEXT-003", "AVAILABILITY / HOSTING STATEMENT")]:
    "Demonstration hosting: [TO BE FILLED - NOT CONFIRMED FROM PROJECT]. Default development run is local ASP.NET Core host + local PostgreSQL. No measured uptime is claimed.",

  [ph("SC-TEXT-004", "FACT-FINDING DETAILS")]:
    "Fact-finding for this report used SmartCart source code, Database-Postgres scripts, appsettings and Ecommerce/context/. Live retail-store interviews: [TO BE FILLED - NOT CONFIRMED FROM PROJECT].",

  [ph("SC-TEXT-005", "CONFIRM DEVELOPMENT ORDER")]:
    "Inferred development order: database scripts/seed → admin catalogue/auth (EF) → storefront shop/cart/wishlist/auth (Dapper) → checkout/orders → admin orders/inventory/purchases → homepage → POS sales/returns → documentation.",

  [ph("SC-TEXT-006", "DEPLOYMENT ENVIRONMENT USED FOR DEMONSTRATION")]:
    "Demonstration environment: [TO BE FILLED - NOT CONFIRMED FROM PROJECT]. Default config uses local host and PostgreSQL on localhost (PostgresConnection / DatabaseSettings).",

  [ph("SC-TEXT-007", "CONFIRM METHODOLOGY")]:
    "Methodology: incremental module-wise development inside a phased academic plan (requirements → design → database → implementation → integration → testing → documentation).",

  [ph("SC-TEXT-008", "CONFIRM ORDER CONFIRMATION SCREEN")]:
    "CONFIRMED FROM PROJECT: Dedicated page GET /Checkout/Confirmation/{orderId} (CheckoutController.Confirmation; view title \"Order placed\") shows the placed order for the customer.",

  [ph("SC-TEXT-009", "CONFIRM AUTOMATED TESTS")]:
    "Automated test suite: [TO BE FILLED - NOT CONFIRMED FROM PROJECT]. Chapter 11 cases are for manual execution. No automated test project was verified for this fill.",

  [ph("SC-TEXT-010", "ADDITIONAL LIMITATIONS OBSERVED IN TESTING")]:
    "Code-verified limitations: dashboard KPIs are template placeholders; UPI rejected as available soon; Billing permissions without UI; no guest cart; ratings table without live API; no customer invoice PDF module.",

  [ph("SC-TEXT-011", "FUTURE ARCHITECTURE NOTE")]:
    "Future (SmartCart Then): multi-store/zone, online payments, billing/invoices, live analytics, ratings, guest checkout, mobile clients. Not present in the current single-store PostgreSQL implementation.",

  [ph("SC-TEXT-013", "ADDITIONAL INFORMATION")]:
    "No further institutional data found in the repository. Personal/guide details remain under SC-INFO / SC-CERT placeholders.",

  [ph("SC-TEXT-001", "PERSONAL ACKNOWLEDGEMENT (OPTIONAL)")]:
    "[TO BE FILLED - NOT CONFIRMED FROM PROJECT] — optional personal acknowledgement.",

  [ph("SC-TABLE-004", "VERIFY SMARTCART NOW vs THEN TABLE")]:
    "VERIFIED: Now = single-store ASP.NET Core 8 MVC + PostgreSQL; COD; Dapper storefront procedures; EF Core admin; JWT cookies; RBAC. Then (not implemented): multi-store, UPI/online pay, Billing UI/invoices, live dashboard KPIs, ratings UI, guest cart, mobile app. Deployment: [TO BE FILLED - NOT CONFIRMED FROM PROJECT].",

  [ph("SC-TABLE-001", "REVIEW REQUIREMENT TABLE")]:
    "Reviewed vs code: shop/search/cart/wishlist/COD/orders IMPLEMENTED; admin catalogue/inventory/purchases/orders/homepage/employees/roles/POS IMPLEMENTED; dashboard KPIs PARTIAL; UPI/Billing/ratings/multi-store NOT IMPLEMENTED. Search (FR-B2) is IMPLEMENTED.",

  [ph("SC-TABLE-002", "VALIDATE COMPARISON TABLE")]:
    "Validated: SmartCart adds online catalogue, SKU stock, COD orders and RBAC versus manual processes. Do not claim payment-gateway or multi-store benefits as current features.",

  [ph("SC-TABLE-006", "CONFIRM RISK REGISTER")]:
    "Verified risk themes: stock consistency (SP deduction/restock); auth (hash+JWT); permission misuse (RequirePermission); PostgreSQL availability; local upload storage. Probability scores: [TO BE FILLED - NOT CONFIRMED FROM PROJECT].",

  [ph("SC-TABLE-007", "CONFIRM MODULE STATUS")]:
    "Re-checked Controllers: Account/Shop/Cart/Wishlist/Checkout/Orders IMPLEMENTED; Admin Auth/Category/SubCategory/Brand/Product/ProductVariant/Variant/VariantValue/Supplier/Purchase/Inventory/Order/Homepage/Employee/UserRole/Sale/SalesReturn IMPLEMENTED; Dashboard PARTIAL; Billing NOT IMPLEMENTED; Ratings NOT IMPLEMENTED.",

  [ph("SC-TEST-001", "ACCOUNT TEST RESULTS")]:
    "Actual Result / Status for TC-01..TC-06: [TO BE FILLED AFTER TEST EXECUTION]. Attach evidence to SC-IMG placeholders after testing.",

  [ph("SC-TEST-002", "AUTHENTICATION / PRODUCT / CART TEST RESULTS")]:
    "Actual Result / Status: [TO BE FILLED AFTER TEST EXECUTION].",

  [ph("SC-TEST-003", "WISHLIST / ORDER / ADMIN TEST RESULTS")]:
    "Actual Result / Status: [TO BE FILLED AFTER TEST EXECUTION].",

  [ph("SC-TEST-004", "TEST SUMMARY AND DEFECTS")]:
    "Pass/fail counts and defects: [TO BE FILLED AFTER TEST EXECUTION]. Do not invent results.",

  [ph("SC-REF-001", "VERIFY REFERENCES")]:
    "Core references aligned to stack: ASP.NET Core, PostgreSQL, Dapper, EF Core, JWT Bearer, Bootstrap. Add IGNOU MCA guidelines/textbooks actually used: [TO BE FILLED - NOT CONFIRMED FROM PROJECT].",

  [ph("SC-CODE-001", "SAMPLE STORED PROCEDURE")]:
    "Sample place_order (PostgreSQL): CREATE OR REPLACE PROCEDURE place_order(... INOUT p_result refcursor) LANGUAGE plpgsql — validates user, address, phone, pincode; rejects UPI; accepts COD; checks stock/SellOnline; writes orders/orderitems/payments/shipping; deducts stock. File: Database-Postgres/02_Procedures/02_Users/05_Orders/place_order.sql.",

  [ph("SC-CODE-002", "SAMPLE ACTUAL STORED PROCEDURE")]:
    "Wired procedures (StoredProcedures.cs): get_products, get_shop_filters, get_product_details, get_bag_counts, get_cart, add_cart_item, update_cart_item, remove_cart_item, clear_cart, get_wishlist, get_wishlist_status, toggle_wishlist, remove_wishlist_item, register_user, get_user_by_email, get_user_by_id, get_checkout_preview, place_order, get_order, get_orders, cancel_order.",

  [ph("SC-CODE-003", "ACTUAL AUTHENTICATION CODE")]:
    "Customer: get_user_by_email + IPasswordHasher + UserJwtTokenService → cookie smartcart.user.token. Admin: AdminAuthRepository on adminusers + AdminJwtTokenService → smartcart.admin.token. See UserAuthRepository.cs and AdminAuthRepository.cs.",

  [ph("SC-CODE-004", "BUSINESS LOGIC CODE EXAMPLE")]:
    "CartRepository.AddCartItemAsync → StoredProcedures.Cart.AddCartItem (add_cart_item) via IDataAccessDapper; stock/SellOnline enforced inside the procedure.",

  [ph("SC-CODE-005", "FRONTEND CODE EXAMPLE")]:
    "wwwroot/js/shop-listing.js collects searchName and filters, POSTs to /Shop/GetProducts, and renders product cards on the listing page.",

  [ph("SC-CODE-006", "ADDITIONAL CODE SNIPPETS")]:
    "See Filters/RequirePermissionAttribute.cs, Middleware/AdminAuthMiddleware.cs, and DataAccess/DataAccessDapper.cs for PostgreSQL CALL handling.",
};

for (const [marker, text] of Object.entries(fillsByMarker)) {
  replaceWtContaining(marker, text);
}

// Instruction paragraphs adjacent to filled markers
const adjacent = {
  "The repository also contains a PostgreSQL build script (Database.sql run with psql). Confirm which engine is used for the final demonstration and update Sections 7.2, 10.4 and the software requirements accordingly.":
    "Final demonstration database engine: PostgreSQL (Database-Postgres/Database.sql via psql). Sections 7.2, 10.4 and software requirements are aligned to this implementation.",
  "Insert the exact server edition and version (e.g. from SELECT @@VERSION).":
    "Exact PostgreSQL version for demonstration: [TO BE FILLED - NOT CONFIRMED FROM PROJECT] (record SELECT version();).",
  "Project documentation states 38 tables excluding the 4 homepage tables. Confirm the final count against the live database (e.g. INFORMATION_SCHEMA.TABLES) and update this table.":
    "Script-based count is 42 tables. Live INFORMATION_SCHEMA count on the demo database: [TO BE FILLED - NOT CONFIRMED FROM PROJECT].",
  "Insert data types, nullability and defaults for each column (or reference Appendix B).":
    "Column types follow Database-Postgres CREATE TABLE scripts (IDENTITY INT, TEXT, BOOLEAN, NUMERIC, TIMESTAMP). See also SC-DATA-010.",
  "Insert the seeded modules and permission codes from the seed scripts if required.":
    "Permission codes are seeded from Database-Postgres seed scripts (Category.*, Product.*, Order.*, Inventory.*, Homepage.*, Employee.*, UserRole.*, Billing.* without Billing UI).",
  "Confirm whether the storefront provides free-text search in addition to category/brand/price filters, and update FR-B2 accordingly.":
    "FR-B2 status: IMPLEMENTED (free-text searchName plus category/brand/price filters).",
  "State whether a dedicated confirmation page exists or whether a message/redirect to the order details is used, and describe it here.":
    "Dedicated confirmation page: views/Checkout/Confirmation.cshtml at route Checkout/Confirmation/{orderId}.",
  "Confirm the Deployment row once the final hosting/demonstration environment is decided.":
    "Deployment/demo environment: [TO BE FILLED - NOT CONFIRMED FROM PROJECT].",
  "Code: [INSERT A SHORT EXTRACT (max 15\u201320 lines) FROM THE ACTUAL PROCEDURE]":
    "Code extract: Database-Postgres/02_Procedures/02_Users/05_Orders/place_order.sql (validation and COD/UPI branch). Attach full listing in Appendix if required.",
  "Code: [INSERT A SHORT EXTRACT, MAX 15\u201320 LINES]":
    'Illustrative constant: public const string PlaceOrder = "place_order"; (Models/StoredProcedures.cs).',
  "Code: [INSERT CODE HERE \u2014 max 15\u201320 lines]":
    "See Repository/UserAuthRepository.cs and Repository/Admin/AdminAuthRepository.cs (ASP.NET Core Identity PasswordHasher).",
  "Code: [INSERT CODE HERE \u2013 max 15\u201320 lines]":
    "See Repository/UserAuthRepository.cs and Repository/Admin/AdminAuthRepository.cs (ASP.NET Core Identity PasswordHasher).",
  "[SC-TEXT-012] Add personal learning outcomes here.":
    "[TO BE FILLED - NOT CONFIRMED FROM PROJECT].",
  "[SC-INFO-010] \u2014 insert IDE, database tool, browser and version-control tool actually used":
    "Tools: .NET SDK 8; IDE [TO BE FILLED - NOT CONFIRMED FROM PROJECT]; PostgreSQL client [TO BE FILLED - NOT CONFIRMED FROM PROJECT]; browser [TO BE FILLED - NOT CONFIRMED FROM PROJECT]; Git (repository present).",
};

for (const [from, to] of Object.entries(adjacent)) {
  if (xml.includes(from)) {
    xml = xml.split(from).join(esc(to));
    console.log("ADJ OK", from.slice(0, 50));
  } else {
    // try replacing inside w:t via marker-less contains
    console.warn("ADJ MISS", from.slice(0, 70));
  }
}

// Lone SC-TEXT-009 if it is its own short w:t
replaceWtContaining("[SC-TEXT-009]", "Automated tests: see confirmation note in this section.");

// Integrity checks — must keep screenshot/diagram/personal markers
const mustKeep = [
  "INSERT ACTUAL SMARTCART SCREENSHOT HERE",
  "INSERT SMARTCART DIAGRAM HERE",
  "[SC-IMG-001]",
  "[SC-IMG-036]",
  "[SC-DIAGRAM-001]",
  "[SC-DIAGRAM-020]",
  "[SC-INFO-001]",
  "[SC-CERT-001]",
  "[SC-CERT-002]",
  "[SC-CERT-003]",
];
for (const m of mustKeep) {
  if (!xml.includes(m)) {
    console.error("CRITICAL lost:", m);
    process.exit(1);
  }
}

const leftPlaceholders = (xml.match(/PLACEHOLDER \u2014/g) || []).length;
console.log("Remaining PLACEHOLDER em-dash lines:", leftPlaceholders);

fs.writeFileSync(DOC_XML, xml, "utf8");

try {
  fs.unlinkSync(OUT_TMP);
} catch (_) {}

const ps = `
Add-Type -AssemblyName System.IO.Compression.FileSystem
$src = '${UNZIPPED.replace(/\\/g, "\\\\")}'
$dest = '${OUT_TMP.replace(/\\/g, "\\\\")}'
if (Test-Path $dest) { Remove-Item $dest -Force }
[System.IO.Compression.ZipFile]::CreateFromDirectory($src, $dest, [System.IO.Compression.CompressionLevel]::Optimal, $false)
`;
execFileSync("powershell.exe", ["-NoProfile", "-Command", ps], { stdio: "inherit" });
fs.copyFileSync(OUT_TMP, OUT_DOCX);
console.log("Wrote", OUT_DOCX, "size", fs.statSync(OUT_DOCX).size);
console.log("Changed bytes:", xml.length - original.length);
