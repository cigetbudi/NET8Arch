using Core.Interfaces;
using Infrastructure.Repositories;
using Core.Entities;
using API.Helpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;


var builder = WebApplication.CreateBuilder(args);

// Add DI
var configuration = builder.Configuration;

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

var mssqlConn = configuration.GetConnectionString("MSSQLConn")
                ?? throw new ArgumentNullException("MSSQLConn", "Connection string cannot be null.");
var pgsqlConn = configuration.GetConnectionString("PGSQLConn")
                ?? throw new ArgumentNullException("PGSQLConn", "Connection string cannot be null.");

builder.Services.AddScoped<ITodoRepository>(provider => new TodoRepository(mssqlConn));
builder.Services.AddScoped<IProductRepository>(provider => new ProductRepository(pgsqlConn));

var app = builder.Build();

// Pipeline
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Openned Endpoints
app.MapPost("/login", (LoginReq request, IConfiguration configuration1, IConfiguration config) =>
{
    var credentials = config.GetSection("Credentials");
    var username = credentials["Username"];
    var password = credentials["Password"];

    if (request.Username == username && request.Password == password)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(jwtSettings["SecretKey"] ?? throw new ArgumentNullException("SecretKey", "SecretKey cannot be null or empty."));
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, request.Username)
            }),
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            Issuer = jwtSettings["Issuer"],
            Audience = jwtSettings["Audience"]
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        return Results.Ok(new { Token = tokenString });
    }
    return Results.Unauthorized();
});

// Secured Endpoints
app.MapGet("/", () => "Hello World!").RequireAuthorization();

app.MapGet("/todos", async (ITodoRepository repository) => await repository.GetAllTodosAsync()).RequireAuthorization();

app.MapGet("/todos/{id}", async (int id, ITodoRepository repository) =>
{
    var item = await repository.GetTodoByIdAsync(id);
    return item is not null ? Results.Ok(item) : Results.NotFound();
}).RequireAuthorization();

app.MapPost("/todos", async (TodoItem item, ITodoRepository repository) =>
{
    var createdItem = await repository.AddTodoAsync(item);
    return Results.Created($"/todos/{createdItem.Id}", createdItem);
}).RequireAuthorization();

app.MapPut("/todos/{id}", async (int id, TodoItem updatedItem, ITodoRepository repository) =>
{
    var item = await repository.GetTodoByIdAsync(id);
    if (item is null) return Results.NotFound();

    item.Title = updatedItem.Title;
    item.IsCompleted = updatedItem.IsCompleted;

    await repository.UpdateTodoAsync(item);
    return Results.NoContent();
}).RequireAuthorization();

app.MapDelete("/todos/{id}", async (int id, ITodoRepository repository) =>
{
    var item = await repository.GetTodoByIdAsync(id);
    if (item is null) return Results.NotFound();

    await repository.DeleteTodoAsync(id);
    return Results.NoContent();
}).RequireAuthorization();

app.MapGet("/products", async (IProductRepository repository) => await repository.GetAllProductsAsync()).RequireAuthorization();

app.MapGet("/products/{id}", async (int id, IProductRepository repository) =>
{
    var item = await repository.GetProductByIdAsync(id);
    return item is not null ? Results.Ok(item) : Results.NotFound();
}).RequireAuthorization();

app.MapPost("/products", async (Product item, IProductRepository repository) =>
{
    var createdItem = await repository.AddProductAsync(item);
    return Results.Created($"/products/{createdItem.Id}", createdItem);
}).RequireAuthorization();

app.MapPut("/products/{id}", async (int id, Product updatedItem, IProductRepository repository) =>
{
    var item = await repository.GetProductByIdAsync(id);
    if (item is null) return Results.NotFound();

    item.Name = updatedItem.Name;
    item.Price = updatedItem.Price;

    await repository.UpdateProductAsync(item);
    return Results.NoContent();
}).RequireAuthorization();

app.MapDelete("/products/{id}", async (int id, IProductRepository repository) =>
{
    var item = await repository.GetProductByIdAsync(id);
    if (item is null) return Results.NotFound();

    await repository.DeleteProductAsync(id);
    return Results.NoContent();
}).RequireAuthorization();

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
