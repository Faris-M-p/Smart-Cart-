using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SmartCartPro.Helpers;
using SmartCartPro.Models;
using SmartCartPro.Services.Interfaces;

namespace SmartCartPro.Filters;

/// <summary>
/// Blocks the action unless at least one of the given features is enabled.
/// Used together with <see cref="ModuleAccessAttribute"/>, which still checks the parent module.
/// Several codes mean any of them. Platform super admin always passes.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public sealed class FeatureAccessAttribute : TypeFilterAttribute
{
    public FeatureAccessAttribute(params string[] featureCodes)
        : base(typeof(FeatureAccessFilter))
    {
        if (featureCodes is null || featureCodes.Length == 0)
            throw new ArgumentException("At least one feature code is required.", nameof(featureCodes));

        Arguments = new object[] { featureCodes };
    }
}

/// <summary>Inner filter — resolved by the DI container so it can depend on scoped services.</summary>
public sealed class FeatureAccessFilter : IAsyncActionFilter
{
    private readonly string[] _featureCodes;
    private readonly ICurrentUserAccess _access;

    public FeatureAccessFilter(string[] featureCodes, ICurrentUserAccess access)
    {
        _featureCodes = featureCodes;
        _access = access;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var ct = context.HttpContext.RequestAborted;
        var allowed = false;
        foreach (var code in _featureCodes)
        {
            if (await _access.FeatureEnabledAsync(code, ct))
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
                        "This feature is not enabled for your company.",
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
