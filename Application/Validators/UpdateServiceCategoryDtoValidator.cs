using Application.DTOs.ServiceCategory;
using FluentValidation;

namespace Application.Validators
{
    public class UpdateServiceCategoryDtoValidator : AbstractValidator<UpdateServiceCategoryDto>
    {
        public UpdateServiceCategoryDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .When(x => x.Name != null);

            RuleFor(x => x.SortOrder)
                .GreaterThanOrEqualTo(0)
                .When(x => x.SortOrder.HasValue);
        }
    }
}
