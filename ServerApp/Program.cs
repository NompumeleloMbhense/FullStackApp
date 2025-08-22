using System.ComponentModel.DataAnnotations;
using FullStackApp.Models;
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


// API endpoint to add a new product
app.MapPost("/api/products", (Product newProduct) =>
{

    // Automatically validate the new product using model validation
    var validationResults = new List<ValidationResult>();
    var context = new ValidationContext(newProduct);

    if (!Validator.TryValidateObject(newProduct, context, validationResults, true))
    {
        return Results.BadRequest(validationResults);
    }

    // Ensure products is not empty before incrementing ID
    newProduct.Id = products.Any() ? products.Max(p => p.Id) + 1 : 1;

    // Add the new product to the list
    products.Add(newProduct);

    // return the added product
    return Results.Created($"/api/products/{newProduct.Id}", newProduct);
});


// API endpoint to update an existing product
app.MapPut("/api/products/{id:int}", (int id, Product updatedProduct) =>
{
    var existingProduct = products.FirstOrDefault(p => p.Id == id);

    if (existingProduct is null)
        return Results.NotFound(new { error = "Product not found" });


    // Automatically validate the updated product using model validation
    var validationResults = new List<ValidationResult>();
    var context = new ValidationContext(updatedProduct);

    if (!Validator.TryValidateObject(updatedProduct, context, validationResults, true))
    {
        return Results.BadRequest(validationResults);
    }

    // Update the existing product
    existingProduct.Name = updatedProduct.Name;
    existingProduct.Price = updatedProduct.Price;
    existingProduct.Stock = updatedProduct.Stock;
    existingProduct.Categories = updatedProduct.Categories ?? new List<string>();
    existingProduct.Supplier = updatedProduct.Supplier;

    return Results.Ok(existingProduct);

});

// API endpoint to partially update an existing product
app.MapPatch("/api/products/{id:int}", (int id, ProductPatchDto partialUpdate) =>
{
    // Find the product by ID
    var existingProduct = products.FirstOrDefault(p => p.Id == id);
    if (existingProduct is null)
        return Results.NotFound(new { error = "Product not found" });

    var validationResults = new List<ValidationResult>();

    // Name validation and update
    if (partialUpdate.Name is not null)
    {
        var context = new ValidationContext(partialUpdate) { MemberName = nameof(ProductPatchDto.Name) };
        if (!Validator.TryValidateProperty(partialUpdate.Name, context, validationResults))
            return Results.BadRequest(validationResults);

        existingProduct.Name = partialUpdate.Name;
    }

    // Price validation & update
    if (partialUpdate.Price.HasValue)
    {
        var context = new ValidationContext(partialUpdate) { MemberName = nameof(ProductPatchDto.Price) };
        if (!Validator.TryValidateProperty(partialUpdate.Price, context, validationResults))
            return Results.BadRequest(validationResults);

        existingProduct.Price = partialUpdate.Price.Value;
    }

    // Stock validation & update
    if (partialUpdate.Stock.HasValue)
    {
        var context = new ValidationContext(partialUpdate) { MemberName = nameof(ProductPatchDto.Stock) };
        if (!Validator.TryValidateProperty(partialUpdate.Stock, context, validationResults))
            return Results.BadRequest(validationResults);

        existingProduct.Stock = partialUpdate.Stock.Value;
    }

    // Update categories
    if (partialUpdate.Categories is not null && partialUpdate.Categories.Any())
    {
        existingProduct.Categories = partialUpdate.Categories;
    }

    // Update supplier (if you want Supplier to be a simple string from DTO)
    if (partialUpdate.Supplier is not null)
    {
        if (existingProduct.Supplier is null)
        {
            existingProduct.Supplier = new Supplier { Name = string.Empty, Location = string.Empty };
        }

        if (partialUpdate.Supplier.Name is not null)
        {
            existingProduct.Supplier.Name = partialUpdate.Supplier.Name;
        }

        if (partialUpdate.Supplier.Location is not null)
        {
            existingProduct.Supplier.Location = partialUpdate.Supplier.Location;
        }
    }


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