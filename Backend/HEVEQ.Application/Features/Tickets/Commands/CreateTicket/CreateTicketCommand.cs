using HEVEQ.Application.Features.Tickets.DTOs;
using HEVEQ.Domain.Enums;
using MediatR;
using System.Collections.Generic;

namespace HEVEQ.Application.Features.Tickets.Commands.CreateTicket;

public record CreateTicketAttachmentInput(
    string FileUrl,
    string FileName,
    AttachmentFileType FileType
);

public record CreateTicketCommand(
    string Subject,
    TicketCategory Category,
    string Message,
    Guid? BookingId,
    Guid? MarketplaceOrderId,
    List<CreateTicketAttachmentInput>? Attachments = null
) : IRequest<CreateTicketResult>;