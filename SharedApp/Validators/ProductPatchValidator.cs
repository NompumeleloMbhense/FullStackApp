using FluentValidation;
using FullStackApp.Models;

namespace SharedApp.Validators
{
    public class ProductPatchValidator : AbstractValidator<ProductPatchDto>
    {
        public ProductPatchValidator()
        {
            RuleFor(p => p.Name)
                .NotEmpty().WithMessage("Name cannot be empty")
                .When(p => p.Name is not null);

            RuleFor(p => p.Price)
                .GreaterThan(0).WithMessage("Price must be greater than zero")
                .When(p => p.Price is not null);

            RuleFor(p => p.Stock)
                .GreaterThanOrEqualTo(0).WithMessage("Stock must be zero or more")
                .When(p => p.Stock is not null);

            RuleFor(p => p.Categories)
                .Must(c => c is not null && c.Any())
                .WithMessage("At least one category is required")
                .When(p => p.Categories is not null);
        }
    }
}