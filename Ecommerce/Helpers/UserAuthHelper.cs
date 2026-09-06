using System.Security.Claims;

namespace Ecommerce.Helpers.UserAuth
{
    public static class UserAuthHelper
    {
        public const string TokenCookieName = "smartcart.user.token";
        public const string ClaimUserId = "UserId";
        public const string ClaimEmail = "Email";
        public const string ClaimFullName = "FullName";

        public const string InvalidCredentialsMessage = "Invalid email or password.";
        public const string UnauthorizedMessage = "Please log in to continue.";
        public const string LoginSuccessMessage = "Login successful.";
        public const string RegisterSuccessMessage = "Account created. You can log in now.";

        public static bool TryGetUserId(ClaimsPrincipal? user, out int userId)
        {
            userId = 0;
            var value = user?.FindFirst(ClaimUserId)?.Value;
            return int.TryParse(value, out userId) && userId > 0;
        }

        public static string NormalizeEmail(string? email) =>
            (email ?? string.Empty).Trim().ToLowerInvariant();

        public static string NormalizeName(string? name) =>
            (name ?? string.Empty).Trim();
    }
}
