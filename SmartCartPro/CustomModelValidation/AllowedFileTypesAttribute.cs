using System.ComponentModel.DataAnnotations;

namespace SmartCartPro.CustomModelValidation
{
    public class AllowedFileTypesAttribute:ValidationAttribute
    {
        private readonly string[] _allowedExtensions =
        {
            ".pdf", ".doc", ".docx", ".xls", ".xlsx", // Documents
            ".jpg", ".jpeg", ".png", ".gif", // Images
            ".mp3", ".wav", // Audio
            ".mp4", ".avi", ".mov" // Video
        };

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is string fileName && !string.IsNullOrWhiteSpace(fileName))
            {
                string extension = Path.GetExtension(fileName)?.ToLower();
                if (!_allowedExtensions.Contains(extension))
                {
                    return new ValidationResult(ErrorMessage ??
                        "Invalid file type. Allowed types: Documents (.pdf, .doc, .docx, .xls, .xlsx), " +
                        "Images (.jpg, .jpeg, .png, .gif), Audios (.mp3, .wav), Videos (.mp4, .avi, .mov).");
                }
            }

            return ValidationResult.Success;
        }
    }
}
