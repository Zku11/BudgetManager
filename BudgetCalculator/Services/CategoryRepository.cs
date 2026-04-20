using BudgetCalculator.Models;
using Dapper;
using Microsoft.Data.SqlClient;

namespace BudgetCalculator.Services
{
    public interface ICategoryRepository
    {
        Task Create(Category category);
        Task Delete(int id);
        Task<IEnumerable<Category>> Get(int userId, OperationType operationType);
        Task<Category> GetById(int id, int userId);
        Task<IEnumerable<Category>> GetByUserId(int userId);
        Task Update(Category category);
    }

    public class CategoryRepository : ICategoryRepository
    {
        private readonly string connectionString;

        public CategoryRepository(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task Create(Category category)
        {
            using SqlConnection connection = new SqlConnection(connectionString);
            int id = await connection.QuerySingleAsync<int>(
                                    "INSERT INTO Categories (Name, OperationTypeId, UserId) Values (@Name, @OperationTypeId, @UserId);" +
                                    "SELECT SCOPE_IDENTITY();",
                                    category
                                    );
            category.Id = id;
        }

        public async Task<IEnumerable<Category>> GetByUserId(int userId)
        {
            using SqlConnection connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<Category>("SELECT * FROM Categories WHERE UserId = @UserId", new {userId});
        }

        public async Task<IEnumerable<Category>> Get(int userId, OperationType operationTypeId)
        {
            using SqlConnection connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<Category>("SELECT * FROM Categories WHERE UserId = @UserId AND OperationTypeId = @OperationTypeId", new { userId, operationTypeId});
        }

        public async Task<Category> GetById(int id, int userId)
        {
            using SqlConnection connection = new SqlConnection(connectionString);
            return await connection.QueryFirstOrDefaultAsync<Category>(@"SELECT * FROM Categories WHERE Id = @Id AND UserId = @UserId", new {id, userId});
        }

        public async Task Update(Category category)
        {
            using SqlConnection connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(@"UPDATE Categories SET Name = @Name, OperationTypeId = @OperationTypeId WHERE Id = @Id", category);
        }

        public async Task Delete(int id)
        {
            using SqlConnection connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(@"DELETE Categories WHERE Id = @Id", new {id});
        }
    }
}
