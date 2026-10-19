using System.ComponentModel.DataAnnotations;

namespace SmartCartPro.CustomModelValidation
{
    public class NotNegativeAttribute : ValidationAttribute
    {
        public NotNegativeAttribute()
        {
            ErrorMessage = "{0} must be selected.";
        }
        //[Display(Name = "Employee")]
        //[NotNegative(ErrorMessage = "{0} must be selected.")]
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            string displayName = validationContext.DisplayName ?? validationContext.MemberName;

            if (value == null)
                return ValidationResult.Success; // Not mandatory

            if (value is long longValue)
            {
                // Allow 0 ? means optional
                if (longValue == 0)
                    return ValidationResult.Success;

                // Disallow -1 & negative numbers
                if (longValue < 0)
                {
                    string message = string.Format(ErrorMessageString, displayName);
                    return new ValidationResult(message);
                }
            }

            return ValidationResult.Success;
        }
    }
}
