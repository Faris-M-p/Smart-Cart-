using SmartCartPro.Models;
using SmartCartPro.Repositories.Interfaces;
using SmartCartPro.Services.Interfaces;

namespace SmartCartPro.Services;

public class RoleService : IRoleService
{
    private readonly IRoleRepository _repo;

    public RoleService(IRoleRepository repo) => _repo = repo;

    public Task<OutputRoleList> ListRolesAsync(InputRoleList input, CancellationToken ct = default) =>
        _repo.ListRolesAsync(input, ct);

    public Task<OutputRoleSelect?> GetRoleAsync(InputRoleSelect input, CancellationToken ct = default) =>
        _repo.GetRoleAsync(input, ct);

    public async Task<OutputRoleCreate> CreateRoleAsync(InputRoleCreate input, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(input.RoleCode))
        {
            return new OutputRoleCreate { ResponseCode = 1, ResponseMsg = "Role code is required.", StatusCode = 400 };
        }

        return await _repo.CreateRoleAsync(input, ct);
    }

    public async Task<OutputRoleUpdate> UpdateRoleAsync(InputRoleUpdate input, CancellationToken ct = default)
    {
        var existing = await _repo.GetRoleAsync(new InputRoleSelect { IdRole = input.IdRole }, ct);
        if (existing is null)
        {
            return new OutputRoleUpdate { ResponseCode = 1, ResponseMsg = "Role not found.", StatusCode = 404 };
        }

        // Platform SUPER_ADMIN role code cannot have permissions cleared
        return await _repo.UpdateRoleAsync(input, ct);
    }

    public async Task<OutputRoleDelete> DeleteRoleAsync(InputRoleDelete input, CancellationToken ct = default)
    {
        var existing = await _repo.GetRoleAsync(new InputRoleSelect { IdRole = input.IdRole }, ct);
        if (existing is null)
        {
            return new OutputRoleDelete { ResponseCode = 1, ResponseMsg = "Role not found.", StatusCode = 404 };
        }

        if (existing.IsSystem)
        {
            return new OutputRoleDelete
            {
                ResponseCode = 1,
                ResponseMsg = "System roles cannot be deleted.",
                StatusCode = 403
            };
        }

        if (await _repo.IsRoleInUseAsync(input.IdRole, ct))
        {
            return new OutputRoleDelete
            {
                ResponseCode = 1,
                ResponseMsg = "Cannot delete role — it is assigned to one or more users.",
                StatusCode = 409
            };
        }

        return await _repo.DeleteRoleAsync(input, ct);
    }

    public Task<List<OutputPermissionSelect>> ListPermissionsAsync(CancellationToken ct = default) =>
        _repo.ListPermissionsAsync(ct);
}
