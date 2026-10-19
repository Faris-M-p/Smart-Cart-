using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace SmartCartPro.CustomModelValidation
{
    public class PastOrCurrentUtcDateTimeAttribute : ValidationAttribute
    {
        private const string DefaultErrorMessage = "The {0} must be now or a past UTC date/time.";

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return ValidationResult.Success; // Allow null values
            }

            if (value is DateTime dateValue)
            {
                // Ensure the value is in UTC
                if (dateValue.Kind != DateTimeKind.Utc)
                {
                    return new ValidationResult($"{validationContext.DisplayName ?? validationContext.MemberName} must be a UTC DateTime.");
                }

                // ?? Capture current time once
                var currentUtc = DateTime.UtcNow;
                // (Optional) You can replace Debug.WriteLine with Serilog, NLog, log4net, or ILogger if you have a logging framework
                if (dateValue > DateTime.UtcNow)
                {
                    var displayName = validationContext.DisplayName ?? validationContext.MemberName;
                     return new ValidationResult(ErrorMessage ?? string.Format(DefaultErrorMessage, displayName));

                    //// ?? Build the message and assign it to ErrorMessage
                    //ErrorMessage = $"The {displayName} must be now or a past UTC date/time. " +
                    //               $"Input: {dateValue:o}, Current: {currentUtc:o}";

                    //return new ValidationResult(ErrorMessage);
                }
            }

            return ValidationResult.Success;
        }
    }
}