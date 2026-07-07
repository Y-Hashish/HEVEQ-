using HEVEQ.Application.Common.Exceptions;
using HEVEQ.Application.Common.Interfaces;
using HEVEQ.Application.Features.Media.DTOs;
using MediatR;

namespace HEVEQ.Application.Features.Media.Commands.UploadImage
{
    public class UploadImageCommandHandler(IImageStorageService imageStorageService, ICurrentUserService currentUserService) : IRequestHandler<UploadImageCommand, ImageUploadResponse>
    {
        public async Task<ImageUploadResponse> Handle(UploadImageCommand request, CancellationToken cancellationToken)
        {
            if (!currentUserService.UserId.HasValue)
                throw new ForbiddenAccessException("User is not authenticated.");

            var folder = BuildFolder(request.Purpose, currentUserService.UserId.Value, request.ReferenceId);

            var result = await imageStorageService.UploadImageAsync(request.FileStream, request.FileName, request.ContentType, folder, cancellationToken);

            return new ImageUploadResponse
            {
                Url = result.Url,
                PublicId = result.PublicId,
                Folder = result.Folder,
                Format = result.Format,
                Width = result.Width,
                Height = result.Height,
                Bytes = result.Bytes,
                IsPublic = result.IsPublic,
                Message = "تم رفع الصورة بنجاح"
            };
        }

        private static string BuildFolder(string? purpose, Guid userId, Guid? referenceId)
        {
            var normalizedPurpose = NormalizePurpose(purpose);

            var ownerSegment = referenceId.HasValue ? referenceId.Value.ToString("N") : userId.ToString("N");

            return $"heveq/{normalizedPurpose}/{ownerSegment}";
        }

        private static string NormalizePurpose(string? purpose)
        {
            var value = purpose?.Trim().ToLowerInvariant();

            return value switch
            {
                "documents" or "document" => "documents",
                "service-listings" or "service-listing" => "service-listings",
                "marketplace-listings" or "marketplace-listing" => "marketplace-listings",
                "operators" or "operator" => "operators",
                "booking-evidence" or "completion-evidence" => "booking-evidence",
                "disputes" or "dispute" => "disputes",
                "tickets" or "ticket" => "tickets",
                "chat" or "messages" => "chat",
                "profile" or "profiles" => "profiles",
                _ => "general"
            };
        }
    }
}