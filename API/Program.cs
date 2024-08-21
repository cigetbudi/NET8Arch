using API.Handlers;
using API.Helpers;
using API.Validators;
using Core.Entities;
using Core.Interfaces;
using FluentValidation;
using FluentValidation.AspNetCore;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add DI
var configuration = builder.Configuration;

// DI FluentValidation
builder.Services.AddFluentValidationAutoValidation()
                .AddFluentValidationClientsideAdapters();

builder.Services.AddValidatorsFromAssemblyContaining<TodoItemValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<ProductValidator>();

// JWT configuration
var jwtSettings = configuration.GetSection("JwtSettings");
var key = Encoding.ASCII.GetBytes(jwtSettings["SecretKey"] ?? throw new ArgumentNullException("SecretKey", "SecretKey cannot be null or empty."));
builder.Services.AddAuthorization();
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

// DI Repositories

builder.Services.AddScoped<ITodoRepository>(provider =>
    new TodoRepository(configuration.GetConnectionString("MSSQLConn")
        ?? throw new ArgumentNullException("MSSQLConn", "Connection string cannot be null.")));

builder.Services.AddScoped<IProductRepository>(provider =>
    new ProductRepository(configuration.GetConnectionString("PGSQLConn")
        ?? throw new ArgumentNullException("PGSQLConn", "Connection string cannot be null.")));

var app = builder.Build();

// Pipeline
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Openned Endpoints
app.MapPost("/login", (LoginReq request, IConfiguration configuration) =>
{
    try
    {
        var tokenString = Helper.GenerateJwtToken(request, configuration);
        return Results.Ok(new { Token = tokenString });
    }
    catch (UnauthorizedAccessException)
    {
        return Results.Unauthorized();
    }
});

// Secured Endpoints
app.MapGet("/", () => "Hello World!").RequireAuthorization();

// Todo Endpoints
app.MapGet("/todos", async (ITodoRepository repository) => await TodoHandlers.GetAllTodosAsync(repository)).RequireAuthorization();
app.MapGet("/todos/{id}", async (int id, ITodoRepository repository) => await TodoHandlers.GetTodoByIdAsync(id, repository)).RequireAuthorization();
app.MapPost("/todos", async (TodoItem item, ITodoRepository repository, IValidator<TodoItem> validator) => await TodoHandlers.AddTodoAsync(item, repository, validator)).RequireAuthorization();
app.MapPut("/todos/{id}", async (int id, TodoItem updatedItem, ITodoRepository repository, IValidator<TodoItem> validator) => await TodoHandlers.UpdateTodoAsync(id, updatedItem, repository, validator)).RequireAuthorization();
app.MapDelete("/todos/{id}", async (int id, ITodoRepository repository) => await TodoHandlers.DeleteTodoAsync(id, repository)).RequireAuthorization();

// Product Endpoints
app.MapGet("/products", async (IProductRepository repository) => await ProductHandlers.GetAllProductAsync(repository)).RequireAuthorization();
app.MapGet("/products/{id}", async (int id, IProductRepository repository) => await ProductHandlers.GetProductByIdAsync(id, repository)).RequireAuthorization();
app.MapPost("/products", async (Product item, IProductRepository repository, IValidator<Product> validator) => await ProductHandlers.AddProductAsync(item, repository, validator)).RequireAuthorization();
app.MapPut("/products/{id}", async (int id, Product updatedItem, IProductRepository repository, IValidator<Product> validator) => await ProductHandlers.UpdateProductAsync(id, updatedItem, repository, validator)).RequireAuthorization();
app.MapDelete("/products/{id}", async (int id, IProductRepository repository) => await ProductHandlers.DeleteProductAsync(id, repository)).RequireAuthorization();

app.MapGet("/proxy", async () =>
{
    string url = "https://catfact.ninja/fact";
    var result = await Helper.MakeHttpRequest(url, HttpMethod.Get);
    return Results.Ok(result);
}).RequireAuthorization();


// Endpoint to convert string to Base64
app.MapPost("/convert-to-base64", (InputModel model) =>
{
    string base64 = Helper.ConvertStringToBase64(model.Input);
    return Results.Ok(base64);
}).RequireAuthorization();


app.Run();
