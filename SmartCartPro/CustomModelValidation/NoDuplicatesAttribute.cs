using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace SmartCartPro.CustomModelValidation
{
    public class NoDuplicatesAttribute: ValidationAttribute
    {
        private const string DefaultErrorMessage = "Duplicate values are not allowed for the {0} field.";
        private readonly string _propertyName;

        public NoDuplicatesAttribute(string propertyName)
        {
            _propertyName = propertyName;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is IEnumerable list) // Use non-generic IEnumerable
            {
                var seenValues = new HashSet<object>();

                foreach (var item in list)
                {
                    if (item == null) continue;

                    // Use reflection to get the value of the specified property
                    var property = item.GetType().GetProperty(_propertyName, BindingFlags.Public | BindingFlags.Instance);

                    if (property == null)
                    {
                        return new ValidationResult($"Property '{_propertyName}' does not exist in the items.");
                    }

                    var propertyValue = property.GetValue(item);

                    if (propertyValue != null && !seenValues.Add(propertyValue))
                    {
                        var displayName = validationContext.DisplayName ?? validationContext.MemberName;
                        return new ValidationResult(ErrorMessage ?? string.Format(DefaultErrorMessage, _propertyName));
                    }
                }
            }

            return ValidationResult.Success;
        }
    }
}
