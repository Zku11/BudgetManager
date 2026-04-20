using BudgetCalculator.Validations;
using System.ComponentModel.DataAnnotations;

namespace BudgetCalculator.Models
{
    public class Acount
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido")]
        [StringLength(50)]
        [FirstLetter]
        [Display(Name = "Nombre")]
        public string Name { get; set; }

        [Display(Name = "Tipo de cuenta")]
        public int AcountTypeId { get; set; }

        public string AcountType { get; set; }

        public decimal Balance { get; set; }

        [StringLength(maximumLength: 1000)]
        [Display(Name = "Descripción")]
        public string Description { get; set; }

    }
}
