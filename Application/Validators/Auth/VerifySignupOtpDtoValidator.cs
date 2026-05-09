using Application.DTOs.Auth;
using FluentValidation;

namespace Application.Validators.Auth
{
    public class VerifySignupOtpDtoValidator : AbstractValidator<VerifySignupOtpDto>
    {
        public VerifySignupOtpDtoValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress()
                .MaximumLength(256);

            RuleFor(x => x.Otp)
                .NotEmpty()
                .Matches(@"^[0-9]{6}$");
        }
    }
}
