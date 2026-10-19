using System.ComponentModel.DataAnnotations;

namespace SmartCartPro.CustomModelValidation
{
    public class EmailIfNotEmptyAttribute:ValidationAttribute
    {
        private const string DefaultErrorMessage = "Invalid email address format.";

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return ValidationResult.Success; 
            }

            if (value is string email)
            {
                var emailAttribute = new EmailAddressAttribute();

                if (!emailAttribute.IsValid(email))
                {
                    var displayName = validationContext.DisplayName ?? validationContext.MemberName;
                    return new ValidationResult(ErrorMessage ?? $"{displayName} is not a valid email address.");
                }
            }
            return ValidationResult.Success;
        }
    }
}
