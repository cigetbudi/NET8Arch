using Core.Interfaces;
using Infrastructure.Repositories;
using Core.Entities;

var builder = WebApplication.CreateBuilder(args);

// Add DI
var configuration = builder.Configuration;

builder.Services.AddScoped<ITodoRepository>(provider => new TodoRepository(configuration.GetConnectionString("MSSQLConn")));
builder.Services.AddScoped<IProductRepository>(provider => new ProductRepository(configuration.GetConnectionString("PGSQLConn")));

var app = builder.Build();

// Pipeline
app.UseHttpsRedirection();

// Endpoints
app.MapGet("/", () => "Hello World!");

app.MapGet("/todos", async (ITodoRepository repository) => await repository.GetAllTodosAsync());

app.MapGet("/todos/{id}", async (int id, ITodoRepository repository) =>
{
    var item = await repository.GetTodoByIdAsync(id);
    return item is not null ? Results.Ok(item) : Results.NotFound();
});

app.MapPost("/todos", async (TodoItem item, ITodoRepository repository) =>
{
    var createdItem = await repository.AddTodoAsync(item);
    return Results.Created($"/todos/{createdItem.Id}", createdItem);
});

app.MapPut("/todos/{id}", async (int id, TodoItem updatedItem, ITodoRepository repository) =>
{
    var item = await repository.GetTodoByIdAsync(id);
    if (item is null) return Results.NotFound();

    item.Title = updatedItem.Title;
    item.IsCompleted = updatedItem.IsCompleted;

    await repository.UpdateTodoAsync(item);
    return Results.NoContent();
});

app.MapDelete("/todos/{id}", async (int id, ITodoRepository repository) =>
{
    var item = await repository.GetTodoByIdAsync(id);
    if (item is null) return Results.NotFound();

    await repository.DeleteTodoAsync(id);
    return Results.NoContent();
});

app.Run();
