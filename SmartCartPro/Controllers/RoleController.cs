using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCartPro.Filters;
using SmartCartPro.Helpers;
using SmartCartPro.Models;
using SmartCartPro.Services.Interfaces;

namespace SmartCartPro.Controllers;

/// <summary>
/// Roles and the permission matrix (views in ~/Views/Role/).
/// ModuleAccess: USERS | Permission: users.manage. Permissions can only come from the seeded catalog;
/// the built-in SUPER_ADMIN definition is editable by the platform super admin only.
/// </summary>
[Authorize]
[ModuleAccess("USERS")]
[RequirePermission("users.manage")]
public class RoleController : Controller
{
    private readonly IRoleService _svc;

    public RoleController(IRoleService svc) => _svc = svc;

    private int ActorId => AuthClaims.GetUserId(User) ?? 0;

    private bool IsPlatformAdmin => AuthClaims.IsPlatformSuperAdminUser(User);

    /// <summary>
    /// Lists roles with their employee and permission counts, search, status filter, sorting and paging.
    /// Company administrators open this from the Administration menu to maintain access control.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] InputRoleList input, CancellationToken ct)
    {
        var model = await _svc.ListRolesAsync(input, ct);
        return View(model);
    }

    /// <summary>
    /// Shows the new-role form with the permission matrix grouped by module.
    /// Administrators open it from the role list to define a custom role.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Create(string? returnUrl, CancellationToken ct)
    {
        ViewBag.Permissions = await _svc.ListPermissionsAsync(ct);
        ViewBag.ReturnUrl = SafeReturnUrl(returnUrl);
        return View(new InRoleCreateView());
    }

    /// <summary>
    /// Creates a role and attaches the selected permissions, which must exist in the seeded catalog.
    /// Administrators submit the new-role form after choosing a code, name and permissions.
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(InRoleCreateView model, string? returnUrl, CancellationToken ct)
    {
        CommonHelpers.ValidateModelState(ModelState);

        var input = new InputRoleCreate
        {
            ActorUserId = ActorId,
            RoleCode = model.RoleCode,
            RoleName = model.RoleName,
            Description = model.Description,
            IsActive = model.IsActive,
            PermissionIds = model.PermissionIds
        };
        var result = await _svc.CreateRoleAsync(input, ct);

        var listUrl = SafeReturnUrl(returnUrl) ?? Url.Action(nameof(Index))!;
        return CommonHelpers.SuccessResponse(
            "Role created successfully.",
            new { id = result.IdRole, redirectUrl = listUrl });
    }

    /// <summary>
    /// Loads one role into the edit form with its current permissions ticked in the matrix.
    /// Administrators open it from the role list; locked roles open read-only.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Edit(int id, string? returnUrl, CancellationToken ct)
    {
        var selectInput = new InputRoleSelect { IdRole = id, ActorIsPlatformAdmin = IsPlatformAdmin };
        var item = await _svc.GetRoleAsync(selectInput, ct);
        if (item is null) return NotFound();

        ViewBag.Permissions = await _svc.ListPermissionsAsync(ct);
        ViewBag.ReturnUrl = SafeReturnUrl(returnUrl);
        return View(ToUpdateView(item));
    }

    /// <summary>
    /// Saves the role name, description and permission set (the role code never changes).
    /// Administrators submit the edit form; the built-in SUPER_ADMIN role is refused for non-platform users.
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, InRoleUpdateView model, string? returnUrl, CancellationToken ct)
    {
        model.IdRole = id;
        ModelState.Remove(nameof(model.IdRole));
        CommonHelpers.ValidateModelState(ModelState);

        var input = new InputRoleUpdate
        {
            ActorUserId = ActorId,
            ActorIsPlatformAdmin = IsPlatformAdmin,
            IdRole = model.IdRole,
            RoleName = model.RoleName,
            Description = model.Description,
            PermissionIds = model.PermissionIds
        };
        var result = await _svc.UpdateRoleAsync(input, ct);

        var listUrl = SafeReturnUrl(returnUrl) ?? Url.Action(nameof(Index))!;
        return CommonHelpers.SuccessResponse(
            "Role updated successfully.",
            new { id = result.IdRole, redirectUrl = listUrl });
    }

    /// <summary>
    /// Activates or deactivates a role from the list; a role that employees still hold cannot be deactivated.
    /// Administrators use it after confirming the status change.
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SetActive(int id, bool isActive, CancellationToken ct)
    {
        var input = new InputRoleStatusUpdate
        {
            ActorUserId = ActorId,
            IdRole = id,
            IsActive = isActive
        };
        var result = await _svc.UpdateRoleStatusAsync(input, ct);

        return CommonHelpers.SuccessResponse(
            result.IsActive ? "Role activated." : "Role deactivated.",
            new { id = result.IdRole, isActive = result.IsActive });
    }

    /// <summary>
    /// Soft-deletes a custom role that no employee holds; system roles and roles in use are refused.
    /// Administrators use it from the list after confirming; the role code stays reserved.
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var input = new InputRoleDelete
        {
            ActorUserId = ActorId,
            IdRole = id
        };
        var result = await _svc.DeleteRoleAsync(input, ct);

        return CommonHelpers.SuccessResponse("Role deleted.", new { id = result.IdRole });
    }

    // ------------------------------------------------------------------ helpers

    private static InRoleUpdateView ToUpdateView(OutputRoleSelect item) => new()
    {
        IdRole = item.IdRole,
        RoleName = item.RoleName,
        Description = item.Description,
        PermissionIds = item.PermissionIds,
        RoleCode = item.RoleCode,
        IsSystem = item.IsSystem,
        IsActive = item.IsActive,
        IsLocked = item.IsLocked,
        UserCount = item.UserCount
    };

    private string? SafeReturnUrl(string? returnUrl)
    {
        if (string.IsNullOrWhiteSpace(returnUrl)) return null;
        if (!Url.IsLocalUrl(returnUrl)) return null;
        return returnUrl;
    }
}
