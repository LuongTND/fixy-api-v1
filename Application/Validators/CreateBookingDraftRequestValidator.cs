using Application.DTOs.BookingDraft;
using Domain.Enum;
using FluentValidation;

namespace Application.Validators
{
    public class CreateBookingDraftRequestValidator : AbstractValidator<CreateBookingDraftRequest>
    {
        public CreateBookingDraftRequestValidator()
        {
            RuleFor(x => x.CategoryId)
                .NotEmpty();

            RuleFor(x => x.Description)
                .NotEmpty();

            RuleFor(x => x.MediaIds)
                .Must(list => list.Count <= 5)
                .WithMessage("Maximum 5 media items allowed");

            RuleFor(x => x.ScheduledAt)
                .NotNull()
                .When(x => x.ScheduledType == BookingScheduledType.Scheduled);

            RuleFor(x => x)
                .Must(x =>
                    x.AddressId.HasValue ||
                    (!string.IsNullOrWhiteSpace(x.Address) && x.Lat.HasValue && x.Lng.HasValue))
                    .WithMessage("Address information is required");

            RuleFor(x => x.WorkerId)
                .NotEmpty()
                .When(x => !x.AutoMatch);
        }
    }
}
