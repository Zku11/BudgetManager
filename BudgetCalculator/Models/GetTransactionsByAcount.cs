namespace BudgetCalculator.Models
{
    public class GetTransactionsByAcount
    {
        public int UserId { get; set; }
        public int AcountId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
