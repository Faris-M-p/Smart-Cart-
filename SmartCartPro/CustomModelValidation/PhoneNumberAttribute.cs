using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace SmartCartPro.CustomModelValidation
{
    public class PhoneNumberAttribute:ValidationAttribute
    {
        private static readonly Regex PhoneRegex = new Regex(@"^\+?(\d[\d- ]?){7,15}$", RegexOptions.Compiled);

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is string phone && !string.IsNullOrWhiteSpace(phone))
            {
                phone = phone.Trim(); // Trim spaces
                var displayName = validationContext.DisplayName ?? validationContext.MemberName;
                if (!PhoneRegex.IsMatch(phone))
                { 
                    return new ValidationResult(ErrorMessage ?? $"{displayName} is not a valid phone number.");
                }
                if (IsSequential(phone))
                {
                    return new ValidationResult(ErrorMessage ?? $"{displayName} cannot be a sequential pattern (e.g., 1234567890).");
                }

                if (HasRepeatedPairs(phone))
                {
                    return new ValidationResult(ErrorMessage ?? $"{displayName} cannot contain repeated pairs (e.g., 1212121212).");
                }
            }
            return ValidationResult.Success;
        }

        // Check for sequential pattern (e.g., 1234567890 or 9876543210)
        private bool IsSequential(string number)
        {
            string ascending = "0123456789";
            string descending = "9876543210";
            return ascending.Contains(number) || descending.Contains(number);
        }

        // Check for repeated pairs (e.g., 1212121212, 3434343434)
        private bool HasRepeatedPairs(string number)
        {
            for (int i = 0; i < number.Length - 2; i += 2)
            {
                string pair = number.Substring(i, 2);
                if (number.Replace(pair, "").Length == 0)
                    return true;
            }
            return false;
        }


    }
}
