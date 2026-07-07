using HEVEQ.Application.Common.Interfaces;
using HEVEQ.Application.Features.Admin.DTOs;
using HEVEQ.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using HEVEQ.Application.Common.Services;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity;
using HEVEQ.Domain.Identity;

namespace HEVEQ.Application.Features.Admin.Command.AddTicketMessage
{
    public class AddTicketMessageCommandHandler(
        IApplicationDbContext context,
        NotificationHelper notificationHelper,
        UserManager<ApplicationUser> userManager) 
        : IRequestHandler<AddTicketMessageCommand, AddTicketMessageResponse>
    {
        public async Task<AddTicketMessageResponse> Handle(AddTicketMessageCommand request, CancellationToken cancellationToken)
        {
            // 1. التأكد من وجود التذكرة
            var ticket = await context.Tickets
                .FirstOrDefaultAsync(t => t.Id == request.TicketId, cancellationToken);

            if (ticket == null)
            {
                return new AddTicketMessageResponse { IsSuccess = false, StatusCode = 404, Message = "Ticket not found." };
            }

            // 2. التحقق من صلاحية الرد والتعيين التلقائي
            if (ticket.AssignedToUserId.HasValue)
            {
                if (ticket.AssignedToUserId.Value != request.AdminId)
                {
                    return new AddTicketMessageResponse 
                    { 
                        IsSuccess = false, 
                        StatusCode = 403, 
                        Message = "This ticket is assigned to another staff member. You cannot reply unless you claim or take over the ticket." 
                    };
                }
            }
            else
            {
                ticket.AssignedToUserId = request.AdminId;
                ticket.AssignedByUserId = request.AdminId;
                ticket.AssignedAt = DateTime.UtcNow;
                ticket.Status = HEVEQ.Domain.Enums.TicketStatus.InProgress;
            }

            
            var message = new TicketMessage
            {
                Id = Guid.NewGuid(),
                TicketId = ticket.Id,
                SenderId = request.AdminId, 
                IsInternal = request.IsInternal,
                CreatedAt = DateTime.UtcNow,
                Body = request.Body
            };

            context.TicketMessages.Add(message);

            ticket.UpdatedAt = DateTime.UtcNow;

            if (!request.IsInternal)
                notificationHelper.TicketReplied(ticket.SubmittedById,ticket.Id, ticket.TicketNumber);

            await context.SaveChangesAsync(cancellationToken);

            // 4. إرسال إشعار ذكي (Smart Notification)
            // إذا لم تكن الرسالة داخلية، نرسل إشعاراً للمستخدم صاحب التذكرة
            //if (!request.IsInternal && notificationService != null)
            //{
            //    await notificationService.SendAsync(
            //        userId: ticket.UserId,
            //        title: "تحديث على تذكرتك",
            //        message: $"تم الرد على تذكرتك رقم {ticket.TicketNumber}. يرجى مراجعة التفاصيل."
            //    );
            //}

            var adminUser = await userManager.FindByIdAsync(request.AdminId.ToString());
            var senderName = adminUser != null ? $"{adminUser.FirstName} {adminUser.LastName}".Trim() : "System/Admin";

            return new AddTicketMessageResponse
            {
                IsSuccess = true,
                StatusCode = 200,
                Id = message.Id,
                Message = "Ticket message added successfully",
                SenderId = request.AdminId,
                SenderName = senderName,
                SenderRole = "Admin",
                Body = message.Body,
                Content = message.Body,
                CreatedAt = message.CreatedAt,
                IsInternal = message.IsInternal
            };
        }
    }
}
