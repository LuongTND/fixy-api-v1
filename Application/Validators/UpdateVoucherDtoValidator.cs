using Application.DTOs.Voucher;
using FluentValidation;

namespace Application.Validators
{
    public class UpdateVoucherDtoValidator : AbstractValidator<UpdateVoucherDto>
    {
        public UpdateVoucherDtoValidator()
        {
            RuleFor(x => x.Value)
                .GreaterThan(0)
                .When(x => x.Value.HasValue);

            RuleFor(x => x.MinOrderValue)
                .GreaterThanOrEqualTo(0)
                .When(x => x.MinOrderValue.HasValue);

            RuleFor(x => x.MaxUsage)
                .GreaterThan(0)
                .When(x => x.MaxUsage.HasValue);

            RuleFor(x => x.MaxUsagePerUser)
                .GreaterThan(0)
                .When(x => x.MaxUsagePerUser.HasValue);
        }
    }
}
