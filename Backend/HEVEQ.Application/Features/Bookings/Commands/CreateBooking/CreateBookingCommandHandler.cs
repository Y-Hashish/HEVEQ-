using HEVEQ.Application.Common.Interfaces;
using HEVEQ.Application.Features.Bookings.Services;
using HEVEQ.Application.Features.Bookings.Services.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using HEVEQ.Application.Features.Bookings.DTOs;
using HEVEQ.Application.Features.Bookings.Helpers;
using HEVEQ.Application.Common.Services;

namespace HEVEQ.Application.Features.Bookings.Commands.CreateBooking
{
    public sealed class CreateBookingCommandHandler : IRequestHandler<CreateBookingCommand, CreateBookingResponseDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IBookingAddressResolver _addressResolver;
        private readonly IBookingCreationService _bookingCreationService;
        private readonly NotificationHelper _notificationHelper;

        public CreateBookingCommandHandler(IApplicationDbContext context, IBookingAddressResolver addressResolver, IBookingCreationService bookingCreationService, NotificationHelper notificationHelper)
        {
            _context = context;
            _addressResolver = addressResolver;
            _bookingCreationService = bookingCreationService;
            _notificationHelper = notificationHelper;
        }

        public async Task<CreateBookingResponseDto> Handle(CreateBookingCommand request, CancellationToken cancellationToken)
        {
            var customerExists = await _context.CustomerProfiles.AnyAsync(x => x.UserId == request.CustomerId, cancellationToken);
            if (!customerExists)
                throw new InvalidOperationException("Only customers can create bookings.");


            var addressSnapshot = await _addressResolver.ResolveAsync(request, cancellationToken);

            var listing = await _context.ServiceListings
                .Include(x => x.ProviderProfile)
                .Include(x => x.Availability)
                .Include(x => x.BlackoutDates)
                .FirstOrDefaultAsync(x => x.Id == request.ServiceListingId, cancellationToken);

            if (listing is null)
                throw new InvalidOperationException("Service listing was not found.");

            if (listing.ProviderProfile is null)
                throw new InvalidOperationException("Provider profile was not found.");

            if (listing.ProviderProfile.UserId == request.CustomerId)
                throw new InvalidOperationException("لا يمكن للمزود حجز خدمته الخاصة.");

            await EnsureCustomerHasNoOpenBookingForSameServiceAsync(request, cancellationToken);
            await EnsureServiceListingHasNoCommittedConflictAsync(request, cancellationToken);

            var booking = _bookingCreationService.Create(request, listing, addressSnapshot);

            _context.Bookings.Add(booking);
            _notificationHelper.BookingRequested(listing.ProviderProfile.UserId, booking.Id, booking.BookingNumber, listing.Title);
            await _context.SaveChangesAsync(cancellationToken);
            return new CreateBookingResponseDto
            {
                Id = booking.Id,
                BookingNumber = booking.BookingNumber,
                Status = booking.Status.ToString(),
                StatusAr = BookingDisplayHelper.GetStatusAr(booking.Status),
                ServiceTitle = listing.Title,
                ProviderCompany = listing.ProviderProfile.CompanyName,
                RequestedStartDate = booking.RequestedStartDate,
                RequestedStartTime = booking.RequestedStartTime,
                EstimatedDurationHours = booking.EstimatedDurationHours,
                HourlyRateSnapshot = booking.HourlyRateSnapshot,
                EstimatedTotal = booking.EstimatedTotal,
                Message = "Booking request submitted successfully"
            };
        }

        private async Task EnsureCustomerHasNoOpenBookingForSameServiceAsync(
            CreateBookingCommand request,
            CancellationToken cancellationToken)
        {
            var existingBooking = await _context.Bookings
                .AsNoTracking()
                .Where(x => x.CustomerId == request.CustomerId
                            && x.ServiceListingId == request.ServiceListingId
                            && BookingScheduleConflictHelper.CustomerOpenBookingStatuses.Contains(x.Status))
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new { x.BookingNumber, x.Status })
                .FirstOrDefaultAsync(cancellationToken);

            if (existingBooking is not null)
            {
                throw new InvalidOperationException(
                    $"لديك حجز قائم بالفعل على هذه الخدمة برقم {existingBooking.BookingNumber}. يمكنك حجز نفس الخدمة مرة أخرى بعد اكتمال أو إلغاء الحجز الحالي.");
            }
        }

        private async Task EnsureServiceListingHasNoCommittedConflictAsync(
            CreateBookingCommand request,
            CancellationToken cancellationToken)
        {
            var requestedStart = BookingScheduleConflictHelper.ToScheduledStart(
                request.RequestedStartDate,
                request.RequestedStartTime);
            var requestedEnd = BookingScheduleConflictHelper.ToScheduledEnd(
                request.RequestedStartDate,
                request.RequestedStartTime,
                request.EstimatedDurationHours);

            var sameDayBookings = await _context.Bookings
                .AsNoTracking()
                .Where(x => x.ServiceListingId == request.ServiceListingId
                            && x.RequestedStartDate == request.RequestedStartDate
                            && BookingScheduleConflictHelper.BlockingStatuses.Contains(x.Status))
                .Select(x => new
                {
                    x.BookingNumber,
                    x.RequestedStartDate,
                    x.RequestedStartTime,
                    x.EstimatedDurationHours
                })
                .ToListAsync(cancellationToken);

            var conflictingBooking = sameDayBookings.FirstOrDefault(x =>
                BookingScheduleConflictHelper.Overlaps(
                    requestedStart,
                    requestedEnd,
                    BookingScheduleConflictHelper.ToScheduledStart(x.RequestedStartDate, x.RequestedStartTime),
                    BookingScheduleConflictHelper.ToScheduledEnd(x.RequestedStartDate, x.RequestedStartTime, x.EstimatedDurationHours)));

            if (conflictingBooking is not null)
            {
                throw new InvalidOperationException(
                    $"هذه الخدمة محجوزة بالفعل في هذا التوقيت ضمن الحجز رقم {conflictingBooking.BookingNumber}. من فضلك اختر وقتاً آخر.");
            }
        }
    }
}