using FluentValidation;
using sales.infra.DTOs;

namespace sales.API.Validators
{
    public class CreateOrderDtoValidator : AbstractValidator<CreateOrderDto>
    {
        public CreateOrderDtoValidator()
        {
            RuleFor(x => x.OrderItems)
                .NotEmpty().WithMessage("Order must have at least one item.")
                .Must(items => items != null && items.Count > 0)
                .WithMessage("Order must have at least one item.");

            RuleForEach(x => x.OrderItems).ChildRules(item =>
            {
                item.RuleFor(x => x.ProductId)
                    .GreaterThan(0).WithMessage("Product ID must be greater than zero.");

                item.RuleFor(x => x.Quantity)
                    .GreaterThan(0).WithMessage("Quantity must be greater than zero.")
                    .LessThanOrEqualTo(1000).WithMessage("Quantity cannot exceed 1000.");
            });
        }
    }
}