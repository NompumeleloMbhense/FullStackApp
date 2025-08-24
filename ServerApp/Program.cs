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

app.UseCors();

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
        Supplier = new Supplier { Name="Tech World", Location="Roodepoort"}
    },
    new Product
    {
        Id = 2,
        Name = "Headphones",
        Price = 450.00M,
        Stock = 100,
        Categories = new List<string> { "Accessories", "Audio" },
        Supplier = new Supplier { Name = "Sound Co", Location = "Randburg" }
    }
};

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

// update supplier for a product
app.MapPatch("/api/products/{id:int}/supplier", (int id, SupplierPatchDto supplierUpdate, IValidator<SupplierPatchDto> validator) =>
{
    var product = products.FirstOrDefault(p => p.Id == id);
    if (product is null)
        return Results.NotFound(new { error = "Product not found" });

    // Validate the supplier patch patch DTO
    var result = validator.Validate(supplierUpdate);
    if (!result.IsValid)
        return Results.BadRequest(result.Errors);


    //Ensure the supplier exists
    product.Supplier ??= new Supplier { Name = "Default Supplier Name" };


    // Apply updates
    if (supplierUpdate.Name is not null)
        product.Supplier. Name = supplierUpdate.Name;

    if (supplierUpdate.Location is not null)
            product.Supplier.Location = supplierUpdate.Location;

   

    return Results.Ok(product.Supplier);

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