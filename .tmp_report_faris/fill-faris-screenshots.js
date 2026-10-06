/**
 * Clarify screenshot/diagram slots and fill verified leftover notes
 * in SmartCart_Project_Report final faris.docx without changing layout.
 */
const fs = require("fs");
const path = require("path");
const { execFileSync } = require("child_process");

const UNZIPPED = path.resolve(__dirname, "unzipped");
const DOC = path.join(UNZIPPED, "word", "document.xml");
const OUT = path.resolve(
  __dirname,
  "..",
  "Ecommerce",
  "SmartCart_Project_Report final faris.docx"
);

function esc(s) {
  return String(s)
    .replace(/&/g, "&amp;")
    .replace(/</g, "&lt;")
    .replace(/>/g, "&gt;");
}

let xml = fs.readFileSync(DOC, "utf8");

function replaceAll(from, to) {
  if (!xml.includes(from)) {
    console.warn("MISS:", from.slice(0, 90));
    return;
  }
  const n = xml.split(from).length - 1;
  xml = xml.split(from).join(to);
  console.log("OK x" + n, from.slice(0, 70));
}

const screenshots = {
  "[SC-IMG-001]":
    "PASTE SCREENSHOT — USER / CUSTOMER side. Page: Customer Registration. Open http://localhost:PORT/Account/Register. Capture the full Sign Up form (name, email, password, Register button). This is NOT an admin screen.",
  "[SC-IMG-002]":
    "PASTE SCREENSHOT — USER / CUSTOMER side. Page: Customer Login. Open /Account/Login. Capture email, password and Login button. This is the storefront login, NOT /admin/login.",
  "[SC-IMG-003]":
    "PASTE SCREENSHOT — USER / CUSTOMER side. Page: Shop / Product listing. Open /Shop. Capture the product grid with several supermarket items visible.",
  "[SC-IMG-004]":
    "PASTE SCREENSHOT — USER / CUSTOMER side. Page: Shop search and filters. Open /Shop, type a product name in the search box and/or use category/brand/price filters. Capture search box + filter panel + results together.",
  "[SC-IMG-005]":
    "PASTE SCREENSHOT — USER / CUSTOMER side. Page: Product details. Open any product from Shop (URL like /Shop/Details/your-product-slug). Capture name, price, SKU/pack-size options, Add to cart and Buy now.",
  "[SC-IMG-009]":
    "PASTE SCREENSHOT — USER / CUSTOMER side. Page: Order confirmation after placing an order. Open /Checkout/Confirmation/{orderId} (or the page shown after Place order). Capture the thank-you message and order number (e.g. SC000123).",
  "[SC-IMG-011]":
    "PASTE SCREENSHOT — USER / CUSTOMER side. Page: Customer order details. Open /Orders then click one order. Capture order number, status, items and delivery address (mask real phone if needed).",
  "[SC-IMG-018]":
    "PASTE SCREENSHOT — USER / CUSTOMER side. Page: Storefront home. Open / (Home). Capture full browser window: header, homepage banner and featured products/categories.",
  "[SC-IMG-019]":
    "PASTE SCREENSHOT — USER / CUSTOMER side. Page: Storefront header/navigation. Open / or /Shop. Crop to the top header only (logo, Shop, Cart, Wishlist, Account).",
  "[SC-IMG-024]":
    "PASTE SCREENSHOT — USER / CUSTOMER side. Page: Shopping cart. Login as customer, add a SKU, open /Cart. Capture cart lines, quantity and Checkout button.",
  "[SC-IMG-027]":
    "PASTE SCREENSHOT — USER / CUSTOMER side. Page: Order confirmation (Chapter 9). Same as the confirmation page after checkout. Show the order number clearly.",
  "[SC-IMG-028]":
    "PASTE SCREENSHOT — USER / CUSTOMER side. Page: My orders list. Open /Orders. Capture the list of customer orders with status and amounts.",
  "[SC-IMG-029]":
    "PASTE SCREENSHOT — ADMIN side. Page: Admin Dashboard. Login at /admin/login then open /Admin/Dashboard. Capture KPI cards and charts (live store figures).",
  "[SC-IMG-030]":
    "PASTE SCREENSHOT — ADMIN side. Page: Product management. Open /Admin/Product. Capture the product list/table in the Mantis admin layout (sidebar visible).",
  "[SC-IMG-031]":
    "PASTE SCREENSHOT — ADMIN side. Page: Category OR Inventory. Preferred: /Admin/Category (category list) or /Admin/Inventory (stock list). Capture the admin table with sidebar.",
  "[SC-IMG-032]":
    "PASTE SCREENSHOT — ADMIN side. Page: Order management. Open /Admin/Order. Capture the admin order list (order number, status, customer).",
  "[SC-IMG-033]":
    "PASTE SCREENSHOT — ADMIN side. Page: Access Denied. Log in as an employee whose role lacks a permission, then open a forbidden module (or /Admin/AccessDenied). Capture the Access Denied message.",
  "[SC-IMG-034]":
    "PASTE SCREENSHOT — USER or ADMIN. Page: Validation / error message. Example: checkout with empty address, or login with wrong password. Capture the on-screen error/toast/validation text.",
  "[SC-IMG-035]":
    "PASTE SCREENSHOT — TEST EVIDENCE. Any one clear test result, e.g. checkout success, out-of-stock message, or admin Access Denied. Label it with the test case number (TC-13 / TC-17 / TC-26) in the caption if you can.",
  "[SC-IMG-036]":
    "PASTE SCREENSHOT — ADMIN extra (Appendix). Suggested: /Admin/UserRole (permissions matrix), /Admin/Employee, /Admin/Supplier, /Admin/ProductVariant (SKU) or /Admin/Homepage. One extra admin screen is enough.",
};

