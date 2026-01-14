using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Ecommerce.CustomModelValidation
{
    public class NoScriptTagsAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is string strValue)
            {
                // Pattern to detect script tags and other potentially dangerous characters
                var pattern = @"<script\b[^<]*(?:(?!<\/script>)<[^<]*)*<\/script>|[<>]";

                if (Regex.IsMatch(strValue, pattern, RegexOptions.IgnoreCase))
                {
                    return new ValidationResult("Invalid characters detected.");
                }
            }
            return ValidationResult.Success;
        }
    }
}
