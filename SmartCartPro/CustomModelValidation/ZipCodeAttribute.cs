using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace SmartCartPro.CustomModelValidation
{
    public class ZipCodeAttribute:ValidationAttribute
    {
        private const string DefaultErrorMessage = "Invalid ZIP code format.";

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return ValidationResult.Success; 
            }
            if (value is string zipCode)
            {
                var pattern = @"^\d{5}(-\d{4})?$|^[A-Za-z]\d[A-Za-z][ -]?\d[A-Za-z]\d$|^[A-Za-z0-9][A-Za-z0-9 -]*$";
                TimeSpan regexTimeout = TimeSpan.FromMilliseconds(500);
                var regex = new Regex(pattern, RegexOptions.IgnoreCase, regexTimeout);
                if (!regex.IsMatch(zipCode))
                {
                    return new ValidationResult(ErrorMessage ?? DefaultErrorMessage);
                }

            }

            return ValidationResult.Success;
        }
    }
}
