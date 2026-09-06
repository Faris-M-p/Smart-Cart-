using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Models
{
    public class UserAuthModel
    {
        public class StorefrontUser
        {
            public int UserId { get; set; }
            public string Email { get; set; } = string.Empty;
        }

        public class UserRegisterInput
        {
            [Required]
            [StringLength(150, MinimumLength = 2)]
            public string Name { get; set; } = string.Empty;

            [Required]
            [EmailAddress]
            [StringLength(100)]
            public string Email { get; set; } = string.Empty;

            [Required]
            [StringLength(100, MinimumLength = 6)]
            public string Password { get; set; } = string.Empty;
        }

        public class UserLoginInput
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; } = string.Empty;

            [Required]
            public string Password { get; set; } = string.Empty;
        }

        public class UserAuthRow
        {
            public int UserId { get; set; }
            public string FullName { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string PasswordHash { get; set; } = string.Empty;
            public bool Cancelled { get; set; }
        }

        public class UserSessionData
        {
            public int UserId { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
        }

        public class UserLoginData
        {
            public string AccessToken { get; set; } = string.Empty;
            public DateTime ExpiresAt { get; set; }
            public UserSessionData User { get; set; } = new();
        }
    }
}
