using System.ComponentModel.DataAnnotations;

namespace Ecommerce.CustomModelValidation
{
    public class RequiredNotEmptyAttribute : ValidationAttribute
    {
        public RequiredNotEmptyAttribute()
        {
            ErrorMessage = "{0} is required and cannot be empty."; // Default error message with placeholder
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var displayName = validationContext.DisplayName ?? validationContext.MemberName;

            if (value == null)
            {
                var errorMessage = string.Format(ErrorMessage ?? "{0} is required.", displayName);
                return new ValidationResult(errorMessage);
            }

            if (value is string strValue && string.IsNullOrWhiteSpace(strValue))
            {
                var errorMessage = string.Format(ErrorMessage ?? "{0} cannot be empty.", displayName);
                return new ValidationResult(errorMessage);
            }

            return ValidationResult.Success;
        }
    }
}
