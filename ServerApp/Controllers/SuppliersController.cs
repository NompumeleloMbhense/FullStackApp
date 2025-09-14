using FluentValidation;
using SharedApp.Validators;
using Microsoft.AspNetCore.Mvc;
using SharedApp.Models;

namespace ServerApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SuppliersController : ControllerBase
    {
        private readonly ISupplierRepository _repo;
        private readonly IValidator<Supplier> _validator;
        private readonly IValidator<SupplierPatchDto> _patchValidator;

        public SuppliersController(ISupplierRepository repo, IValidator<Supplier> validator, IValidator<SupplierPatchDto> patchValidator)
        {
            _repo = repo;
            _validator = validator;
            _patchValidator = patchValidator;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var suppliers = await _repo.GetAllAsync();
            return Ok(suppliers);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var supplier = await _repo.GetByIdAsync(id);
            if (supplier is null)
                return NotFound(new { error = "Supplier not found" });

            return Ok(supplier);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Supplier newSupplier)
        {
            var validationResult = _validator.Validate(newSupplier);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors);

            await _repo.AddAsync(newSupplier);

            return CreatedAtAction(nameof(GetById), new { id = newSupplier.SupplierId }, newSupplier);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, Supplier updatedSupplier)
        {
            var existingSupplier = await _repo.GetByIdAsync(id);
            if (existingSupplier is null)
                return NotFound(new { error = "Supplier not found" });

            var validationResult = _validator.Validate(updatedSupplier);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors);

            existingSupplier.Name = updatedSupplier.Name;
            existingSupplier.Location = updatedSupplier.Location;

            await _repo.UpdateAsync(existingSupplier);

            return Ok(existingSupplier);
        }

        [HttpPatch("{id:int}")]
        public async Task<IActionResult> Patch(int id, SupplierPatchDto partialUpdate)
        {
            var existingSupplier = await _repo.GetByIdAsync(id);
            if (existingSupplier is null)
                return NotFound(new { error = "Supplier not found" });

            var validationResult = _patchValidator.Validate(partialUpdate);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors);

            if (partialUpdate.Name is not null)
                existingSupplier.Name = partialUpdate.Name;

            if (partialUpdate.Location is not null)
                existingSupplier.Location = partialUpdate.Location;

            await _repo.UpdateAsync(existingSupplier);

            return Ok(existingSupplier);
        }

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
