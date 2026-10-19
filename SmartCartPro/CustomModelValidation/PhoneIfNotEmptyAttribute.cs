using System.ComponentModel.DataAnnotations;

namespace SmartCartPro.CustomModelValidation
{
    public class PhoneIfNotEmptyAttribute : ValidationAttribute
    {

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return ValidationResult.Success; 
            }

            if (value is string phone)
            {
                var phoneAttribute = new PhoneAttribute();

                if (!phoneAttribute.IsValid(phone))
                {
                    var displayName = validationContext.DisplayName ?? validationContext.MemberName;
                    return new ValidationResult(ErrorMessage ?? $"{displayName} is not a valid phone number.");
                }
            }

            return ValidationResult.Success;
        }
    }
}
