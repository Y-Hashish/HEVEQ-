using HEVEQ.Application.Common.Interfaces;
using HEVEQ.Application.Common.Services;
using HEVEQ.Application.Features.Admin.DTOs;
using HEVEQ.Domain.Entities;
using HEVEQ.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HEVEQ.Application.Features.Admin.Command.TakeoverTicket
{
    public class TakeoverTicketCommandHandler(
        IApplicationDbContext context,
        NotificationHelper notificationHelper)
        : IRequestHandler<TakeoverTicketCommand, TicketActionResponse>
    {
        public async Task<TicketActionResponse> Handle(TakeoverTicketCommand request, CancellationToken cancellationToken)
        {
            if (request.AdminRole != "Admin")
            {
                return new TicketActionResponse
                {
                    IsSuccess = false,
                    StatusCode = 403,
                    Message = "Only Admins can take over tickets from other staff members."
                };
            }

            if (string.IsNullOrWhiteSpace(request.Reason))
            {
                return new TicketActionResponse
                {
                    IsSuccess = false,
                    StatusCode = 400,
                    Message = "Reason is required to take over a ticket."
                };
            }

            var ticket = await context.Tickets
                .FirstOrDefaultAsync(t => t.Id == request.TicketId, cancellationToken);

            if (ticket == null)
            {
                return new TicketActionResponse { IsSuccess = false, StatusCode = 404, Message = "Ticket not found." };
            }

            if (ticket.Status is TicketStatus.Resolved or TicketStatus.Closed)
            {
                return new TicketActionResponse
                {
                    IsSuccess = false,
                    StatusCode = 400,
                    Message = "Cannot take over a resolved or closed ticket."
                };
            }

            var oldAssigneeId = ticket.AssignedToUserId;

            ticket.AssignedToUserId = request.AdminId;
            ticket.AssignedByUserId = request.AdminId;
            ticket.AssignedAt = DateTime.UtcNow;

            if (ticket.Status == TicketStatus.Open || ticket.Status == TicketStatus.Reopened)
            {
                ticket.Status = TicketStatus.InProgress;
            }

            ticket.UpdatedAt = DateTime.UtcNow;

            var internalMessage = new TicketMessage
            {
                Id = Guid.NewGuid(),
                TicketId = ticket.Id,
                SenderId = request.AdminId,
                Body = $"[استحواذ] قام الآدمين بالاستحواذ على التذكرة. السبب: {request.Reason}",
                IsInternal = true,
                CreatedAt = DateTime.UtcNow
            };
            context.TicketMessages.Add(internalMessage);

            notificationHelper.TicketAssigned(request.AdminId, ticket.Id, ticket.TicketNumber, "الاستحواذ");

            if (oldAssigneeId.HasValue && oldAssigneeId.Value != request.AdminId)
            {
                notificationHelper.TicketUnassigned(oldAssigneeId.Value, ticket.Id, ticket.TicketNumber, "الاستحواذ");
            }

            await context.SaveChangesAsync(cancellationToken);

            return new TicketActionResponse
            {
                IsSuccess = true,
                StatusCode = 200,
                TicketId = ticket.Id,
                Status = ticket.Status.ToString(),
                StatusAr = ticket.Status switch
                {
                    TicketStatus.InProgress => "قيد المعالجة",
                    TicketStatus.Resolved => "محلولة",
                    TicketStatus.Closed => "مغلقة",
                    _ => ticket.Status.ToString()
                },
                Message = "Ticket taken over successfully."
            };
        }
    }
}
