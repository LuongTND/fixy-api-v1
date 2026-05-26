using Application.DTOs.Voucher;
using FluentValidation;

namespace Application.Validators
{
    public class ApplyVoucherRequestValidator : AbstractValidator<ApplyVoucherRequest>
    {
        public ApplyVoucherRequestValidator()
        {
            RuleFor(x => x.Code)
                .NotEmpty()
                .MaximumLength(50);

            RuleFor(x => x.BookingId)
                .NotEmpty();
        }
    }
}
