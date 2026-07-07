using FluentValidation;

namespace HEVEQ.Application.Features.Documents.Commands.UploadDocument
{
    public class UploadDocumentCommandValidator : AbstractValidator<UploadDocumentCommand>
    {
        public UploadDocumentCommandValidator()
        {
            RuleFor(x => x.Request)
                .NotNull()
                .WithMessage("Request body is required.");

            When(x => x.Request is not null, () =>
            {
                RuleFor(x => x.Request.FileUrl)
                    .Cascade(CascadeMode.Stop)
                    .NotEmpty()
                    .WithMessage("FileUrl is required.")
                    .MaximumLength(500)
                    .WithMessage("FileUrl must not exceed 500 characters.")
                    .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _))
                    .WithMessage("FileUrl must be a valid absolute URL.");

                RuleFor(x => x.Request.DocumentType)
                    .IsInEnum()
                    .WithMessage("Invalid document type.");

                RuleFor(x => x.Request.ExpiryDate)
                    .GreaterThan(DateOnly.FromDateTime(DateTime.UtcNow))
                    .When(x => x.Request.ExpiryDate.HasValue)
                    .WithMessage("ExpiryDate must be a future date.");
            });
        }
    }
}