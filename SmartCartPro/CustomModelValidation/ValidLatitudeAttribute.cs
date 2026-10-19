using System.ComponentModel.DataAnnotations;

namespace SmartCartPro.CustomModelValidation
{
    public class ValidLatitudeAttribute : ValidationAttribute
    {
        public ValidLatitudeAttribute()
        {
            ErrorMessage = "{0} must be between -90 and 90.";
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            string displayName = validationContext.DisplayName ?? validationContext.MemberName;

            if (value == null)
                return ValidationResult.Success; // allow null if optional

            if (value is double lat && (lat < -90 || lat > 90))
            {
                string error = string.Format(ErrorMessageString, displayName);
                return new ValidationResult(error);
            }

            return ValidationResult.Success;
        }
    }
}