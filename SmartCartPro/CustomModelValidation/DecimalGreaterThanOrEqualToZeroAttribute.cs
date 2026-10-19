using System.ComponentModel.DataAnnotations;

namespace SmartCartPro.CustomModelValidation
{
    public class DecimalGreaterThanOrEqualToZeroAttribute : ValidationAttribute
    {
        private const string DefaultErrorMessage = "The {0} field must be greater than or equal to 0.";

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return ValidationResult.Success; // Allow null, use [Required] if needed.
            }

            if (value is decimal decimalValue)
            {
                if (decimalValue < 0)  // ALLOW Zero
                {
                    var displayName = validationContext.DisplayName ?? validationContext.MemberName;
                    return new ValidationResult(ErrorMessage ?? string.Format(DefaultErrorMessage, displayName));
                }
            }

            return ValidationResult.Success;
        }
    }
}
