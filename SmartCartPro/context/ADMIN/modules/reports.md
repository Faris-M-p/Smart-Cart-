# Reports — Phase 7

## Status
IMPLEMENTED (Phase 7) — expanded with 5 JSON data actions.

## Files
| Layer | File |
|---|---|
| Models | `Models/ReportModels.cs` |
| Service interface | `Services/Interfaces/IReportService.cs` |
| Service impl | `Services/ReportService.cs` |
| Controller | `Controllers/ReportsController.cs` (rewritten) |

## Module & Permission
- ModuleAccess: **REPORTS**
- Permission: **reports.view**

## JSON Data Endpoints
| Method | Route | Parameters | Description |
|---|---|---|---|
| GET | `/Reports/SalesByDate` | dateFrom, dateTo, fkStore? | Daily sales totals |
| GET | `/Reports/StockValuation` | fkStore?, fkCategory? | Inventory value (qty × cost) |
| GET | `/Reports/LowStock` | fkStore?, fkCategory?, page, pageSize | Items at/below reorder level |
| GET | `/Reports/TaxSummary` | dateFrom, dateTo, fkStore? | Tax collected by type |
| GET | `/Reports/TopSelling` | dateFrom, dateTo, fkStore?, top | Top N products by quantity sold |

## Shell Views (HTML, data loaded via AJAX)
- `/Reports/Sales` — sales dashboard shell
- `/Reports/Products` — products report shell
- `/Reports/Customers` — customers report shell

## Implementation Notes
- All aggregations use EF LINQ (no Dapper for these reports)
- `fkStore` defaults to the user's store claim when not provided and user has a store context
- `LowStock` reuses `OutputLowStockList` / `OutputInventorySelect` from `InventoryModels.cs`
- `TaxSummary` currently aggregates total tax per store group — detail-level tax-code breakdown can be added when tax codes are linked at sale-detail level
