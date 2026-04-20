using BudgetCalculator.Models;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Collections;

namespace BudgetCalculator.Services
{
    public interface IAcountTypesRepository
    {
        Task<bool> AlreadyExists(string name, int userId);
        public Task Create(AcountType acountType);
        Task<AcountType> GetById(int id, int userId);
        Task<IEnumerable<AcountType>> GetByUser(int UserId);
        Task Update(AcountType acountType);
        Task Delete(int id);
        Task Order(IEnumerable<AcountType> acountTypes);
    }

    public class AcountTypesRepository : IAcountTypesRepository
    {
        private readonly string connectionString;

        public AcountTypesRepository(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task Create(AcountType acountType)
        {
            using SqlConnection connection = new SqlConnection(connectionString);
            int id = await connection.QuerySingleAsync<int>(
                "InsertAcountType",
                new {Name = acountType.Name, UserId = acountType.UserId},
                commandType: System.Data.CommandType.StoredProcedure
                );
            acountType.Id = id;
        }

        public async Task<bool> AlreadyExists(string name, int userId)
        {
            using SqlConnection connection = new SqlConnection(connectionString);
            int exists = await connection.QueryFirstOrDefaultAsync<int>(
                @"SELECT 1 FROM AcountTypes WHERE Name = @Name AND UserID = @UserId",
                new { name, userId }
            );
            return exists == 1;
        }

        public async Task<IEnumerable<AcountType>> GetByUser(int UserId)
        {
            using SqlConnection connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<AcountType>(
                @"SELECT Id, Name, ShowOrder FROM AcountTypes WHERE UserId = @UserId ORDER BY ShowOrder",
                new {UserId});
        }

        public async Task Update(AcountType acountType)
        {
            using SqlConnection connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(@"UPDATE AcountTypes SET Name = @Name WHERE Id = @Id", acountType);
        }

        public async Task<AcountType> GetById(int id, int userId)
        {
            using SqlConnection connection = new SqlConnection(connectionString);
            return await connection.QueryFirstOrDefaultAsync<AcountType>(
                @"SELECT Id, Name, ShowOrder FROM AcountTypes WHERE Id = @Id AND UserId = @UserId",
                new {id, userId});
        }

        public async Task Delete(int id)
        {
            using SqlConnection connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync("DELETE AcountTypes WHERE Id = @Id", new {id});
        }

        public async Task Order(IEnumerable<AcountType> acountTypes)
        {
            using SqlConnection connection = new SqlConnection(connectionString);
            string query = "UPDATE AcountTypes SET ShowOrder = @ShowOrder WHERE Id = @Id";
            await connection.ExecuteAsync(query, acountTypes);
        }
    }
}
