using Application.DTOs.Auth;
using FluentValidation;

namespace Application.Validators.Auth
{
    public class RequestPasswordOtpDtoValidator : AbstractValidator<RequestPasswordOtpDto>
    {
        public RequestPasswordOtpDtoValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress()
                .MaximumLength(256);
        }
    }
}
