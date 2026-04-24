namespace BudgetCalculator.Models
{
    public class MonthlyReportViewModel
    {
        public IEnumerable<MonthlyReport> TransactionsByMonth {  get; set; }
        public decimal Incomes => TransactionsByMonth.Sum(x => x.Incomes);
        public decimal Expenses => TransactionsByMonth.Sum(x => x.Expenses);
        public decimal Total => Incomes - Expenses;
        public int Year {  get; set; }
    }
}
