# SmartCartPro — Purchase Database Architecture Comparison

**Status:** Read-Only Review & Architecture Analysis  
**Branch:** `Pro_6_purchase`  
**Generated Documentation:**
1. [`before.dbml`](file:///d:/Faris/Work%20Area/Smart-Cart-/docs/purchase-db-review/before.dbml) — Original PostgreSQL schema
2. [`after.dbml`](file:///d:/Faris/Work%20Area/Smart-Cart-/docs/purchase-db-review/after.dbml) — Current Codex implementation
3. [`proposed.dbml`](file:///d:/Faris/Work%20Area/Smart-Cart-/docs/purchase-db-review/proposed.dbml) — Proposed clean SmartCartPro architecture
4. [`comparison.md`](file:///d:/Faris/Work%20Area/Smart-Cart-/docs/purchase-db-review/comparison.md) — Comprehensive technical comparison report

---

## 1. Executive Summary

A critical architectural divergence was introduced during the recent Codex implementation:
- **Original Intent:** The baseline SmartCartPro schema already had dedicated `purchases` and `purchase_details` tables designed for purchase transactions, alongside `suppliers`, `inventory`, and `stock_movements`.
- **Codex Implementation:** Rather than evolving `purchases` and `purchase_details`, Codex treated them as "legacy purchase orders" and created an entire **parallel duplicate billing system** (`purchase_bills`, `purchase_bill_lines`), a separate supplier return subsystem (`supplier_returns`, `supplier_return_lines`), and a fragmented supplier subledger (`supplier_money_transactions`, `supplier_ledger_entries`, `supplier_allocations`) with heavy table-level denormalization and complex PL/pgSQL triggers.
- **Proposed Architecture:** In accordance with SmartCartPro core requirements, we reuse `purchases` and `purchase_details` as the primary transaction tables, implement purchase returns following the existing `sales_returns` pattern (`purchase_returns`, `purchase_return_details`), consolidate supplier money and ledger tracking into a single clean `supplier_transactions` table, and reuse `suppliers` without parallel account master tables.

---

## 2. Git Status & Change Categorization

| File | Status | Description |
|---|---|---|
| `SmartCartPro/Database-Postgres/01_Tables/046_PurchaseBills.sql` | **Staged** (New) | Introduces 7 new tables (`purchase_bills`, `purchase_bill_lines`, `supplier_returns`, `supplier_return_lines`, `supplier_money_transactions`, `supplier_ledger_entries`, `supplier_allocations`), alters `stock_movements`, and adds 5 PL/pgSQL trigger functions. |
| `SmartCartPro/Database-Postgres/01_Tables/create.sql` | **Staged** (Modified) | Appends `\i 046_PurchaseBills.sql` to the DDL initialization script. |
| `SmartCartPro/Database-Postgres/02_Seed/008_PurchaseAccess.sql` | **Staged** (New) | Seeds 8 granular permissions and 3 module features (`PURCHASES.BILLS`, `PURCHASES.RETURNS`, `PURCHASES.ACCOUNTS`). |
| `SmartCartPro/Database-Postgres/02_Seed/create.sql` | **Staged** (Modified) | Appends `\i 008_PurchaseAccess.sql` to the seed script. |
| `SmartCartPro/Database-Postgres/03_Patches/V004__PurchaseBillsAndSupplierAccounts.sql` | **Staged** (New) | Migration patch duplicating `046_PurchaseBills.sql` plus patch history entry. |
| `SmartCartPro/Entities/PurchaseBill.cs` | **Staged** (New) | EF Core entities for `purchase_bills`, `purchase_bill_lines`, `supplier_returns`, `supplier_return_lines`. |
| `SmartCartPro/Entities/SupplierAccount.cs` | **Staged** (New) | EF Core entities for `supplier_money_transactions`, `supplier_ledger_entries`, `supplier_allocations`. |
| `SmartCartPro/Entities/StockMovement.cs` | **Staged** (Modified) | Added `source_line_id` and `unit_cost` properties. |
| `SmartCartPro/Data/SmartCartDbContext.cs` | **Staged** (Modified) | Registered 7 new `DbSet` collections and model configurations. |
| `SmartCartPro/Database-Postgres/01_Tables/023_Purchases.sql` | **Committed** (Unchanged) | Original purchase table. |
| `SmartCartPro/Database-Postgres/01_Tables/024_PurchaseDetails.sql` | **Committed** (Unchanged) | Original purchase details table. |
| `SmartCartPro/Database-Postgres/01_Tables/018_Suppliers.sql` | **Committed** (Unchanged) | Original supplier master table. |
| `SmartCartPro/Database-Postgres/01_Tables/021_Inventory.sql` | **Committed** (Unchanged) | Original inventory snapshot table. |
| `SmartCartPro/Database-Postgres/01_Tables/022_StockMovements.sql` | **Committed** (Unchanged) | Original stock movement ledger table. |

---

## 3. Detailed Architectural Comparison

### 3.1 Original Tables & Responsibilities (BEFORE)
1. **`purchases`**: Primary purchase document header. Store-scoped (`fk_store`), linked to `suppliers` (`fk_supplier`). Included financial totals (`subtotal`, `taxamount`, `discountamount`, `totalamount`), status lifecycle (`draft`, `posted`, `partially_received`, `received`, `cancelled`), and cancellation audit trail.
2. **`purchase_details`**: Line items for purchases. Stored product and optional SKU variant (`fk_product`, `fk_productvariant`), quantity, unit cost, tax amount, and line total.
3. **`suppliers`**: Company-wide vendor master with contact info, tax registration, address, and status.
4. **`inventory`**: Store-level current on-hand and reserved physical inventory balances.
5. **`stock_movements`**: Immutable inventory movement ledger tracking transactions via `movementtype` (`purchase`, `sale`, `adjustment`, `transfer_in`, `transfer_out`, `return_in`, `return_out`, `opening`) and polymorphic references (`referencetype`, `referenceid`).

### 3.2 New Tables Introduced by Codex (AFTER)
Codex introduced **7 new tables** that bypass the original schema:
1. `purchase_bills`: A duplicate purchase header table containing supplier invoice info, status (`Pending`, `Approved`, `Cancelled`), financial snapshot amounts, and heavy denormalization (`supplier_name`, `supplier_tax_number`, `store_name`).
2. `purchase_bill_lines`: Duplicate line item table with denormalized text columns (`product_code`, `description`, `unit_name`, `tax_name`).
3. `supplier_returns`: Return header table strictly referencing `purchase_bills(id)`.
4. `supplier_return_lines`: Return line items referencing `purchase_bill_lines(id)`.
5. `supplier_money_transactions`: Tracks payments and refunds with self-referencing reversal links (`fk_reversal_of`).
6. `supplier_ledger_entries`: Append-only subledger recording signed financial effects.
7. `supplier_allocations`: Cross-reference table tracking which payments/credits settle which bills.

### 3.3 Duplicate and Overlapping Functionality
- **Dual Purchase Systems:** Two coexisting purchase systems (`purchases` vs `purchase_bills`) that both record vendor buying transactions. Staff must navigate separate menus ("Purchase Orders" vs "Purchases").
- **Redundant Snapshot Columns:** `purchase_bills` denormalizes supplier name, store name, and tax numbers on every row; `purchase_bill_lines` denormalizes product codes, descriptions, and unit names, duplicating master table data.
- **Incompatible Return Mechanism:** `supplier_returns` cannot return goods bought under `purchases`; it is hard-locked to `purchase_bills`.

### 3.4 New Columns Added to Existing Tables
- **`stock_movements.source_line_id`**: Added by Codex to point to `purchase_bill_lines(id)` or `supplier_return_lines(id)`.
- **`stock_movements.unit_cost`**: Added by Codex to capture invoice-time cost for inventory valuation.

### 3.5 Database Triggers and Functions Introduced
Codex introduced 5 PL/pgSQL functions and 8 triggers:
1. `supplier_validate_stock_source()`: Trigger on `stock_movements` enforcing that movements with `referencetype IN ('PurchaseBill', 'SupplierReturn')` exactly match persisted bill/return lines in SKU, quantity, and unit cost.
2. `supplier_reject_financial_mutation()`: Triggers `tr_supplier_money_immutable` and `tr_supplier_ledger_immutable` throwing exceptions on any `UPDATE` or `DELETE`.
3. `supplier_guard_allocation_history()`: Trigger preventing deletion or unapproved edits of allocations.
4. `supplier_guard_posted_return()`: Trigger preventing updates to posted supplier returns.
5. `supplier_guard_recorded_bill()`: Trigger preventing changes to approved purchase bills except through returns.

### 3.6 Stock Posting Changes
- **Before:** Stock movements were posted with `referencetype = 'purchase'` and `referenceid = id_purchase` when goods were received.
- **After (Codex):** Pending bills do not touch stock. On "Approval", an EF transaction creates `stock_movements` with `referencetype = 'PurchaseBill'`, `referenceid = bill.id`, and `source_line_id = line.id`.

### 3.7 Supplier Balance and Ledger Conventions
- **Codex Convention:** Positive (`+`) was defined as **payable to supplier**; Negative (`-`) was defined as **receivable/advance**.
- **Confirmed SmartCartPro Requirement:**
  > **Positive (`+`) = receivable / advance held by supplier.**  
  > **Negative (`-`) = payable to supplier.**
- **Calculation Rule:** Balances must always be calculated dynamically from posted transactions, never stored in an unreliable master balance column.
  - Approved Purchase: Increases amount owed → **Negative effect** (`-TotalAmount`)
  - Purchase Return: Decreases amount owed → **Positive effect** (`+ReturnAmount`)
  - Payment to Supplier: Decreases amount owed / creates advance → **Positive effect** (`+PaymentAmount`)
  - Refund Received: Decreases receivable/advance → **Negative effect** (`-RefundAmount`)
  - **Net Supplier Balance = $\sum(\text{Signed Effects})$**  
    If negative: company owes supplier. If positive: supplier owes company or holds advance.

---

## 4. Object-by-Object Comparison Table

| Object | Before (Original Schema) | After (Codex Implementation) | Proposed (Target Schema) | Recommendation |
|---|---|---|---|---|
| **`purchases`** | Active purchase transaction table. Status: `draft`, `posted`, `received`, `cancelled`. | Demoted to "legacy PO". Replaced by `purchase_bills`. | **Primary Purchase Transaction Table.** Status: `Pending`, `Approved`, `Cancelled`. Add approval audit & charges columns. | **Keep & Evolve.** Eliminate `purchase_bills` and use `purchases` as the sole purchase table. |
| **`purchase_details`** | Line items (`quantity`, `unitcost`, `taxamount`, `linetotal`). | Demoted to "legacy PO lines". | **Primary Purchase Detail Table.** Add line `discountamount`, `taxableamount`, `taxrate`, `taxinclusive`. | **Keep & Evolve.** Eliminate `purchase_bill_lines`. |
| **`purchase_bills`** | Did not exist. | New table for vendor invoices with denormalized names. | **Eliminated.** Redundant. | **Drop table.** Merge required workflow fields into `purchases`. |
| **`purchase_bill_lines`** | Did not exist. | New table for invoice items. | **Eliminated.** Redundant. | **Drop table.** Consolidate into `purchase_details`. |
| **`supplier_returns`** | Did not exist. | Return header linked strictly to `purchase_bills`. | Replaced by standard `purchase_returns` linked to `purchases`. | **Refactor** to `purchase_returns` matching `sales_returns` conventions. |
| **`supplier_return_lines`** | Did not exist. | Return lines linked to `purchase_bill_lines`. | Replaced by `purchase_return_details` linked to `purchase_details`. | **Refactor** to `purchase_return_details`. |
| **`suppliers`** | Vendor master with contact & tax info. | Unchanged vendor master; balance computed via subledger. | Reused as the sole vendor master. Dynamic balance calculation. | **Retain.** Do not introduce parallel vendor account tables. |
| **`supplier_money_transactions`** | Did not exist. | Separate cash/bank transactions table. | Consolidated into `supplier_transactions` with consistent signed balance convention. | **Consolidate** into `supplier_transactions`. |
| **`supplier_ledger_entries`** | Did not exist. | Separate subledger entries table with inverted sign convention. | Unified with `supplier_transactions` or derived via query/view. | **Consolidate** into unified financial transactions table. |
| **`supplier_allocations`** | Did not exist. | Complex allocation table with release triggers. | Simplified `supplier_payment_allocations` linking `fk_purchase` and `fk_suppliertransaction`. | **Simplify** to clean many-to-many allocation structure. |
| **`inventory`** | Store-level on-hand and reserved quantity. | Unchanged. | Reused as-is. | **Retain.** |
| **`stock_movements`** | Core movement ledger (`referencetype`, `referenceid`). | Altered with `source_line_id`, `unit_cost`, and strict trigger. | Retain `unitcost` for invoice valuation. Clean trigger logic. | **Retain `unitcost`.** Remove rigid `source_line_id` dependency on deleted bill tables. |

---

## 5. Risks of Retaining Current Codex Design

1. **Dual System Confusion:** Maintaining two co-located purchasing systems (`purchases` vs `purchase_bills`) causes cognitive overhead for developers and operational confusion for store staff.
2. **Data Model Divergence:** Denormalizing product codes and tax rates directly into invoice lines diverges from SmartCartPro's established POS (`sales`, `sale_details`) architecture.
3. **Trigger Fragility:** PL/pgSQL triggers (`supplier_validate_stock_source`) reject stock adjustments or returns if exact string names or IDs mismatch, creating migration and batch-import friction.
4. **Convention Inversion:** Codex's subledger uses positive = payable, which directly conflicts with the confirmed SmartCartPro specification (positive = receivable/advance, negative = payable).

---

## 6. Recommended Transition Plan (For Future Implementation)

When approved for implementation:
1. **Schema Migration:**
   - Add missing purchase workflow columns (`purchasenumber`, `invoicedate`, `taxableamount`, `charges`, `roundoff`, `approvedby`, `approvedat`, `cancelledby`) to `purchases`.
   - Add line discount and tax detail columns (`discountamount`, `taxableamount`, `taxrate`, `taxinclusive`) to `purchase_details`.
   - Create `purchase_returns` and `purchase_return_details` matching `sales_returns` pattern.
   - Create `supplier_transactions` (payments, refunds, advances, opening balances) with the confirmed signed balance convention.
   - Create `supplier_payment_allocations` linking payments to purchases.
   - Drop redundant Codex tables (`purchase_bills`, `purchase_bill_lines`, `supplier_returns`, `supplier_return_lines`, `supplier_money_transactions`, `supplier_ledger_entries`, `supplier_allocations`).
2. **C# Application Layer:**
   - Update `Purchase` and `PurchaseDetail` entities to include the new fields.
   - Redirect all purchase bill and return services to use `purchases` and `purchase_returns`.
   - Align supplier subledger calculations with the signed balance rule (+ = advance/receivable, - = payable).

