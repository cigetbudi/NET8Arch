using Core.Entities;
using Core.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;

namespace Infrastructure.Repositories
{
    public class TodoRepository(string connectionString) : ITodoRepository
    {
        private readonly string _connectionString = connectionString;

        private SqlConnection DBTodoConn()
        {
            return new SqlConnection(_connectionString);
        }

        public async Task<TodoItem> AddTodoAsync(TodoItem item)
        {
            using var db = DBTodoConn();
            var sql = "INSERT INTO TodoItems (Title, IsCompleted) VALUES (@Title, @IsCompleted); SELECT CAST(SCOPE_IDENTITY() as int)";
            item.Id = await db.ExecuteScalarAsync<int>(sql, item);
            return item;
        }

        public async Task DeleteTodoAsync(int id)
        {
            using var db = DBTodoConn();
            var sql = "DELETE FROM TodoItems WHERE Id = @Id";
            await db.ExecuteAsync(sql, new { Id = id });
        }

        public async Task<IEnumerable<TodoItem>> GetAllTodosAsync()
        {
            using var db = DBTodoConn();
            return await db.QueryAsync<TodoItem>("SELECT * FROM TodoItems");

        }

        public async Task<TodoItem> GetTodoByIdAsync(int id)
        {
            using var db = DBTodoConn();
            var todoItem = await db.QueryFirstOrDefaultAsync<TodoItem>("SELECT * FROM TodoItems WHERE Id = @Id", new { Id = id });
            return todoItem;
        }

        public async Task<TodoItem> UpdateTodoAsync(TodoItem item)
        {
            using var db = DBTodoConn();
            var sql = "UPDATE TodoItems SET Title = @Title, IsCompleted = @IsCompleted WHERE Id = @Id";
            await db.ExecuteAsync(sql, item);
            return item;
        }
    }

}