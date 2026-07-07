using System;
using System.Collections.Generic;
using System.Text;

namespace HEVEQ.Application.Common.Persistence.Models
{
    /// <summary>
    /// Minimal context record needed by the post-rejection re-engagement flow.
    /// Populated by a single JOIN across Bookings -> ServiceListings -> ProviderProfiles.
    /// </summary>
    public record BookingContext(
        Guid BookingId,
        Guid CustomerId,
        Guid ServiceListingId,
        string JobTitle,
        string Governorate,
        decimal HourlyRateSnapshot,
        string? ProviderRejectionReason,
        int CategoryId,
        Guid ProviderProfileId,
        double SiteLatitude,
        double SiteLongitude
    );
}