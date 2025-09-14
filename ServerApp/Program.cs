using System.ComponentModel.DataAnnotations;
using FluentValidation;
using FluentValidation.AspNetCore;
using FullStackApp.Models;
using Microsoft.EntityFrameworkCore;
using SharedApp.Data;
using SharedApp.Models;
using SharedApp.Validators;

var builder = WebApplication.CreateBuilder(args);

// EF Core with MSSQL
builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("StoreDbConnection")
                           ?? throw new InvalidOperationException("Connection string 'StoreDbConnection' not found.");
    options.UseSqlServer(connectionString);
});

builder.Services.AddControllers()
        .AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<ProductValidator>());

builder.Services.AddScoped<ISupplierRepository, SupplierRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();


builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5047")
        .AllowAnyHeader()
        .AllowAnyMethod();
    });
});

// Fluent Validation Service
builder.Services
    .AddEndpointsApiExplorer()
    .AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<ProductValidator>());

var app = builder.Build();

// Global error handler for bad requests 
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (BadHttpRequestException ex)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;

        var error = ex.Message.Contains("Implicit body inferred")
            ? "Request body is required."
            : ex.Message;

        await context.Response.WriteAsJsonAsync(new { error });
    }

});

app.UseAuthentication();
app.UseAuthorization();
app.UseRouting();
app.UseCors();

app.MapControllers();


SeedData.EnsurePopulated(app);
app.Run();