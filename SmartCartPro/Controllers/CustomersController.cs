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

    /// <summary>
    /// Lists customer accounts used for loyalty, receipts, and repeat POS visits.
    /// Staff with pos.view browse this from the POS module customer master screen.
    /// </summary>
    [HttpGet]
    [RequirePermission("pos.view")]
    public async Task<IActionResult> Index([FromQuery] InputCustomerList input, CancellationToken ct)
    {
        input.Page = Math.Max(1, input.Page);
        return View(await _svc.ListCustomersAsync(input, ct));
    }

    /// <summary>
    /// Shows the form to register a shopper for checkout attachment and follow-up contact.
    /// Cashiers or supervisors with pos.manage use it when adding walk-in customers.
    /// </summary>
    [HttpGet]
    [RequirePermission("pos.manage")]
    public IActionResult Create() => View(new InCustomerCreateView { IsActive = true });

    /// <summary>
    /// Saves a new customer record linkable to sales tickets and phone search at register.
    /// Staff with pos.manage submit the create customer form during or after a sale.
    /// </summary>
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

    /// <summary>
    /// Loads one customer for editing name, phone, email, or active status.
    /// Supervisors with pos.manage open this from the customer list to fix profile data.
    /// </summary>
    [HttpGet]
    [RequirePermission("pos.manage")]
    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        var item = await _svc.GetCustomerAsync(new InputCustomerSelect { IdCustomer = id }, ct);
        if (item is null) return NotFound();
        return View(item.ToUpdateView());
    }

    /// <summary>
    /// Persists customer profile updates used on future POS lookups and receipts.
    /// Staff with pos.manage submit the edit customer form after correcting details.
    /// </summary>
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
