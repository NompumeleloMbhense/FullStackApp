using System.ComponentModel.DataAnnotations;
using FluentValidation;
using FluentValidation.AspNetCore;
using FullStackApp.Models;
using SharedApp.Models;
using SharedApp.Validators;

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
var suppliers = new List<Supplier>
{
    new Supplier
    {
        Id = 1,
        Name = "Tech World",
        Location = "Roodepoort"
    },
    new Supplier
    {
        Id = 2,
        Name = "Sound Co",
        Location = "Randburg"
    }
};


// In-memory product list 
var products = new List<Product>()
{
    new Product
    {
        Id = 1,
        Name = "Laptop",
        Price = 12000.50M,
        Stock = 25,
        Categories = new List<string> {"Electronics", "Computers"},
        SupplierId = 1
    },
    new Product
    {
        Id = 2,
        Name = "Headphones",
        Price = 450.00M,
        Stock = 100,
        Categories = new List<string> { "Accessories", "Audio" },
        SupplierId = 2
    }
};


// --------------------------------- SUPPLIERS ENDPOINTS -------------------------------


// Get All Suppliers
app.MapGet("/api/suppliers", () =>
{
    return Results.Ok(suppliers);
});

// Get Supplier By Id
app.MapGet("/api/suppliers/{id:int}", (int id) =>
{
    var supplier = suppliers.FirstOrDefault(s => s.Id == id);

    return supplier is not null ? Results.Ok(supplier) : Results.NotFound(new { error = "Supplier not found" });
});

// Create Supplier
app.MapPost("/api/suppliers", (Supplier? newSupplier, IValidator<Supplier> validator) =>
{
    
    var result = validator.Validate(newSupplier);
    if (!result.IsValid)
        return Results.BadRequest(result.Errors);

    newSupplier.Id = suppliers.Any() ? suppliers.Max(s => s.Id) + 1 : 1;
    suppliers.Add(newSupplier);

    return Results.Created($"/api/suppliers/{newSupplier.Id}", newSupplier);
});

// Update Supplier 
app.MapPut("/api/suppliers/{id:int}", (int id, Supplier updatedSupplier, IValidator<Supplier> validator) =>
{
    var supplier = suppliers.FirstOrDefault(s => s.Id == id);
    if (supplier is null)
        return Results.NotFound(new { error = "Supplier not found" });

    var result = validator.Validate(updatedSupplier);
    if (!result.IsValid)
        return Results.BadRequest(result.Errors);

    supplier.Name = updatedSupplier.Name;
    supplier.Location = updatedSupplier.Location;

    return Results.Ok(supplier);
});

// Partially update supplier
app.MapPatch("/api/suppliers/{id:int}", (int id, SupplierPatchDto supplierUpdate, IValidator<SupplierPatchDto> validator) =>
{
    var supplier = suppliers.FirstOrDefault(s => s.Id == id);

    if (supplier is null)
        return Results.NotFound(new { error = "Supplier not found" });

    var result = validator.Validate(supplierUpdate);
    if (!result.IsValid)
        return Results.BadRequest(result.Errors);


    if (supplier is not null && supplierUpdate.Name is not null)
        supplier.Name = supplierUpdate.Name;

    if (supplier is not null && supplierUpdate.Location is not null)
        supplier.Location = supplierUpdate.Location;

    return Results.Ok(supplier);
});

// When a user send a PATCH request without an Id
app.MapPatch("/api/suppliers", () =>
{
    return Results.BadRequest(new { error = "Supplier ID is required in the URL." });
});


// Delete Supplier
app.MapDelete("/api/suppliers/{id:int}", (int id) =>
{
    var supplier = suppliers.FirstOrDefault(s => s.Id == id);
    if (supplier is null)
        return Results.NotFound(new { error = "Supplier not found" });

    suppliers.Remove(supplier);

    // Also detach from products (set SupplierId = 0 for orphaned products)
    foreach (var product in products.Where(p => p.SupplierId == id))
    {
        product.SupplierId = 0;
    }

    return Results.NoContent();
});

// API endpoint to get all products
app.MapGet("/api/products", () =>
{
    return Results.Ok(products);
});

// API endpoint to get a product by ID
app.MapGet("/api/products/{id:int}", (int id) =>
{
    // Find the product by ID
    var product = products.FirstOrDefault(p => p.Id == id);
    return product is not null ? Results.Ok(product) : Results.NotFound(new { error = "Product not found" });

});


// Get supplier for a product
app.MapGet("/api/products/{id:int}/supplier", (int id) =>
{
    var product = products.FirstOrDefault(p => p.Id == id);
    if (product is null || product.Supplier is null)
        return Results.NotFound(new { error = "Supplier not found" });

    return Results.Ok(product.Supplier);
});



// API endpoint to add a new product
app.MapPost("/api/products", (Product newProduct, IValidator<Product> validator) =>
{

    // Validate the new product using FluentValidation
    var result = validator.Validate(newProduct);
    if (!result.IsValid)
        return Results.BadRequest(result.Errors);

    // Ensure products is not empty before incrementing ID
    newProduct.Id = products.Any() ? products.Max(p => p.Id) + 1 : 1;

    // Add the new product to the list
    products.Add(newProduct);

    // return the added product
    return Results.Created($"/api/products/{newProduct.Id}", newProduct);
});


// API endpoint to update an existing product
app.MapPut("/api/products/{id:int}", (int id, Product updatedProduct, IValidator<Product> validator) =>
{
    var existingProduct = products.FirstOrDefault(p => p.Id == id);

    if (existingProduct is null)
        return Results.NotFound(new { error = "Product not found" });


    // Validate the updated product using FluentValidation
    var result = validator.Validate(updatedProduct);
    if (!result.IsValid)
        return Results.BadRequest(result.Errors);


    // Update the existing product
    existingProduct.Name = updatedProduct.Name;
    existingProduct.Price = updatedProduct.Price;
    existingProduct.Stock = updatedProduct.Stock;
    existingProduct.Categories = updatedProduct.Categories ?? new List<string>();
    existingProduct.Supplier = updatedProduct.Supplier;

    return Results.Ok(existingProduct);

});

// API endpoint to partially update an existing product
app.MapPatch("/api/products/{id:int}", (int id, ProductPatchDto partialUpdate, IValidator<ProductPatchDto> validator) =>
{
    // Find the product by ID
    var existingProduct = products.FirstOrDefault(p => p.Id == id);

    if (existingProduct is null)
        return Results.NotFound(new { error = "Product not found" });

    // Validate the partial update using FluentValidation
    var result = validator.Validate(partialUpdate);
    if (!result.IsValid)
        return Results.BadRequest(result.Errors);


    // Name validation and update
    if (partialUpdate.Name is not null)
        existingProduct.Name = partialUpdate.Name;


    // Price validation & update
    if (partialUpdate.Price.HasValue)
        existingProduct.Price = partialUpdate.Price.Value;


    // Stock validation & update
    if (partialUpdate.Stock.HasValue)
        existingProduct.Stock = partialUpdate.Stock.Value;


    // Update categories
    if (partialUpdate.Categories is not null && partialUpdate.Categories.Any())
        existingProduct.Categories = partialUpdate.Categories;


    return Results.Ok(existingProduct);
});



// API endpoint to delete a product
app.MapDelete("/api/products/{id:int}", (int id) =>
{
    // Find the product by ID
    var existingProduct = products.FirstOrDefault(p => p.Id == id);

    if (existingProduct is null)
        return Results.NotFound(new { error = "Product not found" });

    // Remove the product from the list
    products.Remove(existingProduct);
    return Results.NoContent();

});


app.Run();