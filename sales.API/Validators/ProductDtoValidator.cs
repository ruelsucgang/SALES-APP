using FluentValidation;
using sales.infra.DTOs;

namespace sales.API.Validators
{
    public class ProductDtoValidator : AbstractValidator<ProductDto>
    {
        public ProductDtoValidator()
        {
            RuleFor(p => p.Name)
                .NotEmpty()
                .WithMessage("Name is required.")
                .MaximumLength(100)
                .WithMessage("Name must not exceed 100 characters.");

            RuleFor(p => p.Price)
                .GreaterThan(0)
                .WithMessage("Price must be greater than zero.");

            RuleFor(p => p.Description)
                .MaximumLength(500)
                .WithMessage("Description must not exceed 500 characters.")
                .When(p => !string.IsNullOrEmpty(p.Description));
        }
    }
}
