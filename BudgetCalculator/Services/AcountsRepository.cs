using BudgetCalculator.Models;
using Dapper;
using Microsoft.Data.SqlClient;

namespace BudgetCalculator.Services
{
    public interface IAcountsRepository
    {
        Task Create(Acount acount);
        Task Delete(int id);
        Task<Acount> GetById(int id, int userId);
        Task<IEnumerable<Acount>> Search(int userId);
        Task Update(CreateAcountViewModel createAcountViewModel);
    }

    public class AcountsRepository : IAcountsRepository
    {
        private readonly string connectionString;

        public AcountsRepository(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task Create(Acount acount)
        {
            using SqlConnection connection = new SqlConnection(connectionString);
            int id = await connection.QuerySingleAsync<int>(
                @"INSERT INTO Acounts (Name, AcountTypeId, Balance, Description) VALUES (@Name, @AcountTypeId, @Balance, @Description);
                SELECT SCOPE_IDENTITY();",
                acount);
            acount.Id = id;
        }

        public async Task<IEnumerable<Acount>> Search(int userId)
        {
            using SqlConnection connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<Acount>(
                @"SELECT Acounts.Id, Acounts.Name, Balance, ac.Name AS AcountType
                FROM Acounts INNER JOIN AcountTypes ac ON Acounts.AcountTypeId = ac.Id WHERE ac.UserId = @UserId
                ORDER BY ac.ShowOrder", new{userId});
        }

        public async Task<Acount> GetById(int id, int userId)
        {
            using SqlConnection connection = new SqlConnection(connectionString);
            return await connection.QueryFirstOrDefaultAsync<Acount>(
                @"SELECT Acounts.Id, Acounts.Name, Balance, Description, AcountTypeId
                FROM Acounts INNER JOIN AcountTypes ac ON Acounts.AcountTypeId = ac.Id
                WHERE ac.UserId = @UserId AND Acounts.Id = @Id",
                new {Id = id, UserId = userId});
        }

        public async Task Update(CreateAcountViewModel createAcountViewModel)
        {
            using SqlConnection connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(
                @"UPDATE Acounts SET Name = @Name, Balance = @Balance, Description = @Description, AcountTypeId = @AcountTypeId WHERE Id = @Id",
                createAcountViewModel);
        }

        public async Task Delete(int id)
        {
            using SqlConnection connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync("DELETE FROM Acounts WHERE Id = @Id", new {id});
        }
    }
}
