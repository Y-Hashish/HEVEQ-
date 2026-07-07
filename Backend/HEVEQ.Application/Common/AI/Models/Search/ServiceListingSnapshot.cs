using System;

namespace HEVEQ.Application.Common.AI.Models.Search;

/// <summary>
/// Hydrated SQL Server projection of an active ServiceListing joined to its
/// ProviderProfile. Grounded in v3.0 schema columns.
/// </summary>
public sealed record ServiceListingSnapshot(
    Guid Id,
    Guid ProviderProfileId,
    string Title,
    string Description,
    int CategoryId,
    decimal HourlyRate,
    decimal? DailyRate,
    string ProviderCompanyName,
    decimal ProviderAverageRating,
    double? DistanceKm);