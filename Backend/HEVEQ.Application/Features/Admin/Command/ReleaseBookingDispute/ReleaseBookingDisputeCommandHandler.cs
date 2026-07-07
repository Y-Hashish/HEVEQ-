using HEVEQ.Application.Common.Interfaces;
using HEVEQ.Application.Features.Admin.DTOs;
using HEVEQ.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using HEVEQ.Application.Common.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace HEVEQ.Application.Features.Admin.Command.ReleaseBookingDispute
{
    public class ReleaseBookingDisputeCommandHandler(
        IApplicationDbContext context, NotificationHelper notificationHelper) 
        : IRequestHandler<ReleaseBookingDisputeCommand, ReleaseBookingDisputeResponse>
    {
        public async Task<ReleaseBookingDisputeResponse> Handle(ReleaseBookingDisputeCommand request, CancellationToken cancellationToken)
        {
            var booking = await context.Bookings
                .Include(b => b.EscrowRecords)
                .Include(b => b.ServiceListing) 
                    .ThenInclude(b => b.ProviderProfile)
                .FirstOrDefaultAsync(b => b.Id == request.BookingId, cancellationToken);

            if (booking == null)
            {
                return new ReleaseBookingDisputeResponse { IsSuccess = false, StatusCode = 404, Message = "Booking not found." };
            }
            if (booking.Status != BookingStatus.Disputed &&
                booking.Status != BookingStatus.PendingFieldVerification &&
                booking.Status != BookingStatus.FieldVerificationComplete)
            {
                return new ReleaseBookingDisputeResponse
                {
                    IsSuccess = false,
                    StatusCode = 400,
                    Message = $"Cannot resolve dispute. Current booking status is {booking.Status}."
                };
            }

            var activeEscrow = booking.EscrowRecords
                .FirstOrDefault(e => e.Status == EscrowStatus.Held || e.Status == EscrowStatus.Frozen);

            if (activeEscrow == null)
            {
                return new ReleaseBookingDisputeResponse
                {
                    IsSuccess = false,
                    StatusCode = 400,
                    Message = "No Held or Frozen escrow funds found for this booking."
                };
            }

            booking.Status = BookingStatus.ResolvedReleased;
            activeEscrow.Status = EscrowStatus.Released; 
            activeEscrow.ReleasedAt = DateTime.UtcNow;

            notificationHelper.DisputeReleased(booking.ServiceListing.ProviderProfile.UserId,booking.Id,booking.BookingNumber);
            notificationHelper.DisputeResolved(booking.CustomerId,booking.Id,booking.BookingNumber);
            await context.SaveChangesAsync(cancellationToken);

            // 7. إرسال الإشعارات للطرفين (Smart Notifications)
            //if (notificationService != null)
            //{
            //    // إشعار للمزود بأنه كسب النزاع (لن نرسل له الـ Note)
            //    await notificationService.SendAsync(
            //        userId: booking.ServiceListing.ProviderProfileId,
            //        title: "تم حسم النزاع لصالحك",
            //        message: $"تم حل النزاع للحجز رقم {booking.BookingNumber} وتم تحويل الأموال إلى محفظتك."
            //    );

            //    // إشعار للعميل يوضح قرار الإدارة (استخدمنا الـ DecisionNote هنا)
            //    await notificationService.SendAsync(
            //        userId: booking.CustomerId,
            //        title: "قرار الإدارة بشأن النزاع",
            //        message: $"تم إغلاق النزاع للحجز رقم {booking.BookingNumber} لصالح المزود. ملاحظة الإدارة: {request.DecisionNote}"
            //    );
            //}

            return new ReleaseBookingDisputeResponse
            {
                IsSuccess = true,
                StatusCode = 200,
                BookingId = booking.Id,
                Status = booking.Status.ToString(),
                StatusAr = "تم الحل لصالح المزود",
                EscrowStatus = activeEscrow.Status.ToString(),
                Message = "Dispute resolved and funds released to provider"
            };
        }
    }
}
