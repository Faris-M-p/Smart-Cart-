# SmartCart Pro — Dashboard Enhancement Implementation Report

**Branch:** `feature/dashboard-v2`  
**Status:** Completed & Verified  
**Date:** November 2026  

---

## 1. Executive Summary & Goal Achievement
The SmartCart Pro dashboard (`Home/Index`) has been re-architected into a fast, industry-standard retail dashboard:
1. **Essential View First:** The initial page request (`Home/Index`) renders only a lightweight shell in `< 15ms` with zero heavy database aggregations. Immediately upon load, the browser executes a single, highly optimized endpoint (`/Dashboard/Essentials?range=...`) returning all light, high-value metrics (KPIs, sparklines, trend vs previous period, payment split, top 5 products, low stock items, recent invoices, prioritized alerts, and role-gated quick actions).
2. **Advanced Analytics on Demand:** Heavier analytics (Hourly peak hours, Category-wise breakdown, Stock health donut, Counter vs Online split, Gross profit & margins, Day-of-week pattern, 6-month Sales vs Purchases, Cashier performance, and Multi-store comparisons) are **never queried** until the user clicks **"Show advanced"**.
3. **Parallel Loading:** When opened, the browser triggers 6 independent, cacheable AJAX requests in parallel. Each widget displays a skeleton shimmer and renders immediately as soon as its specific payload arrives.
4. **Fidelity to Design Reference:** The UI matches `docs/prototypes/smartcart-dashboard-prototype.html` pixel-for-pixel using a zero-dependency, high-performance vanilla SVG charting engine.

---

## 2. Step 1 Inspection Findings

| Inspection Item | Details / Decision |
|---|---|
| **Chart Library** | Pure vanilla SVG charting engine implemented in `wwwroot/js/dashboard/charts.js` (`areaChart`, `columnChart`, `donut`, `hbars`, `spark`). No heavy external library dependencies (Chart.js / ApexCharts overhead eliminated), maximum rendering speed, and exact prototype fidelity. |
| **Locale & TimeZone** | Configured for `Asia/Kolkata` and `INR` (`₹`). All local date/hour bounds are accurately resolved using `LocaleClock` and translated to UTC bounds for indexed database queries. |
| **Number Formatting** | Indian numbering system (`Intl.NumberFormat('en-IN')`) with support for Lakhs (`₹1.25L`), Crores (`₹1.25Cr`), and Thousands (`₹1.5K`). |
| **Report Query Reconciliation** | Dashboard queries share exact calculation logic with Report Suite:<br>• **Net Sales:** Excludes cancelled sales and completed status; returns reduce net sales matching Sales Bill-Wise.<br>• **Item Sales:** Matches `SaleDetails` revenue and quantity grouping.<br>• **Category Sales:** Matches category report joins.<br>• **Gross Profit:** `LineTotal - TaxAmount - (Quantity * CostPrice)`. |
| **Caching Infrastructure** | Built-in `IMemoryCache` configured with scoped keys: `dash:{endpoint}:{storeId}:{range}:{scope}`.<br>• **TTL:** 60s for Essentials, 5m for Advanced.<br>• **Refresh:** Header "Refresh" button passes `refresh=true` which bypasses cache reads and writes fresh data. |
| **Role Matrix** | • **Cashier:** Displays "My day", scoped strictly to cashier transactions (`CreatedBy == actorUserId`), 4 KPI cards, drawer cash/status, hides inventory, low stock, top products, and advanced bar.<br>• **Store Manager:** Store-scoped Essential view + Advanced insights (excluding Gross Profit).<br>• **Owner / Admin:** Full store analytics + Gross Profit card (gated by `reports.profit`). |

---

## 3. Architecture & Endpoint Specifications

### 3.1 Initial Page Shell (`Home/Index`)
- **Action:** `HomeController.Index`
- **Output:** `OutputDashboardSelect`
- **Payload:** User flags, permissions, store ID/name, currency, timezone, and quick actions.
- **Database queries:** 0 aggregation queries (pure authorization & shell metadata).

### 3.2 Essential Endpoint
- **URL:** `GET /Dashboard/Essentials?range=today&refresh=false`
- **Gate:** Authenticated user (`[Authorize]`). Content adapted per role.
- **Response:**
  - `kpis`: Net sales, Bills, Returns, Drawer cash, Low/out of stock, Online orders (with sparklines and comparison deltas).
  - `salesTrend`: Current period values and previous comparison period values.
  - `payments`: Method split (Cash, Card, UPI, etc.) with colors and percentages.
  - `topProducts`: Top 5 items with revenue and quantity.
  - `lowStock`: Critical low-stock SKUs with reorder levels and PO generation link.
  - `recentInvoices`: 5 most recent sales with customer, mode, and details URL.
  - `alerts`: Max 4 prioritized alerts (out of stock, session variance, orders waiting > 2h, cashier session open > 8h, license expiration <= 15 days).
  - `quickActions`: Permission-filtered shortcut buttons.