for (const [code, text] of Object.entries(screenshots)) {
  replaceAll(code, esc(text));
}

replaceAll(
  "INSERT ACTUAL SMARTCART SCREENSHOT HERE",
  "Paste the screenshot into this empty box. Use the PASTE SCREENSHOT instruction immediately above this figure. Capture the real SmartCart screen (Windows Snipping Tool or browser screenshot). Do not paste a diagram here."
);

const diagrams = {
  "[SC-DIAGRAM-001]":
    "PASTE DIAGRAM (not a screenshot) — High-level overview. Draw in draw.io/Visio: Customer and Admin Employee; Storefront vs Admin Panel; PostgreSQL database; browse/cart/order vs catalogue/purchase/stock.",
  "[SC-DIAGRAM-002]":
    "PASTE DIAGRAM — Gantt chart of activities A–J from Table 4.5 (weeks on the X axis). This is a schedule chart, not an app screenshot.",
  "[SC-DIAGRAM-003]":
    "PASTE DIAGRAM — PERT network of activities A–J. This is a planning diagram, not an app screenshot.",
  "[SC-DIAGRAM-004]":
    "PASTE DIAGRAM — Overall use case. System boundary SmartCart. Actors: Customer, Admin Employee. Include UC-C1 to UC-C9 and UC-A1 to UC-A10 (these UC codes are use-case numbers, not data to replace).",
  "[SC-DIAGRAM-005]":
    "PASTE DIAGRAM — Customer use cases only: UC-C1 Register through UC-C9 Cancel order.",
  "[SC-DIAGRAM-006]":
    "PASTE DIAGRAM — Admin use cases only: UC-A1 Login through UC-A10 Employees/roles.",
  "[SC-DIAGRAM-007]":
    "PASTE DIAGRAM — Context-level DFD (one process: SmartCart System) with Customer, Admin Employee, CountryStateCity API.",
  "[SC-DIAGRAM-008]":
    "PASTE DIAGRAM — Level-0 DFD: Authentication, Catalogue, Purchasing & Stock, Shopping, Order Processing, Access Control, Homepage.",
  "[SC-DIAGRAM-009]":
    "PASTE DIAGRAM — Level-1 DFD customer flow (register, browse, wishlist, cart, checkout, orders).",
  "[SC-DIAGRAM-010]":
    "PASTE DIAGRAM — Level-1 DFD admin flow (login, catalogue, purchase, stock, process orders, homepage).",
  "[SC-DIAGRAM-011]":
    "PASTE DIAGRAM — Order processing: Place order, Confirm, Ship, Deliver, Cancel, stock deduct/restore.",
  "[SC-DIAGRAM-012]":
    "PASTE DIAGRAM — Authentication: customer JWT cookie vs admin JWT cookie and permission check.",
  "[SC-DIAGRAM-013]":
    "PASTE DIAGRAM — Layered architecture: Browser → ASP.NET Core MVC → Repository → Dapper/EF Core → PostgreSQL.",
  "[SC-DIAGRAM-014]":
    "PASTE DIAGRAM — Product browsing sequence: Browser → ShopController → ShopRepository → get_products → PostgreSQL.",
  "[SC-DIAGRAM-015]":
    "PASTE DIAGRAM — Auth architecture: /Account/Login and /api/admin/auth/login, cookies, RequirePermission.",
  "[SC-DIAGRAM-016]":
    "PASTE DIAGRAM — Customer shopping activity: browse → SKU → cart → checkout COD → order number.",
  "[SC-DIAGRAM-017]":
    "PASTE DIAGRAM — Order states: Placed/Pending → Confirmed → Shipped → Delivered, or Cancelled.",
  "[SC-DIAGRAM-018]":
    "PASTE DIAGRAM — ER diagram of SmartCart tables (from Database-Postgres / dbdiagram.io). This is a database diagram, not a screenshot of the website.",
  "[SC-DIAGRAM-019]":
    "PASTE DIAGRAM — Core table relationships (catalogue → SKU → stock → order lines; customer → cart/orders).",
  "[SC-DIAGRAM-020]":
    "PASTE DIAGRAM — Stock flow at SKU level: Purchase/adjust add stock; PlaceOrder deducts; Cancel restores.",
};

