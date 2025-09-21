using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SharedApp.Validators;
using SharedApp.Models;
using SharedApp.Models.Dto;


namespace ServerApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SuppliersController : ControllerBase
    {
        private readonly ISupplierRepository _repo;
        private readonly IValidator<SupplierCreateDto> _createValidator;
        private readonly IValidator<SupplierUpdateDto> _updateValidator;
        private readonly IValidator<SupplierPatchDto> _patchValidator;

        public SuppliersController(
            ISupplierRepository repo,
            IValidator<SupplierCreateDto> createValidator,
            IValidator<SupplierUpdateDto> updateValidator,
            IValidator<SupplierPatchDto> patchValidator)
        {
            _repo = repo;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _patchValidator = patchValidator;
        }

        // GET: api/suppliers
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var suppliers = await _repo.GetAllAsync();

            var result = suppliers.Select(s => new SupplierReadDto
            {
                SupplierId = s.SupplierId,
                Name = s.Name,
                Location = s.Location
            });

            return Ok(result);
        }

        // GET: api/suppliers/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var supplier = await _repo.GetByIdAsync(id);
            if (supplier is null)
                return NotFound(new { error = "Supplier not found" });

            var result = new SupplierReadDto
            {
                SupplierId = supplier.SupplierId,
                Name = supplier.Name,
                Location = supplier.Location
            };

            return Ok(result);
        }

        // POST: api/suppliers
        [HttpPost]
        public async Task<IActionResult> Create(SupplierCreateDto dto)
        {
            var validationResult = _createValidator.Validate(dto);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors);

            var supplier = new Supplier
            {
                Name = dto.Name,
                Location = dto.Location
            };

            await _repo.AddAsync(supplier);

            var result = new SupplierReadDto
            {
                SupplierId = supplier.SupplierId,
                Name = supplier.Name,
                Location = supplier.Location
            };

            return CreatedAtAction(nameof(GetById), new { id = supplier.SupplierId }, result);
        }


        // api/suppliers/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, SupplierUpdateDto dto)
        {
            var existingSupplier = await _repo.GetByIdAsync(id);
            if (existingSupplier is null)
                return NotFound(new { error = "Supplier not found" });

            var validationResult = _updateValidator.Validate(dto);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors);

            existingSupplier.Name = dto.Name;
            existingSupplier.Location = dto.Location;

            await _repo.UpdateAsync(existingSupplier);

            var result = new SupplierReadDto
            {
                SupplierId = existingSupplier.SupplierId,
                Name = existingSupplier.Name,
                Location = existingSupplier.Location
            };

            return Ok(result);
        }


        // PATCH: api/suppliers/5
        [HttpPatch("{id:int}")]
        public async Task<IActionResult> Patch(int id, SupplierPatchDto dto)
        {
            var existingSupplier = await _repo.GetByIdAsync(id);
            if (existingSupplier is null)
                return NotFound(new { error = "Supplier not found" });

            var validationResult = _patchValidator.Validate(dto);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors);

            if (dto.Name is not null)
                existingSupplier.Name = dto.Name;

            if (dto.Location is not null)
                existingSupplier.Location = dto.Location;

            await _repo.UpdateAsync(existingSupplier);

            var result = new SupplierReadDto
            {
                SupplierId = existingSupplier.SupplierId,
                Name = existingSupplier.Name,
                Location = existingSupplier.Location
            };

            return Ok(result);
        }

        // DELETE: api/suppliers/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var existingSupplier = await _repo.GetByIdAsync(id);
            if (existingSupplier is null)
                return NotFound(new { error = "Supplier not found" });

            await _repo.DeleteAsync(id);
            return NoContent();
        }
    }
}
