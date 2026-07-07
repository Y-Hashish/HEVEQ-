using HEVEQ.Application.Features.Documents.DTOs;
using HEVEQ.Domain.Enums;
using MediatR;

namespace HEVEQ.Application.Features.Documents.Commands.UploadDocument
{
    public record UploadDocumentCommand(UploadDocumentRequest Request) : IRequest<UploadDocumentResponse>;
    public class UploadDocumentRequest
    {
        public DocumentType DocumentType { get; set; }
        public string FileUrl { get; set; } = string.Empty;
        public DateOnly? ExpiryDate { get; set; }
        public Guid? ServiceListingId { get; set; }
        public Guid? MarketplaceListingId { get; set; }
        public Guid? OperatorId { get; set; }
    }
}