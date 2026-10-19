using System.ComponentModel.DataAnnotations;

namespace SmartCartPro.CustomModelValidation
{
    public class FilterDictionaryOptionalAttribute : ValidationAttribute
    {
        private readonly HashSet<string> _allowedKeys;

        public FilterDictionaryOptionalAttribute(params string[] allowedKeys)
        {
            _allowedKeys = new HashSet<string>(allowedKeys, StringComparer.OrdinalIgnoreCase);
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            // ? Allow NULL
            if (value == null)
                return ValidationResult.Success;

            if (value is not Dictionary<string, string> filterDictionary)
            {
                return new ValidationResult(
                    $"Something went wrong/The {validationContext.DisplayName} must be a dictionary.");
            }

            // ? Allow EMPTY dictionary
            if (!filterDictionary.Any())
                return ValidationResult.Success;

            // ? Check invalid keys
            var invalidKeys = filterDictionary.Keys
                .Where(k => !_allowedKeys.Contains(k))
                .ToList();

            if (invalidKeys.Any())
            {
                return new ValidationResult(
                    $"Something went wrong/The {validationContext.DisplayName} contains invalid keys: {string.Join(", ", invalidKeys)}.");
            }

            // ? No need to force non-empty values — optional filter
            return ValidationResult.Success;
        }
    }
}
