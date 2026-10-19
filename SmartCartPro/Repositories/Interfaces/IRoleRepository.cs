using SmartCartPro.Models;

namespace SmartCartPro.Repositories.Interfaces;

public interface IRoleRepository
{
    Task<OutputRoleList> ListRolesAsync(InputRoleList input, CancellationToken ct = default);
    Task<OutputRoleSelect?> GetRoleAsync(InputRoleSelect input, CancellationToken ct = default);
    Task<OutputRoleCreate> CreateRoleAsync(InputRoleCreate input, CancellationToken ct = default);

    /// <summary>Updates name, description and the permission set. Returns null when the role does not exist.</summary>
    Task<OutputRoleUpdate?> UpdateRoleAsync(InputRoleUpdate input, CancellationToken ct = default);

    /// <summary>Returns null when the role does not exist.</summary>
    Task<OutputRoleStatusUpdate?> UpdateRoleStatusAsync(InputRoleStatusUpdate input, CancellationToken ct = default);

    /// <summary>Soft delete (cancelled flag). Returns null when the role does not exist.</summary>
    Task<OutputRoleDelete?> DeleteRoleAsync(InputRoleDelete input, CancellationToken ct = default);

    /// <summary>The seeded permission catalog (active permissions of active modules), in module display order.</summary>
    Task<List<OutputPermissionSelect>> ListPermissionsAsync(CancellationToken ct = default);

    /// <summary>True when the code is taken by any role, including deleted ones (the code column is unique across all rows).</summary>
    Task<bool> IsRoleCodeTakenAsync(string roleCode, CancellationToken ct = default);

    /// <summary>True when another role that is not deleted already has this name, ignoring case.</summary>
    Task<bool> IsRoleNameTakenAsync(string roleName, int? excludeIdRole, CancellationToken ct = default);
}
