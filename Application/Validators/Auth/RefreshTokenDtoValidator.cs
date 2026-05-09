using Application.DTOs.Auth;
using FluentValidation;

namespace Application.Validators.Auth
{
    public class RefreshTokenDtoValidator : AbstractValidator<RefreshTokenDto>
    {
        public RefreshTokenDtoValidator()
        {
            RuleFor(x => x.RefreshToken)
                .NotEmpty()
                .MinimumLength(32)
                .MaximumLength(512);
        }
    }
}
