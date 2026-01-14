using System.ComponentModel.DataAnnotations;

namespace Ecommerce.CustomModelValidation
{
    public class GreaterThanZeroAttribute : ValidationAttribute
    {
        public GreaterThanZeroAttribute()
        {
            ErrorMessage = "{0} must be greater than zero."; // Default error message with placeholder
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return ValidationResult.Success; // Let Required attribute handle null values
            }

            if (value is int intValue && intValue <= 0)
            {
                var displayName = validationContext.DisplayName ?? validationContext.MemberName;
                var errorMessage = string.Format(ErrorMessage ?? "{0} must be greater than zero.", displayName);
                return new ValidationResult(errorMessage);
            }

            if (value is long longValue && longValue <= 0)
            {
                var displayName = validationContext.DisplayName ?? validationContext.MemberName;
                var errorMessage = string.Format(ErrorMessage ?? "{0} must be greater than zero.", displayName);
                return new ValidationResult(errorMessage);
            }

            return ValidationResult.Success;
        }
    }
}
