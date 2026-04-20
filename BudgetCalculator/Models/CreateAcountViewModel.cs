using Microsoft.AspNetCore.Mvc.Rendering;

namespace BudgetCalculator.Models
{
    public class CreateAcountViewModel : Acount
    {
        public IEnumerable<SelectListItem> AcountTypes { get; set; }
    }
}
