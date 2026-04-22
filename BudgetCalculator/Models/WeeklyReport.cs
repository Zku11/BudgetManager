namespace BudgetCalculator.Models
{
    public class WeeklyReport
    {
        public int Weeks { get; set; }

        public decimal Monto {  get; set; }

        public OperationType OperationTypeId { get; set; }
    }
}
