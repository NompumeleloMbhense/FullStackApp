using FluentValidation;
using SharedApp.Models;
using SharedApp.Validators;

namespace SharedApp.Validators
{
    public class ProductValidator : AbstractValidator<Product>
    {
        public ProductValidator()
        {
            RuleFor(p => p.Name)
                .NotEmpty().WithMessage("Product name is required")
                .MaximumLength(200);

            RuleFor(p => p.Price)
                .GreaterThan(0).WithMessage("Price must be greater than 0");

            RuleFor(p => p.Stock)
                .GreaterThanOrEqualTo(0).WithMessage("Stock cannot be negative");

            RuleFor(p => p.Category)
                .NotEmpty().WithMessage("Category is required")
                .MaximumLength(100);
                

            RuleFor(p => p.Supplier)
                .SetValidator(new SupplierValidator()!)
                .When(p => p.Supplier != null);
        }
    }
}