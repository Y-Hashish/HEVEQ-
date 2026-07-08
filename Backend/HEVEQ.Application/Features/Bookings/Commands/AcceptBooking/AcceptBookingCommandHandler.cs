using HEVEQ.Application.Common.Interfaces;
using HEVEQ.Application.Common.Services;
using HEVEQ.Application.Features.Bookings.DTOs;
using HEVEQ.Application.Features.Bookings.Helpers;
using HEVEQ.Application.Features.Bookings.Services;
using HEVEQ.Domain.Entities;
using HEVEQ.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Text;

namespace HEVEQ.Application.Features.Bookings.Commands.AcceptBooking
{
    public sealed class AcceptBookingCommandHandler : IRequestHandler<AcceptBookingCommand, AcceptBookingResponseDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly NotificationHelper _notificationHelper;
        public AcceptBookingCommandHandler(IApplicationDbContext context, NotificationHelper notificationHelper)
        {
            _context = context;
            _notificationHelper = notificationHelper;
        }
        public async Task<AcceptBookingResponseDto> Handle(AcceptBookingCommand request, CancellationToken cancellationToken)
        {
            var booking = await _context.Bookings
                .Include(b => b.ServiceListing)
                .FirstOrDefaultAsync(b => b.Id == request.BookingId, cancellationToken);
            if (booking is null) 
                throw new InvalidOperationException("Booking not found");

            if (booking.Status != BookingStatus.PendingProviderResponse)
                throw new InvalidOperationException("Only pending bookings can be accepted.");

            var providerProfile = await _context.ProviderProfiles
                .FirstOrDefaultAsync(x => x.UserId == request.ProviderUserId, cancellationToken);
            if (providerProfile is null)
                throw new InvalidOperationException("Provider profile was not found.");

            if (booking.ServiceListing.ProviderProfileId != providerProfile.Id)
                throw new InvalidOperationException("Only the owner provider can accept this booking.");

            var operatorEntity = await _context.Operators.FirstOrDefaultAsync(
                x => x.Id == request.OperatorId &&
                x.ProviderProfileId == providerProfile.Id &&
                x.IsActive, cancellationToken);

            if (operatorEntity is null)
                throw new InvalidOperationException("Operator is not active or does not belong to this provider.");

            var operatorLinkedToListing = await _context.ServiceListingOperators.AnyAsync(x =>
                x.ListingId == booking.ServiceListingId &&
                x.OperatorId == request.OperatorId,
                cancellationToken);

            if (!operatorLinkedToListing)
                throw new InvalidOperationException("Operator is not linked to this service listing.");

            var scheduledStart = ToDateTime(booking.RequestedStartDate, booking.RequestedStartTime);
            var scheduledEnd = scheduledStart.AddHours((double)booking.EstimatedDurationHours);

            var operatorBlackout = await _context.BlackoutDates.AnyAsync(x =>
            x.ListingId == booking.ServiceListingId &&
            x.OperatorId == request.OperatorId &&
            x.Date == booking.RequestedStartDate, cancellationToken);

            if (operatorBlackout)
                throw new InvalidOperationException("Operator has a blackout date for this booking date.");

            var hasListingConflict = await HasServiceListingConflictAsync(
                booking.Id,
                booking.ServiceListingId,
                booking.RequestedStartDate,
                scheduledStart,
                scheduledEnd,
                cancellationToken);

            if (hasListingConflict)
                throw new InvalidOperationException("لا يمكن قبول هذا الطلب لأن الخدمة محجوزة بالفعل في نفس التوقيت. يجب رفض الطلب أو اقتراح وقت آخر للعميل.");

            var hasConflict = await _context.OperatorAssignments.AnyAsync(x =>
            x.OperatorId == request.OperatorId &&
            x.Status != OperatorAssignmentStatus.Cancelled &&
            x.Status != OperatorAssignmentStatus.Completed &&
            scheduledStart < x.ScheduledEnd &&
            scheduledEnd > x.ScheduledStart, cancellationToken);

            if(hasConflict)
                throw new InvalidOperationException("المشغل لديه مهمة أخرى في نفس التوقيت. اختر مشغلاً آخر أو ارفض الطلب.");

            var assignment = new OperatorAssignment
            {
                BookingId = booking.Id,
                OperatorId = request.OperatorId,
                Status = OperatorAssignmentStatus.Assigned,
                ScheduledStart = scheduledStart,
                ScheduledEnd = scheduledEnd,
                CreatedAt = DateTime.UtcNow
            };

            _context.OperatorAssignments.Add(assignment);

            booking.AssignedOperatorId = request.OperatorId;
            booking.Status = BookingStatus.ConfirmedPendingPayment;
            booking.ConfirmedAt = DateTime.UtcNow;

            _notificationHelper.BookingAccepted(booking.CustomerId, booking.Id, booking.BookingNumber);
            await _context.SaveChangesAsync(cancellationToken);
            return new AcceptBookingResponseDto
            {
                Id = booking.Id,
                BookingNumber = booking.BookingNumber,
                Status = booking.Status.ToString(),
                StatusAr = BookingDisplayHelper.GetStatusAr(booking.Status),
                AssignedOperatorName = operatorEntity.FullName,
                Message = "Booking accepted successfully"
            };

        }

        private async Task<bool> HasServiceListingConflictAsync(
            Guid currentBookingId,
            Guid serviceListingId,
            DateOnly requestedDate,
            DateTime scheduledStart,
            DateTime scheduledEnd,
            CancellationToken cancellationToken)
        {
            var sameDayBookings = await _context.Bookings
                .AsNoTracking()
                .Where(x => x.Id != currentBookingId
                            && x.ServiceListingId == serviceListingId
                            && x.RequestedStartDate == requestedDate
                            && BookingScheduleConflictHelper.BlockingStatuses.Contains(x.Status))
                .Select(x => new
                {
                    x.RequestedStartDate,
                    x.RequestedStartTime,
                    x.EstimatedDurationHours
                })
                .ToListAsync(cancellationToken);

            return sameDayBookings.Any(x =>
                BookingScheduleConflictHelper.Overlaps(
                    scheduledStart,
                    scheduledEnd,
                    BookingScheduleConflictHelper.ToScheduledStart(x.RequestedStartDate, x.RequestedStartTime),
                    BookingScheduleConflictHelper.ToScheduledEnd(x.RequestedStartDate, x.RequestedStartTime, x.EstimatedDurationHours)));
        }

        private static DateTime ToDateTime(DateOnly date, TimeOnly time)
        {
            return date.ToDateTime(time, DateTimeKind.Utc);
        }
    }
}
