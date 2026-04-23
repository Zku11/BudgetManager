namespace BudgetCalculator.Models
{
    public class WeeklyReport
    {
        public int Weeks { get; set; }
        public decimal Monto {  get; set; }
        public OperationType OperationTypeId { get; set; }
        public decimal Incomes { get; set; }
        public decimal Expenses { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
