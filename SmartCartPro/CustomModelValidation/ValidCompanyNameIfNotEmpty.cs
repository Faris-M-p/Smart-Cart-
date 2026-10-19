using System;
using System.ComponentModel.DataAnnotations;
namespace SmartCartPro.CustomModelValidation
{
    public class ValidCompanyNameIfNotEmpty :ValidationAttribute
    {
        public ValidCompanyNameIfNotEmpty(int minLength, int maxLength)
        {
            MinLength = minLength;
            MaxLength = maxLength;
        }

        public int MinLength { get; }
        public int MaxLength { get; }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            // Allow null or empty/whitespace-only strings
            if (value == null || (value is string s && string.IsNullOrWhiteSpace(s)))
            {
                return ValidationResult.Success;
            }

            if (value is string strValue)
            {
                // Trim the string
                string trimmedValue = strValue.Trim();

                // Set the trimmed value back to the model property
                var property = validationContext.ObjectType.GetProperty(validationContext.MemberName);
                if (property != null && property.CanWrite)
                {
                    property.SetValue(validationContext.ObjectInstance, trimmedValue);
                }

                // Check length constraints
                if (trimmedValue.Length < MinLength || trimmedValue.Length > MaxLength)
                {
                    return new ValidationResult(ErrorMessage ??
                        $"{validationContext.DisplayName} must be between {MinLength} and {MaxLength} characters.");
                }
            }

            return ValidationResult.Success;
        }
    }
}
