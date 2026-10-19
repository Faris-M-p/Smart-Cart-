using SmartCartPro.Models;

namespace SmartCartPro.Services.Interfaces;

public interface IRoleService
{
    Task<OutputRoleList> ListRolesAsync(InputRoleList input, CancellationToken ct = default);
    Task<OutputRoleSelect?> GetRoleAsync(InputRoleSelect input, CancellationToken ct = default);
    Task<OutputRoleCreate> CreateRoleAsync(InputRoleCreate input, CancellationToken ct = default);
    Task<OutputRoleUpdate> UpdateRoleAsync(InputRoleUpdate input, CancellationToken ct = default);
    Task<OutputRoleStatusUpdate> UpdateRoleStatusAsync(InputRoleStatusUpdate input, CancellationToken ct = default);
    Task<OutputRoleDelete> DeleteRoleAsync(InputRoleDelete input, CancellationToken ct = default);

    /// <summary>The seeded permission catalog grouped by module order, used to render the permission matrix.</summary>
    Task<List<OutputPermissionSelect>> ListPermissionsAsync(CancellationToken ct = default);
}
