using System.ComponentModel.DataAnnotations;

namespace SmartCartPro.CustomModelValidation
{
    public class StringLengthValidationAttribute : ValidationAttribute
    {//[StringLengthValidation(3, 150, ErrorMessage = "Something went wrong / The SearchName must be between 3 and 150 characters.")]
        private readonly int _minLength;
        private readonly int _maxLength;

        public StringLengthValidationAttribute(int minLength, int maxLength = int.MaxValue)
        {
            _minLength = minLength;
            _maxLength = maxLength;
            ErrorMessage = $"The string length must be between {_minLength} and {_maxLength} characters.";
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {



            if (value is string strValue && !string.IsNullOrEmpty(strValue))
            {
                if (_minLength > 0 && strValue.Length < _minLength)
                {
                    // Use ErrorMessage if set, otherwise default message
                    return new ValidationResult(ErrorMessage ?? $"The string length must be at least {_minLength} characters.",
                          new[] { validationContext.MemberName }
                        );
                }

                if (strValue.Length > _maxLength)
                {
                    return new ValidationResult(
                        ErrorMessage ?? $"The string length must be less than or equal to {_maxLength} characters.",                        
                        new[] { validationContext.MemberName }
                        );
                }
            }




            return ValidationResult.Success;
        }
    }

}
