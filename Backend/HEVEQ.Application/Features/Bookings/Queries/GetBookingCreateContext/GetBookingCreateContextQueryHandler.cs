using HEVEQ.Application.Common.Interfaces;
using HEVEQ.Application.Features.Bookings.DTOs;
using HEVEQ.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using HEVEQ.Application.Common.Helpers;
using HEVEQ.Application.Features.Bookings.Helpers;

namespace HEVEQ.Application.Features.Bookings.Queries.GetBookingCreateContext
{
    public sealed class GetBookingCreateContextQueryHandler : IRequestHandler<GetBookingCreateContextQuery, BookingCreateContextDto>
    {
        private readonly IApplicationDbContext _context;
        public GetBookingCreateContextQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<BookingCreateContextDto> Handle(GetBookingCreateContextQuery request, CancellationToken cancellationToken)
        {
            var listing = await _context.ServiceListings
                .AsNoTracking()
                .Include(x => x.ProviderProfile)
                .Include(x => x.Availability)
                .FirstOrDefaultAsync(x => x.Id == request.ServiceListingId, cancellationToken);

            if (listing is null)
                throw new InvalidOperationException("Service listing was not found.");

            if (listing.Status != ServiceListingStatus.Active)
                throw new InvalidOperationException("Booking is allowed only for active service listings.");

            var customerProfile = await _context.CustomerProfiles
                .AsNoTracking()
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.UserId == request.CustomerId, cancellationToken);

            var defaultAddress = await _context.Addresses
                .AsNoTracking()
                .Where(x => x.UserId == request.CustomerId)
                .OrderByDescending(x => x.IsDefault)
                .ThenByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            var missingRequirements = new List<string>();

            if (customerProfile is null)
            {
                missingRequirements.Add("Customer profile is required.");
            }
            else
            {
                var hasApprovedNationalId = await _context.Documents
                    .AsNoTracking()
                    .AnyAsync(d => d.UserId == request.CustomerId
                                   && d.DocumentType == DocumentType.NationalId
                                   && d.Status == DocumentVerificationStatus.Approved,
                        cancellationToken);

                if (customerProfile.RequiresAdditionalVerification || !hasApprovedNationalId)
                    missingRequirements.Add("يجب توثيق البطاقة الشخصية واعتمادها أولاً.");

                if (!customerProfile.User.PhoneNumberConfirmed)
                    missingRequirements.Add("يجب تأكيد رقم الهاتف.");
            }

            if (defaultAddress is null)
                missingRequirements.Add("At least one saved address is required.");

            var availability = listing.Availability
                .OrderBy(x => x.DayOfWeek)
                .ThenBy(x => x.OpenTime)
                .Select(x => new BookingCreateContextAvailabilityDto
                {
                    Id = x.Id,
                    DayOfWeek = x.DayOfWeek,
                    DayName = DayOfWeekDisplayHelper.GetDayName(x.DayOfWeek),
                    DayNameAr = DayOfWeekDisplayHelper.GetDayNameAr(x.DayOfWeek),
                    OpenTime = x.OpenTime,
                    CloseTime = x.CloseTime
                })
                .ToList();

            var today = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(3));
            var maxDate = today.AddDays(60);

            var blockedBookings = await _context.Bookings
                .AsNoTracking()
                .Where(x => x.ServiceListingId == listing.Id
                            && x.RequestedStartDate >= today
                            && x.RequestedStartDate <= maxDate
                            && BookingScheduleConflictHelper.BlockingStatuses.Contains(x.Status))
                .Select(x => new
                {
                    x.Id,
                    x.BookingNumber,
                    x.RequestedStartDate,
                    x.RequestedStartTime,
                    x.EstimatedDurationHours,
                    x.Status
                })
                .ToListAsync(cancellationToken);

            var unavailableSlots = blockedBookings
                .Select(x =>
                {
                    var end = BookingScheduleConflictHelper.ToScheduledEnd(
                        x.RequestedStartDate,
                        x.RequestedStartTime,
                        x.EstimatedDurationHours);

                    return new BookingUnavailableSlotDto
                    {
                        BookingId = x.Id,
                        BookingNumber = x.BookingNumber,
                        Date = x.RequestedStartDate,
                        StartTime = x.RequestedStartTime,
                        EndTime = TimeOnly.FromDateTime(end),
                        Status = x.Status.ToString(),
                        StatusAr = BookingDisplayHelper.GetStatusAr(x.Status)
                    };
                })
                .OrderBy(x => x.Date)
                .ThenBy(x => x.StartTime)
                .ToList();

            return new BookingCreateContextDto
            {
                ServiceListingId = listing.Id,
                ServiceTitle = listing.Title,
                ProviderCompany = listing.ProviderProfile.CompanyName,
                HourlyRate = listing.HourlyRate,
                DailyRate = listing.DailyRate,
                MinimumBookingHours = listing.MinimumBookingHours,
                ProviderBaseLatitude = listing.ProviderProfile.BaseLatitude,
                ProviderBaseLongitude = listing.ProviderProfile.BaseLongitude,
                ServiceRadiusKm = listing.ProviderProfile.ServiceRadiusKm,
                OutOfZoneSurchargePerKm = 25m,
                Availability = availability,
                UnavailableSlots = unavailableSlots,
                DefaultAddress = defaultAddress is null ? null
                    : new BookingCreateContextAddressDto
                    {
                        Id = defaultAddress.Id,
                        Label = defaultAddress.Label,
                        Governorate = defaultAddress.Governorate,
                        District = defaultAddress.District,
                        Street = defaultAddress.Street,
                        Latitude = defaultAddress.Latitude,
                        Longitude = defaultAddress.Longitude
                    },
                CustomerEligibility = new BookingCustomerEligibilityDto
                {
                    CanBook = missingRequirements.Count == 0,
                    MissingRequirements = missingRequirements
                }
            };
        }
    }
}