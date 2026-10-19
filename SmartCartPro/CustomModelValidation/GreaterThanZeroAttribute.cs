using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace SmartCartPro.CustomModelValidation
{
    public class GreaterThanZeroAttribute : ValidationAttribute
    {
        public GreaterThanZeroAttribute()
        {
            ErrorMessage = "{0} must be greater than zero.";
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            string displayName = validationContext.DisplayName ?? validationContext.MemberName;

            if (value == null || (value is string s && string.IsNullOrWhiteSpace(s)))
            {
                return new ValidationResult(string.Format(ErrorMessageString, displayName));
            }

            try
            {
                var number = Convert.ToDecimal(value, CultureInfo.InvariantCulture);
                if (number <= 0)
                {
                    return new ValidationResult(string.Format(ErrorMessageString, displayName));
                }
            }
            catch
            {
                return new ValidationResult(string.Format(ErrorMessageString, displayName));
            }

            return ValidationResult.Success;
        }
    }
}
