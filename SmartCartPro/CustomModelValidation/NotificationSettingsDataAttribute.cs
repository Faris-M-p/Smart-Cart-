using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace SmartCartPro.CustomModelValidation
{
    /// <summary>
    /// Validates notification schedule settings on the object instance.
    /// Expects properties: IsRepeat (int), IsSpecificDate (bool), StartsOn/EndsOn (DateTimeOffset?).
    /// </summary>
    public class NotificationSettingsDataAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var instance = validationContext.ObjectInstance;
            if (instance == null)
                return ValidationResult.Success;

            var type = instance.GetType();
            var isRepeat = GetInt(type, instance, "IsRepeat");
            var isSpecificDate = GetBool(type, instance, "IsSpecificDate");
            var startsOn = GetDateTimeOffset(type, instance, "StartsOn");
            var endsOn = GetDateTimeOffset(type, instance, "EndsOn");

            if (isRepeat == 1 && isSpecificDate)
            {
                return new ValidationResult("Something went wrong/ You cannot enable both IsRepeat and IsSpecificDate at the same time.");
            }

            if (isRepeat == 1)
            {
                if (!startsOn.HasValue || !endsOn.HasValue)
                {
                    return new ValidationResult("StartsOn and EndsOn date are required");
                }

                if (startsOn < DateTimeOffset.UtcNow.Date)
                {
                    return new ValidationResult("StartsOn must be today or a future date.");
                }

                if (endsOn < DateTimeOffset.UtcNow.Date)
                {
                    return new ValidationResult("EndsOn must be today or a future date.");
                }

                if (endsOn < startsOn)
                {
                    return new ValidationResult("EndsOn must be greater than or equal to StartsOn.");
                }

                if (isSpecificDate)
                {
                    return new ValidationResult("Something went wrong/IsSpecificDate cannot be 1 when IsRepeat is 1.");
                }
            }

            if (isSpecificDate)
            {
                if (!startsOn.HasValue)
                {
                    return new ValidationResult("StartsOn is required .");
                }
                if (startsOn < DateTimeOffset.UtcNow.Date)
                {
                    return new ValidationResult("StartsOn must be today or a future date.");
                }
            }

            return ValidationResult.Success;
        }

        private static int GetInt(Type type, object instance, string name)
        {
            var prop = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (prop?.GetValue(instance) is int i) return i;
            if (prop?.GetValue(instance) is long l) return (int)l;
            return 0;
        }

        private static bool GetBool(Type type, object instance, string name)
        {
            var prop = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (prop?.GetValue(instance) is bool b) return b;
            return false;
        }

        private static DateTimeOffset? GetDateTimeOffset(Type type, object instance, string name)
        {
            var prop = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            var val = prop?.GetValue(instance);
            if (val is DateTimeOffset dto) return dto;
            if (val is DateTime dt) return new DateTimeOffset(dt);
            return null;
        }
    }
}
