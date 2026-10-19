using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace SmartCartPro.CustomModelValidation
{
    //[AllowedExtension (new string[] { ".jpg","png",".jpeg" })]
    public class AllowedExtensionsAttribute : ValidationAttribute
    {
        private readonly string[] _extension;

        public AllowedExtensionsAttribute(string[] extension)
        {
            _extension = extension;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var file = value as IFormFile;
            if (file != null)
            {
                var extension = Path.GetExtension(file.FileName);
                if (!_extension.Contains(extension.ToLower()))
                {
                    return new ValidationResult("This photo extension is not allowed");
                }
            }
            return ValidationResult.Success;
        }
    }
}
