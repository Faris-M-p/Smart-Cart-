using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace SmartCartPro.CustomModelValidation
{
    public class DisallowCharactersAttribute : ValidationAttribute
    {
        private readonly char[] _disallowedChars;

        public DisallowCharactersAttribute(string disallowedChars)
        {
            _disallowedChars = disallowedChars.ToCharArray();
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
            {
                return ValidationResult.Success;
            }

            string input = value.ToString();
            if (_disallowedChars.Any(input.Contains))
            {
                return new ValidationResult(
                    ErrorMessage ?? $"The field {validationContext.DisplayName} contains invalid characters.",
                    new[] { validationContext.MemberName }
                );
            }

            return ValidationResult.Success;
        }
    }
}
