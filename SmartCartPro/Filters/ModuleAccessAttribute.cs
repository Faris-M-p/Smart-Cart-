using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SmartCartPro.Helpers;
using SmartCartPro.Models;
using SmartCartPro.Services.Interfaces;

namespace SmartCartPro.Filters;

/// <summary>
/// Blocks access to the decorated action/controller unless at least one of the given modules is enabled
/// in <c>installation_modules</c> (e.g. <c>[ModuleAccess("CUSTOMERS", "POS")]</c> for a screen both modules use).
/// Platform super admin always passes. The company Admin role also passes for Users and Settings.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public sealed class ModuleAccessAttribute : TypeFilterAttribute
{
    public ModuleAccessAttribute(params string[] moduleCodes)
        : base(typeof(ModuleAccessFilter))
    {
        if (moduleCodes is null || moduleCodes.Length == 0)
            throw new ArgumentException("At least one module code is required.", nameof(moduleCodes));

        Arguments = new object[] { moduleCodes };
    }
}

/// <summary>Inner filter — resolved by the DI container so it can depend on scoped services.</summary>
public sealed class ModuleAccessFilter : IAsyncActionFilter
{
    private readonly string[] _moduleCodes;
    private readonly ICurrentUserAccess _access;

    public ModuleAccessFilter(string[] moduleCodes, ICurrentUserAccess access)
    {
        _moduleCodes = moduleCodes;
        _access = access;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var ct = context.HttpContext.RequestAborted;
        var allowed = false;
        foreach (var code in _moduleCodes)
        {
            if (await _access.ModuleEnabledAsync(code, ct))
            {
                allowed = true;
                break;
            }
        }

        if (!allowed)
        {
            if (ExceptionErrorMapper.WantsJsonResponse(context.HttpContext.Request))
            {
                context.Result = new ObjectResult(
                    CommonResponseModel.Failure(
                        "This module is not enabled for your company.",
                        statusCode: 403))
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };
                return;
            }

            context.Result = new ForbidResult();
            return;
        }

        await next();
    }
}
