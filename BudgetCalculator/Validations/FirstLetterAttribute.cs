using System.ComponentModel.DataAnnotations;

namespace BudgetCalculator.Validations
{
    public class FirstLetterAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
            {
                return ValidationResult.Success;
            }
            else if (Char.IsUpper(value.ToString()[0]))
            {
                return ValidationResult.Success;
            }
            else
            {
                return new ValidationResult("La primera letra debe ser mayúscula.");
            }
        }
    }
}
