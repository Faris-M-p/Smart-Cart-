using System.Security.Claims;
using System.Text.Json;
using Ecommerce.Helpers.AdminAuth;
using Ecommerce.Interface.Admin;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using static Ecommerce.Models.CommonModel;

namespace Ecommerce.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class RequirePermissionAttribute : Attribute, IAsyncActionFilter
    {
        public RequirePermissionAttribute(string permissionCode)
        {
            PermissionCode = permissionCode;
        }

        public string PermissionCode { get; }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var http = context.HttpContext;
            if (http.User.Identity?.IsAuthenticated != true)
            {
                context.Result = Deny(http, StatusCodes.Status401Unauthorized, AdminAuthHelper.UnauthorizedMessage);
                return;
            }

            var employeeIdValue = http.User.FindFirst(AdminAuthHelper.ClaimEmployeeId)?.Value
                ?? http.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(employeeIdValue, out var employeeId) || employeeId <= 0)
            {
                context.Result = Deny(http, StatusCodes.Status401Unauthorized, AdminAuthHelper.UnauthorizedMessage);
                return;
            }

            var adminAuth = http.RequestServices.GetRequiredService<IAdminAuthInterface>();
            var current = await adminAuth.GetCurrentAsync(employeeId);
            if (!current.Success || current.Session == null)
            {
                context.Result = Deny(http, current.StatusCode, current.Message);
                return;
            }

            var hasPermission = current.Session.Permissions.Any(code =>
                string.Equals(code, PermissionCode, StringComparison.OrdinalIgnoreCase));
            if (!hasPermission)
            {
                context.Result = Deny(http, StatusCodes.Status403Forbidden, AdminAuthHelper.ForbiddenMessage);
                return;
            }

            await next();
        }

        private static IActionResult Deny(HttpContext http, int statusCode, string message)
        {
            if (statusCode == StatusCodes.Status401Unauthorized && IsHtmlNavigation(http.Request))
            {
                return new RedirectResult("/admin/login");
            }

            if (statusCode == StatusCodes.Status403Forbidden && IsHtmlNavigation(http.Request))
            {
                return new RedirectResult("/admin/access-denied");
            }

            return new ContentResult
            {
                StatusCode = statusCode,
                ContentType = "application/json",
                Content = JsonSerializer.Serialize(new ApiResponse<object>
                {
                    Success = false,
                    Message = string.IsNullOrWhiteSpace(message)
                        ? AdminAuthHelper.ForbiddenMessage
                        : message
                }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })
            };
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
