using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace SmartCartPro.CustomModelValidation
{
    public class NoScriptTagsAttribute : ValidationAttribute
    {
        private static readonly Regex ScriptPattern = new(
            @"<script\b[^<]*(?:(?!</script>)<[^<]*)*</script>|[<>]",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public NoScriptTagsAttribute()
        {
            ErrorMessage = "Invalid characters detected.";
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null || (value is string empty && string.IsNullOrEmpty(empty)))
            {
                return ValidationResult.Success;
            }

            if (value is string strValue && ScriptPattern.IsMatch(strValue))
            {
                var displayName = validationContext.DisplayName ?? validationContext.MemberName;
                return new ValidationResult(string.Format(ErrorMessageString, displayName));
            }

            return ValidationResult.Success;
        }
    }
}