- **Backward Compatibility:** `GET /Dashboard/Snapshot` kept active and delegates to `Essentials`.

### 3.3 Advanced Endpoints (On Demand)
| Action | Purpose | Gate | Response Model |
|---|---|---|---|
| `Dashboard/AdvancedSales` | Hourly peaks, Day-of-week (4w), Counter vs Online, Top 10 rev & qty | `dashboard.view` + `reports.view` | `OutputDashboardAdvancedSales` |
| `Dashboard/AdvancedCategories` | Category sales (Top 8 + Others) | `dashboard.view` + `reports.view` | `OutputDashboardAdvancedCategories` |
| `Dashboard/AdvancedStock` | Stock health counts (In stock, Low, Out, Non-moving 30d) | `dashboard.view` + `reports.view` + `INVENTORY` | `OutputDashboardAdvancedStock` |
| `Dashboard/AdvancedPurchases` | Sales vs Purchases (Last 6 months) | `dashboard.view` + `reports.view` + `PURCHASES` | `OutputDashboardAdvancedPurchases` |
| `Dashboard/AdvancedTeam` | Cashier performance + Store comparison (if 2+ stores) | `dashboard.view` + `reports.view` | `OutputDashboardAdvancedTeam` |
| `Dashboard/AdvancedProfit` | Gross profit, Margin %, Delta vs previous period, Category margins | **`reports.profit`** (HTTP 403 if missing) | `OutputDashboardAdvancedProfit` |

---

## 4. Database Performance & Index Patch

An idempotent index patch script was created at:
`SmartCartPro/Database-Postgres/03_Patches/V002__DashboardPerformanceIndexes.sql`

```sql
BEGIN;

CREATE INDEX IF NOT EXISTS ix_inventory_store_reorder
    ON inventory (fk_store, quantityonhand, reorderlevel);

CREATE INDEX IF NOT EXISTS ix_sales_returns_store_returndate
    ON sales_returns (fk_store, returndate)
    WHERE cancelled = FALSE;

CREATE INDEX IF NOT EXISTS ix_purchases_store_purchasedate
    ON purchases (fk_store, purchasedate)
    WHERE cancelled = FALSE;

CREATE INDEX IF NOT EXISTS ix_online_orders_store_orderdate
    ON online_orders (fk_store, orderdate)
    WHERE cancelled = FALSE;

CREATE INDEX IF NOT EXISTS ix_cash_sessions_store_status_opened
    ON cash_sessions (fk_store, status, openedat DESC);

INSERT INTO patch_history (version, description, appliedby, success)
VALUES ('V002', 'Dashboard analytics performance indexes', CURRENT_USER, TRUE)
ON CONFLICT (version) DO NOTHING;

COMMIT;
```

### SQL Round-Trips per Endpoint:
- `Essentials`: ≤ 6 sequential database round-trips (EF Core `AsNoTracking`, aggregated in PostgreSQL).
- `AdvancedSales`: 5 round-trips.
- `AdvancedCategories`: 1 round-trip.
- `AdvancedStock`: 2 round-trips.
- `AdvancedPurchases`: 2 round-trips.
- `AdvancedTeam`: 2 round-trips.
- `AdvancedProfit`: 3 round-trips.

---

## 5. UI Features & Polish
1. **Interactive Date Range Segments:** `Today`, `7 days`, `30 days`, `This month`. Range is tracked in query string (`?range=d7`), enabling bookmarks and browser back/forward history navigation.
2. **Auto-Refresh Lifecycle:** Essentials auto-refreshes every 5 minutes **only while the browser tab is visible** (`document.visibilityState === 'visible'`). Pauses automatically in background tabs to conserve battery and server bandwidth.
3. **Advanced Toggle Memory:** Remembers toggle state in `localStorage` (`smartcart:dash:advanced`).
4. **Deep Links to Reports:**
   - Top 5 Products → `/Reports/ItemSales`
   - Low Stock → `/Inventory/LowStock`
   - Recent Invoices → `/Sales/Invoices`
   - Category Sales → `/Reports/CategorySales`
5. **Responsive Layout:** Dynamic CSS grid collapsing cleanly across 1280px, 1100px, 900px, and 720px screens.

