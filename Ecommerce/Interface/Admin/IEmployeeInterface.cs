using static Ecommerce.Models.CommonModel;
using static Ecommerce.Models.Admin.EmployeeModel;

namespace Ecommerce.Interface.Admin
{
    public interface IEmployeeInterface
    {
        Task<TableOutput<Employee>> GetEmployeeListAsync(EmployeeListInput input);
        Task<Employee?> GetEmployeeByIdAsync(int id);
        Task<List<EmployeeRoleOption>> GetActiveUserRolesAsync();
        Task<CommonResponse> CreateEmployeeAsync(EmployeeUpdateInput input);
        Task<CommonResponse> UpdateEmployeeAsync(EmployeeUpdateInput input);
        Task<CommonResponse> DeleteEmployeeAsync(EmployeeDeleteInput input);
    }
}
