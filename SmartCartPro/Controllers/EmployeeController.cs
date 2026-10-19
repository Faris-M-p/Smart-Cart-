using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCartPro.Exceptions;
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

    /// <summary>
    /// Lists employees (users) for the tenant.
    /// Used by authorized staff managing workforce accounts.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var model = await _userSvc.ListUsersAsync(new InputUserList(), ct);
        return View(model);
    }

    /// <summary>
    /// Renders the new-employee form with roles and stores.
    /// Platform super admins only.
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "PlatformSuperAdmin")]
    public async Task<IActionResult> Create(CancellationToken ct)
    {
        ViewBag.Roles = (await _roleSvc.ListRolesAsync(new InputRoleList(), ct)).Roles;
        ViewBag.Stores = (await _companySvc.ListStoresAsync(new InputStoreList(), ct)).Stores;
        return View(new InUserCreateView());
    }

    /// <summary>
    /// Creates an employee account with role and store access.
    /// Platform super admins submit the create form.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "PlatformSuperAdmin")]
    public async Task<IActionResult> Create(InUserCreateView model, CancellationToken ct)
    {
        CommonHelpers.ValidateModelState(ModelState);

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
            throw new UpdateFailedException("Unable to create employee.", result.ResponseMsg ?? "Unable to create employee.");

        return CommonHelpers.SuccessResponse(
            $"Employee '{result.UserName}' created.",
            new { id = result.IdUser, userName = result.UserName });
    }

    /// <summary>
    /// Loads employee profile fields into the edit form.
    /// Used by authorized users with employee management access.
    /// </summary>
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

    /// <summary>
    /// Updates employee profile and active status.
    /// Used by authorized users with employee management access.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, InUserUpdateView model, CancellationToken ct)
    {
        model.IdUser = id;
        CommonHelpers.ValidateModelState(ModelState);

        var input = new InputUserUpdate
        {
            ActorUserId = ActorId,
            IdUser = model.IdUser,
            FullName = model.FullName,
            Phone = model.Phone,
            EmployeeCode = model.EmployeeCode,
            IsActive = model.IsActive
        };
        var result = await _userSvc.UpdateUserAsync(input, ct);
        if (!result.Ok)
            throw new UpdateFailedException("Unable to update employee.", result.ResponseMsg ?? "Unable to update employee.");

        return CommonHelpers.SuccessResponse("Employee updated.", new { id });
    }

    /// <summary>
    /// Deactivates an employee so they cannot sign in.
    /// Platform super admins from employee administration.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "PlatformSuperAdmin")]
    public async Task<IActionResult> Deactivate(int id, CancellationToken ct)
    {
        await _userSvc.DeactivateUserAsync(id, ct);
        return CommonHelpers.SuccessResponse("Employee deactivated.", new { id });
    }

    /// <summary>
    /// Sets a new password and forces change on next login.
    /// Platform super admins assisting locked-out employees.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "PlatformSuperAdmin")]
    public async Task<IActionResult> ResetPassword(int id, string newPassword, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 8)
            throw new ValidationException("Password must be at least 8 characters.", "Password too short.");

        await _userSvc.ResetPasswordAsync(id, newPassword, ct);
        return CommonHelpers.SuccessResponse("Password reset. Employee must change password on next login.", new { id });
    }

    /// <summary>
    /// Shows store checkboxes for an employee's access.
    /// Platform super admins configure multi-store permissions.
    /// </summary>
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

    /// <summary>
    /// Replaces the employee's allowed store list.
    /// Platform super admins submit the assign-stores form.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "PlatformSuperAdmin")]
    public async Task<IActionResult> AssignStores(int id, List<int> storeIds, CancellationToken ct)
    {
        await _userSvc.AssignStoresToUserAsync(id, storeIds, ct);
        return CommonHelpers.SuccessResponse("Store access updated.", new { id });
    }

    /// <summary>
    /// Assigns a single role to the employee.
    /// Platform super admins from employee administration actions.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "PlatformSuperAdmin")]
    public async Task<IActionResult> AssignRole(int id, int roleId, CancellationToken ct)
    {
        await _userSvc.AssignRoleToUserAsync(id, roleId, ct);
        return CommonHelpers.SuccessResponse("Role assigned.", new { id, roleId });
    }
}
