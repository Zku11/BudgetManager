using BudgetCalculator.Validations;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace BudgetCalculator.Models
{
    public class AcountType //: IValidatableObject
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El campo nombre es requerido.")]
        [FirstLetter]
        [Remote(action: "AcountTypeLiveVerify", controller: "AcountTypes")]
        public string Name { get; set; }
        public int UserId { get; set; }
        public int ShowOrder { get; set; }

        /*public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if(Name != null && Name.Length > 0)
            {
                if (!Char.IsUpper(Name[0]))
                {
                    yield return new ValidationResult("La primera letra debe ser mayúscula", new[] {nameof(Name)});
                }
            }
        }*/
    }
}
