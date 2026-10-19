using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace SmartCartPro.CustomModelValidation
{
    public class ValidTitleAttribute : ValidationAttribute
    {
        private const string DefaultErrorMessage = "The {0} field contains invalid characters or is not a valid title.";

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return ValidationResult.Success; // Allow null or empty values
            }

            if (value is string title)
            {
                // Regex to allow letters, spaces, and periods
                var pattern = @"^[A-Za-z\s\.]+$";

                if (!Regex.IsMatch(title, pattern))
                {
                    var displayName = validationContext.DisplayName ?? validationContext.MemberName;
                    return new ValidationResult(ErrorMessage ?? string.Format(DefaultErrorMessage, displayName));
                }
            }

            return ValidationResult.Success;
        }
    }
}
