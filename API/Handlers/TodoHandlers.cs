using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using FluentValidation;
using API.Helpers;

namespace API.Handlers
{
    public static class TodoHandlers
    {
        public static async Task<IResult> GetAllTodosAsync(ITodoRepository repository)
        {
            var todos = await repository.GetAllTodosAsync();
            return Results.Ok(Helper.CreateResponse(todos, null));
        }

        public static async Task<IResult> GetTodoByIdAsync(int id, ITodoRepository repository)
        {
            try
            {
                var todo = await repository.GetTodoByIdAsync(id);
                return Results.Ok(Helper.CreateResponse(todo, null));
            }
            catch (Exception ex)
            {
                return Results.NotFound(Helper.CreateResponse<string[]>(null, ex.Message));
            }
        }

        public static async Task<IResult> AddTodoAsync(TodoItem item, ITodoRepository repository, IValidator<TodoItem> validator)
        {
            var validationResult = await validator.ValidateAsync(item);

            if (!validationResult.IsValid)
            {
                return Results.BadRequest(Helper.CreateResponse<TodoItem>(null, Helper.ExtractValidationErrors(validationResult)));
            }

            var createdItem = await repository.AddTodoAsync(item);
            return Results.Created($"/todos/{createdItem.Id}", Helper.CreateResponse(createdItem, null));
        }


        public static async Task<IResult> UpdateTodoAsync(int id, TodoItem updatedItem, ITodoRepository repository, IValidator<TodoItem> validator)
        {
            var validationResult = await validator.ValidateAsync(updatedItem);
            if (!validationResult.IsValid)
            {
                return Results.BadRequest(Helper.CreateResponse<TodoItem>(null, Helper.ExtractValidationErrors(validationResult)));
            }

            var item = await repository.GetTodoByIdAsync(id);
            if (item is null) return Results.NotFound(Helper.CreateResponse<string[]>(null, "Item tidak ditemukan"));

            item.Title = updatedItem.Title;
            item.IsCompleted = updatedItem.IsCompleted;

            await repository.UpdateTodoAsync(item);
            return Results.Ok(Helper.CreateResponse<string[]>(null, null));
        }

        public static async Task<IResult> DeleteTodoAsync(int id, ITodoRepository repository)
        {
            var item = await repository.GetTodoByIdAsync(id);
            if (item is null) return Results.NotFound(Helper.CreateResponse<string[]>(null, "Item tidak ditemukan"));

            await repository.DeleteTodoAsync(id);
            return Results.Ok(Helper.CreateResponse<string[]>(null, ""));
        }
    }
}