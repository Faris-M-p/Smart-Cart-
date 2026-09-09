using Ecommerce.DataAccess;
using Ecommerce.Interface.Admin;
using Ecommerce.Models.Entities;
using Microsoft.EntityFrameworkCore;
using static Ecommerce.Models.Admin.AdminOrderModel;
using static Ecommerce.Models.CommonModel;

namespace Ecommerce.Repository.Admin
{
    public class AdminOrderRepository : IAdminOrderInterface
    {
        private readonly EcommerceDbContext _db;

        public AdminOrderRepository(EcommerceDbContext db)
        {
            _db = db;
        }

        public async Task<TableOutput<AdminOrderListItem>> GetOrderListAsync(AdminOrderListInput input)
        {
            input ??= new AdminOrderListInput();
            var pageIndex = input.PageIndex < 1 ? 1 : input.PageIndex;
            var pageSize = input.PageSize < 1 ? 10 : Math.Min(input.PageSize, 100);
            var search = (input.SearchText ?? string.Empty).Trim();
            var status = (input.OrderStatus ?? string.Empty).Trim();

            var query =
                from o in _db.Orders.AsNoTracking()
                join u in _db.Users.AsNoTracking() on o.FK_User equals u.ID_User into uj
                from u in uj.DefaultIfEmpty()
                select new { o, u };

            if (!string.IsNullOrEmpty(search))
            {
                var term = search.ToLowerInvariant();
                query = query.Where(x =>
                    (x.o.OrderNumber != null && x.o.OrderNumber.ToLower().Contains(term))
                    || (x.o.ReceiverName != null && x.o.ReceiverName.ToLower().Contains(term))
                    || (x.o.Phone != null && x.o.Phone.Contains(search))
                    || (x.u != null && x.u.Email.ToLower().Contains(term))
                    || (x.u != null && x.u.FullName != null && x.u.FullName.ToLower().Contains(term))
                    || ("sc" + x.o.ID_Order.ToString("D6")).Contains(term));
            }

            if (!string.IsNullOrEmpty(status))
            {
                if (string.Equals(status, "Cancelled", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(x => x.o.Cancelled == true || x.o.OrderStatus == "Cancelled");
                }
                else
                {
                    query = query.Where(x =>
                        (x.o.Cancelled != true)
                        && x.o.OrderStatus == status);
                }
            }

            if (input.FromDate.HasValue)
            {
                var from = input.FromDate.Value.Date;
                query = query.Where(x => x.o.OrderDate >= from);
            }

            if (input.ToDate.HasValue)
            {
                var toExclusive = input.ToDate.Value.Date.AddDays(1);
                query = query.Where(x => x.o.OrderDate < toExclusive);
            }

            var totalCount = await query.CountAsync();

            var page = await query
                .OrderByDescending(x => x.o.OrderDate)
                .ThenByDescending(x => x.o.ID_Order)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new
                {
                    OrderId = x.o.ID_Order,
                    OrderNumber = x.o.OrderNumber ?? ("SC" + x.o.ID_Order.ToString("D6")),
                    OrderDate = x.o.OrderDate ?? DateTime.MinValue,
                    x.o.TotalAmount,
                    OrderStatus = x.o.Cancelled == true ? "Cancelled" : x.o.OrderStatus,
                    x.o.PaymentMethod,
                    ReceiverName = x.o.ReceiverName ?? string.Empty,
                    Phone = x.o.Phone ?? string.Empty,
                    City = x.o.City ?? string.Empty,
                    CustomerName = x.u == null
                        ? string.Empty
                        : (string.IsNullOrWhiteSpace(x.u.FullName) ? x.u.UserName : x.u.FullName),
                    Cancelled = x.o.Cancelled == true
                })
                .ToListAsync();

            var orderIds = page.Select(p => p.OrderId).ToList();
            var payments = await _db.Payments.AsNoTracking()
                .Where(p => orderIds.Contains(p.FK_Order))
                .GroupBy(p => p.FK_Order)
                .Select(g => new
                {
                    OrderId = g.Key,
                    Status = g.OrderByDescending(p => p.ID_Payment).Select(p => p.PaymentStatus).FirstOrDefault()
                })
                .ToListAsync();

            var itemCounts = await _db.OrderItems.AsNoTracking()
                .Where(i => orderIds.Contains(i.FK_Order))
                .GroupBy(i => i.FK_Order)
                .Select(g => new { OrderId = g.Key, Count = g.Count() })
                .ToListAsync();

            var firstItems = await _db.OrderItems.AsNoTracking()
                .Where(i => orderIds.Contains(i.FK_Order))
                .GroupBy(i => i.FK_Order)
                .Select(g => new
                {
                    OrderId = g.Key,
                    First = g.OrderBy(i => i.ID_OrderItem).Select(i => new { i.ProductName, i.FK_ProductVariant }).FirstOrDefault()
                })
                .ToListAsync();

            var variantIds = firstItems
                .Where(f => f.First != null && f.First.FK_ProductVariant.HasValue)
                .Select(f => f.First!.FK_ProductVariant!.Value)
                .Distinct()
                .ToList();

            var images = await _db.ProductVariantImages.AsNoTracking()
                .Where(m => variantIds.Contains(m.FK_ProductVariant) && m.MediaType == "Image")
                .GroupBy(m => m.FK_ProductVariant)
                .Select(g => new
                {
                    VariantId = g.Key,
                    Url = g.OrderByDescending(m => m.IsPrimary).ThenBy(m => m.DisplayOrder).Select(m => m.ImageUrl).FirstOrDefault()
                })
                .ToListAsync();

            var rows = page.Select(p =>
            {
                var statusName = p.OrderStatus;
                var canConfirm = !p.Cancelled && (statusName == "Placed" || statusName == "Pending");
                var canUpdate = !p.Cancelled && statusName != "Delivered" && statusName != "Cancelled";
                var canDeliver = !p.Cancelled && statusName == "Confirmed";
                var canCancel = !p.Cancelled && (statusName == "Placed" || statusName == "Pending" || statusName == "Confirmed");
                var first = firstItems.FirstOrDefault(f => f.OrderId == p.OrderId)?.First;
                var imageUrl = first?.FK_ProductVariant != null
                    ? images.FirstOrDefault(i => i.VariantId == first.FK_ProductVariant.Value)?.Url ?? string.Empty
                    : string.Empty;

                return new AdminOrderListItem
                {
                    OrderId = p.OrderId,
                    OrderNumber = p.OrderNumber,
                    OrderDate = p.OrderDate,
                    TotalAmount = p.TotalAmount,
                    OrderStatus = statusName,
                    PaymentMethod = p.PaymentMethod,
                    PaymentStatus = payments.FirstOrDefault(x => x.OrderId == p.OrderId)?.Status ?? "Pending",
                    ReceiverName = p.ReceiverName,
                    Phone = p.Phone,
                    City = p.City,
                    CustomerName = p.CustomerName,
                    Cancelled = p.Cancelled,
                    ItemCount = itemCounts.FirstOrDefault(c => c.OrderId == p.OrderId)?.Count ?? 0,
                    FirstProductName = first?.ProductName ?? string.Empty,
                    FirstImageUrl = imageUrl,
                    CanConfirm = canConfirm,
                    CanUpdateStatus = canUpdate,
                    CanDeliver = canDeliver,
                    CanCancel = canCancel
                };
            }).ToList();

            return new TableOutput<AdminOrderListItem>
            {
                TableData = rows,
                TableSettings = new TableOutput_Settings
                {
                    TotalCount = totalCount,
                    PageIndex = pageIndex,
                    PageSize = pageSize
                }
            };
        }

