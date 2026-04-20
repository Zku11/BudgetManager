using System.ComponentModel.DataAnnotations;

namespace BudgetCalculator.Models
{
    public class Category
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido.")]
        [StringLength(maximumLength:50, ErrorMessage = "No puede ser mayor a 50 caracteres.")]
        public string Name { get; set; }

        [Display(Name = "Tipo de operación")]
        public int OperationTypeId { get; set; }
        public int UserId { get; set; }
    }
}
