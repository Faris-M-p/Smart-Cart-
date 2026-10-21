using Microsoft.AspNetCore.Mvc;
using SmartCartPro.Helpers;
using SmartCartPro.Models;
using SmartCartPro.Services.Interfaces;

namespace SmartCartPro.ViewComponents;

/// <summary>
/// Header store picker for users with more than one accessible store.
/// Platform Super Admin and single-store users see nothing.
/// </summary>
public sealed class StoreSwitcherViewComponent : ViewComponent
{
    private readonly ICompanyService _svc;

    public StoreSwitcherViewComponent(ICompanyService svc) => _svc = svc;

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var user = HttpContext?.User;
        if (AuthClaims.IsPlatformSuperAdminUser(user))
        {
            return Content(string.Empty);
        }

        var userId = AuthClaims.GetUserId(user) ?? 0;
        if (userId <= 0)
        {
            return Content(string.Empty);
        }

        var input = new InputStoreAccessibleList { IdUser = userId };
        var list = await _svc.ListAccessibleStoresAsync(input, HttpContext!.RequestAborted);
        if (list.Stores.Count == 0)
        {
            return Content(string.Empty);
        }

        list.CurrentFkStore = AuthClaims.GetFkStore(user) ?? 0;
        return View(list);
    }
}
