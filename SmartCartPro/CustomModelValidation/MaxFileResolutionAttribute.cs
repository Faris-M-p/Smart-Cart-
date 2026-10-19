using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace SmartCartPro.CustomModelValidation
{
    //[maxFileResolution(1)]
    public class MaxFileResolutionAttribute : ValidationAttribute
    {
        private readonly int _maxFileSize;

        public MaxFileResolutionAttribute(int maxFileSize)
        {
            _maxFileSize = maxFileSize;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var file = value as IFormFile;
            if (file != null)
            {
                if (file.Length > (1000000 * _maxFileSize))
                {
                    return new ValidationResult($"Maximum allowed file size is {_maxFileSize} MB");
                }
            }
            return ValidationResult.Success;
        }
    }
}