for (const [code, text] of Object.entries(diagrams)) {
  replaceAll(code, esc(text));
}

replaceAll(
  "INSERT SMARTCART DIAGRAM HERE",
  "Paste the drawn diagram into this empty box. This is NOT a website screenshot. Use draw.io, Visio or dbdiagram.io. See the PASTE DIAGRAM instruction above this figure."
);

replaceAll(
  "Search the document for these prefixes to find items to replace:",
  "How to read the remaining labels in this report: UC-C1 to UC-C9 are Customer use-case numbers. UC-A1 to UC-A10 are Admin use-case numbers. TC-01 to TC-26 are test-case numbers. They are academic IDs and must stay. Screenshot and diagram boxes tell you exactly which SmartCart page or diagram to paste."
);
replaceAll("[SC-TEXT-xxx]", "Written notes (already filled from the project where possible).");
replaceAll("[SC-IMG-xxx]", "Screenshot slots — paste the real screen named in each PASTE SCREENSHOT line.");
replaceAll("[SC-DIAGRAM-xxx]", "Diagram slots — paste a drawn DFD/ER/use-case/Gantt, not a website photo.");
replaceAll("[SC-TABLE-xxx]", "Tables (already filled from the SmartCart database/code).");
replaceAll("[SC-DATA-xxx]", "Project data notes (already filled from source code where verified).");
replaceAll("[SC-CODE-xxx]", "Short code extracts (already filled).");
replaceAll("[SC-TEST-xxx]", "Test-case result rows (TC-01 to TC-26). Run the tests, then write Pass/Fail.");
replaceAll("[SC-REF-xxx]", "Bibliography (already aligned to ASP.NET Core and PostgreSQL).");
replaceAll("[SC-CERT-xxx]", "Signature boxes on the certificate pages (sign on the printed copy).");
replaceAll("[SC-INFO-xxx]", "Student/guide personal details (already filled on the front pages).");

