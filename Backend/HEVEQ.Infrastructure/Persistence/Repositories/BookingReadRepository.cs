
using HEVEQ.Application.Common.AI.Models;
using HEVEQ.Application.Common.Persistence.Interfaces;
using HEVEQ.Application.Common.Persistence.Models;
using Microsoft.EntityFrameworkCore;

namespace HEVEQ.Infrastructure.Persistence.Repositories;
public sealed class BookingReadRepository : IBookingReadRepository
{
    // ── ADAPT: replace with your actual DbContext class name ──────────────────
    private readonly ApplicationDbContext _context;

    public BookingReadRepository(ApplicationDbContext context)
        => _context = context;

    public async Task<BookingContext?> GetBookingContextAsync(
        Guid bookingId,
        CancellationToken ct = default)
    {
        // Single JOIN query: Bookings → ServiceListings → ProviderProfiles
        // All column names match ShareGear Database Schema v3.0.
        return await _context.Bookings
            .AsNoTracking()
            .Where(b => b.Id == bookingId)
            .Select(b => new BookingContext(
                       b.Id,
            b.CustomerId,
            b.ServiceListingId,
            b.JobTitle,
            b.Governorate,
            b.HourlyRateSnapshot,
            b.ProviderRejectionReason,
            b.ServiceListing.CategoryId,
            b.ServiceListing.ProviderProfileId,
            b.Latitude != null ? (double)b.Latitude : 0.0,
            b.Longitude != null ? (double)b.Longitude : 0.0))
        .FirstOrDefaultAsync(ct);
    }
    // ── NEW - pre-booking Concierge/Moderator evaluation ───────────────────────
    public async Task<PreBookingContext?> GetPreBookingContextAsync(
        Guid bookingId,
        CancellationToken ct = default)
    {
        var row = await _context.Bookings
            .AsNoTracking()
            .Where(b => b.Id == bookingId)
            .Select(b => new
            {
                b.Id,
                b.CustomerId,
                b.ServiceListingId,
                b.JobTitle,
                b.Status,
                b.Latitude,
                b.Longitude,
                b.RequestedStartDate,
                b.RequestedStartTime,
                b.EstimatedDurationHours,
                b.AssignedOperatorId,
                b.IsOutOfZoneBooking,
                b.OutOfZoneSurchargeAmount,
                b.OutOfZoneSurchargeAcceptedAt,
                ProviderProfileId = b.ServiceListing.ProviderProfileId,
                ProviderBaseLatitude = b.ServiceListing.ProviderProfile.BaseLatitude,
                ProviderBaseLongitude = b.ServiceListing.ProviderProfile.BaseLongitude,
                ProviderServiceRadiusKm = b.ServiceListing.ProviderProfile.ServiceRadiusKm,
                ProviderTrustScore = b.ServiceListing.ProviderProfile.TrustScore,
                ProviderTrustLevel = b.ServiceListing.ProviderProfile.TrustLevel,
                ProviderCompletedBookingsCount = b.ServiceListing.ProviderProfile.CompletedBookingsCount,
                ProviderResponseRate = b.ServiceListing.ProviderProfile.ResponseRate
            })
            .FirstOrDefaultAsync(ct);

        if (row is null)
        {
            return null;
        }

        return new PreBookingContext(
            BookingId: row.Id,
            CustomerId: row.CustomerId,
            ServiceListingId: row.ServiceListingId,
            JobTitle: row.JobTitle,
            Status: row.Status,
            SiteLatitude: row.Latitude is null ? null : (double)row.Latitude.Value,
            SiteLongitude: row.Longitude is null ? null : (double)row.Longitude.Value,
            RequestedStartDate: row.RequestedStartDate,
            RequestedStartTime: row.RequestedStartTime,
            EstimatedDurationHours: row.EstimatedDurationHours,
            AssignedOperatorId: row.AssignedOperatorId,
            IsOutOfZoneBooking: row.IsOutOfZoneBooking,
            OutOfZoneSurchargeAmount: row.OutOfZoneSurchargeAmount,
            OutOfZoneSurchargeAcceptedAt: row.OutOfZoneSurchargeAcceptedAt,
            ProviderProfileId: row.ProviderProfileId,
            ProviderBaseLatitude: row.ProviderBaseLatitude is null ? null : (double)row.ProviderBaseLatitude.Value,
            ProviderBaseLongitude: row.ProviderBaseLongitude is null ? null : (double)row.ProviderBaseLongitude.Value,
            ProviderServiceRadiusKm: row.ProviderServiceRadiusKm,
            ProviderTrustScore: row.ProviderTrustScore,
            ProviderTrustLevel: row.ProviderTrustLevel,
            ProviderCompletedBookingsCount: row.ProviderCompletedBookingsCount,
            ProviderResponseRate: row.ProviderResponseRate);
    }
}