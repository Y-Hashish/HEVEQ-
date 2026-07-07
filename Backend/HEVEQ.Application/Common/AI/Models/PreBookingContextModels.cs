using HEVEQ.Domain.Enums;

namespace HEVEQ.Application.Common.AI.Models;
public sealed record PreBookingContext(
    Guid BookingId,
    Guid CustomerId,
    Guid ServiceListingId,
    string JobTitle,
    BookingStatus Status,

    double? SiteLatitude,
    double? SiteLongitude,

    DateOnly RequestedStartDate,
    TimeOnly RequestedStartTime,
    decimal EstimatedDurationHours,
    Guid? AssignedOperatorId,

    bool IsOutOfZoneBooking,
    decimal? OutOfZoneSurchargeAmount,
    DateTime? OutOfZoneSurchargeAcceptedAt,

    Guid ProviderProfileId,
    double? ProviderBaseLatitude,
    double? ProviderBaseLongitude,
    int ProviderServiceRadiusKm,
    decimal ProviderTrustScore,
    TrustLevel ProviderTrustLevel,
    int ProviderCompletedBookingsCount,
    decimal ProviderResponseRate);