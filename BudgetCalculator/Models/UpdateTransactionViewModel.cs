namespace BudgetCalculator.Models
{
    public class UpdateTransactionViewModel : TransactionCreationViewModel
    {
        public decimal PreviousMonto { get; set; }

        public int PreviousAcountId { get; set; }

        public string returnUrl { get; set; }
    }
}
