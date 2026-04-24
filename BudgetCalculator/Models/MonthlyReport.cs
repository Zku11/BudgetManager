namespace BudgetCalculator.Models
{
    public class MonthlyReport
    {
        public int Month { get; set; }
        public DateTime ReferenceDate { get; set; }
        public decimal Monto { get; set; }
        public decimal Expenses { get; set; }
        public decimal Incomes { get; set; }
        public OperationType OperationTypeId { get; set; }
    }
}
