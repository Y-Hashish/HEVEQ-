using HEVEQ.Application.Common.Interfaces;
using HEVEQ.Application.Features.Admin.DTOs;
using HEVEQ.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using HEVEQ.Application.Common.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace HEVEQ.Application.Features.Admin.Command.ApproveServiceListing
{
    public class ApproveServiceListingCommandHandler(IApplicationDbContext context, NotificationHelper notificationHelper)
        : IRequestHandler<ApproveServiceListingCommand, ApproveServiceListingResponse>
    {
        public async Task<ApproveServiceListingResponse> Handle(ApproveServiceListingCommand request, CancellationToken cancellationToken)
        {
            var listing = await context.ServiceListings
                .Include(x => x.Availability)
                .Include(x => x.ProviderProfile)
                .Include(x => x.Photos)
                .Include(x => x.ServiceListingOperators)
                    .ThenInclude(x => x.Operator)
                .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

            if (listing == null)
            {
                return new ApproveServiceListingResponse { IsSuccess = false, StatusCode = 404, Message = "Service listing not found." };
            }

            if (listing.Status != ServiceListingStatus.PendingReview)
            {
                return new ApproveServiceListingResponse
                {
                    IsSuccess = false,
                    StatusCode = 400,
                    Message = "Only listings in 'PendingReview' status can be approved."
                };
            }


            var missingRequirements = new List<string>();
            var photosCount = listing.Photos?.Count ?? 0;
            var activeOperatorsCount = listing.ServiceListingOperators
                .Count(x => x.Operator != null && x.Operator.IsActive);
            var availabilityCount = listing.Availability?.Count ?? 0;

            if (photosCount < 3)
                missingRequirements.Add("At least 3 photos are required");

            if (activeOperatorsCount < 1)
                missingRequirements.Add("At least 1 active operator is required");

            if (availabilityCount < 1)
                missingRequirements.Add("At least 1 availability schedule is required");

            if (missingRequirements.Count > 0)
            {
                return new ApproveServiceListingResponse
                {
                    IsSuccess = false,
                    StatusCode = 400,
                    Message = $"Cannot approve listing. Missing requirements: {string.Join(", ", missingRequirements)}."
                };
            }

            listing.Status = ServiceListingStatus.Active;


            notificationHelper.ServiceListingApproved(listing.ProviderProfile.UserId, listing.Id, listing.Title);
            await context.SaveChangesAsync(cancellationToken);

            // 6. إنشاء Notification للـ Provider لو الخدمة جاهزة (لو الـ Service محقونة وليست Null)
            //if (notificationService != null)
            //{
            //    await notificationService.SendAsync(
            //        userId: listing.ProviderId,
            //        title: "تم قبول خدمتك",
            //        message: $"تمت الموافقة على الخدمة '{listing.Title}' وهي الآن نشطة على المنصة."
            //    );
            //}

            // 7. إرجاع المخرجات المتوقعة بدقة
            return new ApproveServiceListingResponse
            {
                IsSuccess = true,
                StatusCode = 200,
                Id = listing.Id,
                Status = "Active",
                StatusText = "Active",
                StatusAr = "نشط",
                Message = "Service listing approved successfully"
            };
        }
    }
}