        public async Task<AdminOrderDetail?> GetOrderByIdAsync(int orderId)
        {
            if (orderId <= 0)
            {
                return null;
            }

            var order = await _db.Orders.AsNoTracking().FirstOrDefaultAsync(o => o.ID_Order == orderId);
            if (order == null)
            {
                return null;
            }

            var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.ID_User == order.FK_User);
            var paymentStatus = await _db.Payments.AsNoTracking()
                .Where(p => p.FK_Order == orderId)
                .OrderByDescending(p => p.ID_Payment)
                .Select(p => p.PaymentStatus)
                .FirstOrDefaultAsync() ?? "Pending";

            var shippingStatus = await _db.Shipping.AsNoTracking()
                .Where(s => s.FK_Order == orderId)
                .OrderByDescending(s => s.ID_Shipping)
                .Select(s => s.ShippingStatus)
                .FirstOrDefaultAsync() ?? string.Empty;

            var cancelled = order.Cancelled == true;
            var statusName = cancelled ? "Cancelled" : order.OrderStatus;

            var header = new AdminOrderHeader
            {
                OrderId = order.ID_Order,
                OrderNumber = order.OrderNumber ?? ("SC" + order.ID_Order.ToString("D6")),
                OrderDate = order.OrderDate ?? DateTime.MinValue,
                TotalAmount = order.TotalAmount,
                OrderStatus = statusName,
                PaymentMethod = order.PaymentMethod,
                PaymentStatus = paymentStatus,
                ShippingStatus = shippingStatus,
                ReceiverName = order.ReceiverName ?? string.Empty,
                Phone = order.Phone ?? string.Empty,
                AddressLine = order.AddressLine ?? string.Empty,
                City = order.City ?? string.Empty,
                Pincode = order.Pincode ?? string.Empty,
                ShippingAddress = order.ShippingAddress ?? string.Empty,
                CustomerName = user == null
                    ? string.Empty
                    : (string.IsNullOrWhiteSpace(user.FullName) ? user.UserName : user.FullName!),
                CustomerEmail = user?.Email ?? string.Empty,
                Cancelled = cancelled,
                CancelledOn = order.CancelledOn,
                CancelledReason = order.CancelledReason ?? string.Empty,
                CanConfirm = !cancelled && (statusName == "Placed" || statusName == "Pending"),
                CanUpdateStatus = !cancelled && statusName != "Delivered" && statusName != "Cancelled",
                CanDeliver = !cancelled && statusName == "Confirmed",
                CanCancel = !cancelled && (statusName == "Placed" || statusName == "Pending" || statusName == "Confirmed")
            };

