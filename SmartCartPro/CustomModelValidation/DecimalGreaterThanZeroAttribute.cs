using System.ComponentModel.DataAnnotations;

namespace SmartCartPro.CustomModelValidation
{
    public class DecimalGreaterThanZeroAttribute : ValidationAttribute
    {
        private const string DefaultErrorMessage = "The {0} field must be greater than 0.";

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return ValidationResult.Success; // Allow null values; use [Required] for mandatory fields.
            }

            if (value is decimal decimalValue)
            {
                if (decimalValue <= 0)
                {
                    var displayName = validationContext.DisplayName ?? validationContext.MemberName;
                    return new ValidationResult(ErrorMessage ?? string.Format(DefaultErrorMessage, displayName));
                }
            }

            return ValidationResult.Success;
        }
    }
}