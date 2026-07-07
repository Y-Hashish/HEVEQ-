using HEVEQ.Application.Common.Interfaces;
using HEVEQ.Application.Common.Services;
using HEVEQ.Application.Features.Admin.DTOs;
using HEVEQ.Domain.Enums;
using HEVEQ.Domain.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HEVEQ.Application.Features.Admin.Command.AssignTicket
{
    public class AssignTicketCommandHandler(
        IApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        NotificationHelper notificationHelper)
        : IRequestHandler<AssignTicketCommand, TicketActionResponse>
    {
        public async Task<TicketActionResponse> Handle(AssignTicketCommand request, CancellationToken cancellationToken)
        {
            if (request.AdminRole != "Admin")
            {
                return new TicketActionResponse
                {
                    IsSuccess = false,
                    StatusCode = 403,
                    Message = "Only Admins can assign tickets to other staff members."
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
                    Message = "Cannot assign a resolved or closed ticket."
                };
            }

            var assignee = await userManager.FindByIdAsync(request.AssignedToUserId.ToString());
            if (assignee == null)
            {
                return new TicketActionResponse { IsSuccess = false, StatusCode = 404, Message = "Assignee user not found." };
            }

            var isAssigneeAdmin = await userManager.IsInRoleAsync(assignee, "Admin");
            var isAssigneeEmployee = await userManager.IsInRoleAsync(assignee, "Employee");
            if (!isAssigneeAdmin && !isAssigneeEmployee)
            {
                return new TicketActionResponse
                {
                    IsSuccess = false,
                    StatusCode = 400,
                    Message = "Tickets can only be assigned to staff members (Admin or Employee)."
                };
            }

            var oldAssigneeId = ticket.AssignedToUserId;

            ticket.AssignedToUserId = request.AssignedToUserId;
            ticket.AssignedByUserId = request.AdminId;
            ticket.AssignedAt = DateTime.UtcNow;

            if (ticket.Status == TicketStatus.Open || ticket.Status == TicketStatus.Reopened)
            {
                ticket.Status = TicketStatus.InProgress;
            }

            ticket.UpdatedAt = DateTime.UtcNow;

            notificationHelper.TicketAssigned(request.AssignedToUserId, ticket.Id, ticket.TicketNumber, "الآدمين");

            if (oldAssigneeId.HasValue && oldAssigneeId.Value != request.AssignedToUserId)
            {
                notificationHelper.TicketUnassigned(oldAssigneeId.Value, ticket.Id, ticket.TicketNumber, "الآدمين");
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
                Message = "Ticket assigned successfully."
            };
        }
    }
}
