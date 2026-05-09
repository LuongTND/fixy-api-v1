using Application.DTOs.Auth;
using FluentValidation;

namespace Application.Validators.Auth
{
    public class SignupDtoValidator : AbstractValidator<SignupDto>
    {
        public SignupDtoValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress()
                .MaximumLength(256);

            RuleFor(x => x.Password)
                .NotEmpty()
                .MinimumLength(8)
                .Matches("[A-Z]")
                .Matches("[a-z]")
                .Matches("[0-9]")
                .Matches("[^a-zA-Z0-9]");

            RuleFor(x => x.Phone)
                .Matches(@"^0[0-9]{9}$")
                .When(x => !string.IsNullOrWhiteSpace(x.Phone))
                .WithMessage("Phone is invalid");
        }
    }
}
