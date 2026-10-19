using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCartPro.Exceptions;
using SmartCartPro.Helpers;
using SmartCartPro.Models;
using SmartCartPro.Services.Interfaces;

namespace SmartCartPro.Controllers;

[Authorize(Policy = "PlatformSuperAdmin")]
public class RoleController : Controller
{
    private readonly IRoleService _svc;

    public RoleController(IRoleService svc) => _svc = svc;

    private int ActorId => AuthClaims.GetUserId(User) ?? 0;

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var model = await _svc.ListRolesAsync(new InputRoleList(), ct);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken ct)
    {
        var permissions = await _svc.ListPermissionsAsync(ct);
        var model = new InRoleCreateView();
        ViewBag.AllPermissions = permissions;
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(InRoleCreateView model, CancellationToken ct)
    {
        CommonHelpers.ValidateModelState(ModelState);

        var input = new InputRoleCreate
        {
            ActorUserId = ActorId,
            RoleCode = model.RoleCode,
            RoleName = model.RoleName,
            Description = model.Description,
            IsActive = model.IsActive,
            PermissionIds = model.PermissionIds ?? new List<int>()
        };
        var result = await _svc.CreateRoleAsync(input, ct);
        if (!result.Ok)
            throw new UpdateFailedException("Unable to create role.", result.ResponseMsg ?? "Unable to create role.");

        return CommonHelpers.SuccessResponse("Role created.", new { id = result.IdRole });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        var role = await _svc.GetRoleAsync(new InputRoleSelect { IdRole = id }, ct);
        if (role is null) return NotFound();

        var allPermissions = await _svc.ListPermissionsAsync(ct);

        var model = new InRoleUpdateView
        {
            IdRole = role.IdRole,
            RoleCode = role.RoleCode,
            RoleName = role.RoleName,
            Description = role.Description,
            IsActive = role.IsActive,
            IsSystem = role.IsSystem,
            PermissionIds = role.PermissionIds,
            AllPermissions = allPermissions
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, InRoleUpdateView model, CancellationToken ct)
    {
        model.IdRole = id;
        CommonHelpers.ValidateModelState(ModelState);

        var input = new InputRoleUpdate
        {
            ActorUserId = ActorId,
            IdRole = model.IdRole,
            RoleName = model.RoleName,
            Description = model.Description,
            IsActive = model.IsActive,
            PermissionIds = model.PermissionIds ?? new List<int>()
        };
        var result = await _svc.UpdateRoleAsync(input, ct);
        if (!result.Ok)
            throw new UpdateFailedException("Unable to update role.", result.ResponseMsg ?? "Unable to update role.");

        return CommonHelpers.SuccessResponse("Role updated.", new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var input = new InputRoleDelete
        {
            ActorUserId = ActorId,
            IdRole = id
        };
        var result = await _svc.DeleteRoleAsync(input, ct);

        if (!result.Ok)
            throw new UpdateFailedException("Unable to delete role.", result.ResponseMsg ?? "Unable to delete role.");

        return CommonHelpers.SuccessResponse("Role deleted.", new { id });
    }
}
