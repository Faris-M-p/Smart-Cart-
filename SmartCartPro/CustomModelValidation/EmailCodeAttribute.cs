using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace SmartCartPro.CustomModelValidation
{
    public class EmailCodeAttribute:ValidationAttribute
    {
        private const string DefaultErrorMessage = "The {0} must be a 6-character alphanumeric code.";

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is not string input || string.IsNullOrWhiteSpace(input))
            {
                return ValidationResult.Success; // Allow null/empty, [Required] should handle required case
            }

            var displayName = validationContext.DisplayName ?? validationContext.MemberName;

            // Regex for exactly 6 alphanumeric characters (letters and digits only)
            if (!Regex.IsMatch(input, @"^[A-Za-z0-9]{6}$"))
            {
                return new ValidationResult(ErrorMessage ?? string.Format(DefaultErrorMessage, displayName));
            }

            return ValidationResult.Success;
        }
    }
}
