using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SmartCartPro.Helpers;
using SmartCartPro.Models;
using SmartCartPro.Services.Interfaces;

namespace SmartCartPro.Filters;

/// <summary>
/// Verifies that the signed-in user holds a permission code via their assigned role.
/// Several codes mean "any of" (e.g. <c>[RequirePermission("customers.view", "pos.sell")]</c> for a lookup that both the
/// customer screens and the POS screen use). Two separate attributes on one action mean "all of".
/// Platform super admin always passes. Uses <see cref="IUserService"/> for the DB lookup.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class RequirePermissionAttribute : TypeFilterAttribute
{
    public RequirePermissionAttribute(params string[] permissionCodes)
        : base(typeof(RequirePermissionFilter))
    {
        if (permissionCodes is null || permissionCodes.Length == 0)
            throw new ArgumentException("At least one permission code is required.", nameof(permissionCodes));

        Arguments = new object[] { permissionCodes };
    }
}

/// <summary>Inner filter — resolved by DI so it gets scoped services.</summary>
public sealed class RequirePermissionFilter : IAsyncActionFilter
{
    private readonly string[] _permissionCodes;
    private readonly IUserService _userService;

    public RequirePermissionFilter(string[] permissionCodes, IUserService userService)
    {
        _permissionCodes = permissionCodes;
        _userService = userService;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // Platform super admin bypasses all permission checks.
        if (AuthClaims.IsPlatformSuperAdminUser(context.HttpContext.User))
        {
            await next();
            return;
        }

        var userId = AuthClaims.GetUserId(context.HttpContext.User);
        if (userId is null or 0)
        {
            context.Result = JsonOrForbid(context, 401, "Your session has expired. Please sign in again.");
            return;
        }

        var permissions = await _userService.GetUserPermissionsAsync(
            userId.Value, context.HttpContext.RequestAborted);

        var hasPermission = permissions.Any(p =>
            _permissionCodes.Any(code => string.Equals(p.PermissionCode, code, StringComparison.OrdinalIgnoreCase)));

        if (!hasPermission)
        {
            context.Result = JsonOrForbid(context, 403, "You do not have permission to perform this action.");
            return;
        }

        await next();
    }

    private static IActionResult JsonOrForbid(ActionExecutingContext context, int statusCode, string message)
    {
        if (ExceptionErrorMapper.WantsJsonResponse(context.HttpContext.Request))
        {
            return new ObjectResult(CommonResponseModel.Failure(message, statusCode: statusCode))
            {
                StatusCode = statusCode
            };
        }

        return statusCode == 401
            ? new UnauthorizedResult()
            : new ForbidResult();
    }
}
