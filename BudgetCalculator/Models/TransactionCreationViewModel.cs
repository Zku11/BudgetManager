using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace BudgetCalculator.Models
{
    public class TransactionCreationViewModel : Transaction
    {
        public IEnumerable<SelectListItem> Acounts { get; set; }
        public IEnumerable<SelectListItem> Categories { get; set; }
    }
}
