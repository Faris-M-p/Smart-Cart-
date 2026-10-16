using SmartCartPro.Entities;
using SmartCartPro.Repositories.Interfaces;
using SmartCartPro.Services.Interfaces;

namespace SmartCartPro.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orders;

    public OrderService(IOrderRepository orders)
    {
        _orders = orders;
    }

    public Task<Order?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _orders.GetByIdAsync(id, cancellationToken);

    public Task<IReadOnlyList<Order>> ListAsync(CancellationToken cancellationToken = default) =>
        _orders.ListAsync(cancellationToken);

    public async Task<Order> CreateAsync(Order order, CancellationToken cancellationToken = default)
    {
        if (order.CreatedAt == default)
        {
            order.CreatedAt = DateTime.UtcNow;
            order.OrderDate = DateTime.UtcNow;
        }

        await _orders.AddAsync(order, cancellationToken);
        await _orders.SaveChangesAsync(cancellationToken);
        return order;
    }
}
