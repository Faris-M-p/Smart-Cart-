using System.ComponentModel.DataAnnotations;

namespace SmartCartPro.CustomModelValidation
{
    public class MaxPercentageAttribute : ValidationAttribute
    {
        private readonly decimal _maxValue;

        public MaxPercentageAttribute(double maxValue = 100) // Use double instead of decimal
        {
            _maxValue = (decimal)maxValue;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return ValidationResult.Success; // Allow null values
            }

            if (value is decimal decimalValue)
            {
                if (decimalValue > _maxValue)
                {
                    // Get the Display Name if available, otherwise fallback to the property name
                    string displayName = validationContext.DisplayName ?? validationContext.MemberName;

                    // Use provided error message or generate a default one
                    string errorMessage = ErrorMessage ?? $"{displayName} must not exceed {_maxValue}.";
                    return new ValidationResult(errorMessage);
                }
            }

            return ValidationResult.Success;
        }
    }
}

/*
 [Display(Name = "Expected Response")]
[MaxPercentage] // Default limit of 100
public decimal ExpectedResponse { get; set; }

[Display(Name = "Discount Percentage")]
[MaxPercentage(80, ErrorMessage = "Discount cannot be more than 80%")]
public decimal DiscountPercentage { get; set; }
 */