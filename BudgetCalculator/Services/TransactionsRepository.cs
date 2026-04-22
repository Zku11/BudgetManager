using BudgetCalculator.Models;
using Dapper;
using Microsoft.Data.SqlClient;

namespace BudgetCalculator.Services
{
    public interface ITransactionsRepository
    {
        Task Create(Transaction transaction);
        Task Delete(int id);
        Task<IEnumerable<Transaction>> GetByAcountId(GetTransactionsByAcount getTransactionsByAcount);
        Task<Transaction> GetById(int id, int userId);
        Task<IEnumerable<Transaction>> GetByUserId(GetTransactionsByUserParameter getTransactionsByUser);
        Task<IEnumerable<WeeklyReport>> GetByWeek(GetTransactionsByUserParameter model);
        Task Update(Transaction transaction, decimal PreviousMonto, int PreviousAcountId);
    }

    public class TransactionsRepository : ITransactionsRepository
    {
        private readonly string connectionString;

        public TransactionsRepository(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task Create(Transaction transaction)
        {
            using SqlConnection sqlConnection = new SqlConnection(connectionString);
            int id = await sqlConnection.QuerySingleAsync<int>(
                    "Insert_Transaction",
                    new {transaction.UserId, transaction.TransactionDate, transaction.Monto,transaction.AcountId, transaction.CategoryId, transaction.Nota},
                    commandType: System.Data.CommandType.StoredProcedure
                    );
            transaction.Id = id;
        }

        public async Task Update(Transaction transaction, decimal PreviousMonto, int PreviousAcountId)
        {
            using SqlConnection sqlConnection = new SqlConnection(connectionString);
            await sqlConnection.ExecuteAsync(
                "Update_Transaction",
                new
                {
                    transaction.Id,
                    transaction.TransactionDate,
                    transaction.Monto,
                    PreviousMonto,
                    transaction.AcountId,
                    PreviousAcountId,
                    transaction.CategoryId,
                    transaction.Nota
                },
                commandType: System.Data.CommandType.StoredProcedure
            );
        }

        public async Task<Transaction> GetById(int id, int userId)
        {
            using SqlConnection sqlConnection = new SqlConnection(connectionString);
            return await sqlConnection.QueryFirstOrDefaultAsync<Transaction>(
                @"SELECT Transactions.*, cat.OperationTypeId
                FROM Transactions INNER JOIN Categories cat
                ON cat.id = Transactions.CategoryId WHERE Transactions.id = @Id AND Transactions.UserId = @UserId",
                new {id, userId}
            );
        }

        public async Task<IEnumerable<WeeklyReport>> GetByWeek(GetTransactionsByUserParameter model)
        {
            using SqlConnection sqlConnection = new SqlConnection(connectionString);
            return await sqlConnection.QueryAsync<WeeklyReport>(@"
                        SELECT DATEDIFF(d, @StartDate, TransactionDate) / 7 + 1 as Weeks, SUM(Monto) AS Monto, cat.OperationTypeId
                        FROM Transactions tra
                        INNER JOIN Categories cat ON cat.id = CategoryId
                        WHERE tra.UserId = @UserId AND TransactionDate BETWEEN @StartDate AND @EndDate
                        GROUP BY DATEDIFF(d, @StartDate, TransactionDate) / 7, cat.OperationTypeId",
                        model);
        }

        public async Task Delete(int id)
        {
            using SqlConnection sqlConnection = new SqlConnection(connectionString);
            await sqlConnection.ExecuteAsync("Delete_Transaction", new {id}, commandType: System.Data.CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<Transaction>> GetByAcountId(GetTransactionsByAcount getTransactionsByAcount)
        {
            using SqlConnection sqlConnection = new SqlConnection(connectionString);
            return await sqlConnection.QueryAsync<Transaction>(@"SELECT t.Id, t.Monto, t.TransactionDate, c.Name as Category, a.Name as Acount, c.OperationTypeId
                                                                FROM Transactions t
                                                                INNER JOIN Categories c
                                                                ON c.id = t.CategoryId
                                                                INNER JOIN Acounts a
                                                                ON a.Id = t.AcountId
                                                                WHERE t.AcountId = @AcountId AND t.UserId = @UserId
                                                                AND TransactionDate BETWEEN @StartDate AND @EndDate",
                                                                getTransactionsByAcount);
        }

        public async Task<IEnumerable<Transaction>> GetByUserId(GetTransactionsByUserParameter getTransactionsByUser)
        {
            using SqlConnection sqlConnection = new SqlConnection(connectionString);
            return await sqlConnection.QueryAsync<Transaction>(@"SELECT t.Id, t.Monto, t.TransactionDate, c.Name as Category, a.Name as Acount, c.OperationTypeId
                                                                FROM Transactions t
                                                                INNER JOIN Categories c
                                                                ON c.id = t.CategoryId
                                                                INNER JOIN Acounts a
                                                                ON a.Id = t.AcountId
                                                                WHERE t.UserId = @UserId
                                                                AND TransactionDate BETWEEN @StartDate AND @EndDate
                                                                ORDER BY t.TransactionDate DESC",
                                                                getTransactionsByUser);
        }
    }
}
