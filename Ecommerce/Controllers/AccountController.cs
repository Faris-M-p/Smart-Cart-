using Ecommerce.Helpers.UserAuth;
using Ecommerce.Interface;
using Microsoft.AspNetCore.Mvc;
using static Ecommerce.Models.CommonModel;
using static Ecommerce.Models.UserAuthModel;

namespace Ecommerce.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserAuthInterface _userAuth;

        public AccountController(UserAuthInterface userAuth)
        {
            _userAuth = userAuth;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl)
        {
            ViewBag.Title = "Login";
            ViewBag.ReturnUrl = SafeReturnUrl(returnUrl);
            return View();
        }

        [HttpGet]
        public IActionResult Register(string? returnUrl)
        {
            ViewBag.Title = "Register";
            ViewBag.ReturnUrl = SafeReturnUrl(returnUrl);
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register([FromBody] UserRegisterInput input)
        {
            if (!ModelState.IsValid)
            {
                return Ok(new CommonResponse
                {
                    ResponseCode = -1,
                    StatusCode = false,
                    ResponseMsg = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).FirstOrDefault()
                        ?? "Please check the form."
                });
            }

            var result = await _userAuth.RegisterAsync(input);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromBody] UserLoginInput input)
        {
            if (!ModelState.IsValid)
            {
                return Ok(new CommonResponse
                {
                    ResponseCode = -1,
                    StatusCode = false,
                    ResponseMsg = UserAuthHelper.InvalidCredentialsMessage
                });
            }

            var (response, login) = await _userAuth.LoginAsync(input);
            if (!response.StatusCode || login == null)
            {
                return Ok(response);
            }

            SetAuthCookie(login.AccessToken, login.ExpiresAt);
            return Ok(new
            {
                response.ResponseCode,
                response.StatusCode,
                response.ResponseMsg,
                user = login.User
            });
        }

        [HttpGet]
        public async Task<IActionResult> Me()
        {
            if (!UserAuthHelper.TryGetUserId(User, out var userId))
            {
                return Unauthorized(new CommonResponse
                {
                    ResponseCode = -1,
                    StatusCode = false,
                    ResponseMsg = UserAuthHelper.UnauthorizedMessage
                });
            }

            var session = await _userAuth.GetCurrentAsync(userId);
            if (session == null)
            {
                ClearAuthCookie();
                return Unauthorized(new CommonResponse
                {
                    ResponseCode = -1,
                    StatusCode = false,
                    ResponseMsg = UserAuthHelper.UnauthorizedMessage
                });
            }

            return Ok(session);
        }

        [HttpPost]
        public IActionResult Logout()
        {
            ClearAuthCookie();
            return Ok(new CommonResponse
            {
                ResponseCode = 0,
                StatusCode = true,
                ResponseMsg = "Logged out."
            });
        }

        private void SetAuthCookie(string token, DateTime expiresAtUtc)
        {
            Response.Cookies.Append(UserAuthHelper.TokenCookieName, token, new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Lax,
                Path = "/",
                Expires = new DateTimeOffset(DateTime.SpecifyKind(expiresAtUtc, DateTimeKind.Utc))
            });
        }

        private void ClearAuthCookie()
        {
            Response.Cookies.Delete(UserAuthHelper.TokenCookieName, new CookieOptions
            {
                Path = "/"
            });
        }

        private static string SafeReturnUrl(string? returnUrl)
        {
            if (string.IsNullOrWhiteSpace(returnUrl) || !returnUrl.StartsWith('/') || returnUrl.StartsWith("//"))
            {
                return "/Shop";
            }

            return returnUrl;
        }
    }
}
