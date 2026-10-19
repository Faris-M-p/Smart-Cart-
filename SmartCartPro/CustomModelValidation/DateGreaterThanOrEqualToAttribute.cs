using System.ComponentModel.DataAnnotations;

namespace SmartCartPro.CustomModelValidation
{

    //[DateGreaterThanOrEqualTo("ValidFrom", ErrorMessage = "ValidTo must be greater than or equal to ValidFrom.")]
    //[DateGreaterThanOrEqualTo("QuotationFrom", ErrorMessage = "QuotationTo must be greater than or equal to QuotationFrom.")]
    public class DateGreaterThanOrEqualToAttribute : ValidationAttribute
    {
        public string ComparisonProperty { get; }

        public DateGreaterThanOrEqualToAttribute(string comparisonProperty)
        {
            ComparisonProperty = comparisonProperty;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var comparisonProperty = validationContext.ObjectType.GetProperty(ComparisonProperty);
            if (comparisonProperty == null)
            {
                return new ValidationResult($"Unknown property: {ComparisonProperty}");
            }
            var comparisonValue = comparisonProperty.GetValue(validationContext.ObjectInstance) as DateTime?;
            var currentValue = value as DateTime?;
            if (!currentValue.HasValue && !comparisonValue.HasValue)
            {
                return ValidationResult.Success;
            }
            if (!currentValue.HasValue || !comparisonValue.HasValue)
            {
                string missingField = !currentValue.HasValue ? validationContext.DisplayName : ComparisonProperty;
                var errorMessage = $"{missingField} is required.";
                return new ValidationResult(errorMessage);
            }
            if (currentValue.Value < comparisonValue.Value)
            {
                var errorMessage = ErrorMessage ?? $"{validationContext.DisplayName} must be greater than or equal to {ComparisonProperty}.";
                return new ValidationResult(errorMessage);
            }
            return ValidationResult.Success;
        }

    }
}
