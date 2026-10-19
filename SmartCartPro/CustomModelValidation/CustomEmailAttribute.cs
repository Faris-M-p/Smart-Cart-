using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace SmartCartPro.CustomModelValidation
{
    public class CustomEmailAttribute:ValidationAttribute
    {
        private static readonly Regex EmailRegex = new Regex(@"^[^\s@]+@[^\s@]+\.[^\s@]+$", RegexOptions.Compiled);

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is string email)
            {
                email = email.Trim(); // Trim spaces

                if (email.Contains(" "))
                {
                    return new ValidationResult("Invalid email format");
                }

                if (!EmailRegex.IsMatch(email))
                {
                    return new ValidationResult("Invalid email format.");
                }
            }
            return ValidationResult.Success;
        }
    }
}
