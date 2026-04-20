namespace BudgetCalculator.Models
{
    public class GetTransactionsByUserParameter
    {
        public int UserId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
