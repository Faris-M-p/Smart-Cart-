using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace SmartCartPro.CustomModelValidation
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class FilterDictionaryOnlyAllowAttribute : ValidationAttribute
    {
        private readonly HashSet<string> _allowedKeys;

        public FilterDictionaryOnlyAllowAttribute(params string[] allowedKeys)
        {
            _allowedKeys = new HashSet<string>(allowedKeys);
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is not Dictionary<string, string> filterDictionary)
            {
                return new ValidationResult($"Something went wrong/The {validationContext.DisplayName} must be a dictionary.");
            }

            // Get disallowed keys
            var disallowedKeys = filterDictionary.Keys.Except(_allowedKeys).ToList();

            if (disallowedKeys.Any())
            {
                return new ValidationResult($"Something went wrong/The {validationContext.DisplayName} contains invalid keys: {string.Join(", ", disallowedKeys)}.");
            }

            return ValidationResult.Success;
        }
    }
}
