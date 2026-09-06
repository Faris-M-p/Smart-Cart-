using Ecommerce.DataAccess;
using Ecommerce.Helpers.Inventory;
using Ecommerce.Interface.Admin;
using Ecommerce.Models.Entities;
using Microsoft.EntityFrameworkCore;
using static Ecommerce.Models.Admin.InventoryModel;
using static Ecommerce.Models.CommonModel;

namespace Ecommerce.Repository.Admin
{
    public class InventoryRepository : IInventoryInterface
    {
        private readonly EcommerceDbContext _db;

        public InventoryRepository(EcommerceDbContext db)
        {
            _db = db;
        }

        public async Task<TableOutput<StockRow>> GetStockListAsync(StockListInput input)
        {
            if (input == null)
            {
                return InventoryHelper.EmptyTableOutput(null);
            }

            try
            {
                var normalized = InventoryHelper.NormalizeInput(input);

                // Aggregate batch stock into per-SKU available quantity.
                var baseQuery =
                    from pv in _db.ProductVariants.AsNoTracking()
                    join p in _db.Products.AsNoTracking() on pv.FK_Product equals p.ID_Product
                    join s in _db.Stock.AsNoTracking() on pv.ID_ProductVariant equals s.FK_ProductVariant into stockGrp
                    from sg in stockGrp.DefaultIfEmpty()
                    where !pv.Cancelled
                    group new { pv, p, sg } by new
                    {
                        pv.ID_ProductVariant,
                        pv.SKU,
                        p.Name
                    }
                    into g
                    select new StockRow
                    {
                        FK_ProductVariant = g.Key.ID_ProductVariant,
                        SKU = g.Key.SKU ?? string.Empty,
                        ProductName = g.Key.Name ?? string.Empty,
                        AvailableQty = g.Sum(x => x.sg != null && !x.sg.Cancelled ? x.sg.Quantity : 0),
                        ReservedQty = 0,
                        ReorderLevel = 10,
                        LastUpdated = g.Max(x => x.sg != null && !x.sg.Cancelled ? (DateTime?)x.sg.CreatedOn : null)
                    };

                if (normalized.SearchLower != null)
                {
                    var s = normalized.SearchLower;
                    baseQuery = baseQuery.Where(x =>
                        (x.ProductName ?? string.Empty).ToLower().Contains(s) ||
                        (x.SKU ?? string.Empty).ToLower().Contains(s));
                }

                var total = await baseQuery.LongCountAsync();

                var orderedQuery = normalized.LowStockOnly
                    ? baseQuery
                        .OrderBy(x => x.AvailableQty >= x.ReorderLevel)
                        .ThenBy(x => x.AvailableQty)
                        .ThenBy(x => x.ProductName)
                        .ThenBy(x => x.SKU)
                    : baseQuery
                        .OrderBy(x => x.ProductName)
                        .ThenBy(x => x.SKU);

                var rows = await orderedQuery
                    .Skip((normalized.PageIndex - 1) * normalized.PageSize)
                    .Take(normalized.PageSize)
                    .ToListAsync();

                return new TableOutput<StockRow>
                {
                    TableData = rows,
                    TableSettings = new TableOutput_Settings
                    {
                        PageIndex = normalized.PageIndex,
                        PageSize = normalized.PageSize,
                        TotalCount = total
                    }
                };
            }
            catch
            {
                throw;
            }
        }

        public async Task<CommonResponse> AdjustStockAsync(AdjustStockInput input)
        {
            try
            {
                if (input == null || input.FK_ProductVariant <= 0)
                {
                    return Fail("Invalid SKU.");
                }

                if (input.AdjustBy == 0)
                {
                    return Fail("Adjustment cannot be 0.");
                }

                var skuOk = await _db.ProductVariants.AnyAsync(pv => pv.ID_ProductVariant == input.FK_ProductVariant && !pv.Cancelled);
                if (!skuOk)
                {
                    return Fail("SKU not found.");
                }

                var now = DateTime.Now;
                _db.Stock.Add(new StockEntity
                {
                    FK_PurchaseDetail = 0,
                    FK_ProductVariant = input.FK_ProductVariant,
                    Quantity = input.AdjustBy,
                    CreatedOn = now,
                    EnterBy = input.EnterBy,
                    Cancelled = false,
                    CancelledReason = string.IsNullOrWhiteSpace(input.Reason) ? "Manual adjust" : input.Reason.Trim()
                });

                await _db.SaveChangesAsync();
                return Ok(0, "Stock adjusted successfully.");
            }
            catch
            {
                throw;
            }
        }

        private static CommonResponse Ok(int code, string msg) =>
            new CommonResponse { StatusCode = true, ResponseCode = code, ResponseMsg = msg };

        private static CommonResponse Fail(string msg) =>
            new CommonResponse { StatusCode = false, ResponseCode = -1, ResponseMsg = msg };
    }
}

