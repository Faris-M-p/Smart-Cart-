using SmartCartPro.Models;

namespace SmartCartPro.Services.Interfaces;

public interface IRoleService
{
    Task<OutputRoleList> ListRolesAsync(InputRoleList input, CancellationToken ct = default);
    Task<OutputRoleSelect?> GetRoleAsync(InputRoleSelect input, CancellationToken ct = default);
    Task<OutputRoleCreate> CreateRoleAsync(InputRoleCreate input, CancellationToken ct = default);
    Task<OutputRoleUpdate> UpdateRoleAsync(InputRoleUpdate input, CancellationToken ct = default);
    Task<OutputRoleDelete> DeleteRoleAsync(InputRoleDelete input, CancellationToken ct = default);
    Task<List<OutputPermissionSelect>> ListPermissionsAsync(CancellationToken ct = default);
}
