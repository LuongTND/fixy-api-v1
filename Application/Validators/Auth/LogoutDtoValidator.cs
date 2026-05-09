using Application.DTOs.Auth;
using FluentValidation;

namespace Application.Validators.Auth
{
    public class LogoutDtoValidator : AbstractValidator<LogoutDto>
    {
        public LogoutDtoValidator()
        {
            RuleFor(x => x.RefreshToken)
                .NotEmpty()
                .MinimumLength(32)
                .MaximumLength(512);
        }
    }
}
