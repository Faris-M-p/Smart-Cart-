using static Ecommerce.Models.Admin.AdminAuthModel;

namespace Ecommerce.Interface.Admin
{
    public interface IAdminAuthInterface
    {
        Task<AdminAuthOperationResult> LoginAsync(AdminLoginRequest request);
        Task<AdminAuthOperationResult> GetCurrentAsync(int employeeId);
    }
}
