using System.ComponentModel.DataAnnotations;

namespace BudgetCalculator.Models
{
    public class Transaction
    {
        public int Id { get; set; }
        public int UserId { get; set; }

        [Display(Name = "Fecha de la transacción")]
        [DataType(DataType.Date)]
        public DateTime TransactionDate { get; set; } = DateTime.Today;
        public decimal Monto { get; set; }

        [Range(0, maximum:int.MaxValue, ErrorMessage = "Debe seleccionar una categoría")]
        [Display(Name = "Categoría")]
        public int CategoryId { get; set; }

        [StringLength(maximumLength:1000, ErrorMessage = "La nota no puede pasar de {1} caracteres")]
        public string Nota { get; set; }

        [Display(Name = "Cuenta")]
        [Range(0, maximum: int.MaxValue, ErrorMessage = "Debe seleccionar una categoría")]
        public int AcountId {  get; set; }

        [Display(Name = "Tipo de operación")]
        public OperationType OperationTypeId { get; set; } = OperationType.Income;

        public string Acount { get; set; }
        public string Category { get; set; }
    }
}
