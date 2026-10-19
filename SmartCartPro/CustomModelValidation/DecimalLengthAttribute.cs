
using System.ComponentModel.DataAnnotations;

namespace SmartCartPro.CustomModelValidation
{
    public class DecimalLengthAttribute : ValidationAttribute
    {
        private const string DefaultErrorMessage = "The {0} field must have at most 10 digits before the decimal and up to 4 digits after the decimal.";
        private readonly int _maxIntegerDigits;
        private readonly int _maxDecimalDigits;

        public DecimalLengthAttribute(int maxIntegerDigits, int maxDecimalDigits)
        {
            _maxIntegerDigits = maxIntegerDigits;
            _maxDecimalDigits = maxDecimalDigits;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return ValidationResult.Success; // Allow null values
            }

            if (value is decimal decimalValue)
            {
                // Split the number into integer and fractional parts
                var parts = decimalValue.ToString("G29").Split('.'); // "G29" removes trailing zeros
                var integerPart = parts[0];
                var decimalPart = parts.Length > 1 ? parts[1] : string.Empty;

                if (integerPart.Length > _maxIntegerDigits || decimalPart.Length > _maxDecimalDigits)
                {
                    var displayName = validationContext.DisplayName ?? validationContext.MemberName;
                    return new ValidationResult(ErrorMessage ?? string.Format(DefaultErrorMessage, displayName));
                }
            }

            return ValidationResult.Success;
        }
    }
}

