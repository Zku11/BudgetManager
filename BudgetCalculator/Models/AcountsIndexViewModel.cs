
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BudgetCalculator.Models
{
    public class AcountsIndexViewModel
    {
        public string AcountType { get; set; }
        public IEnumerable<Acount> Acounts { get; set; }

        public decimal Balance => Acounts.Sum(x => x.Balance);

    }
}
