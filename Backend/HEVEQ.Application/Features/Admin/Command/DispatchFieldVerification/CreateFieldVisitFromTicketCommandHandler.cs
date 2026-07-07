using HEVEQ.Application.Common.Interfaces;
using HEVEQ.Application.Features.Admin.DTOs;
using HEVEQ.Domain.Entities;
using HEVEQ.Domain.Enums;
using HEVEQ.Domain.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using HEVEQ.Application.Common.Services;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HEVEQ.Application.Features.Admin.Command.DispatchFieldVerification
{
    public class CreateFieldVisitFromTicketCommandHandler(
        IApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        NotificationHelper notificationHelper)
        : IRequestHandler<CreateFieldVisitFromTicketCommand, DispatchFieldVerificationResponse>
    {
        public async Task<DispatchFieldVerificationResponse> Handle(CreateFieldVisitFromTicketCommand request, CancellationToken cancellationToken)
        {
            var ticket = await context.Tickets
                .FirstOrDefaultAsync(t => t.Id == request.TicketId, cancellationToken);

            if (ticket == null)
            {
                return new DispatchFieldVerificationResponse { IsSuccess = false, StatusCode = 404, Message = "Ticket not found." };
            }

            if (!ticket.BookingId.HasValue)
            {
                return new DispatchFieldVerificationResponse 
                { 
                    IsSuccess = false, 
                    StatusCode = 400, 
                    Message = "Field visits are only supported for booking-related tickets." 
                };
            }

            var booking = await context.Bookings
                .Include(b => b.JobCompletionEvidenceForms)
                .FirstOrDefaultAsync(b => b.Id == ticket.BookingId.Value, cancellationToken);

            if (booking == null)
            {
                return new DispatchFieldVerificationResponse { IsSuccess = false, StatusCode = 404, Message = "Linked booking not found." };
            }

            var employee = await userManager.Users
                .Include(u => u.EmployeeProfile)
                .FirstOrDefaultAsync(u => u.Id == request.EmployeeUserId, cancellationToken);

            if (employee == null)
            {
                return new DispatchFieldVerificationResponse { IsSuccess = false, StatusCode = 404, Message = "Employee not found." };
            }

            var isEmployeeRole = await userManager.IsInRoleAsync(employee, "Employee");
            if (!isEmployeeRole)
            {
                return new DispatchFieldVerificationResponse { IsSuccess = false, StatusCode = 400, Message = "Selected user does not have the Employee role." };
            }

            if (employee.EmployeeProfile == null || !employee.EmployeeProfile.IsAvailableForDispatch)
            {
                return new DispatchFieldVerificationResponse { IsSuccess = false, StatusCode = 400, Message = "Employee is currently not available for dispatch." };
            }

            // Linked evidence form is optional for field visit creation now
            var linkedEvidence = booking.JobCompletionEvidenceForms.OrderByDescending(e => e.CreatedAt).FirstOrDefault();

            var verificationForm = new FieldVerificationForm
            {
                Id = Guid.NewGuid(),
                BookingId = booking.Id,
                TicketId = ticket.Id,
                DispatchedEmployeeId = employee.Id,
                DispatchedByAdminId = request.AdminId,
                LinkedEvidenceFormId = linkedEvidence?.Id, // Nullable
                DispatchInstructions = request.DispatchInstructions,
                VisitStatus = VisitStatus.Dispatched,
                DispatchedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            context.FieldVerificationForms.Add(verificationForm);

            // Update booking status to indicate field verification in progress
            booking.Status = BookingStatus.PendingFieldVerification;
            booking.FieldVerificationDispatchedAt = DateTime.UtcNow;

            // Update ticket status to reflect pending field verification
            ticket.Status = TicketStatus.PendingFieldVerification;
            ticket.UpdatedAt = DateTime.UtcNow;

            notificationHelper.FieldVerificationAssigned(employee.Id, verificationForm.Id, booking.BookingNumber);
            await context.SaveChangesAsync(cancellationToken);

            return new DispatchFieldVerificationResponse
            {
                IsSuccess = true,
                StatusCode = 200,
                FieldVerificationFormId = verificationForm.Id,
                BookingId = booking.Id,
                VisitStatus = verificationForm.VisitStatus.ToString(),
                VisitStatusAr = "تم الإرسال",
                Message = "Field verification dispatched successfully from ticket"
            };
        }
    }
}
