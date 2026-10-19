using System;
using System.ComponentModel.DataAnnotations;

namespace SmartCartPro.CustomModelValidation
{
    public class FutureOrCurrentUtcDateTimeAttribute : ValidationAttribute
    {
        private const string DefaultErrorMessage = "The {0} must be the current time or a future time (UTC).";

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
                return ValidationResult.Success; // Allow nulls; [Required] should handle if needed

            if (value is DateTime dateTimeValue)
            {
                // Ensure it's in UTC kind
                if (dateTimeValue.Kind != DateTimeKind.Utc)
                {
                    return new ValidationResult($"{validationContext.DisplayName ?? validationContext.MemberName} must be a UTC date.");
                }

                // Compare full datetime including time
                if (dateTimeValue < DateTime.UtcNow)
                {
                    var displayName = validationContext.DisplayName ?? validationContext.MemberName;
                    return new ValidationResult(ErrorMessage ?? string.Format(DefaultErrorMessage, displayName));
                }
            }

            return ValidationResult.Success;
        }
    }
}
