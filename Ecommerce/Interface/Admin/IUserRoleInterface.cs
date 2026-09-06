using static Ecommerce.Models.CommonModel;
using static Ecommerce.Models.Admin.UserRoleModel;

namespace Ecommerce.Interface.Admin
{
    public interface IUserRoleInterface
    {
        Task<UserRole?> GetUserRoleByIdAsync(int id);
        Task<TableOutput<UserRole>> GetUserRoleListAsync(UserRoleListInput input);
        Task<List<PermissionGroupNode>> GetPermissionTreeAsync();
        Task<CommonResponse> CreateUserRoleAsync(UserRoleUpdateInput input);
        Task<CommonResponse> UpdateUserRoleAsync(UserRoleUpdateInput input);
        Task<CommonResponse> SaveUserRolePermissionsAsync(UserRolePermissionSaveInput input);
        Task<CommonResponse> DeleteUserRoleAsync(UserRoleDeleteInput input);
    }
}
