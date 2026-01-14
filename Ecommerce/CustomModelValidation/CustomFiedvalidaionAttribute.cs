using System.ComponentModel.DataAnnotations;

namespace Ecommerce.CustomModelValidation
{
    // This attribute is not currently used in the Ecommerce project
    // If needed in the future, it should be implemented with proper model references
    public class CustomFiedvalidaionAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            // Placeholder implementation - not currently used
            // If this attribute is needed, implement proper validation logic
            return ValidationResult.Success;
        }
    }
}
  
           





