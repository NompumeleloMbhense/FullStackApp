using FluentValidation;
using SharedApp.Models;

namespace SharedApp.Validators
{
    public class SupplierValidator : AbstractValidator<Supplier>
    {
        public SupplierValidator()
        {
            RuleFor(s => s.Name)
                .NotEmpty().WithMessage("Supplier name is required")
                .MaximumLength(100);

            RuleFor(s => s.Location)
                .MaximumLength(200);
        }
    }
}