using System.ComponentModel.DataAnnotations;

namespace SmartCartPro.CustomModelValidation
{
    public class PastOrTodayDateAttribute: ValidationAttribute
    {
        private const string DefaultErrorMessage = "The {0} must be today or a Past date.";

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return ValidationResult.Success; // Allow null values
            }

            if (value is DateTime dateValue)
            {
                // Normalize to UTC date
                var inputDate = dateValue.ToUniversalTime().Date;
                var todayDate = DateTime.UtcNow.Date;

                if (inputDate > todayDate)
                {
                    var displayName = validationContext.DisplayName ?? validationContext.MemberName;
                     return new ValidationResult(ErrorMessage ?? string.Format(DefaultErrorMessage, displayName));

                    // ?? Build the message and assign it to ErrorMessage
                    //ErrorMessage = $"The {dateValue} must be now or a past UTC date time. " +
                    //               $"Input: {inputDate}, Current: {todayDate}";

                    //return new ValidationResult(ErrorMessage);
                }
            }

            return ValidationResult.Success;
        }
    }
}
