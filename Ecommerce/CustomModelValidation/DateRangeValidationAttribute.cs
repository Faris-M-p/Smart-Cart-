using System.ComponentModel.DataAnnotations;

namespace Ecommerce.CustomModelValidation
{
    public class DateRangeValidationAttribute : ValidationAttribute
    {
        private readonly string _toDatePropertyName;

        public DateRangeValidationAttribute(string toDatePropertyName)
        {
            _toDatePropertyName = toDatePropertyName;
            ErrorMessage = "From date must be earlier than or equal to To date."; // Default message
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            // Get the value of the "To Date" property
            var toDateProperty = validationContext.ObjectType.GetProperty(_toDatePropertyName);
            if (toDateProperty == null)
            {
                return new ValidationResult($"Property '{_toDatePropertyName}' not found.");
            }

            var fromDate = value as DateTimeOffset?;
            var toDate = toDateProperty.GetValue(validationContext.ObjectInstance) as DateTimeOffset?;

            // If either date is not set, no validation is performed
            if (!fromDate.HasValue || !toDate.HasValue)
            {
                return ValidationResult.Success;
            }

            // Check if From Date is later than To Date
            if (fromDate > toDate)
            {
                // Use the custom message if provided, otherwise the default
                return new ValidationResult(ErrorMessage ?? "From date must be earlier than or equal to To date.");
            }

            return ValidationResult.Success;
        }
    }
}
