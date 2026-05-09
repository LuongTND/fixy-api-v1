using Application.DTOs.Auth;
using FluentValidation;

namespace Application.Validators.Auth
{
    public class ChangePasswordByOtpDtoValidator : AbstractValidator<ChangePasswordByOtpDto>
    {
        public ChangePasswordByOtpDtoValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress()
                .MaximumLength(256);

            RuleFor(x => x.Otp)
                .NotEmpty()
                .Matches(@"^[0-9]{6}$");

            RuleFor(x => x.NewPassword)
                .NotEmpty()
                .MinimumLength(8)
                .Matches("[A-Z]")
                .Matches("[a-z]")
                .Matches("[0-9]")
                .Matches("[^a-zA-Z0-9]");
        }
    }
}
