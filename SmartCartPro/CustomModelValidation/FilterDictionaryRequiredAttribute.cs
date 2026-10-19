using System.ComponentModel.DataAnnotations;

namespace SmartCartPro.CustomModelValidation
{
    public class FilterDictionaryRequiredAttribute: ValidationAttribute
    {
        private readonly HashSet<string> _allowedKeys;
        public FilterDictionaryRequiredAttribute(params string[] allowedKeys)
        {
            _allowedKeys = new HashSet<string>(allowedKeys);
        }
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is not Dictionary<string, string> filterDictionary)
            {
                return new ValidationResult($"Something went wrong/The {validationContext.DisplayName} must be a dictionary.");
            }
            if (!filterDictionary.Any())
            {
                return new ValidationResult($"Something went wrong/The {validationContext.DisplayName} must contain at least one key-value pair.");
            }
            // Check for keys that are not allowed
            var invalidKeys = filterDictionary.Keys.Except(_allowedKeys).ToList();
            if (invalidKeys.Any())
            {
                return new ValidationResult($"Something went wrong/ The {validationContext.DisplayName} contains invalid keys: {string.Join(", ", invalidKeys)}.");
            }
            // Check that at least one allowed key has a non-empty value
            bool hasNonEmptyValue = filterDictionary
                .Where(kvp => _allowedKeys.Contains(kvp.Key))
                .Any(kvp => !string.IsNullOrWhiteSpace(kvp.Value));
            if (!hasNonEmptyValue)
            {
                return new ValidationResult($"Something went wrong/The {validationContext.DisplayName} must have at least one allowed key with a non-empty value.");
            }
            return ValidationResult.Success;
        }

    }
}