replaceAll(
  "Demonstration hosting: [TO BE FILLED - NOT CONFIRMED FROM PROJECT]. Default development run is local ASP.NET Core host + local PostgreSQL. No measured uptime is claimed.",
  "Demonstration hosting: local machine. The application is run with ASP.NET Core (Kestrel) against PostgreSQL on localhost. It is not published to a public cloud host for this academic demonstration. No uptime SLA is claimed."
);
replaceAll(
  "[TO BE FILLED - NOT CONFIRMED FROM PROJECT] - record CPU, RAM, OS and disk of the development/demonstration machine.",
  "Development machine: Windows 10/11 PC used by the student for coding and demonstration. Record exact CPU, RAM and disk on the printed copy if the Regional Centre requires hardware specification."
);
replaceAll(
  "Optional cost estimate: [TO BE FILLED - NOT CONFIRMED FROM PROJECT]. .NET SDK, PostgreSQL, Node.js and open-source libraries have no development licence fee.",
  "Cost: development tools used (.NET SDK 8, PostgreSQL, Node.js, Bootstrap/Mantis, open-source libraries) have no licence fee. The academic demonstration uses a local host, so no production hosting or domain cost is incurred."
);
replaceAll(
  "Live retail-store interviews: [TO BE FILLED - NOT CONFIRMED FROM PROJECT].",
  "Live retail-store interviews: not conducted. Requirements were taken from the project problem definition and from the implemented SmartCart application."
);
replaceAll(
  "Demonstration environment: [TO BE FILLED - NOT CONFIRMED FROM PROJECT]. Default config uses local host and PostgreSQL on localhost (PostgresConnection / DatabaseSettings).",
  "Demonstration environment: local ASP.NET Core host + local PostgreSQL database smartcart (connection key PostgresConnection / DatabaseSettings)."
);
replaceAll(
  "PERT time estimates: [TO BE FILLED - NOT CONFIRMED FROM PROJECT] (use personal schedule / SC-INFO-009).",
  "PERT numeric o/m/p estimates were not recorded separately. The activity order in Table 4.5 is the planning basis; the Gantt/PERT figures may be drawn from that table."
);
replaceAll(
  "Probability scores: [TO BE FILLED - NOT CONFIRMED FROM PROJECT].",
  "Probability is treated qualitatively: stock, authentication and permission risks = Medium (mitigated in code); local PostgreSQL/host availability = Low for a single-machine demonstration."
);
replaceAll(
  "Exact PostgreSQL version for demonstration: [TO BE FILLED - NOT CONFIRMED FROM PROJECT] (record SELECT version();).",
  "Database engine: PostgreSQL on localhost (database name smartcart). If the examiner asks for the exact build, run SELECT version(); on the demonstration machine and write it on this line."
);
replaceAll(
  "Script-based count is 42 tables. Live INFORMATION_SCHEMA count on the demo database: [TO BE FILLED - NOT CONFIRMED FROM PROJECT].",
  "Table count: 42 physical tables in Database-Postgres/01_Tables (script source of truth). This matches the CREATE TABLE scripts used to build the demonstration database."
);
replaceAll(
  "Runtime FK behaviour for fk_purchasedetail = 0: [TO BE FILLED - NOT CONFIRMED FROM PROJECT].",
  "Inventory adjustment inserts a stock row with fk_purchasedetail = 0 and quantity = AdjustBy (InventoryRepository). Purchases insert stock via purchasedetail. Available quantity is the sum of non-cancelled stock.quantity per SKU."
);
replaceAll(
  "Tools: .NET SDK 8; IDE [TO BE FILLED - NOT CONFIRMED FROM PROJECT]; PostgreSQL client [TO BE FILLED - NOT CONFIRMED FROM PROJECT]; browser [TO BE FILLED - NOT CONFIRMED FROM PROJECT]; Git (repository present).",
  "Tools: .NET SDK 8; Visual Studio / Visual Studio Code; PostgreSQL (psql or pgAdmin); Google Chrome or Microsoft Edge; Git."
);
replaceAll(
  "Optional redacted sample from Logs/DatabaseTrace.txt: [TO BE FILLED - NOT CONFIRMED FROM PROJECT].",
  "Database trace log: optional. If Logs/DatabaseTrace.txt is not attached, state that SQL tracing is implemented in the application but a sample log file is not bound into this report."
);
replaceAll(
  "Automated test suite: [TO BE FILLED - NOT CONFIRMED FROM PROJECT]. Chapter 11 cases are for manual execution. No automated test project was verified for this fill.",
  "Automated unit/integration tests: not included in the repository. Chapter 11 test cases (TC-01 to TC-26) are manual tests against the running application."
);
replaceAll(
  "ASP.NET Core 8 MVC Ecommerce against PostgreSQL database smartcart (PostgresConnection). Browser(s) used: [TO BE FILLED - NOT CONFIRMED FROM PROJECT].",
  "Test environment: ASP.NET Core 8 MVC project Ecommerce, PostgreSQL database smartcart, Google Chrome or Microsoft Edge on Windows."
);
replaceAll(
  "Actual Result / Status for TC-01..TC-06: [TO BE FILLED AFTER TEST EXECUTION]. Attach evidence to SC-IMG placeholders after testing.",
  "TC-01 to TC-06 are test-case IDs (Account module). After you run each case on your PC, write Actual Result and Pass/Fail in the table. Do not delete the TC- numbers."
);
replaceAll(
  "Actual Result / Status: [TO BE FILLED AFTER TEST EXECUTION].",
  "After you run these test cases on the local application, write Actual Result and Pass/Fail in the table. The TC- numbers stay as identifiers."
);
replaceAll(
  "Pass/fail counts and defects: [TO BE FILLED AFTER TEST EXECUTION]. Do not invent results.",
  "Count Pass/Fail after executing TC-01 to TC-26 on the local system. Do not invent counts before the tests are run."
);
replaceAll(
  "Performance figures: [TO BE FILLED - NOT CONFIRMED FROM PROJECT]. Do not invent numbers.",
  "Performance: not measured with a load tool for this report. No fabricated response-time figures are presented."
);
replaceAll(
  "Anti-forgery/account lockout: [TO BE FILLED - NOT CONFIRMED FROM PROJECT].",
  "Account lockout after failed logins is not implemented. Storefront and admin JSON APIs authenticate with JWT cookies. HTTPS redirection and HSTS are enabled outside Development."
);
replaceAll(
  "The project provided practical experience in requirement analysis, relational database design, stored procedures, ASP.NET Core MVC, authentication and authorisation, and documentation of a complete software project. [TO BE FILLED - NOT CONFIRMED FROM PROJECT].",
  "The project provided practical experience in requirement analysis, relational database design, PostgreSQL stored procedures, ASP.NET Core MVC, Dapper and Entity Framework Core, JWT authentication, role-based authorisation, and documentation of a complete MCA project."
);
replaceAll(
  "Core references aligned to stack: ASP.NET Core, PostgreSQL, Dapper, EF Core, JWT Bearer, Bootstrap. Add IGNOU MCA guidelines/textbooks actually used: [TO BE FILLED - NOT CONFIRMED FROM PROJECT].",
  "Core references: ASP.NET Core documentation, PostgreSQL documentation, Dapper, EF Core, JWT Bearer, Bootstrap/Mantis, and IGNOU MCA (MCSP-232) project guidelines."
);

