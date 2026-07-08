using HEVEQ.Application.Common.Interfaces;
using HEVEQ.Application.Features.ServiceListings.DTOs;
using HEVEQ.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HEVEQ.Application.Features.ServiceListings.Queries.GetPublicServiceListingById;

public class GetPublicServiceListingByIdQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetPublicServiceListingByIdQuery, PublicServiceListingDetailDto?>
{
    public async Task<PublicServiceListingDetailDto?> Handle(
        GetPublicServiceListingByIdQuery request, CancellationToken cancellationToken)
    {
        // 1. fetch listing with essential includes and apply active filter for guests/customers
        var listing = await context.ServiceListings
            .AsNoTracking()
            .Include(l => l.Category)
            .Include(l => l.ProviderProfile)
            .Include(l => l.Photos)
            .Include(l => l.Availability)
            .Include(l => l.ServiceListingOperators)
                .ThenInclude(lo => lo.Operator)
            .FirstOrDefaultAsync(l => l.Id == request.Id && l.Status == ServiceListingStatus.Active, cancellationToken);

        // 2. return null if listing not found or not active
        if (listing == null)
        {
            return null;
        }

        // 3. map photos list safely
        var photos = listing.Photos != null
            ? listing.Photos.OrderBy(p => p.DisplayOrder).Select(p => p.PhotoUrl).ToList()
            : new List<string>();

        // 4. map operators list safely using fallback for null entries
        var operators = listing.ServiceListingOperators != null
            ? listing.ServiceListingOperators
                .Where(lo => lo.Operator != null)
                .Select(lo => new PublicOperatorSummaryDto(
                    lo.Operator!.Id,
                    lo.Operator.FullName ?? "Unknown Operator",
                    lo.Operator.LicenseType,
                    4.5 // Temporary dummy rating for UI until rating system is linked
                )).ToList()
            : new List<PublicOperatorSummaryDto>();

        // 5. map provider summary details with fallbacks
        var providerDto = new PublicProviderSummaryDto(
            ProviderProfileId: listing.ProviderProfileId,
            listing.ProviderProfile != null ? listing.ProviderProfile.CompanyName : "Unknown Provider",
            listing.ProviderProfile != null ? (double)listing.ProviderProfile.AverageRating : 0.0,
            listing.ProviderProfile != null ? listing.ProviderProfile.CompletedBookingsCount : 0,
            listing.ProviderProfile != null ? (int)listing.ProviderProfile.TrustScore : 0,
            listing.ProviderProfile != null ? listing.ProviderProfile.TrustLevel.ToString() : "Standard",
            listing.Governorate
        );

        // 6. map real availability slots
        var availability = listing.Availability
            .OrderBy(a => a.DayOfWeek)
            .ThenBy(a => a.OpenTime)
            .Select(a => new PublicAvailabilityDto(a.Id, a.DayOfWeek, a.OpenTime, a.CloseTime))
            .ToList();


        // 7. live, listing-specific review aggregate. Some old reviews were saved
        // against BookingId only, so include reviews whose booking belongs to this service.

        var listingReviews = await context.Reviews
                            .AsNoTracking()
                            .Where(r => r.IsPublished &&
                                (r.ServiceListingId == listing.Id ||
                                 (r.BookingId != null && context.Bookings.Any(b => b.Id == r.BookingId && b.ServiceListingId == listing.Id))))
                            .Select(r => r.Rating)
                            .ToListAsync(cancellationToken);

        var reviewsSummary = new PublicReviewsSummaryDto(
            AverageRating: listingReviews.Count > 0 ? listingReviews.Average() : 0.0,
            TotalReviewsCount: listingReviews.Count
        );

        // 7. compose final structural public detail dto
        return new PublicServiceListingDetailDto(
            listing.Id,
            listing.Title,
            listing.Description ?? string.Empty,
            listing.Category != null ? listing.Category.Name : "General",
            listing.EquipmentModel,
            listing.EquipmentCapacity,
            listing.EquipmentCondition,
            listing.YearOfManufacture,
            listing.HourlyRate ?? 0,
            listing.DailyRate ?? 0,
            listing.MinimumBookingHours,
            photos,
            availability,
            providerDto,
            operators,
            reviewsSummary,
            true // canRequestBooking default flag
        );
    }
}