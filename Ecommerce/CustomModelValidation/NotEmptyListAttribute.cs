using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace Ecommerce.CustomModelValidation
{
    public class NotEmptyListAttribute : ValidationAttribute
    {
        public NotEmptyListAttribute()
        {
            ErrorMessage = "The list must contain at least one item."; // Default message
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            // Ensure the value is a collection
            if (value is ICollection collection)
            {
                if (collection.Count == 0)
                {
                    // Use custom message if provided, otherwise use the default
                    return new ValidationResult(ErrorMessage ?? "The list must contain at least one item.");
                }
            }
            else
            {
                return new ValidationResult("Invalid data type. Expected a collection.");
            }

            return ValidationResult.Success;
        }
    }
}
