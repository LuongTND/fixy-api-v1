using Application.DTOs.Voucher;
using Domain.Enum;
using FluentValidation;

namespace Application.Validators
{
    public class CreateVoucherDtoValidator : AbstractValidator<CreateVoucherDto>
    {
        public CreateVoucherDtoValidator()
        {
            RuleFor(x => x.Code)
                .NotEmpty()
                .MaximumLength(50);

            RuleFor(x => x.ExpiresAt)
                .GreaterThan(x => x.StartsAt)
                .WithMessage("ExpiresAt must be after StartsAt");

            RuleFor(x => x.MaxDiscount)
                .NotNull()
                .WithMessage("MaxDiscount is required for Percent type vouchers")
                .When(x => x.Type == VoucherType.Percent);

            RuleFor(x => x.Value)
                .InclusiveBetween(1, 100)
                .WithMessage("Percent value must be between 1 and 100")
                .When(x => x.Type == VoucherType.Percent);

            RuleFor(x => x.Value)
                .GreaterThan(0)
                .WithMessage("Fixed discount value must be greater than 0")
                .When(x => x.Type == VoucherType.Fixed);

            RuleFor(x => x.MinOrderValue)
                .GreaterThanOrEqualTo(0);

            RuleFor(x => x.MaxUsage)
                .GreaterThan(0)
                .When(x => x.MaxUsage.HasValue);

            RuleFor(x => x.MaxUsagePerUser)
                .GreaterThan(0)
                .When(x => x.MaxUsagePerUser.HasValue);
        }
    }
}
