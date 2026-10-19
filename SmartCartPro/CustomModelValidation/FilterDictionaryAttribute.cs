using System.ComponentModel.DataAnnotations;

namespace SmartCartPro.CustomModelValidation
{
    //[Display(Name = "Filter")]
    //[FilterDictionary("FilterAssignee", "FilterTags", "FilterPriority")]
    //public Dictionary<string, string> Filter { get; set; } = new();


    public class FilterDictionaryAttribute : ValidationAttribute
    {
        private readonly HashSet<string> _requiredKeys;

        public FilterDictionaryAttribute(params string[] requiredKeys)
        {
            _requiredKeys = new HashSet<string>(requiredKeys);
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is not Dictionary<string, string> filterDictionary)
            {
                return new ValidationResult($"Something went wrong/The {validationContext.DisplayName} must be a dictionary.");
            }

            // Check for missing keys
            var missingKeys = _requiredKeys.Except(filterDictionary.Keys).ToList();

            if (missingKeys.Any())
            {
                return new ValidationResult($"Something went wrong/The {validationContext.DisplayName} is missing required keys: {string.Join(", ", missingKeys)}.");
            }
            foreach (var key in _requiredKeys)
            {
                if (string.IsNullOrWhiteSpace(filterDictionary[key]))
                {
                    return new ValidationResult($"Something went wrong/The value for {key} in {validationContext.DisplayName} cannot be null or empty.");
                }
            }
            return ValidationResult.Success;
        }
    }
}
