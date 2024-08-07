using Core.Entities;

namespace Core.Interfaces
{
    public interface ITodoRepository
    {
        Task<IEnumerable<TodoItem>> GetAllTodosAsync();
        Task<TodoItem> GetTodoByIdAsync(int id);
        Task<TodoItem> AddTodoAsync(TodoItem item);
        Task<TodoItem> UpdateTodoAsync(TodoItem item);
        Task DeleteTodoAsync(int id);
    }
}