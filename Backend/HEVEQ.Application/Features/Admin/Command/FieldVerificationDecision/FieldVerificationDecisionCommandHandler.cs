using HEVEQ.Application.Common.Interfaces;
using HEVEQ.Application.Common.Services;
using HEVEQ.Application.Features.Admin.DTOs;
using HEVEQ.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using HEVEQ.Application.Common.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace HEVEQ.Application.Features.Admin.Command.FieldVerificationDecision
{
    public class FieldVerificationDecisionCommandHandler(IApplicationDbContext context, NotificationHelper notificationHelper)
        : IRequestHandler<FieldVerificationDecisionCommand, FieldVerificationDecisionResponse>
    {
        public async Task<FieldVerificationDecisionResponse> Handle(FieldVerificationDecisionCommand request, CancellationToken cancellationToken)
        {
            var verificationForm = await context.FieldVerificationForms
                .Include(f => f.Booking)
                    .ThenInclude(b => b.ServiceListing)
                        .ThenInclude(s => s.ProviderProfile)
                .FirstOrDefaultAsync(f => f.Id == request.Id, cancellationToken);

            if (verificationForm == null)
            {
                return new FieldVerificationDecisionResponse { IsSuccess = false, StatusCode = 404, Message = "Field verification form not found." };
            }

            if (verificationForm.VisitStatus != VisitStatus.Completed && verificationForm.VisitStatus != VisitStatus.FailedAccess)
            {
                return new FieldVerificationDecisionResponse
                {
                    IsSuccess = false,
                    StatusCode = 400,
                    Message = $"Cannot make a decision. Visit status must be Completed or FailedAccess, currently is {verificationForm.VisitStatus}."
                };
            }

            if (!Enum.TryParse<FieldVerificationAdminDecision>(request.AdminDecision, true, out var parsedDecision))
            {
                return new FieldVerificationDecisionResponse
                {
                    IsSuccess = false,
                    StatusCode = 400,
                    Message = "Invalid admin decision type."
                };
            }

            verificationForm.AdminDecision = parsedDecision;
            verificationForm.AdminDecisionNote = request.AdminDecisionNote;

            verificationForm.DecidedByAdminId = request.AdminId;
            verificationForm.DecidedAt = DateTime.UtcNow;

            notificationHelper.FieldVerificationDecisionMade(verificationForm.Booking.CustomerId,verificationForm.Id,verificationForm.Booking.BookingNumber);

            notificationHelper.FieldVerificationDecisionMade(verificationForm.Booking.ServiceListing.ProviderProfile.UserId,verificationForm.Id,verificationForm.Booking.BookingNumber);
            await context.SaveChangesAsync(cancellationToken);

            string decisionAr = request.AdminDecision switch
            {
                "ReleaseToProvider" => "صرف للمزود",
                "RefundCustomer" => "رد للعميل",
                "RefundToCustomer" => "رد للعميل",
                "PartialSettlement" => "تسوية جزئية",
                _ => request.AdminDecision
            };

            return new FieldVerificationDecisionResponse
            {
                IsSuccess = true,
                StatusCode = 200,
                FieldVerificationFormId = verificationForm.Id,
                AdminDecision = verificationForm.AdminDecision.ToString(),
                AdminDecisionAr = decisionAr,
                Message = "Field verification decision saved successfully"
            };
        }
    }
}
