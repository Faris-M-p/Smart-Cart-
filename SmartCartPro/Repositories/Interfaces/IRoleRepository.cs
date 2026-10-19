using SmartCartPro.Models;

namespace SmartCartPro.Repositories.Interfaces;

public interface IRoleRepository
{
    Task<OutputRoleList> ListRolesAsync(InputRoleList input, CancellationToken ct = default);
    Task<OutputRoleSelect?> GetRoleAsync(InputRoleSelect input, CancellationToken ct = default);
    Task<OutputRoleCreate> CreateRoleAsync(InputRoleCreate input, CancellationToken ct = default);
    Task<OutputRoleUpdate> UpdateRoleAsync(InputRoleUpdate input, CancellationToken ct = default);
    Task<OutputRoleDelete> DeleteRoleAsync(InputRoleDelete input, CancellationToken ct = default);
    Task<List<OutputPermissionSelect>> ListPermissionsAsync(CancellationToken ct = default);
    Task<List<int>> GetRolePermissionIdsAsync(int idRole, CancellationToken ct = default);
    Task SetRolePermissionsAsync(int idRole, List<int> permissionIds, CancellationToken ct = default);
    Task<bool> IsRoleInUseAsync(int idRole, CancellationToken ct = default);
}
