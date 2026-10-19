using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SmartCartPro.Helpers;
using SmartCartPro.Models;
using SmartCartPro.Services.Interfaces;

namespace SmartCartPro.Filters;

/// <summary>
/// Blocks access to the decorated action/controller if the given module is not enabled
/// in <c>installation_modules</c>. Platform super admin always bypasses.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public sealed class ModuleAccessAttribute : TypeFilterAttribute
{
    public ModuleAccessAttribute(string moduleCode)
        : base(typeof(ModuleAccessFilter))
    {
        Arguments = new object[] { moduleCode };
    }
}

/// <summary>Inner filter — resolved by the DI container so it can depend on scoped services.</summary>
public sealed class ModuleAccessFilter : IAsyncActionFilter
{
    private readonly string _moduleCode;
    private readonly IModuleService _moduleService;

    public ModuleAccessFilter(string moduleCode, IModuleService moduleService)
    {
        _moduleCode = moduleCode;
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
        if (!enabled.Contains(_moduleCode, StringComparer.OrdinalIgnoreCase))
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
