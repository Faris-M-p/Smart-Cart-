namespace Ecommerce.Helpers.AdminAuth
{
    public static class AdminAuthHelper
    {
        public const string TokenCookieName = "smartcart.admin.token";

        public const string ClaimEmployeeId = "EmployeeId";
        public const string ClaimUserName = "UserName";
        public const string ClaimUserRoleId = "UserRoleId";
        public const string ClaimUserRoleName = "UserRoleName";

        public const string InvalidCredentialsMessage = "Invalid username or password.";
        public const string InactiveAccountMessage = "This account is inactive.";
        public const string InvalidRoleMessage = "This account is not assigned a valid role.";
        public const string UnauthorizedMessage = "Unauthorized.";
        public const string ForbiddenMessage = "You do not have permission to perform this action.";
        public const string LoginSuccessMessage = "Login successful.";

        public static string NormalizeUserName(string? userName) =>
            (userName ?? string.Empty).Trim();
    }
}
