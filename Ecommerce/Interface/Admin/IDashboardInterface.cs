using static Ecommerce.Models.Admin.DashboardModel;

namespace Ecommerce.Interface.Admin
{
    public interface IDashboardInterface
    {
        Task<DashboardSummary> GetSummaryAsync(CancellationToken cancellationToken = default);
    }
}
