using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCartPro.Filters;
using SmartCartPro.Helpers;
using SmartCartPro.Models;
using SmartCartPro.Services.Interfaces;

namespace SmartCartPro.Controllers;

/// <summary>
/// Employee accounts: list, create, edit, activate/deactivate and password reset (views in ~/Views/Employee/).
/// ModuleAccess: USERS | Permission: users.manage. Platform super admin accounts are never listed or editable here.
/// </summary>
[Authorize]
[ModuleAccess("USERS")]
[RequirePermission("users.manage")]
public class EmployeeController : Controller
{
    private readonly IUserService _userSvc;

    public EmployeeController(IUserService userSvc) => _userSvc = userSvc;

    private int ActorId => AuthClaims.GetUserId(User) ?? 0;

    private int? ActorCompanyId => AuthClaims.GetFkCompany(User);

    /// <summary>
    /// Lists employees with search, role, store and status filters, sorting and paging.
    /// Company administrators open this from the Administration menu to manage staff accounts.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] InputUserList input, CancellationToken ct)
    {
        var model = await _userSvc.ListUsersAsync(input, ct);
        await LoadOptionsAsync(ct);
        ViewBag.CurrentUserId = ActorId;
        return View(model);
    }

    /// <summary>
    /// Shows the new-employee form with the role and store pickers and the password options.
    /// Administrators open it from the employee list before creating a staff account.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Create(string? returnUrl, CancellationToken ct)
    {
        await LoadOptionsAsync(ct);
        ViewBag.ReturnUrl = SafeReturnUrl(returnUrl);
        return View(new InUserCreateView());
    }

    /// <summary>
    /// Creates the employee with one role, store access and an initial or generated password.
    /// A generated temporary password is returned once in the response and must be changed at first sign-in.
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(InUserCreateView model, string? returnUrl, CancellationToken ct)
    {
        CommonHelpers.ValidateModelState(ModelState);

        var input = new InputUserCreate
        {
            ActorUserId = ActorId,
            ActorFkCompany = ActorCompanyId,
            UserName = model.UserName,
            Email = model.Email,
            FullName = model.FullName,
            Phone = model.Phone,
            EmployeeCode = model.EmployeeCode,
            FkRole = model.FkRole ?? 0,
            StoreIds = model.StoreIds,
            DefaultStoreId = model.DefaultStoreId,
            GeneratePassword = model.GeneratePassword,
            InitialPassword = model.InitialPassword,
            MustChangePassword = model.MustChangePassword
        };
        var result = await _userSvc.CreateUserAsync(input, ct);

        // The response can carry a temporary password: never let a browser or proxy cache it.
        Response.Headers.CacheControl = "no-store";

        var listUrl = SafeReturnUrl(returnUrl) ?? Url.Action(nameof(Index))!;
        var message = result.TemporaryPassword is null
            ? "Employee created successfully."
            : "Employee created. Copy the temporary password now, it is shown only once.";
        return CommonHelpers.SuccessResponse(
            message,
            new
            {
                id = result.IdUser,
                userName = result.UserName,
                temporaryPassword = result.TemporaryPassword,
                redirectUrl = listUrl
            });
    }

    /// <summary>
    /// Loads one employee into the edit form with the role and store pickers and account details.
    /// Administrators open it from the employee list to change a profile, role or store access.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Edit(int id, string? returnUrl, CancellationToken ct)
    {
        var selectInput = new InputUserSelect { IdUser = id };
        var item = await _userSvc.GetUserAsync(selectInput, ct);
        if (item is null) return NotFound();

        await LoadOptionsAsync(ct);
        ViewBag.ReturnUrl = SafeReturnUrl(returnUrl);
        ViewBag.CurrentUserId = ActorId;
        return View(ToUpdateView(item));
    }

