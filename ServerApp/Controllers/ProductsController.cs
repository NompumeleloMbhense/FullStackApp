using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SharedApp.Validators;
using SharedApp.Models;
using SharedApp.Models.Dto;

namespace ServerApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductRepository _repo;
        private readonly IValidator<ProductCreateDto> _createValidator;
        private readonly IValidator<ProductUpdateDto> _updateValidator;
        private readonly IValidator<ProductPatchDto> _patchValidator;

        public ProductsController(
            IProductRepository repo,
            IValidator<ProductCreateDto> createValidator,
            IValidator<ProductUpdateDto> updateValidator,
            IValidator<ProductPatchDto> patchValidator)
        {
            _repo = repo;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _patchValidator = patchValidator;
        }

        // GET: api/products
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var products = await _repo.GetAllAsync();

            var result = products.Select(p => new ProductReadDto
            {
                ProductId = p.ProductId,
                Name = p.Name,
                Price = p.Price,
                Stock = p.Stock,
                Category = p.Category,
                Available = p.Available,
                SupplierId = p.SupplierId
            });

            return Ok(result);
        }

        // GET: api/products/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _repo.GetByIdAsync(id);
            if (product is null)
                return NotFound(new { error = "Product not found" });

            var result = new ProductReadDto
            {
                ProductId = product.ProductId,
                Name = product.Name,
                Price = product.Price,
                Stock = product.Stock,
                Category = product.Category,
                Available = product.Available,
                SupplierId = product.SupplierId
            };

            return Ok(result);
        }

        // POST: api/products
        [HttpPost]
        public async Task<IActionResult> Create(ProductCreateDto dto)
        {
            var validationResult = _createValidator.Validate(dto);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors);

            var product = new Product
            {
                Name = dto.Name,
                Price = dto.Price,
                Stock = dto.Stock,
                Category = dto.Category,
                SupplierId = dto.SupplierId
            };

            await _repo.AddAsync(product);

            var result = new ProductReadDto
            {
                ProductId = product.ProductId,
                Name = product.Name,
                Price = product.Price,
                Stock = product.Stock,
                Category = product.Category,
                Available = product.Available,
                SupplierId = product.SupplierId
            };

            return CreatedAtAction(nameof(GetById), new { id = product.ProductId }, result);
        }

        // PUT: api/products/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, ProductUpdateDto dto)
        {
            var existingProduct = await _repo.GetByIdAsync(id);
            if (existingProduct is null)
                return NotFound(new { error = "Product not found" });

            var validationResult = _updateValidator.Validate(dto);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors);

            existingProduct.Name = dto.Name;
            existingProduct.Price = dto.Price;
            existingProduct.Stock = dto.Stock;
            existingProduct.Category = dto.Category;
            existingProduct.SupplierId = dto.SupplierId;

            await _repo.UpdateAsync(existingProduct);

            var result = new ProductReadDto
            {
                ProductId = existingProduct.ProductId,
                Name = existingProduct.Name,
                Price = existingProduct.Price,
                Stock = existingProduct.Stock,
                Category = existingProduct.Category,
                Available = existingProduct.Available,
                SupplierId = existingProduct.SupplierId
            };

            return Ok(result);
        }

        // PATCH: api/products/5
        [HttpPatch("{id:int}")]
        public async Task<IActionResult> Patch(int id, ProductPatchDto dto)
        {
            var existingProduct = await _repo.GetByIdAsync(id);
            if (existingProduct is null)
                return NotFound(new { error = "Product not found" });

            var validationResult = _patchValidator.Validate(dto);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors);

            if (dto.Name is not null)
                existingProduct.Name = dto.Name;
            if (dto.Price is not null)
                existingProduct.Price = dto.Price.Value;
            if (dto.Stock is not null)
                existingProduct.Stock = dto.Stock.Value;
            if (dto.Category is not null)
                existingProduct.Category = dto.Category;
            if (dto.SupplierId is not null)
                existingProduct.SupplierId = dto.SupplierId.Value;

            await _repo.UpdateAsync(existingProduct);

            var result = new ProductReadDto
            {
                ProductId = existingProduct.ProductId,
                Name = existingProduct.Name,
                Price = existingProduct.Price,
                Stock = existingProduct.Stock,
                Category = existingProduct.Category,
                SupplierId = existingProduct.SupplierId
            };

            return Ok(result);
        }

        // DELETE: api/products/5
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