using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Configuration
{
    public class DatabaseSettings
    {
        public const string SectionName = "DatabaseSettings";

        [Required]
        public string ServerName { get; set; } = string.Empty;

        [Required]
        public string DatabaseName { get; set; } = string.Empty;

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        [Range(1, 600)]
        public int Timeout { get; set; } = 30;
    }
}
