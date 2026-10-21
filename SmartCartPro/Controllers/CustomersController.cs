using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCartPro.Filters;
using SmartCartPro.Helpers;
using SmartCartPro.Models;
using SmartCartPro.Services.Interfaces;

namespace SmartCartPro.Controllers;

/// <summary>Customer master used by POS, invoices, and online orders.</summary>
[Authorize]
[ModuleAccess("CUSTOMERS", "POS")]
public class CustomersController : Controller
{
    private readonly ICustomerService _svc;

    public CustomersController(ICustomerService svc) => _svc = svc;

    /// <summary>
    /// Lists shoppers with search, status, sort and paging for the customer master.
    /// Staff with customers.view open this from Customers; cashiers with POS access use the same list.
    /// </summary>
    [HttpGet]
    [RequirePermission("customers.view")]
    public async Task<IActionResult> Index([FromQuery] InputCustomerList input, CancellationToken ct)
        => View(await _svc.ListCustomersAsync(input, ct));

    /// <summary>
    /// Shows the form to register a shopper for checkout, invoices, and follow-up contact.
    /// Staff with customers.manage use it when adding a walk-in or online customer.
    /// </summary>
    [HttpGet]
    [RequirePermission("customers.manage")]
    public IActionResult Create(string? returnUrl = null)
    {
        ViewBag.ReturnUrl = SafeReturnUrl(returnUrl);
        return View(new InCustomerCreateView { IsActive = true });
    }

    /// <summary>
    /// Saves a new customer record linkable to sales tickets, invoices, and phone search.
    /// Staff with customers.manage submit the create customer form during or after a sale.
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    [RequirePermission("customers.manage")]
    public async Task<IActionResult> Create(InCustomerCreateView model, string? returnUrl, CancellationToken ct)
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

        var listUrl = SafeReturnUrl(returnUrl) ?? Url.Action(nameof(Index))!;
        return CommonHelpers.SuccessResponse(
            "Customer created.",
            new { id = result.IdCustomer, redirectUrl = listUrl });
    }

    /// <summary>
    /// Loads one customer for editing name, phone, email, code, or active status.
    /// Supervisors with customers.manage open this from the customer list to fix profile data.
    /// </summary>
    [HttpGet]
    [RequirePermission("customers.manage")]
    public async Task<IActionResult> Edit(int id, string? returnUrl, CancellationToken ct)
    {
        var item = await _svc.GetCustomerAsync(new InputCustomerSelect { IdCustomer = id }, ct);
        if (item is null) return NotFound();
        ViewBag.ReturnUrl = SafeReturnUrl(returnUrl);
        return View(item.ToUpdateView());
    }

    /// <summary>
    /// Persists customer profile updates used on future POS lookups, receipts, and invoices.
    /// Staff with customers.manage submit the edit customer form after correcting details.
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    [RequirePermission("customers.manage")]
    public async Task<IActionResult> Edit(int id, InCustomerUpdateView model, string? returnUrl, CancellationToken ct)
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

        var listUrl = SafeReturnUrl(returnUrl) ?? Url.Action(nameof(Index))!;
        return CommonHelpers.SuccessResponse(
            "Customer updated.",
            new { id = result.IdCustomer, redirectUrl = listUrl });
    }

    /// <summary>
    /// Activates or deactivates a customer from the list without opening the edit form.
    /// Inactive customers stay on existing invoices but are hidden from POS search.
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    [RequirePermission("customers.manage")]
    public async Task<IActionResult> SetActive(int id, bool isActive, CancellationToken ct)
    {
        var input = new InputCustomerSetActive { IdCustomer = id, IsActive = isActive };
        var result = await _svc.SetCustomerActiveAsync(input, ct);

        return CommonHelpers.SuccessResponse(
            isActive ? "Customer activated." : "Customer deactivated.",
            new { id = result.IdCustomer, isActive = result.IsActive });
    }

    /// <summary>
    /// Looks up active customers by phone, name, or code for POS and customer pickers.
    /// Cashiers with pos.sell and staff with customers.view share this JSON search.
    /// </summary>
    [HttpGet]
    [RequirePermission("customers.view", "pos.sell")]
    public async Task<IActionResult> Search(string? term, int take = 10, CancellationToken ct = default)
    {
        var input = new InputCustomerSearch { Term = term, Take = take };
        var items = await _svc.SearchCustomersAsync(input, ct);
        return CommonHelpers.SuccessResponse("Customers loaded.", items);
    }

    /// <summary>
    /// Creates an active walk-in customer from POS with a generated code when none is supplied.
    /// Cashiers with pos.sell and staff with customers.manage submit name, phone, and email.
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    [RequirePermission("customers.manage", "pos.sell")]
    public async Task<IActionResult> QuickCreate(InCustomerQuickCreateView model, CancellationToken ct)
    {
        CommonHelpers.ValidateModelState(ModelState);

        var input = new InputCustomerQuickCreate
        {
            FullName = model.FullName,
            Phone = model.Phone,
            Email = model.Email
        };
        var result = await _svc.QuickCreateCustomerAsync(input, ct);

        return CommonHelpers.SuccessResponse(
            "Customer created.",
            new
            {
                idCustomer = result.IdCustomer,
                customerCode = result.CustomerCode,
                fullName = result.FullName,
                phone = result.Phone,
                email = result.Email
            });
    }

    private string? SafeReturnUrl(string? returnUrl)
    {
        if (string.IsNullOrWhiteSpace(returnUrl))
            return null;
        if (!Url.IsLocalUrl(returnUrl))
            return null;
        return returnUrl;
    }
}
