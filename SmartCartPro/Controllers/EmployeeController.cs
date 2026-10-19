using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCartPro.Helpers;
using SmartCartPro.Models;
using SmartCartPro.Services.Interfaces;

namespace SmartCartPro.Controllers;

[Authorize]
public class EmployeeController : Controller
{
    private readonly IUserService _userSvc;
    private readonly IRoleService _roleSvc;
    private readonly ICompanyService _companySvc;

    public EmployeeController(IUserService userSvc, IRoleService roleSvc, ICompanyService companySvc)
    {
        _userSvc = userSvc;
        _roleSvc = roleSvc;
        _companySvc = companySvc;
    }

    private int ActorId => AuthClaims.GetUserId(User) ?? 0;
    private bool IsPlatformAdmin => AuthClaims.IsPlatformSuperAdminUser(User);

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var model = await _userSvc.ListUsersAsync(new InputUserList(), ct);
        return View(model);
    }

    [HttpGet]
    [Authorize(Policy = "PlatformSuperAdmin")]
    public async Task<IActionResult> Create(CancellationToken ct)
    {
        ViewBag.Roles = (await _roleSvc.ListRolesAsync(new InputRoleList(), ct)).Roles;
        ViewBag.Stores = (await _companySvc.ListStoresAsync(new InputStoreList(), ct)).Stores;
        return View(new InUserCreateView());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "PlatformSuperAdmin")]
    public async Task<IActionResult> Create(InUserCreateView model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Roles = (await _roleSvc.ListRolesAsync(new InputRoleList(), ct)).Roles;
            ViewBag.Stores = (await _companySvc.ListStoresAsync(new InputStoreList(), ct)).Stores;
            return View(model);
        }

        var input = new InputUserCreate
        {
            ActorUserId = ActorId,
            UserName = model.UserName,
            Email = model.Email,
            FullName = model.FullName,
            Phone = model.Phone,
            EmployeeCode = model.EmployeeCode,
            FkRole = model.FkRole,
            StoreIds = model.StoreIds
        };

        var result = await _userSvc.CreateUserAsync(input, model.InitialPassword, ct);
        if (!result.Ok)
        {
            ModelState.AddModelError(string.Empty, result.ResponseMsg);
            ViewBag.Roles = (await _roleSvc.ListRolesAsync(new InputRoleList(), ct)).Roles;
            ViewBag.Stores = (await _companySvc.ListStoresAsync(new InputStoreList(), ct)).Stores;
            return View(model);
        }

        TempData["SuccessMessage"] = $"Employee '{result.UserName}' created.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        var user = await _userSvc.GetUserAsync(new InputUserSelect { IdUser = id }, ct);
        if (user is null) return NotFound();

        var model = new InUserUpdateView
        {
            IdUser = user.IdUser,
            FullName = user.FullName,
            Phone = user.Phone,
            EmployeeCode = user.EmployeeCode,
            IsActive = user.IsActive
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, InUserUpdateView model, CancellationToken ct)
    {
        model.IdUser = id;
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _userSvc.UpdateUserAsync(InputUserUpdate.From(model, ActorId), ct);
        if (!result.Ok)
        {
            ModelState.AddModelError(string.Empty, result.ResponseMsg);
            return View(model);
        }

        TempData["SuccessMessage"] = "Employee updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "PlatformSuperAdmin")]
    public async Task<IActionResult> Deactivate(int id, CancellationToken ct)
    {
        await _userSvc.DeactivateUserAsync(id, ct);
        TempData["SuccessMessage"] = "Employee deactivated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "PlatformSuperAdmin")]
    public async Task<IActionResult> ResetPassword(int id, string newPassword, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 8)
        {
            TempData["ErrorMessage"] = "Password must be at least 8 characters.";
            return RedirectToAction(nameof(Edit), new { id });
        }

        await _userSvc.ResetPasswordAsync(id, newPassword, ct);
        TempData["SuccessMessage"] = "Password reset. Employee must change password on next login.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [Authorize(Policy = "PlatformSuperAdmin")]
    public async Task<IActionResult> AssignStores(int id, CancellationToken ct)
    {
        var user = await _userSvc.GetUserAsync(new InputUserSelect { IdUser = id }, ct);
        if (user is null) return NotFound();

        ViewBag.User = user;
        ViewBag.AllStores = (await _companySvc.ListStoresAsync(new InputStoreList(), ct)).Stores;
        return View(user);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "PlatformSuperAdmin")]
    public async Task<IActionResult> AssignStores(int id, List<int> storeIds, CancellationToken ct)
    {
        await _userSvc.AssignStoresToUserAsync(id, storeIds, ct);
        TempData["SuccessMessage"] = "Store access updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "PlatformSuperAdmin")]
    public async Task<IActionResult> AssignRole(int id, int roleId, CancellationToken ct)
    {
        await _userSvc.AssignRoleToUserAsync(id, roleId, ct);
        TempData["SuccessMessage"] = "Role assigned.";
        return RedirectToAction(nameof(Index));
    }
}
