using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SmartCartPro.Helpers;
using SmartCartPro.Models;
using SmartCartPro.Services.Interfaces;

namespace SmartCartPro.Filters;

/// <summary>
/// Verifies that the signed-in user has the specified permission code via their assigned role.
/// Platform super admin always passes. Uses <see cref="IUserService"/> for DB lookup.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class RequirePermissionAttribute : TypeFilterAttribute
{
    public RequirePermissionAttribute(string permissionCode)
        : base(typeof(RequirePermissionFilter))
    {
        Arguments = new object[] { permissionCode };
    }
}

/// <summary>Inner filter — resolved by DI so it gets scoped services.</summary>
public sealed class RequirePermissionFilter : IAsyncActionFilter
{
    private readonly string _permissionCode;
    private readonly IUserService _userService;

    public RequirePermissionFilter(string permissionCode, IUserService userService)
    {
        _permissionCode = permissionCode;
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
            string.Equals(p.PermissionCode, _permissionCode, StringComparison.OrdinalIgnoreCase));

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
