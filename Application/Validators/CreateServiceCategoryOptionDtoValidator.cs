using Application.DTOs.ServiceCategory;
using FluentValidation;

namespace Application.Validators
{
    public class CreateServiceCategoryOptionDtoValidator : AbstractValidator<CreateServiceCategoryOptionDto>
    {
        public CreateServiceCategoryOptionDtoValidator()
        {
            RuleFor(x => x.DurationMinutes)
                .GreaterThan(0)
                .WithMessage("DurationMinutes must be greater than 0");

            RuleFor(x => x.Price)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Price must be greater than or equal to 0");

            RuleFor(x => x.SortOrder)
                .GreaterThanOrEqualTo(0)
                .When(x => x.SortOrder.HasValue);
        }
    }
}
