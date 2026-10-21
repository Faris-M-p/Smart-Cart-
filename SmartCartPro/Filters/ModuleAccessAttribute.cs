using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SmartCartPro.Helpers;
using SmartCartPro.Models;
using SmartCartPro.Services.Interfaces;

namespace SmartCartPro.Filters;

/// <summary>
/// Blocks access to the decorated action/controller unless at least one of the given modules is enabled
/// in <c>installation_modules</c> (e.g. <c>[ModuleAccess("CUSTOMERS", "POS")]</c> for a screen both modules use).
/// Platform super admin always bypasses.
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
    private readonly IModuleService _moduleService;

    public ModuleAccessFilter(string[] moduleCodes, IModuleService moduleService)
    {
        _moduleCodes = moduleCodes;
        _moduleService = moduleService;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // Platform super admin always has full access.
        if (AuthClaims.IsPlatformSuperAdminUser(context.HttpContext.User))
        {
            await next();
            return;
        }

        var enabled = await _moduleService.GetEnabledModuleCodesAsync(context.HttpContext.RequestAborted);
        if (!_moduleCodes.Any(code => enabled.Contains(code, StringComparer.OrdinalIgnoreCase)))
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
