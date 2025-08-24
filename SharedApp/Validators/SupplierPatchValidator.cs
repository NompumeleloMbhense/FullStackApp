using FluentValidation;
using FullStackApp.Models;

namespace SharedApp.Validators
{
    public class SupplierPatchValidator : AbstractValidator<SupplierPatchDto>
    {
        public SupplierPatchValidator()
        {
            RuleFor(s => s.Name)
                .NotEmpty().WithMessage("Name cannot be empty")
                .When(s => s.Name is not null);

            RuleFor(s => s.Location)
                .NotEmpty().WithMessage("Supplier location cannot be empty")
                .When(s => s.Location is not null);
        }
    }
}