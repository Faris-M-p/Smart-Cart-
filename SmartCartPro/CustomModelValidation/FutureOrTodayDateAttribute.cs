using System.ComponentModel.DataAnnotations;

namespace SmartCartPro.CustomModelValidation
{
    public class FutureOrTodayDateAttribute: ValidationAttribute
    {
        private const string DefaultErrorMessage = "The {0} must be today or a future date.";

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return ValidationResult.Success; // Allow null values
            }

            if (value is DateTime dateValue)
            {
                // Compare with today's date
                if (dateValue.Date < DateTimeOffset.UtcNow.Date)
                {
                    var displayName = validationContext.DisplayName ?? validationContext.MemberName;
                    return new ValidationResult(ErrorMessage ?? string.Format(DefaultErrorMessage, displayName));
                }
            }

            return ValidationResult.Success;
        }
    }
}
