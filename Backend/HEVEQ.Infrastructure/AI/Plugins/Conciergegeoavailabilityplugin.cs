using System.ComponentModel;
using System.Text.Json;
using HEVEQ.Application.Common.Interfaces;
using HEVEQ.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;

namespace HEVEQ.Infrastructure.AI.Plugins;

/// <summary>
/// Concierge's data-access surface. Every function queries EF Core directly and returns plain
/// JSON facts — no reasoning happens here. The ConciergeAgent (ChatCompletionAgent) decides what
/// the facts mean; this plugin never computes a verdict.
/// Owned exclusively by Concierge — Moderator never references this plugin.
/// </summary>
public sealed class ConciergeGeoAvailabilityPlugin
{
    private readonly IApplicationDbContext _db;

    public ConciergeGeoAvailabilityPlugin(IApplicationDbContext db)
    {
        _db = db;
    }

    [KernelFunction("check_geo_fence")]
    [Description("Checks whether the booking's site location falls inside the provider's service " +
                 "radius, or whether an out-of-zone surcharge was already accepted by the customer. " +
                 "Returns JSON: { withinServiceRadius, distanceKm, providerServiceRadiusKm, " +
                 "isOutOfZoneBooking, outOfZoneSurchargeAccepted }.")]
    public async Task<string> CheckGeoFenceAsync(
        [Description("The booking's Id")] Guid bookingId,
        CancellationToken ct = default)
    {
        var booking = await _db.Bookings
            .AsNoTracking()
            .Where(b => b.Id == bookingId)
            .Select(b => new
            {
                b.Latitude,
                b.Longitude,
                b.IsOutOfZoneBooking,
                b.OutOfZoneSurchargeAcceptedAt,
                ProviderBaseLatitude = b.ServiceListing.ProviderProfile.BaseLatitude,
                ProviderBaseLongitude = b.ServiceListing.ProviderProfile.BaseLongitude,
                ProviderServiceRadiusKm = b.ServiceListing.ProviderProfile.ServiceRadiusKm
            })
            .FirstOrDefaultAsync(ct);

        if (booking is null)
        {
            return JsonSerializer.Serialize(new { error = "booking_not_found" });
        }

        double? distanceKm = null;
        bool withinServiceRadius = true;

        if (booking.Latitude.HasValue && booking.Longitude.HasValue &&
            booking.ProviderBaseLatitude.HasValue && booking.ProviderBaseLongitude.HasValue)
        {
            distanceKm = HaversineKm(
                (double)booking.Latitude.Value, (double)booking.Longitude.Value,
                (double)booking.ProviderBaseLatitude.Value, (double)booking.ProviderBaseLongitude.Value);

            withinServiceRadius = distanceKm.Value <= booking.ProviderServiceRadiusKm;
        }

        return JsonSerializer.Serialize(new
        {
            withinServiceRadius,
            distanceKm = distanceKm.HasValue
    ? (double?)Math.Round(distanceKm.Value, 1)
    : null,
            providerServiceRadiusKm = booking.ProviderServiceRadiusKm,
            isOutOfZoneBooking = booking.IsOutOfZoneBooking,
            outOfZoneSurchargeAccepted = booking.OutOfZoneSurchargeAcceptedAt.HasValue
        });
    }

    [KernelFunction("check_schedule_availability")]
    [Description("Checks the requested booking slot against the listing's weekly availability " +
                 "window, blackout dates, and the assigned operator's existing assignments for " +
                 "overlap. Returns JSON: { withinListedHours, isBlackoutDate, hasOperatorConflict, " +
                 "requestedStartDate, requestedStartTime, estimatedDurationHours }.")]
    public async Task<string> CheckScheduleAvailabilityAsync(
        [Description("The booking's Id")] Guid bookingId,
        CancellationToken ct = default)
    {
        var booking = await _db.Bookings
            .AsNoTracking()
            .Where(b => b.Id == bookingId)
            .Select(b => new
            {
                b.ServiceListingId,
                b.AssignedOperatorId,
                b.RequestedStartDate,
                b.RequestedStartTime,
                b.EstimatedDurationHours
            })
            .FirstOrDefaultAsync(ct);

        if (booking is null)
        {
            return JsonSerializer.Serialize(new { error = "booking_not_found" });
        }

        var dayOfWeek = (int)booking.RequestedStartDate.DayOfWeek;
        var requestedEndTime = booking.RequestedStartTime.AddHours((double)booking.EstimatedDurationHours);

        var withinListedHours = await _db.ServiceListingAvailability
            .AsNoTracking()
            .Where(a => a.ListingId == booking.ServiceListingId && a.DayOfWeek == dayOfWeek)
            .AnyAsync(a => a.OpenTime <= booking.RequestedStartTime && a.CloseTime >= requestedEndTime, ct);

        var isBlackoutDate = await _db.BlackoutDates
            .AsNoTracking()
            .AnyAsync(bd =>
                bd.ListingId == booking.ServiceListingId &&
                bd.Date == booking.RequestedStartDate &&
                (bd.OperatorId == null || bd.OperatorId == booking.AssignedOperatorId), ct);

        var hasOperatorConflict = false;
        if (booking.AssignedOperatorId.HasValue)
        {
            var requestedStart = booking.RequestedStartDate.ToDateTime(booking.RequestedStartTime);
            var requestedEnd = requestedStart.AddHours((double)booking.EstimatedDurationHours);

            hasOperatorConflict = await _db.OperatorAssignments
                .AsNoTracking()
                .Where(oa => oa.OperatorId == booking.AssignedOperatorId.Value && oa.BookingId != bookingId)
                .Where(oa => oa.Status != OperatorAssignmentStatus.Cancelled)
                .AnyAsync(oa => oa.ScheduledStart < requestedEnd && oa.ScheduledEnd > requestedStart, ct);
        }

        return JsonSerializer.Serialize(new
        {
            withinListedHours,
            isBlackoutDate,
            hasOperatorConflict,
            requestedStartDate = booking.RequestedStartDate.ToString("yyyy-MM-dd"),
            requestedStartTime = booking.RequestedStartTime.ToString("HH:mm"),
            estimatedDurationHours = booking.EstimatedDurationHours
        });
    }

    private static double HaversineKm(double lat1, double lon1, double lat2, double lon2)
    {
        const double earthRadiusKm = 6371.0;
        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return earthRadiusKm * c;
    }

    private static double ToRadians(double degrees) => degrees * Math.PI / 180.0;
}