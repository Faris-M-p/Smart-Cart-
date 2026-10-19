using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace SmartCartPro.CustomModelValidation
{
    public class StrongPasswordAttribute : ValidationAttribute
    {
        public int MinLength { get; }
        public int MaxLength { get; }

        // Regex rules
        private readonly bool _requireUppercase;
        private readonly bool _requireLowercase;
        private readonly bool _requireDigit;
        private readonly bool _requireSpecial;

        private const string DefaultErrorMessage = "Password must be between {0}-{1} characters and contain at least one uppercase letter, one lowercase letter, one digit, and one special character.";

        public StrongPasswordAttribute(
            int minLength = 8,
            int maxLength = 16,
            bool requireUppercase = true,
            bool requireLowercase = true,
            bool requireDigit = true,
            bool requireSpecial = true)
        {
            MinLength = minLength;
            MaxLength = maxLength;
            _requireUppercase = requireUppercase;
            _requireLowercase = requireLowercase;
            _requireDigit = requireDigit;
            _requireSpecial = requireSpecial;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is not string password || string.IsNullOrWhiteSpace(password))
            {
                return ValidationResult.Success; // Allow empty, handle required separately
            }

            var displayName = validationContext.DisplayName ?? validationContext.MemberName;

            if (password.Length < MinLength || password.Length > MaxLength)
            {
                return new ValidationResult(ErrorMessage ?? string.Format(DefaultErrorMessage, MinLength, MaxLength));
            }

            if (_requireUppercase && !Regex.IsMatch(password, "[A-Z]"))
            {
                return new ValidationResult(ErrorMessage ?? $"{displayName} must contain at least one uppercase letter.");
            }

            if (_requireLowercase && !Regex.IsMatch(password, "[a-z]"))
            {
                return new ValidationResult(ErrorMessage ?? $"{displayName} must contain at least one lowercase letter.");
            }

            if (_requireDigit && !Regex.IsMatch(password, "[0-9]"))
            {
                return new ValidationResult(ErrorMessage ?? $"{displayName} must contain at least one digit.");
            }

            if (_requireSpecial && !Regex.IsMatch(password, "[^a-zA-Z0-9]"))
            {
                return new ValidationResult(ErrorMessage ?? $"{displayName} must contain at least one special character.");
            }

            return ValidationResult.Success;
        }
    }

}
