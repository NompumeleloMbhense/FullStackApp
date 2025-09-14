using FluentValidation;
using FullStackApp.Models;
using Microsoft.AspNetCore.Mvc;
using SharedApp.Models;

namespace ServerApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductRepository _repo;
        private readonly IValidator<Product> _validator;
        private readonly IValidator<ProductPatchDto> _patchValidator;

        public ProductsController(IProductRepository repo, IValidator<Product> validator, IValidator<ProductPatchDto> patchValidator)
        {
            _repo = repo;
            _validator = validator;
            _patchValidator = patchValidator;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var products = await _repo.GetAllAsync();
            return Ok(products);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _repo.GetByIdAsync(id);
            if (product is null)
                return NotFound(new { error = "Product not found" });

            return Ok(product);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Product newProduct)
        {
            var validationResult = _validator.Validate(newProduct);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors);

            await _repo.AddAsync(newProduct);

            return CreatedAtAction(nameof(GetById), new { id = newProduct.ProductId }, newProduct);
        }

        [HttpPut("{int:id}")]
        public async Task<IActionResult> Update(int id, Product updatedProduct)
        {
            var existingProduct = await _repo.GetByIdAsync(id);

            if (existingProduct is null)
                return NotFound(new { error = "Product not found" });

            var validationResult = _validator.Validate(updatedProduct);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors);

            existingProduct.Name = updatedProduct.Name;
            existingProduct.Price = updatedProduct.Price;
            existingProduct.Stock = updatedProduct.Stock;
            existingProduct.Category = updatedProduct.Category;
            existingProduct.SupplierId = updatedProduct.SupplierId;

            await _repo.UpdateAsync(existingProduct);

            return Ok(existingProduct);
        }

        [HttpPatch("{int:id}")]
        public async Task<IActionResult> Patch(int id, ProductPatchDto partialUpdate)
        {
            var existingProduct = await _repo.GetByIdAsync(id);
            if (existingProduct is null)
                return NotFound(new { error = "Product not found" });

            var validationResult = _patchValidator.Validate(partialUpdate);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors);

            if (partialUpdate.Name is not null) existingProduct.Name = partialUpdate.Name;
            if (partialUpdate.Price.HasValue) existingProduct.Price = partialUpdate.Price.Value;
            if (partialUpdate.Stock.HasValue) existingProduct.Stock = partialUpdate.Stock.Value;
            if (partialUpdate.Category is not null) existingProduct.Category = partialUpdate.Category;

            await _repo.UpdateAsync(existingProduct);

            return Ok(existingProduct);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var existingProduct = await _repo.GetByIdAsync(id);
            if (existingProduct is null)
                return NotFound(new { error = "Product not found" });

            await _repo.DeleteAsync(id);
            return NoContent();
        }
    }
}