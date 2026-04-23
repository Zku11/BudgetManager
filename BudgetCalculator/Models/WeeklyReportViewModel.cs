namespace BudgetCalculator.Models
{
    public class WeeklyReportViewModel
    {
        public IEnumerable<WeeklyReport> WeeklyReports { get; set; }
        public decimal Incomes => WeeklyReports.Sum(x => x.Incomes);
        public decimal Expenses => WeeklyReports.Sum(x => x.Expenses);
        public decimal Total => Incomes - Expenses;
        public DateTime ReferenceDate { get; set; }
    }
}
