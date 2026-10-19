using SmartCartPro.Exceptions;
using SmartCartPro.Models;
using SmartCartPro.Repositories.Interfaces;
using SmartCartPro.Services.Interfaces;

namespace SmartCartPro.Services;

/// <summary>
/// Role management rules: unique code and name, permissions only from the seeded catalog,
/// system roles are protected, the built-in SUPER_ADMIN definition is platform-only, and a role
/// that employees still hold can be neither deactivated nor deleted.
/// </summary>
public class RoleService : IRoleService
{
    private const string SuperAdminRoleCode = "SUPER_ADMIN";
    private const string FieldSummary = "Please correct the highlighted fields.";

    private readonly IRoleRepository _repo;
    private readonly IAuditService _audit;

    public RoleService(IRoleRepository repo, IAuditService audit)
    {
        _repo = repo;
        _audit = audit;
    }

    public async Task<OutputRoleList> ListRolesAsync(InputRoleList input, CancellationToken ct = default)
    {
        input.Page = Math.Max(1, input.Page);
        input.PageSize = input.PageSize <= 0 ? 25 : Math.Min(input.PageSize, 100);
        input.Search = string.IsNullOrWhiteSpace(input.Search) ? null : input.Search.Trim();
        input.SortBy = string.IsNullOrWhiteSpace(input.SortBy) ? "rolename" : input.SortBy.Trim().ToLowerInvariant();

        var result = await _repo.ListRolesAsync(input, ct);

        // The requested page no longer exists (for example after filtering): show the last page instead.
        if (result.Roles.Count == 0 && result.TotalCount > 0 && input.Page > 1)
        {
            input.Page = (int)Math.Ceiling(result.TotalCount / (double)input.PageSize);
            result = await _repo.ListRolesAsync(input, ct);
        }

        result.Filter = input;
        return result;
    }

    public async Task<OutputRoleSelect?> GetRoleAsync(InputRoleSelect input, CancellationToken ct = default)
    {
        var role = await _repo.GetRoleAsync(input, ct);
        if (role is null) return null;

        role.IsLocked = IsLockedFor(role, input.ActorIsPlatformAdmin);
        return role;
    }

    public Task<List<OutputPermissionSelect>> ListPermissionsAsync(CancellationToken ct = default) =>
        _repo.ListPermissionsAsync(ct);

    public async Task<OutputRoleCreate> CreateRoleAsync(InputRoleCreate input, CancellationToken ct = default)
    {
        var roleCode = input.RoleCode.Trim().ToUpperInvariant();
        var roleName = input.RoleName.Trim();
        var errors = new Dictionary<string, string[]>();

        if (await _repo.IsRoleCodeTakenAsync(roleCode, ct))
        {
            AddError(errors, "RoleCode", "This role code is already in use. Codes of deleted roles cannot be reused.");
        }

        if (await _repo.IsRoleNameTakenAsync(roleName, null, ct))
        {
            AddError(errors, "RoleName", "A role with this name already exists.");
        }

        var permissionIds = await ValidatePermissionsAsync(input.PermissionIds, errors, ct);

        ThrowIfInvalid(errors);

        input.RoleCode = roleCode;
        input.RoleName = roleName;
        input.PermissionIds = permissionIds;

        var created = await _repo.CreateRoleAsync(input, ct);

        await AuditAsync(input.ActorUserId, created.IdRole, "Create",
            $"Role '{created.RoleCode}' created with {permissionIds.Count} permission(s).", ct);

        return created;
    }

    public async Task<OutputRoleUpdate> UpdateRoleAsync(InputRoleUpdate input, CancellationToken ct = default)
    {
        var existing = await _repo.GetRoleAsync(
                new InputRoleSelect { IdRole = input.IdRole, ActorIsPlatformAdmin = input.ActorIsPlatformAdmin }, ct)
            ?? throw new ResourceNotFoundException("Role not found.", $"IdRole={input.IdRole}");

        if (IsLockedFor(existing, input.ActorIsPlatformAdmin))
        {
            throw new ForbiddenException(
                "The Super Administrator role can only be changed by the platform administrator.",
                $"Actor {input.ActorUserId} tried to edit {existing.RoleCode}.");
        }

        var roleName = input.RoleName.Trim();
        var errors = new Dictionary<string, string[]>();

        if (await _repo.IsRoleNameTakenAsync(roleName, input.IdRole, ct))
        {
            AddError(errors, "RoleName", "A role with this name already exists.");
        }

        var permissionIds = await ValidatePermissionsAsync(input.PermissionIds, errors, ct);

        ThrowIfInvalid(errors);

        input.RoleName = roleName;
        input.PermissionIds = permissionIds;

        var updated = await _repo.UpdateRoleAsync(input, ct)
            ?? throw new ResourceNotFoundException("Role not found.", $"IdRole={input.IdRole}");

        await AuditAsync(input.ActorUserId, updated.IdRole, "Update",
            $"Role '{existing.RoleCode}' updated ({permissionIds.Count} permission(s) selected).", ct);

        return updated;
    }

