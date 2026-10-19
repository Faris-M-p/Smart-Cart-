using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        if (!ModelState.IsValid)
        {
            ViewBag.AllPermissions = await _svc.ListPermissionsAsync(ct);
            return View(model);
        }

        var result = await _svc.CreateRoleAsync(InputRoleCreate.From(model, ActorId), ct);
        if (!result.Ok)
        {
            ModelState.AddModelError(string.Empty, result.ResponseMsg);
            ViewBag.AllPermissions = await _svc.ListPermissionsAsync(ct);
            return View(model);
        }

        TempData["SuccessMessage"] = "Role created.";
        return RedirectToAction(nameof(Index));
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
        if (!ModelState.IsValid)
        {
            model.AllPermissions = await _svc.ListPermissionsAsync(ct);
            return View(model);
        }

        var result = await _svc.UpdateRoleAsync(InputRoleUpdate.From(model, ActorId), ct);
        if (!result.Ok)
        {
            ModelState.AddModelError(string.Empty, result.ResponseMsg);
            model.AllPermissions = await _svc.ListPermissionsAsync(ct);
            return View(model);
        }

        TempData["SuccessMessage"] = "Role updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var result = await _svc.DeleteRoleAsync(new InputRoleDelete { ActorUserId = ActorId, IdRole = id }, ct);

        TempData[result.Ok ? "SuccessMessage" : "ErrorMessage"] =
            result.Ok ? "Role deleted." : result.ResponseMsg;

        return RedirectToAction(nameof(Index));
    }
}
