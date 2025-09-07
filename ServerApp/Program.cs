using System.ComponentModel.DataAnnotations;
using FluentValidation;
using FluentValidation.AspNetCore;
using FullStackApp.Models;
using Microsoft.EntityFrameworkCore;
using ServerApp.Data;
using SharedApp.Models;
using SharedApp.Validators;

var builder = WebApplication.CreateBuilder(args);

// EF Core with SQLite
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=app.db"));


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

app.UseCors();


// In-memory suppliers
// var suppliers = new List<Supplier>
// {
//     new Supplier
//     {
//         Id = 1,
//         Name = "Tech World",
//         Location = "Roodepoort"
//     },
//     new Supplier
//     {
//         Id = 2,
//         Name = "Sound Co",
//         Location = "Randburg"
//     }
// };


// // In-memory product list 
// var products = new List<Product>()
// {
//     new Product
//     {
//         Id = 1,
//         Name = "Laptop",
//         Price = 12000.50M,
//         Stock = 25,
//         Categories = new List<string> {"Electronics", "Computers"},
//         SupplierId = 1
//     },
//     new Product
//     {
//         Id = 2,
//         Name = "Headphones",
//         Price = 450.00M,
//         Stock = 100,
//         Categories = new List<string> { "Accessories", "Audio" },
//         SupplierId = 2
//     }
// };


// --------------------------------- SUPPLIERS ENDPOINTS -------------------------------


// Get All Suppliers
app.MapGet("/api/suppliers", async (AppDbContext db) =>
{
    var suppliers = await db.Suppliers
                        .Include(s => s.Products)
                        .ToListAsync();

    return Results.Ok(suppliers);
});

// Get Supplier By Id
app.MapGet("/api/suppliers/{id:int}", async (int id, AppDbContext db) =>
{
    var supplier = await db.Suppliers
                .Include(s => s.Products)
                .FirstOrDefaultAsync(s => s.SupplierId == id);

    return supplier is not null
        ? Results.Ok(supplier) 
        : Results.NotFound(new { error = "Supplier not found" });
});

// Create Supplier
app.MapPost("/api/suppliers", async (Supplier newSupplier, IValidator<Supplier> validator, AppDbContext db) =>
{

    var result = validator.Validate(newSupplier);
    if (!result.IsValid)
        return Results.BadRequest(result.Errors);

    db.Suppliers.Add(newSupplier);
    await db.SaveChangesAsync();

    return Results.Created($"/api/suppliers/{newSupplier.SupplierId}", newSupplier);
});


// Update Supplier 
app.MapPut("/api/suppliers/{id:int}", async (int id, Supplier updatedSupplier, IValidator<Supplier> validator, AppDbContext db) =>
{
    var supplier = await db.Suppliers.FindAsync(id);
    if (supplier is null)
        return Results.NotFound(new { error = "Supplier not found" });

    var result = validator.Validate(updatedSupplier);
    if (!result.IsValid)
        return Results.BadRequest(result.Errors);

    supplier.Name = updatedSupplier.Name;
    supplier.Location = updatedSupplier.Location;

    await db.SaveChangesAsync();

    return Results.Ok(supplier);
});

// Partially update supplier
app.MapPatch("/api/suppliers/{id:int}", async (int id, SupplierPatchDto supplierUpdate, IValidator<SupplierPatchDto> validator, AppDbContext db) =>
{
    var supplier = await db.Suppliers.FindAsync(id);

    if (supplier is null)
        return Results.NotFound(new { error = "Supplier not found" });

    var result = validator.Validate(supplierUpdate);
    if (!result.IsValid)
        return Results.BadRequest(result.Errors);


    if (supplierUpdate.Name is not null)
        supplier.Name = supplierUpdate.Name;

    if (supplierUpdate.Location is not null)
        supplier.Location = supplierUpdate.Location;

    await db.SaveChangesAsync();

    return Results.Ok(supplier);
});


// When a user send a PATCH request without an Id
app.MapPatch("/api/suppliers", () =>
{
    return Results.BadRequest(new { error = "Supplier ID is required in the URL." });
});


// Delete Supplier
app.MapDelete("/api/suppliers/{id:int}", async (int id, AppDbContext db) =>
{
    var supplier = await db.Suppliers
        .Include(s => s.Products)
        .FirstOrDefaultAsync(s => s.SupplierId == id);


    if (supplier is null)
        return Results.NotFound(new { error = "Supplier not found" });


    // Handle orphaned products
    foreach (var product in supplier.Products)
    {
        product.SupplierId = 0;
    }

    db.Suppliers.Remove(supplier);
    await db.SaveChangesAsync();
    
    return Results.NoContent();
});


