using FluentValidation;
using HEVEQ.Application.Common.Interfaces;
using HEVEQ.Application.Features.Bookings.DTOs;
using HEVEQ.Application.Features.Bookings.Helpers;
using HEVEQ.Application.Features.Bookings.Services;
using HEVEQ.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using HEVEQ.Application.Common.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace HEVEQ.Application.Features.Bookings.Commands.ConfirmBookingCompletion
{
    public sealed class ConfirmBookingCompletionCommandHandler : IRequestHandler<ConfirmBookingCompletionCommand, ConfirmBookingCompletionResponseDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly NotificationHelper _notificationHelper;

        public ConfirmBookingCompletionCommandHandler(IApplicationDbContext context, NotificationHelper notificationHelper)
        {
            _context = context;
            _notificationHelper = notificationHelper;
        }

        public async Task<ConfirmBookingCompletionResponseDto> Handle(ConfirmBookingCompletionCommand request, CancellationToken cancellationToken)
        {
            var booking = await _context.Bookings
                .Include(x => x.ServiceListing)
                    .ThenInclude(x => x.ProviderProfile)
                .Include(x => x.JobCompletionEvidenceForms)
                .FirstOrDefaultAsync(x => x.Id == request.BookingId, cancellationToken);

            if (booking is null)
                throw new InvalidOperationException("Booking was not found.");

            if (booking.CustomerId != request.CustomerId)
                throw new InvalidOperationException("Only the booking customer can confirm completion.");

            if (booking.Status != BookingStatus.PendingCustomerConfirmation)
                throw new InvalidOperationException("Booking can only be confirmed after provider marks it as completed.");

            var hasOpenTicket = await _context.Tickets
                .AnyAsync(x =>
                    x.BookingId == booking.Id &&
                    x.Status != TicketStatus.Resolved &&
                    x.Status != TicketStatus.Closed,
                    cancellationToken);

            if (hasOpenTicket)
                throw new InvalidOperationException("Booking completion cannot be confirmed while there is an open ticket for this booking.");

            var hasUnpaidApprovedTimeAdjustment = await _context.BookingTimeAdjustmentRequests
                .AnyAsync(x =>
                    x.BookingId == booking.Id &&
                    x.Status == BookingTimeAdjustmentStatus.PendingPayment,
                    cancellationToken);

            if (hasUnpaidApprovedTimeAdjustment)
                throw new InvalidOperationException("There is an approved time adjustment that must be paid before confirming completion.");

            // Any pending time adjustment request expires when the customer confirms completion,
            // because the booking is now closed and no additional cost can be approved after completion.
            var pendingAdjustments = await _context.BookingTimeAdjustmentRequests
                .Where(x =>
                    x.BookingId == booking.Id &&
                    x.Status == BookingTimeAdjustmentStatus.Pending)
                .ToListAsync(cancellationToken);

            foreach (var adjustment in pendingAdjustments)
            {
                adjustment.Status = BookingTimeAdjustmentStatus.Expired;
            }
            var now = DateTime.UtcNow;
            var latestEvidenceForm = booking.JobCompletionEvidenceForms
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefault();

            if (latestEvidenceForm is not null && latestEvidenceForm.Status == EvidenceFormStatus.Submitted)
            {
                latestEvidenceForm.Status = EvidenceFormStatus.AcceptedAsEvidence;
                latestEvidenceForm.ReviewedAt = now;
                latestEvidenceForm.AdminReviewNote = "Accepted automatically after customer confirmed booking completion.";
            }
            booking.Status = BookingStatus.Completed;
            booking.CompletionConfirmedAt = now;

            _notificationHelper.BookingCompleted(booking.ServiceListing.ProviderProfile.UserId, booking.Id, booking.BookingNumber);
            await _context.SaveChangesAsync(cancellationToken);
            return new ConfirmBookingCompletionResponseDto
            {
                BookingId = booking.Id,
                Status = booking.Status.ToString(),
                StatusAr = BookingDisplayHelper.GetStatusAr(booking.Status),
                Message = "Booking completion confirmed successfully"
            };
        }
    }
}
