using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace SmartCartPro.CustomModelValidation
{

    //[MatchRegex(@"^\d{3}-\d{2}-\d{4}$", ErrorMessage = "SSN must be in the format XXX-XX-XXXX.")]
    public class MatchRegexAttribute : ValidationAttribute
    {
        private readonly string _pattern;

        public MatchRegexAttribute(string pattern)
        {
            _pattern = pattern;
            ErrorMessage = "Something went wrong/The value does not match the required format.";
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
            {
                return ValidationResult.Success;
            }
            return Regex.IsMatch(value.ToString(), _pattern)
                ? ValidationResult.Success
                : new ValidationResult(ErrorMessage);
        }
    }
}
