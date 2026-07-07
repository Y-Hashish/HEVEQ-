using HEVEQ.Application.Common.Interfaces;
using HEVEQ.Application.Features.Admin.DTOs;
using HEVEQ.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HEVEQ.Application.Features.Admin.Command.ClaimTicket
{
    public class ClaimTicketCommandHandler(IApplicationDbContext context)
        : IRequestHandler<ClaimTicketCommand, TicketActionResponse>
    {
        public async Task<TicketActionResponse> Handle(ClaimTicketCommand request, CancellationToken cancellationToken)
        {
            if (request.CurrentUserRole != "Admin" && request.CurrentUserRole != "Employee")
            {
                return new TicketActionResponse
                {
                    IsSuccess = false,
                    StatusCode = 403,
                    Message = "Only staff members (Admin or Employee) can claim tickets."
                };
            }

            var ticket = await context.Tickets
                .FirstOrDefaultAsync(t => t.Id == request.TicketId, cancellationToken);

            if (ticket == null)
            {
                return new TicketActionResponse { IsSuccess = false, StatusCode = 404, Message = "Ticket not found." };
            }

            if (ticket.AssignedToUserId.HasValue)
            {
                return new TicketActionResponse
                {
                    IsSuccess = false,
                    StatusCode = 400,
                    Message = "Ticket is already assigned to another staff member."
                };
            }

            if (ticket.Status is TicketStatus.Resolved or TicketStatus.Closed)
            {
                return new TicketActionResponse
                {
                    IsSuccess = false,
                    StatusCode = 400,
                    Message = "Cannot claim a resolved or closed ticket."
                };
            }

            ticket.AssignedToUserId = request.CurrentUserId;
            ticket.AssignedByUserId = request.CurrentUserId;
            ticket.AssignedAt = DateTime.UtcNow;

            if (ticket.Status == TicketStatus.Open || ticket.Status == TicketStatus.Reopened)
            {
                ticket.Status = TicketStatus.InProgress;
            }

            ticket.UpdatedAt = DateTime.UtcNow;

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
                Message = "Ticket claimed successfully."
            };
        }
    }
}
