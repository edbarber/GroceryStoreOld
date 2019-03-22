using System;
using System.ComponentModel.DataAnnotations;

namespace Validations
{
    public class NoSpaces : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            string something = value?.ToString().Trim();

            if (something?.Contains(" ") == true)
            {
                return new ValidationResult(ErrorMessage);
            }

            return ValidationResult.Success;
        }
    }
}
