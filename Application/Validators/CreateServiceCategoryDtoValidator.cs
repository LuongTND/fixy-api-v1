using Application.DTOs.ServiceCategory;
using FluentValidation;

namespace Application.Validators
{
    public class CreateServiceCategoryDtoValidator : AbstractValidator<CreateServiceCategoryDto>
    {
        public CreateServiceCategoryDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty();

            RuleFor(x => x.SortOrder)
                .GreaterThanOrEqualTo(0)
                .When(x => x.SortOrder.HasValue);
        }
    }
}
