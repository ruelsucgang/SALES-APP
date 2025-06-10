using FluentValidation;
using sales.infra.DTOs;

namespace sales.API.Validators
{
    public class CustomerUpdateDtoValidator : AbstractValidator<CustomerUpdateDto>
    {
        public CustomerUpdateDtoValidator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Full name is required.")
                .MaximumLength(150).WithMessage("Full name cannot exceed 150 characters.");

            RuleFor(x => x.BillingAddress)
                .NotEmpty().WithMessage("Billing address is required.")
                .MaximumLength(250).WithMessage("Billing address cannot exceed 250 characters.");

            RuleFor(x => x.ContactNumber)
                .NotEmpty().WithMessage("Contact number is required.")
                .MaximumLength(50).WithMessage("Contact number cannot exceed 50 characters.");
        }
    }
}