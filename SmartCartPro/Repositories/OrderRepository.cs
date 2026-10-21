using Microsoft.EntityFrameworkCore;
using SmartCartPro.Data;
using SmartCartPro.Entities;
using SmartCartPro.Models;
using SmartCartPro.Repositories.Interfaces;

namespace SmartCartPro.Repositories;

/// <summary>
/// Online order repository — Phase 7 rewrite.
/// Entities (Order, OnlineOrderItem) are used only inside this class.
/// On status transitions:
///   pending → confirmed  : deducts stock (movement type "sale") via IStockRepository
///   confirmed/packed/shipped → cancelled : restores stock (movement type "return_in") via IStockRepository
/// </summary>
public class OnlineOrderRepository : IOnlineOrderRepository
{
    private readonly SmartCartDbContext _db;
    private readonly IStockRepository _stock;

    // Valid forward transitions (from → allowed nexts)
    private static readonly Dictionary<string, string[]> AllowedTransitions = new(StringComparer.OrdinalIgnoreCase)
    {
        ["pending"]   = ["confirmed", "cancelled"],
        ["confirmed"] = ["packed", "cancelled"],
        ["packed"]    = ["shipped", "cancelled"],
        ["shipped"]   = ["delivered", "cancelled"],
        ["delivered"] = [],
        ["cancelled"] = [],
    };

    // Statuses from which stock was already deducted (cancel → must restore)
    private static readonly HashSet<string> StockDeductedStatuses = new(StringComparer.OrdinalIgnoreCase)
        { "confirmed", "packed", "shipped", "delivered" };

    public OnlineOrderRepository(SmartCartDbContext db, IStockRepository stock)
    {
        _db = db;
        _stock = stock;
    }

    public async Task<OutputOnlineOrderSelect?> GetOrderAsync(InputOnlineOrderSelect input, CancellationToken ct = default)
    {
        var q = _db.Orders.Where(o => o.IdOnlineOrder == input.IdOnlineOrder);
        if (input.FkStore is > 0)
            q = q.Where(o => o.FkStore == input.FkStore.Value);

        var order = await q
            .Select(o => new OutputOnlineOrderSelect
            {
                IdOnlineOrder  = o.IdOnlineOrder,
                FkStore        = o.FkStore,
                StoreName      = o.Store != null ? o.Store.StoreName : string.Empty,
                FkCustomer     = o.FkCustomer,
                CustomerName   = _db.Customers
                                    .Where(c => c.IdCustomer == o.FkCustomer)
                                    .Select(c => c.FullName)
                                    .FirstOrDefault() ?? string.Empty,
                CustomerPhone  = _db.Customers
                                    .Where(c => c.IdCustomer == o.FkCustomer)
                                    .Select(c => c.Phone)
                                    .FirstOrDefault(),
                OrderNumber    = o.OrderNumber,
                OrderDate      = o.OrderDate,
                Status         = o.Status,
                Subtotal       = o.Subtotal,
                TaxAmount      = o.TaxAmount,
                ShippingAmount = o.ShippingAmount,
                DiscountAmount = o.DiscountAmount,
                TotalAmount    = o.TotalAmount,
                ShippingAddress = o.ShippingAddress,
                Notes          = o.Notes,
                Cancelled      = o.Cancelled,
                CancelledOn    = o.CancelledOn,
                CancelledReason = o.CancelledReason,
                CreatedAt      = o.CreatedAt,
                UpdatedAt      = o.UpdatedAt,
                Items = o.Items.Select(i => new OutputOnlineOrderItem
                {
                    IdOnlineOrderItem = i.IdOnlineOrderItem,
                    FkProduct         = i.FkProduct,
                    ProductName       = i.Product != null ? i.Product.ProductName : string.Empty,
                    VariantName       = i.FkProductVariant == null
                        ? null
                        : _db.ProductVariants
                            .Where(v => v.IdProductVariant == i.FkProductVariant)
                            .Select(v => v.VariantName)
                            .FirstOrDefault(),
                    FkProductVariant  = i.FkProductVariant,
                    Quantity          = i.Quantity,
                    UnitPrice         = i.UnitPrice,
                    LineTotal         = i.LineTotal
                }).ToList()
            })
            .FirstOrDefaultAsync(ct);

        if (order is null) return null;

        order.AllowedNextStatuses = NextStatuses(order.Status);
        order.StockWasDeducted = StockDeductedStatuses.Contains(order.Status);
        return order;
    }

