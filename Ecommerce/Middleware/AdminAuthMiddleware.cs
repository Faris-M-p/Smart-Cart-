using System.Security.Claims;
using Ecommerce.Helpers.AdminAuth;
using Ecommerce.Interface.Admin;
using System.Text.Json;

namespace Ecommerce.Middleware
{
    public class AdminAuthMiddleware
    {
        private readonly RequestDelegate _next;

        public AdminAuthMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IAdminAuthInterface adminAuth)
        {
            var path = context.Request.Path;
            if (!IsAdminPath(path))
            {
                await _next(context);
                return;
            }

            var hasValidSession = await HasValidEmployeeSessionAsync(context, adminAuth);

            if (IsAnonymousAdminPath(path))
            {
                if (IsLoginPage(path) && hasValidSession && IsHtmlNavigation(context.Request))
                {
                    context.Response.Redirect("/Admin/Dashboard");
                    return;
                }

                await _next(context);
                return;
            }

            if (hasValidSession)
            {
                await _next(context);
                return;
            }

            RejectInvalidSession(context);

            if (IsHtmlNavigation(context.Request))
            {
                context.Response.Redirect("/admin/login");
                return;
            }

            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                success = false,
                message = AdminAuthHelper.UnauthorizedMessage
            }));
        }

        private static async Task<bool> HasValidEmployeeSessionAsync(HttpContext context, IAdminAuthInterface adminAuth)
        {
            if (context.User.Identity?.IsAuthenticated != true)
            {
                return false;
            }

            var employeeIdValue = context.User.FindFirst(AdminAuthHelper.ClaimEmployeeId)?.Value
                ?? context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(employeeIdValue, out var employeeId) || employeeId <= 0)
            {
                return false;
            }

            var current = await adminAuth.GetCurrentAsync(employeeId);
            return current.Success && current.Session != null;
        }

        private static void RejectInvalidSession(HttpContext context)
        {
            if (context.Request.Cookies.ContainsKey(AdminAuthHelper.TokenCookieName))
            {
                context.Response.Cookies.Delete(AdminAuthHelper.TokenCookieName, new CookieOptions
                {
                    Path = "/"
                });
            }
        }

        private static bool IsAdminPath(PathString path)
        {
            var value = path.Value ?? string.Empty;
            return value.StartsWith("/admin", StringComparison.OrdinalIgnoreCase)
                || value.StartsWith("/api/admin", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsAnonymousAdminPath(PathString path)
        {
            var value = path.Value?.TrimEnd('/') ?? string.Empty;
            return value.Equals("/admin/login", StringComparison.OrdinalIgnoreCase)
                || value.Equals("/api/admin/auth/login", StringComparison.OrdinalIgnoreCase)
                || value.Equals("/api/admin/auth/logout", StringComparison.OrdinalIgnoreCase)
                || value.Equals("/api/admin/auth/me", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsLoginPage(PathString path)
        {
            var value = path.Value?.TrimEnd('/') ?? string.Empty;
            return value.Equals("/admin/login", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsHtmlNavigation(HttpRequest request)
        {
            if (!HttpMethods.IsGet(request.Method) && !HttpMethods.IsHead(request.Method))
            {
                return false;
            }

            var accept = request.Headers.Accept.ToString();
            return string.IsNullOrWhiteSpace(accept) || accept.Contains("text/html", StringComparison.OrdinalIgnoreCase);
        }
    }
}
