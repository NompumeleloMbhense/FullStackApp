using SharedApp.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5047")
        .AllowAnyHeader()
        .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors();

app.MapGet("/api/productlist", () =>
{
    return new[]
    {
        new { Id = 1, Name = "Laptop", Price = 12000.50, Stock = 25, Category = new {Id=101, Name = "Electronics"}},
        new { Id = 2, Name = "Headphones", Price = 450.00, Stock = 100, Category = new {Id=102, Name = "Accessories"}}
    };
});

app.Run();

