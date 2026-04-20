namespace BudgetCalculator.Models
{
    public class DetailedTransactionReport
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public IEnumerable<TransactionsByDate> transactionsByDateList { get; set; }
        public decimal DepositsBalance => transactionsByDateList.Sum(x => x.DepositsBalance);
        public decimal ExpensesBalance => transactionsByDateList.Sum(x => x.ExpensesBalance);
        public decimal Total => DepositsBalance - ExpensesBalance;
    }

    public class TransactionsByDate
    {
        public DateTime TransactionDate { get; set; }
        public IEnumerable<Transaction> Transactions { get; set; }
        public decimal DepositsBalance => Transactions.Where(x => x.OperationTypeId == OperationType.Income).Sum(x => x.Monto);
        public decimal ExpensesBalance => Transactions.Where(x => x.OperationTypeId == OperationType.Expense).Sum(x => x.Monto);
    }
}
