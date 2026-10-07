using static Ecommerce.Models.Admin.AdminReviewModel;
using static Ecommerce.Models.CommonModel;

namespace Ecommerce.Interface.Admin
{
    public interface IAdminReviewInterface
    {
        Task<TableOutput<AdminReviewListItem>> GetReviewListAsync(AdminReviewListInput input);

        /// <summary>Soft-removes a review (cancelled = true) so it no longer shows on the storefront.</summary>
        Task<CommonResponse> RemoveReviewAsync(int reviewId, int employeeId, string? reason);
    }
}
