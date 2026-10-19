using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace SmartCartPro.CustomModelValidation
{
    public class MobileNumberAttribute:ValidationAttribute
    {

        private static readonly Regex MobileRegex = new Regex(@"^\+?[6-9]\d{9,14}$", RegexOptions.Compiled);

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is string mobile && !string.IsNullOrWhiteSpace(mobile))
            {
                mobile = mobile.Trim(); // Trim spaces

                var displayName = validationContext.DisplayName ?? validationContext.MemberName;
                if (!MobileRegex.IsMatch(mobile))
                {
                    return new ValidationResult(ErrorMessage ?? $"{displayName} is not a valid mobile number.");
                }
                //if (IsSequential(mobile))
                //{
                //    return new ValidationResult(ErrorMessage ?? $"{displayName} cannot be a sequential pattern (e.g., 1234567890).");
                //}

                if (HasRepeatedPairs(mobile))
                {
                    return new ValidationResult(ErrorMessage ?? $"{displayName} cannot contain repeated pairs (e.g., 1212121212).");
                }

                if (IsPalindrome(mobile))
                {
                    return new ValidationResult(ErrorMessage ?? $"{displayName} cannot be a palindrome.");
                }
            }
            return ValidationResult.Success;
        }
        //// Check for sequential pattern (e.g., 1234567890 or 9876543210)
        //private bool IsSequential(string number)
        //{
        //    string ascending = "0123456789";
        //    string descending = "9876543210";
        //    return ascending.Contains(number) || descending.Contains(number);
        //}

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

        // Check if the number is a palindrome (reads the same forward and backward)
        private bool IsPalindrome(string number)
        {
            char[] reversed = number.ToCharArray();
            Array.Reverse(reversed);
            return number == new string(reversed);
        }
    }
}