            var itemsRaw = await (
                from oi in _db.OrderItems.AsNoTracking()
                where oi.FK_Order == orderId
                join p in _db.Products.AsNoTracking() on oi.FK_Product equals p.ID_Product into pj
                from p in pj.DefaultIfEmpty()
                join pv in _db.ProductVariants.AsNoTracking() on oi.FK_ProductVariant equals pv.ID_ProductVariant into pvj
                from pv in pvj.DefaultIfEmpty()
                orderby oi.ID_OrderItem
                select new
                {
                    oi.ID_OrderItem,
                    oi.FK_Product,
                    oi.FK_ProductVariant,
                    Name = oi.ProductName,
                    Slug = p != null ? p.Slug : string.Empty,
                    Label = oi.VariantLabel ?? string.Empty,
                    SKU = oi.SKU ?? (pv != null ? pv.SKU : string.Empty),
                    Price = oi.UnitPrice,
                    MRP = pv != null ? pv.MRP : oi.UnitPrice,
                    oi.Quantity,
                    oi.LineTotal
                }).ToListAsync();

            var variantIds = itemsRaw.Where(i => i.FK_ProductVariant.HasValue).Select(i => i.FK_ProductVariant!.Value).Distinct().ToList();
            var stockQty = await _db.Stock.AsNoTracking()
                .Where(s => variantIds.Contains(s.FK_ProductVariant) && s.Cancelled != true)
                .GroupBy(s => s.FK_ProductVariant)
                .Select(g => new { VariantId = g.Key, Qty = g.Sum(x => x.Quantity) })
                .ToListAsync();

            var images = await _db.ProductVariantImages.AsNoTracking()
                .Where(m => variantIds.Contains(m.FK_ProductVariant) && m.MediaType == "Image")
                .GroupBy(m => m.FK_ProductVariant)
                .Select(g => new
                {
                    VariantId = g.Key,
                    Url = g.OrderByDescending(m => m.IsPrimary).ThenBy(m => m.DisplayOrder).Select(m => m.ImageUrl).FirstOrDefault()
                })
                .ToListAsync();

            var items = itemsRaw.Select(i => new AdminOrderItem
            {
                OrderItemId = i.ID_OrderItem,
                ProductId = i.FK_Product,
                ProductVariantId = i.FK_ProductVariant ?? 0,
                Name = i.Name,
                Slug = i.Slug,
                Label = i.Label,
                SKU = i.SKU,
                Price = i.Price,
                MRP = i.MRP,
                Quantity = i.Quantity,
                LineTotal = i.LineTotal,
                InStock = i.FK_ProductVariant.HasValue
                    && (stockQty.FirstOrDefault(s => s.VariantId == i.FK_ProductVariant.Value)?.Qty ?? 0) > 0,
                ImageUrl = i.FK_ProductVariant.HasValue
                    ? images.FirstOrDefault(img => img.VariantId == i.FK_ProductVariant.Value)?.Url ?? string.Empty
                    : string.Empty
            }).ToList();

            return new AdminOrderDetail
            {
                Order = header,
                Items = items
            };
        }

        public async Task<CommonResponse> ConfirmOrderAsync(int orderId)
        {
            var order = await _db.Orders.FirstOrDefaultAsync(o => o.ID_Order == orderId);
            if (order == null)
            {
                return Fail("Order was not found.");
            }

            if (order.Cancelled == true || order.OrderStatus == "Cancelled")
            {
                return Fail("This order is already cancelled.");
            }

            if (order.OrderStatus != "Placed" && order.OrderStatus != "Pending")
            {
                return Fail("Only placed/pending orders can be confirmed.");
            }

            order.OrderStatus = "Confirmed";
            await _db.SaveChangesAsync();
            return Ok(orderId, "Order confirmed.");
        }

