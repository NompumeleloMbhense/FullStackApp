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
        new Product
        {
            Id = 1,
            Name = "Laptop",
            Price = 12000.50,
            Stock = 25,
            Categories = new List<string> {"Electronics", "Computers"},
            Supplier = new Supplier { Name="Tech World", Location="Roodepoort"}
        },    
        new Product
        {
            Id = 2,
            Name = "Headphones",
            Price = 450.00,
            Stock = 100,
            Categories = new List<string> {"Accessories", "Audio"},
            Supplier = new Supplier {Name="Sound Co", Location="Randburg"}
        }
    };
});

app.Run();