    public async Task<OutputRoleStatusUpdate> UpdateRoleStatusAsync(InputRoleStatusUpdate input, CancellationToken ct = default)
    {
        var existing = await _repo.GetRoleAsync(new InputRoleSelect { IdRole = input.IdRole }, ct)
            ?? throw new ResourceNotFoundException("Role not found.", $"IdRole={input.IdRole}");

        if (existing.IsActive == input.IsActive)
        {
            return new OutputRoleStatusUpdate { IdRole = existing.IdRole, IsActive = existing.IsActive };
        }

        if (!input.IsActive)
        {
            if (existing.IsSystem)
            {
                throw new ConflictException(
                    "System roles cannot be deactivated.",
                    $"Role {existing.RoleCode} is a system role.");
            }

            if (existing.UserCount > 0)
            {
                throw new ConflictException(
                    $"{existing.UserCount} employee(s) still hold this role. Move them to another role first.",
                    $"Role {existing.RoleCode} is in use by {existing.UserCount} user(s).");
            }
        }

        var updated = await _repo.UpdateRoleStatusAsync(input, ct)
            ?? throw new ResourceNotFoundException("Role not found.", $"IdRole={input.IdRole}");

        await AuditAsync(input.ActorUserId, updated.IdRole, input.IsActive ? "Activate" : "Deactivate",
            $"Role '{existing.RoleCode}' {(input.IsActive ? "activated" : "deactivated")}.", ct);

        return updated;
    }

    public async Task<OutputRoleDelete> DeleteRoleAsync(InputRoleDelete input, CancellationToken ct = default)
    {
        var existing = await _repo.GetRoleAsync(new InputRoleSelect { IdRole = input.IdRole }, ct)
            ?? throw new ResourceNotFoundException("Role not found.", $"IdRole={input.IdRole}");

        if (existing.IsSystem)
        {
            throw new ForbiddenException(
                "System roles cannot be deleted.",
                $"Role {existing.RoleCode} is a system role.");
        }

        if (existing.UserCount > 0)
        {
            throw new ConflictException(
                $"{existing.UserCount} employee(s) still hold this role. Move them to another role first.",
                $"Role {existing.RoleCode} is in use by {existing.UserCount} user(s).");
        }

        var deleted = await _repo.DeleteRoleAsync(input, ct)
            ?? throw new ResourceNotFoundException("Role not found.", $"IdRole={input.IdRole}");

        await AuditAsync(input.ActorUserId, deleted.IdRole, "Delete", $"Role '{existing.RoleCode}' deleted.", ct);

        return deleted;
    }

    // ------------------------------------------------------------------ helpers

    /// <summary>The built-in SUPER_ADMIN definition may only be edited by the platform super admin.</summary>
    private static bool IsLockedFor(OutputRoleSelect role, bool actorIsPlatformAdmin) =>
        role.IsSystem
        && string.Equals(role.RoleCode, SuperAdminRoleCode, StringComparison.OrdinalIgnoreCase)
        && !actorIsPlatformAdmin;

    /// <summary>Every permission id must exist in the seeded catalog. Returns the distinct ids.</summary>
    private async Task<List<int>> ValidatePermissionsAsync(
        List<int> requested, Dictionary<string, string[]> errors, CancellationToken ct)
    {
        var distinct = requested.Distinct().ToList();
        if (distinct.Count == 0) return distinct;

        var catalog = (await _repo.ListPermissionsAsync(ct)).Select(p => p.IdPermission).ToHashSet();
        if (distinct.Any(id => !catalog.Contains(id)))
        {
            AddError(errors, "PermissionIds", "One or more selected permissions are not part of the permission catalog.");
        }

        return distinct;
    }

    private static void AddError(Dictionary<string, string[]> errors, string field, string message)
    {
        errors.TryAdd(field, new[] { message });
    }

    private static void ThrowIfInvalid(Dictionary<string, string[]> errors)
    {
        if (errors.Count == 0) return;

        var developerMessage = string.Join(" ", errors.SelectMany(e => e.Value));
        throw new ValidationException(FieldSummary, developerMessage, errors);
    }

    private Task AuditAsync(int actorUserId, int idRole, string action, string details, CancellationToken ct) =>
        _audit.WriteLogAsync(new InputAuditLogCreate
        {
            ActorUserId = actorUserId,
            EntityName = "Role",
            EntityId = idRole,
            Action = action,
            Details = details
        }, ct);
}