        public async Task<CommonResponse> UpdateOrderStatusAsync(int orderId, string orderStatus)
        {
            var status = (orderStatus ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(status))
            {
                return Fail("Order status is required.");
            }

            var order = await _db.Orders.FirstOrDefaultAsync(o => o.ID_Order == orderId);
            if (order == null)
            {
                return Fail("Order was not found.");
            }

            if (order.Cancelled == true || order.OrderStatus == "Cancelled")
            {
                return Fail("Cancelled orders cannot be updated.");
            }

            if (order.OrderStatus == "Delivered")
            {
                return Fail("Delivered orders cannot be updated.");
            }

            order.OrderStatus = status;
            await _db.SaveChangesAsync();
            return Ok(orderId, "Order status updated.");
        }

        public async Task<CommonResponse> DeliverOrderAsync(int orderId)
        {
            var order = await _db.Orders.FirstOrDefaultAsync(o => o.ID_Order == orderId);
            if (order == null)
            {
                return Fail("Order was not found.");
            }

            if (order.Cancelled == true || order.OrderStatus == "Cancelled")
            {
                return Fail("This order is already cancelled.");
            }

            if (order.OrderStatus != "Confirmed")
            {
                return Fail("Only confirmed orders can be marked delivered.");
            }

            await using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                order.OrderStatus = "Delivered";

                var shipping = await _db.Shipping
                    .Where(s => s.FK_Order == orderId && s.Cancelled != true)
                    .OrderByDescending(s => s.ID_Shipping)
                    .FirstOrDefaultAsync();

                if (shipping != null)
                {
                    shipping.ShippingStatus = "Delivered";
                    shipping.ShippingDate ??= DateTime.UtcNow;
                }

                await _db.SaveChangesAsync();
                await tx.CommitAsync();
                return Ok(orderId, "Order delivered.");
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        public async Task<CommonResponse> CancelOrderAsync(int orderId, string reason)
        {
            var cancelReason = string.IsNullOrWhiteSpace(reason) ? "Cancelled by admin" : reason.Trim();
            var order = await _db.Orders.FirstOrDefaultAsync(o => o.ID_Order == orderId);
            if (order == null)
            {
                return Fail("Order was not found.");
            }

            if (order.Cancelled == true || order.OrderStatus == "Cancelled")
            {
                return Fail("This order is already cancelled.");
            }

            if (order.OrderStatus is not ("Placed" or "Pending" or "Confirmed"))
            {
                return Fail("This order can no longer be cancelled.");
            }

            await using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                var items = await _db.OrderItems
                    .Where(i => i.FK_Order == orderId && i.FK_ProductVariant != null && i.FK_ProductVariant > 0)
                    .ToListAsync();

                foreach (var item in items)
                {
                    var stock = await _db.Stock
                        .Where(s => s.FK_ProductVariant == item.FK_ProductVariant!.Value && s.Cancelled != true)
                        .OrderByDescending(s => s.CreatedOn)
                        .ThenByDescending(s => s.ID_Stock)
                        .FirstOrDefaultAsync();

                    if (stock != null)
                    {
                        stock.Quantity += item.Quantity;
                    }
                }

                var payments = await _db.Payments
                    .Where(p => p.FK_Order == orderId && p.Cancelled != true)
                    .ToListAsync();
                foreach (var payment in payments)
                {
                    payment.Cancelled = true;
                    payment.CancelledOn = DateTime.UtcNow;
                    payment.CancelledReason = cancelReason;
                    payment.PaymentStatus = "Cancelled";
                }

                var shipments = await _db.Shipping
                    .Where(s => s.FK_Order == orderId && s.Cancelled != true)
                    .ToListAsync();
                foreach (var shipment in shipments)
                {
                    shipment.Cancelled = true;
                    shipment.CancelledOn = DateTime.UtcNow;
                    shipment.CancelledReason = cancelReason;
                    shipment.ShippingStatus = "Cancelled";
                }

                order.Cancelled = true;
                order.CancelledOn = DateTime.UtcNow;
                order.CancelledReason = cancelReason;
                order.OrderStatus = "Cancelled";

                await _db.SaveChangesAsync();
                await tx.CommitAsync();
                return Ok(orderId, "Order cancelled.");
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        private static CommonResponse Ok(long code, string message) => new()
        {
            ResponseCode = code,
            StatusCode = true,
            ResponseMsg = message
        };

        private static CommonResponse Fail(string message) => new()
        {
            ResponseCode = -1,
            StatusCode = false,
            ResponseMsg = message
        };
    }
}
