using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCartPro.Exceptions;
using SmartCartPro.Filters;
using SmartCartPro.Helpers;
using SmartCartPro.Models;
using SmartCartPro.Services.Interfaces;

namespace SmartCartPro.Controllers;

/// <summary>Customer master CRUD + phone search JSON.</summary>
[Authorize]
[ModuleAccess("POS")]
public class CustomersController : Controller
{
    private readonly ICustomerService _svc;
    private int ActorId => AuthClaims.GetUserId(User) ?? 0;

    public CustomersController(ICustomerService svc) => _svc = svc;

    [HttpGet]
    [RequirePermission("pos.view")]
    public async Task<IActionResult> Index([FromQuery] InputCustomerList input, CancellationToken ct)
    {
        input.Page = Math.Max(1, input.Page);
        return View(await _svc.ListCustomersAsync(input, ct));
    }

    [HttpGet]
    [RequirePermission("pos.manage")]
    public IActionResult Create() => View(new InCustomerCreateView { IsActive = true });

    [HttpPost, ValidateAntiForgeryToken]
    [RequirePermission("pos.manage")]
    public async Task<IActionResult> Create(InCustomerCreateView model, CancellationToken ct)
    {
        CommonHelpers.ValidateModelState(ModelState);

        var input = new InputCustomerCreate
        {
            CustomerCode = model.CustomerCode,
            FullName = model.FullName,
            Email = model.Email,
            Phone = model.Phone,
            IsActive = model.IsActive
        };
        var result = await _svc.CreateCustomerAsync(input, ct);
        if (!result.Ok)
            throw new UpdateFailedException("Unable to create customer.", result.ResponseMsg ?? "Unable to create customer.");

        return CommonHelpers.SuccessResponse("Customer created.", new { id = result.IdCustomer });
    }

    [HttpGet]
    [RequirePermission("pos.manage")]
    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        var item = await _svc.GetCustomerAsync(new InputCustomerSelect { IdCustomer = id }, ct);
        if (item is null) return NotFound();
        return View(item.ToUpdateView());
    }

    [HttpPost, ValidateAntiForgeryToken]
    [RequirePermission("pos.manage")]
    public async Task<IActionResult> Edit(int id, InCustomerUpdateView model, CancellationToken ct)
    {
        model.IdCustomer = id;
        CommonHelpers.ValidateModelState(ModelState);

        var input = new InputCustomerUpdate
        {
            IdCustomer = model.IdCustomer,
            CustomerCode = model.CustomerCode,
            FullName = model.FullName,
            Email = model.Email,
            Phone = model.Phone,
            IsActive = model.IsActive
        };
        var result = await _svc.UpdateCustomerAsync(input, ct);
        if (!result.Ok)
            throw new UpdateFailedException("Unable to update customer.", result.ResponseMsg ?? "Unable to update customer.");

        return CommonHelpers.SuccessResponse("Customer updated.", new { id = result.IdCustomer });
    }

    /// <summary>JSON search by phone for POS customer lookup.</summary>
    [HttpGet]
    public async Task<IActionResult> SearchByPhone(string phone, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(phone)) return BadRequest();
        var result = await _svc.FindByPhoneAsync(phone.Trim(), ct);
        if (result is null) return NotFound();
        return Json(new { result.IdCustomer, result.FullName, result.Phone, result.Email });
    }
}