    public async Task<OutputOnlineOrderList> ListOrdersAsync(InputOnlineOrderList input, CancellationToken ct = default)
    {
        var q = _db.Orders.AsQueryable();

        if (input.FkStore.HasValue)
            q = q.Where(o => o.FkStore == input.FkStore.Value);

        if (!string.IsNullOrWhiteSpace(input.Status))
            q = q.Where(o => o.Status == input.Status);

        if (input.From.HasValue)
            q = q.Where(o => o.OrderDate >= input.From.Value);

        if (input.To.HasValue)
        {
            var to = input.To.Value.TimeOfDay == TimeSpan.Zero
                ? input.To.Value.Date.AddDays(1)
                : input.To.Value;
            q = q.Where(o => o.OrderDate < to);
        }

        if (!string.IsNullOrWhiteSpace(input.Search))
        {
            var term = input.Search.Trim().ToLower();
            q = q.Where(o =>
                o.OrderNumber.ToLower().Contains(term)
                || _db.Customers.Any(c => c.IdCustomer == o.FkCustomer && c.FullName.ToLower().Contains(term)));
        }

        q = (input.SortBy ?? string.Empty).ToLowerInvariant() switch
        {
            "ordernumber" => input.SortDesc
                ? q.OrderByDescending(o => o.OrderNumber)
                : q.OrderBy(o => o.OrderNumber),
            "status" => input.SortDesc
                ? q.OrderByDescending(o => o.Status).ThenByDescending(o => o.OrderDate)
                : q.OrderBy(o => o.Status).ThenByDescending(o => o.OrderDate),
            "totalamount" => input.SortDesc
                ? q.OrderByDescending(o => o.TotalAmount)
                : q.OrderBy(o => o.TotalAmount),
            "customername" => input.SortDesc
                ? q.OrderByDescending(o => _db.Customers.Where(c => c.IdCustomer == o.FkCustomer).Select(c => c.FullName).FirstOrDefault())
                : q.OrderBy(o => _db.Customers.Where(c => c.IdCustomer == o.FkCustomer).Select(c => c.FullName).FirstOrDefault()),
            _ => input.SortDesc
                ? q.OrderByDescending(o => o.OrderDate)
                : q.OrderBy(o => o.OrderDate)
        };

        var total = await q.CountAsync(ct);

        var rows = await q
            .Skip((input.Page - 1) * input.PageSize)
            .Take(input.PageSize)
            .Select(o => new OutputOnlineOrderListItem
            {
                IdOnlineOrder = o.IdOnlineOrder,
                FkStore       = o.FkStore,
                StoreName     = o.Store != null ? o.Store.StoreName : string.Empty,
                FkCustomer    = o.FkCustomer,
                CustomerName  = _db.Customers
                                    .Where(c => c.IdCustomer == o.FkCustomer)
                                    .Select(c => c.FullName)
                                    .FirstOrDefault() ?? string.Empty,
                OrderNumber   = o.OrderNumber,
                OrderDate     = o.OrderDate,
                Status        = o.Status,
                TotalAmount   = o.TotalAmount,
                Cancelled     = o.Cancelled
            })
            .ToListAsync(ct);

        return new OutputOnlineOrderList
        {
            TotalCount = total,
            Page       = input.Page,
            PageSize   = input.PageSize,
            Items      = rows
        };
    }

