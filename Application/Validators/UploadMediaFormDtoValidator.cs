using Application.DTOs.Media;
using FluentValidation;

namespace Application.Validators
{
    public class UploadMediaFormDtoValidator : AbstractValidator<UploadMediaFormDto>
    {
        public UploadMediaFormDtoValidator()
        {
            RuleFor(x => x.Category)
                .IsInEnum();

            RuleFor(x => x.OwnerType)
                .IsInEnum();

            RuleFor(x => x.Files)
                .NotNull()
                .Must(files => files.Count > 0)
                .WithMessage("No files uploaded")
                .Must(files => files.Count <= 5)
                .WithMessage("Maximum 5 files allowed")
                .Must(files => files.All(file => file.Length > 0))
                .WithMessage("One or more files are empty");
        }
    }
}
