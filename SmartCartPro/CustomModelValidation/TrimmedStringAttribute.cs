using System;
using System.ComponentModel.DataAnnotations;

namespace SmartCartPro.CustomModelValidation
{
    public class TrimmedStringAttribute : ValidationAttribute
    {
        public TrimmedStringAttribute(int minLength, int maxLength)
        {
            MinLength = minLength;
            MaxLength = maxLength;
        }

        public int MinLength { get; }
        public int MaxLength { get; }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null || (value is string s && string.IsNullOrWhiteSpace(s)))
            {
                // Skip validation for null or empty strings
                return ValidationResult.Success;
            }

            if (value is string strValue)
            {
                // Trim the value
                strValue = strValue.Trim();

                // Set the trimmed value back to the property
                var property = validationContext.ObjectType.GetProperty(validationContext.MemberName);
                if (property != null && property.CanWrite)
                {
                    property.SetValue(validationContext.ObjectInstance, strValue);
                }

                // Validate length
                if (strValue.Length < MinLength || strValue.Length > MaxLength)
                {
                    return new ValidationResult(ErrorMessage ?? $"{validationContext.DisplayName} must be between {MinLength} and {MaxLength} characters.");
                }
            }

            return ValidationResult.Success;
        }
    }
}

