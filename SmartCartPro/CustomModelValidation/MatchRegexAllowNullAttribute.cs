using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace SmartCartPro.CustomModelValidation
{
    public class MatchRegexAllowNullAttribute: ValidationAttribute
    {
        private readonly string _pattern;

        public MatchRegexAllowNullAttribute(string pattern)
        {
            _pattern = pattern;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
            {
                return ValidationResult.Success;
            }
            if (Regex.IsMatch(value.ToString(), _pattern))
            {
                return ValidationResult.Success;
            }
            return new ValidationResult(
            ErrorMessage ?? $"Something went wrong/The field {validationContext.DisplayName} is invalid.",
            new[] { validationContext.MemberName }  // Include the member name
        );
        }
    }
}