// --------------------------------- PRODUCTS ENDPOINTS -------------------------------


// Get All Products (with Supplier)
app.MapGet("/api/products", async (AppDbContext db) =>
{
    var products = await db.Products
                           .Include(p => p.Supplier) // eager-load supplier
                           .ToListAsync();
    return Results.Ok(products);
});

// Get Product By Id (with Supplier)
app.MapGet("/api/products/{id:int}", async (int id, AppDbContext db) =>
{
    var product = await db.Products
                          .Include(p => p.Supplier)
                          .FirstOrDefaultAsync(p => p.ProductId == id);

    return product is not null 
        ? Results.Ok(product) 
        : Results.NotFound(new { error = "Product not found" });
});


// Get supplier for a product
app.MapGet("/api/products/{id:int}/supplier", async (int id, AppDbContext db) =>
{
    var product = await db.Products
                          .Include(p => p.Supplier)
                          .FirstOrDefaultAsync(p => p.SupplierId == id);


    if (product is null || product.Supplier is null)
        return Results.NotFound(new { error = "Supplier not found" });

    return Results.Ok(product.Supplier);
});


// Create a Product
app.MapPost("/api/products", async (Product newProduct, IValidator<Product> validator, AppDbContext db) =>
{

    // Validate the new product using FluentValidation
    var result = validator.Validate(newProduct);
    if (!result.IsValid)
        return Results.BadRequest(result.Errors);

    // ensure supplier exists if SupplierId is provided
    if (newProduct.SupplierId != 0)
    {
        var supplier = await db.Suppliers.FindAsync(newProduct.SupplierId);
        if (supplier is null)
            return Results.BadRequest(new { error = "Invalid SupplierId" });
    }

    // Add the new product to the list
    db.Products.Add(newProduct);

    await db.SaveChangesAsync();

    // return the added product
    return Results.Created($"/api/products/{newProduct.ProductId}", newProduct);
});


// Update Product (PUT)
app.MapPut("/api/products/{id:int}", async (int id, Product updatedProduct, IValidator<Product> validator, AppDbContext db) =>
{
    var existingProduct = await db.Products.FindAsync(id);
    if (existingProduct is null)
        return Results.NotFound(new { error = "Product not found" });

    var result = validator.Validate(updatedProduct);
    if (!result.IsValid)
        return Results.BadRequest(result.Errors);

    // update fields
    existingProduct.Name = updatedProduct.Name;
    existingProduct.Price = updatedProduct.Price;
    existingProduct.Stock = updatedProduct.Stock;
    existingProduct.Category = updatedProduct.Category;
    existingProduct.SupplierId = updatedProduct.SupplierId;

    await db.SaveChangesAsync();

    return Results.Ok(existingProduct);
});


// Partially Update Product (PATCH)
app.MapPatch("/api/products/{id:int}", async (int id, ProductPatchDto partialUpdate, IValidator<ProductPatchDto> validator, AppDbContext db) =>
{
    var existingProduct = await db.Products.FindAsync(id);
    if (existingProduct is null)
        return Results.NotFound(new { error = "Product not found" });

    var result = validator.Validate(partialUpdate);
    if (!result.IsValid)
        return Results.BadRequest(result.Errors);

    if (partialUpdate.Name is not null)
        existingProduct.Name = partialUpdate.Name;

    if (partialUpdate.Price.HasValue)
        existingProduct.Price = partialUpdate.Price.Value;

    if (partialUpdate.Stock.HasValue)
        existingProduct.Stock = partialUpdate.Stock.Value;

    if (partialUpdate.Category is not null)
        existingProduct.Category = partialUpdate.Category;

    await db.SaveChangesAsync();

    return Results.Ok(existingProduct);
});


// Delete Product
app.MapDelete("/api/products/{id:int}", async (int id, AppDbContext db) =>
{
    var existingProduct = await db.Products.FindAsync(id);
    if (existingProduct is null)
        return Results.NotFound(new { error = "Product not found" });

    db.Products.Remove(existingProduct);
    await db.SaveChangesAsync();

    return Results.NoContent();
});


app.Run();