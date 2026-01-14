using System.ComponentModel.DataAnnotations;

namespace Ecommerce.CustomModelValidation
{
	public class ForbiddenWordsValidatorAttribute: ValidationAttribute
	{
		private readonly string[] _forbiddenWords;

		// Constructor accepts forbidden words to validate against
		public ForbiddenWordsValidatorAttribute(params string[] forbiddenWords)
		{
			// If no forbidden words are passed, use the default ones
			_forbiddenWords = forbiddenWords.Length > 0 ? forbiddenWords : new[] { "Delete", "drop", "truncate", "insert", "update" };
		}

		protected override ValidationResult IsValid(object value, ValidationContext validationContext)
		{
			// Validate the input string against the forbidden words
			if (value is string strValue && _forbiddenWords.Any(word => strValue.Contains(word, StringComparison.OrdinalIgnoreCase)))
			{
				// Return a validation error message if forbidden words are found
				return new ValidationResult($"The input contains forbidden words: {string.Join(", ", _forbiddenWords)}.");
			}

			return ValidationResult.Success;
		}
	}

	
}
	




