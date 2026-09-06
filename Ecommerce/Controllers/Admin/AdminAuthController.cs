using System.Security.Claims;
using Ecommerce.Helpers.AdminAuth;
using Ecommerce.Interface.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Ecommerce.Models.Admin.AdminAuthModel;
using static Ecommerce.Models.CommonModel;

namespace Ecommerce.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/auth")]
    public class AdminAuthController : Controller
    {
        private readonly IAdminAuthInterface _adminAuth;

        public AdminAuthController(IAdminAuthInterface adminAuth)
        {
            _adminAuth = adminAuth;
        }

        [AllowAnonymous]
        [HttpGet("/admin/login")]
        public IActionResult LoginPage()
        {
            return View("~/Views/Admin/Login/Index.cshtml");
        }

        [HttpGet("/admin/access-denied")]
        public IActionResult AccessDenied()
        {
            return View("~/Views/Admin/AccessDenied/Index.cshtml");
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] AdminLoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = errors.FirstOrDefault() ?? "Validation failed",
                    Errors = errors
                });
            }

            var result = await _adminAuth.LoginAsync(request);
            if (!result.Success || result.Login == null)
            {
                return StatusCode(result.StatusCode, new ApiResponse<object>
                {
                    Success = false,
                    Message = result.Message
                });
            }

            SetAuthCookie(result.Login.AccessToken, result.Login.ExpiresAt);

            return Ok(new ApiResponse<AdminLoginData>
            {
                Success = true,
                Message = result.Message,
                Data = result.Login
            });
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            var employeeIdValue = User.FindFirst(AdminAuthHelper.ClaimEmployeeId)?.Value
                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(employeeIdValue, out var employeeId))
            {
                return Unauthorized(new ApiResponse<object>
                {
                    Success = false,
                    Message = AdminAuthHelper.UnauthorizedMessage
                });
            }

            var result = await _adminAuth.GetCurrentAsync(employeeId);
            if (!result.Success || result.Session == null)
            {
                if (result.StatusCode == StatusCodes.Status401Unauthorized)
                {
                    ClearAuthCookie();
                }

                return StatusCode(result.StatusCode, new ApiResponse<object>
                {
                    Success = false,
                    Message = result.Message
                });
            }

            return Ok(new ApiResponse<AdminSessionData>
            {
                Success = true,
                Message = result.Message,
                Data = result.Session
            });
        }

        [AllowAnonymous]
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            ClearAuthCookie();
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Logged out."
            });
        }

        private void SetAuthCookie(string token, DateTime expiresAtUtc)
        {
            Response.Cookies.Append(AdminAuthHelper.TokenCookieName, token, new CookieOptions
            {
                HttpOnly = false,
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Lax,
                Path = "/",
                Expires = new DateTimeOffset(DateTime.SpecifyKind(expiresAtUtc, DateTimeKind.Utc))
            });
        }

        private void ClearAuthCookie()
        {
            Response.Cookies.Delete(AdminAuthHelper.TokenCookieName, new CookieOptions
            {
                Path = "/"
            });
        }
    }
}
