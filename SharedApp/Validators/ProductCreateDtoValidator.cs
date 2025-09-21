using FluentValidation;
using SharedApp.Models.Dto;

namespace SharedApp.Validators
{
    public class ProductCreateDtoValidator : AbstractValidator<ProductCreateDto>
    {
        public ProductCreateDtoValidator()
        {
            RuleFor(p => p.Name).NotEmpty().Length(2, 100);
            RuleFor(p => p.Price).GreaterThan(0);
            RuleFor(p => p.Stock).GreaterThanOrEqualTo(0);
            RuleFor(p => p.Category).NotEmpty();
            RuleFor(p => p.SupplierId).GreaterThan(0);
        }
    }

    public class ProductUpdateDtoValidator : AbstractValidator<ProductUpdateDto>
    {
        public ProductUpdateDtoValidator()
        {
            RuleFor(p => p.Name).NotEmpty().Length(2, 100);
            RuleFor(p => p.Price).GreaterThan(0);
            RuleFor(p => p.Stock).GreaterThanOrEqualTo(0);
            RuleFor(p => p.Category).NotEmpty();
            RuleFor(p => p.SupplierId).GreaterThan(0);
        }
    }

    public class ProductPatchDtoValidator : AbstractValidator<ProductPatchDto>
    {
        public ProductPatchDtoValidator()
        {
            When(p => p.Name is not null, () =>
            {
                RuleFor(p => p.Name!).Length(2, 100);
            });

            When(p => p.Price is not null, () =>
            {
                RuleFor(p => p.Price!.Value).GreaterThan(0);
            });

            When(p => p.Stock is not null, () =>
            {
                RuleFor(p => p.Stock!.Value).GreaterThanOrEqualTo(0);
            });

            When(p => p.Category is not null, () =>
            {
                RuleFor(p => p.Category!).NotEmpty();
            });

            When(p => p.SupplierId is not null, () =>
            {
                RuleFor(p => p.SupplierId!.Value).GreaterThan(0);
            });
        }
    }
}