    /// <summary>
    /// Saves profile, role and store access changes for an employee.
    /// Administrators submit the edit form; changing your own role is refused by the service.
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, InUserUpdateView model, string? returnUrl, CancellationToken ct)
    {
        model.IdUser = id;
        ModelState.Remove(nameof(model.IdUser));
        CommonHelpers.ValidateModelState(ModelState);

        var input = new InputUserUpdate
        {
            ActorUserId = ActorId,
            ActorFkCompany = ActorCompanyId,
            IdUser = model.IdUser,
            Email = model.Email,
            FullName = model.FullName,
            Phone = model.Phone,
            EmployeeCode = model.EmployeeCode,
            FkRole = model.FkRole ?? 0,
            StoreIds = model.StoreIds,
            DefaultStoreId = model.DefaultStoreId
        };
        var result = await _userSvc.UpdateUserAsync(input, ct);

        var listUrl = SafeReturnUrl(returnUrl) ?? Url.Action(nameof(Index))!;
        return CommonHelpers.SuccessResponse(
            "Employee updated successfully.",
            new { id = result.IdUser, redirectUrl = listUrl });
    }

    /// <summary>
    /// Activates or deactivates an employee without opening the edit form; inactive accounts cannot sign in.
    /// Administrators use it from the list or edit page after confirming. You cannot deactivate yourself.
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SetActive(int id, bool isActive, CancellationToken ct)
    {
        var input = new InputUserStatusUpdate
        {
            ActorUserId = ActorId,
            IdUser = id,
            IsActive = isActive
        };
        var result = await _userSvc.UpdateUserStatusAsync(input, ct);

        return CommonHelpers.SuccessResponse(
            result.IsActive ? "Employee activated." : "Employee deactivated.",
            new { id = result.IdUser, isActive = result.IsActive });
    }

    /// <summary>
    /// Sets or generates a temporary password for an employee who must change it at next sign-in.
    /// Administrators submit it from the edit page; a generated password is returned once and never stored in plain text.
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(int id, InUserPasswordUpdateView model, CancellationToken ct)
    {
        model.IdUser = id;
        ModelState.Remove(nameof(model.IdUser));
        CommonHelpers.ValidateModelState(ModelState);

        var input = new InputUserPasswordUpdate
        {
            ActorUserId = ActorId,
            IdUser = model.IdUser,
            GeneratePassword = model.GeneratePassword,
            NewPassword = model.NewPassword
        };
        var result = await _userSvc.UpdateUserPasswordAsync(input, ct);

        // The response can carry a temporary password: never let a browser or proxy cache it.
        Response.Headers.CacheControl = "no-store";

        var message = result.TemporaryPassword is null
            ? "Password updated. The employee must change it at next sign-in."
            : "Temporary password generated. Copy it now, it is shown only once.";
        return CommonHelpers.SuccessResponse(
            message,
            new { id = result.IdUser, temporaryPassword = result.TemporaryPassword });
    }

    // ------------------------------------------------------------------ helpers

    /// <summary>Role and store pickers (employee form and list filters), scoped to the actor's company.</summary>
    private async Task LoadOptionsAsync(CancellationToken ct)
    {
        var optionInput = new InputUserOptionList { FkCompany = ActorCompanyId };
        var options = await _userSvc.ListUserOptionsAsync(optionInput, ct);
        ViewBag.Roles = options.Roles;
        ViewBag.Stores = options.Stores;
    }

    private static InUserUpdateView ToUpdateView(OutputUserSelect item) => new()
    {
        IdUser = item.IdUser,
        Email = item.Email,
        FullName = item.FullName,
        Phone = item.Phone,
        EmployeeCode = item.EmployeeCode,
        FkRole = item.CurrentRoleId,
        StoreIds = item.Stores.Select(s => s.IdStore).ToList(),
        DefaultStoreId = item.Stores.FirstOrDefault(s => s.IsDefault)?.IdStore,
        UserName = item.UserName,
        IsActive = item.IsActive,
        MustChangePassword = item.MustChangePassword,
        LastLoginAt = item.LastLoginAt
    };

    private string? SafeReturnUrl(string? returnUrl)
    {
        if (string.IsNullOrWhiteSpace(returnUrl)) return null;
        if (!Url.IsLocalUrl(returnUrl)) return null;
        return returnUrl;
    }
}