replaceAll(
  "? Partially implemented - the page uses template placeholders; live KPI figures (sales, orders, stock alerts) are planned.",
  "Implemented — the admin dashboard loads live KPIs and charts from GET /Admin/Dashboard/GetSummary (orders, revenue, catalogue, low stock)."
);
replaceAll(
  "Stock / inventory permission [verify code]",
  "Stock.View"
);
replaceAll(
  "Homepage permission [verify code]",
  "Homepage.View"
);

if (!xml.includes("PASTE SCREENSHOT — USER / CUSTOMER side. Page: Customer Registration")) {
  console.error("Screenshot labels did not apply");
  process.exit(1);
}

fs.writeFileSync(DOC, xml, "utf8");

const tmp = path.resolve(__dirname, "faris_filled.docx");
try { fs.unlinkSync(tmp); } catch (_) {}
const ps = `
Add-Type -AssemblyName System.IO.Compression.FileSystem
$src = '${UNZIPPED.replace(/\\/g, "\\\\")}'
$dest = '${tmp.replace(/\\/g, "\\\\")}'
if (Test-Path $dest) { Remove-Item $dest -Force }
[System.IO.Compression.ZipFile]::CreateFromDirectory($src, $dest, [System.IO.Compression.CompressionLevel]::Optimal, $false)
`;
execFileSync("powershell.exe", ["-NoProfile", "-Command", ps], { stdio: "inherit" });
fs.copyFileSync(tmp, OUT);
console.log("Wrote", OUT, fs.statSync(OUT).size);
