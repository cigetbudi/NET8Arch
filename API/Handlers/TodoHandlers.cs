using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace API.Handlers
{
    public static class TodoHandlers
    {
        public static async Task<IResult> GetAllTodosAsync(ITodoRepository repository)
        {
            var todos = await repository.GetAllTodosAsync();
            return Results.Ok(todos);
        }

        public static async Task<IResult> GetTodoByIdAsync(int id, ITodoRepository repository)
        {
            var todo = await repository.GetTodoByIdAsync(id);
            return todo is not null ? Results.Ok(todo) : Results.NotFound();
        }

        public static async Task<IResult> AddTodoAsync(TodoItem item, ITodoRepository repository)
        {
            var createdItem = await repository.AddTodoAsync(item);
            return Results.Created($"/todos/{createdItem.Id}", createdItem);
        }

        public static async Task<IResult> UpdateTodoAsync(int id, TodoItem updatedItem, ITodoRepository repository)
        {
            var item = await repository.GetTodoByIdAsync(id);
            if (item is null) return Results.NotFound();

            item.Title = updatedItem.Title;
            item.IsCompleted = updatedItem.IsCompleted;

            await repository.UpdateTodoAsync(item);
            return Results.NoContent();
        }

        public static async Task<IResult> DeleteTodoAsync(int id, ITodoRepository repository)
        {
            var item = await repository.GetTodoByIdAsync(id);
            if (item is null) return Results.NotFound();

            await repository.DeleteTodoAsync(id);
            return Results.NoContent();
        }
    }
}