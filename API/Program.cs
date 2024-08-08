using Core.Interfaces;
using Infrastructure.Repositories;
using Core.Entities;
using API.Utilities;


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

app.MapGet("/products", async (IProductRepository repository) => await repository.GetAllProductsAsync());

app.MapGet("/products/{id}", async (int id, IProductRepository repository) =>
{
    var item = await repository.GetProductByIdAsync(id);
    return item is not null ? Results.Ok(item) : Results.NotFound();
});

app.MapPost("/products", async (Product item, IProductRepository repository) =>
{
    var createdItem = await repository.AddProductAsync(item);
    return Results.Created($"/products/{createdItem.Id}", createdItem);
});

app.MapPut("/products/{id}", async (int id, Product updatedItem, IProductRepository repository) =>
{
    var item = await repository.GetProductByIdAsync(id);
    if (item is null) return Results.NotFound();

    item.Name = updatedItem.Name;
    item.Price = updatedItem.Price;

    await repository.UpdateProductAsync(item);
    return Results.NoContent();
});

app.MapDelete("/products/{id}", async (int id, IProductRepository repository) =>
{
    var item = await repository.GetProductByIdAsync(id);
    if (item is null) return Results.NotFound();

    await repository.DeleteProductAsync(id);
    return Results.NoContent();
});

app.MapGet("/proxy", async () =>
{
    string url = "https://catfact.ninja/fact";
    var result = await Utility.MakeHttpRequest(url, HttpMethod.Get);
    return Results.Ok(result);
});


// Endpoint to convert string to Base64
app.MapPost("/convert-to-base64", (InputModel model) =>
{
    string base64 = Utility.ConvertStringToBase64(model.Input);
    return Results.Ok(base64);
});


app.Run();
