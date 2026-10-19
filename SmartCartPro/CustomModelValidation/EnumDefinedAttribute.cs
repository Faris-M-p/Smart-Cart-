using System.ComponentModel.DataAnnotations;

namespace SmartCartPro.CustomModelValidation
{
    /// <summary>Rejects integers that are not defined members of the given enum.</summary>
    public class EnumDefinedAttribute : ValidationAttribute
    {
        private readonly Type _enumType;

        public EnumDefinedAttribute(Type enumType)
        {
            if (enumType is null || !enumType.IsEnum)
                throw new ArgumentException("Type must be an enum.", nameof(enumType));

            _enumType = enumType;
            ErrorMessage = "{0} is not a valid value.";
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var displayName = validationContext.DisplayName ?? validationContext.MemberName;

            if (value == null)
                return ValidationResult.Success;

            try
            {
                var number = Convert.ToInt32(value);
                if (!Enum.IsDefined(_enumType, number))
                {
                    return new ValidationResult(string.Format(ErrorMessageString, displayName));
                }
            }
            catch
            {
                return new ValidationResult(string.Format(ErrorMessageString, displayName));
            }

            return ValidationResult.Success;
        }
    }
}
