using System;

namespace HEVEQ.Application.Common.AI.Models.Search;

public sealed record MarketplaceListingSnapshot(
    Guid Id,
    Guid SellerId,
    string Title,
    string Description,
    int CategoryId,
    decimal Price,
    string Condition,
    string SellerCompanyName,
    double SellerAverageRating,
    double? DistanceKm);