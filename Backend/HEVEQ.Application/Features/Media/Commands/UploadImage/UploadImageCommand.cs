using HEVEQ.Application.Features.Media.DTOs;
using MediatR;

namespace HEVEQ.Application.Features.Media.Commands.UploadImage
{
    public record UploadImageCommand(Stream FileStream, string FileName, string ContentType, long FileSize, string? Purpose, Guid? ReferenceId) : IRequest<ImageUploadResponse>;
}