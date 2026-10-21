using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;
using SmartCartPro.Helpers;
using SmartCartPro.Models;
using SmartCartPro.Repositories.Interfaces;
using SmartCartPro.Services.Interfaces;

namespace SmartCartPro.Filters;

/// <summary>
/// Rejects requests whose JWT still validates after the user was deactivated or cancelled.
/// Cached per user for 30 seconds so list pages do not hit the users table on every action.
/// Anonymous endpoints are skipped. Register with <c>options.Filters.AddService&lt;ActiveUserFilter&gt;()</c>.
/// </summary>
public sealed class ActiveUserFilter : IAsyncActionFilter
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromSeconds(30);

    private readonly IUserService _userService;
    private readonly IAuthRepository _authRepository;
    private readonly IMemoryCache _cache;

    public ActiveUserFilter(IUserService userService, IAuthRepository authRepository, IMemoryCache cache)
    {
        _userService = userService;
        _authRepository = authRepository;
        _cache = cache;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (context.ActionDescriptor.EndpointMetadata.OfType<IAllowAnonymous>().Any())
        {
            await next();
            return;
        }

        var user = context.HttpContext.User;
        if (user.Identity?.IsAuthenticated != true)
        {
            await next();
            return;
        }

        var userId = AuthClaims.GetUserId(user);
        if (userId is > 0)
        {
            var storedVersion = await _authRepository.GetSessionVersionAsync(
                userId.Value, context.HttpContext.RequestAborted);
            if (storedVersion is null || storedVersion.Value != AuthClaims.GetSessionVersion(user))
            {
                EndSession(context, "You were signed in somewhere else. Please sign in again.");
                return;
            }
        }

        if (AuthClaims.IsPlatformSuperAdminUser(user))
        {
            await next();
            return;
        }

        if (userId is null or 0)
        {
            await next();
            return;
        }

        var cacheKey = "smartcart:user:active:" + userId.Value;
        if (!_cache.TryGetValue(cacheKey, out bool active))
        {
            var row = await _userService.GetUserAsync(
                new InputUserSelect
                {
                    IdUser = userId.Value,
                    ActorFkCompany = AuthClaims.GetFkCompany(user)
                },
                context.HttpContext.RequestAborted);
            active = row is { IsActive: true };
            _cache.Set(cacheKey, active, CacheTtl);
        }

        if (!active)
        {
            EndSession(context, "Your account is no longer active. Please sign in again.");
            return;
        }

        await next();
    }

    private static void EndSession(ActionExecutingContext context, string message)
    {
        context.HttpContext.Response.Cookies.Delete(
            AuthClaims.TokenCookieName,
            new CookieOptions { Path = "/", HttpOnly = true, SameSite = SameSiteMode.Lax });

        if (ExceptionErrorMapper.WantsJsonResponse(context.HttpContext.Request))
        {
            context.Result = new ObjectResult(CommonResponseModel.Failure(message, statusCode: 401))
            {
                StatusCode = StatusCodes.Status401Unauthorized
            };
            return;
        }

        context.Result = new RedirectToActionResult("Login", "Auth", new { returnUrl = "/" });
    }
}