    public async Task<OutputOnlineOrderStatusUpdate> UpdateStatusAsync(
        InputOnlineOrderStatusUpdate input, CancellationToken ct = default)
    {
        var q = _db.Orders.Include(o => o.Items).Where(o => o.IdOnlineOrder == input.IdOnlineOrder);
        if (input.FkStore is > 0)
            q = q.Where(o => o.FkStore == input.FkStore.Value);

        var order = await q.FirstOrDefaultAsync(ct);

        if (order is null)
            return new OutputOnlineOrderStatusUpdate
                { ResponseCode = 1, ResponseMsg = "Order not found.", StatusCode = 404 };

        if (order.Cancelled)
            return new OutputOnlineOrderStatusUpdate
                { ResponseCode = 2, ResponseMsg = "Order is already cancelled.", StatusCode = 422 };

        var currentStatus = order.Status;

        if (!AllowedTransitions.TryGetValue(currentStatus, out var allowed))
            return new OutputOnlineOrderStatusUpdate
                { ResponseCode = 3, ResponseMsg = $"Unknown current status '{currentStatus}'.", StatusCode = 422 };

        if (!allowed.Contains(input.NewStatus, StringComparer.OrdinalIgnoreCase))
            return new OutputOnlineOrderStatusUpdate
            {
                ResponseCode = 4,
                ResponseMsg  = $"Cannot transition from '{currentStatus}' to '{input.NewStatus}'.",
                StatusCode   = 422
            };

        await using var tx = await _db.Database.BeginTransactionAsync(ct);
        try
        {
            // ── Confirm: deduct stock for each item ────────────────
            if (string.Equals(input.NewStatus, "confirmed", StringComparison.OrdinalIgnoreCase))
            {
                foreach (var item in order.Items)
                {
                    var stockResult = await _stock.ApplyMovementAsync(new InputStockMovement
                    {
                        FkStore       = item.FkStore,
                        FkProduct     = item.FkProduct,
                        FkProductVariant = item.FkProductVariant,
                        MovementType  = "sale",
                        Quantity      = item.Quantity,
                        ReferenceType = "online_order",
                        ReferenceId   = order.IdOnlineOrder,
                        Notes         = $"Online order {order.OrderNumber} confirmed",
                        CreatedBy     = input.UpdatedBy
                    }, ct);

                    if (!stockResult.Ok)
                    {
                        await tx.RollbackAsync(ct);
                        return new OutputOnlineOrderStatusUpdate
                        {
                            ResponseCode = 5,
                            ResponseMsg  = $"Stock deduction failed for product {item.FkProduct}: {stockResult.ResponseMsg}",
                            StatusCode   = 422
                        };
                    }
                }
            }

            // ── Cancel from a stock-deducted status: restore stock ─
            if (string.Equals(input.NewStatus, "cancelled", StringComparison.OrdinalIgnoreCase)
                && StockDeductedStatuses.Contains(currentStatus))
            {
                foreach (var item in order.Items)
                {
                    await _stock.ApplyMovementAsync(new InputStockMovement
                    {
                        FkStore       = item.FkStore,
                        FkProduct     = item.FkProduct,
                        FkProductVariant = item.FkProductVariant,
                        MovementType  = "return_in",
                        Quantity      = item.Quantity,
                        ReferenceType = "online_order",
                        ReferenceId   = order.IdOnlineOrder,
                        Notes         = $"Online order {order.OrderNumber} cancelled – stock restored",
                        CreatedBy     = input.UpdatedBy
                    }, ct);
                }
            }

            // ── Update order record ────────────────────────────────
            order.Status    = input.NewStatus.ToLowerInvariant();
            order.UpdatedAt = DateTime.UtcNow;

            if (string.Equals(input.NewStatus, "cancelled", StringComparison.OrdinalIgnoreCase))
            {
                order.Cancelled       = true;
                order.CancelledOn     = DateTime.UtcNow;
                order.CancelledReason = input.Reason;
            }

            await _db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);

            return new OutputOnlineOrderStatusUpdate
            {
                ResponseCode = 0,
                ResponseMsg  = $"Order status updated to '{order.Status}'.",
                StatusCode   = 200,
                NewStatus    = order.Status
            };
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }

    private static List<string> NextStatuses(string status) =>
        AllowedTransitions.TryGetValue(status, out var next) ? next.ToList() : new List<string>();
}
