using Ecommerce.DataAccess;
using Ecommerce.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Helpers.Sales
{
    internal static class StockLedgerHelper
    {
        public static async Task<int> GetAvailableQtyAsync(EcommerceDbContext db, int productVariantId)
        {
            var sum = await db.Stock.AsNoTracking()
                .Where(s => s.FK_ProductVariant == productVariantId && !s.Cancelled)
                .SumAsync(s => (int?)s.Quantity) ?? 0;
            return Math.Max(0, sum);
        }

        public static async Task<string?> DeductFifoAsync(
            EcommerceDbContext db,
            int productVariantId,
            int quantity)
        {
            if (quantity <= 0)
            {
                return "Quantity must be greater than zero.";
            }

            var batches = await db.Stock
                .Where(s => s.FK_ProductVariant == productVariantId && !s.Cancelled && s.Quantity > 0)
                .OrderBy(s => s.CreatedOn)
                .ThenBy(s => s.ID_Stock)
                .ToListAsync();

            var available = batches.Sum(s => s.Quantity);
            if (available < quantity)
            {
                return $"Insufficient stock. Available {available}, required {quantity}.";
            }

            var need = quantity;
            foreach (var batch in batches)
            {
                if (need <= 0)
                {
                    break;
                }

                var take = Math.Min(batch.Quantity, need);
                batch.Quantity -= take;
                need -= take;
            }

            return null;
        }

        public static async Task RestoreAsync(
            EcommerceDbContext db,
            int productVariantId,
            int quantity,
            int? enterBy,
            DateTime now)
        {
            if (quantity <= 0)
            {
                return;
            }

            var latest = await db.Stock
                .Where(s => s.FK_ProductVariant == productVariantId && !s.Cancelled)
                .OrderByDescending(s => s.CreatedOn)
                .ThenByDescending(s => s.ID_Stock)
                .FirstOrDefaultAsync();

            if (latest != null)
            {
                latest.Quantity += quantity;
                return;
            }

            db.Stock.Add(new StockEntity
            {
                FK_PurchaseDetail = 0,
                FK_ProductVariant = productVariantId,
                Quantity = quantity,
                CreatedOn = now,
                EnterBy = enterBy,
                Cancelled = false,
                CancelledReason = "Sale stock restore"
            });
        }
    }
}
