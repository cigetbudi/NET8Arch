using System.Data;
using Core.Entities;
using Core.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;

namespace Infrastructure.Repositories
{
    public class TodoRepository(string connectionString) : ITodoRepository
    {
        private readonly string _connectionString = connectionString;

        public async Task<TodoItem> AddTodoAsync(TodoItem item)
        {
            using IDbConnection db = new SqlConnection(_connectionString);
            var sql = "INSERT INTO TodoItems (Title, IsCompleted) VALUES (@Title, @IsCompleted); SELECT CAST(SCOPE_IDENTITY() as int)";
            item.Id = await db.ExecuteScalarAsync<int>(sql, item);
            return item;
        }

        public async Task DeleteTodoAsync(int id)
        {
            using IDbConnection db = new SqlConnection(_connectionString);
            var sql = "DELETE FROM TodoItems WHERE Id = @Id";
            await db.ExecuteAsync(sql, new { Id = id });
        }

        public async Task<IEnumerable<TodoItem>> GetAllTodosAsync()
        {
            using IDbConnection db = new SqlConnection(_connectionString);
            return await db.QueryAsync<TodoItem>("SELECT * FROM TodoItems");
        }

        public async Task<TodoItem> GetTodoByIdAsync(int id)
        {
            using IDbConnection db = new SqlConnection(_connectionString);
            return await db.QueryFirstOrDefaultAsync<TodoItem>("SELECT * FROM TodoItems WHERE Id = @Id", new { Id = id });
        }

        public async Task<TodoItem> UpdateTodoAsync(TodoItem item)
        {
            using IDbConnection db = new SqlConnection(_connectionString);
            var sql = "UPDATE TodoItems SET Title = @Title, IsCompleted = @IsCompleted WHERE Id = @Id";
            await db.ExecuteAsync(sql, item);
            return item;
        }
    }

}