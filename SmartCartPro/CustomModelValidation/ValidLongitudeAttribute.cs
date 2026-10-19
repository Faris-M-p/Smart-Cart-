using System.ComponentModel.DataAnnotations;

namespace SmartCartPro.CustomModelValidation
{
    public class ValidLongitudeAttribute : ValidationAttribute
    {
        public ValidLongitudeAttribute()
        {
            ErrorMessage = "{0} must be between -180 and 180.";
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            string displayName = validationContext.DisplayName ?? validationContext.MemberName;

            if (value == null)
                return ValidationResult.Success;

            if (value is double lng && (lng < -180 || lng > 180))
            {
                string error = string.Format(ErrorMessageString, displayName);
                return new ValidationResult(error);
            }

            return ValidationResult.Success;
        }
    }
}