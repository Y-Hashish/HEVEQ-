using FluentValidation;

namespace HEVEQ.Application.Features.Media.Commands.UploadImage
{
    public class UploadImageCommandValidator : AbstractValidator<UploadImageCommand>
    {
        private static readonly string[] AllowedContentTypes =
        [
            "image/jpeg",
            "image/jpg",
            "image/png",
            "image/webp"
        ];

        public UploadImageCommandValidator()
        {
            RuleFor(x => x.FileStream)
                .NotNull()
                .WithMessage("Image file is required.");

            RuleFor(x => x.FileName)
                .NotEmpty()
                .WithMessage("File name is required.");

            RuleFor(x => x.ContentType)
                .NotEmpty()
                .WithMessage("Content type is required.")
                .Must(contentType =>
                    AllowedContentTypes.Contains(contentType.ToLowerInvariant()))
                .WithMessage("Only JPEG, PNG, and WEBP images are allowed.");

            RuleFor(x => x.FileSize)
                .GreaterThan(0)
                .WithMessage("Image file is empty.")
                .LessThanOrEqualTo(10 * 1024 * 1024)
                .WithMessage("Image size must not exceed 10 MB.");
        }
    }
}