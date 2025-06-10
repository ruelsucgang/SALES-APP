using FluentValidation;
using sales.infra.DTOs;

namespace sales.API.Validators
{
    public class RequestOtpDtoValidator : AbstractValidator<RequestOtpDto>
    {
        public RequestOtpDtoValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email format.")
                .MaximumLength(150).WithMessage("Email cannot exceed 150 characters.");
        }
    }
}