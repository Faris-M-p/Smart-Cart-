using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Models.Admin
{
    public class AdminAuthModel
    {
        public class AdminLoginRequest
        {
            [Required(ErrorMessage = "Username is required.")]
            public string UserName { get; set; } = string.Empty;

            [Required(ErrorMessage = "Password is required.")]
            public string Password { get; set; } = string.Empty;
        }

        public class AdminAuthEmployee
        {
            public int Id { get; set; }

            public string Name { get; set; } = string.Empty;

            public string UserName { get; set; } = string.Empty;
        }

        public class AdminAuthRole
        {
            public int Id { get; set; }

            public string Name { get; set; } = string.Empty;
        }

        public class AdminLoginData
        {
            public string AccessToken { get; set; } = string.Empty;

            public DateTime ExpiresAt { get; set; }

            public AdminAuthEmployee Employee { get; set; } = new();

            public AdminAuthRole Role { get; set; } = new();

            public List<string> Permissions { get; set; } = new();
        }

        public class AdminSessionData
        {
            public AdminAuthEmployee Employee { get; set; } = new();

            public AdminAuthRole Role { get; set; } = new();

            public List<string> Permissions { get; set; } = new();
        }

        public class AdminAuthOperationResult
        {
            public bool Success { get; set; }

            public int StatusCode { get; set; }

            public string Message { get; set; } = string.Empty;

            public AdminLoginData? Login { get; set; }

            public AdminSessionData? Session { get; set; }
        }
    }
}
