using HEVEQ.Application.Features.MarketPlaceListings.DTOs;
using HEVEQ.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace HEVEQ.Application.Features.MarketPlace.DTOs
{
    public class MarketplaceListingDetailsDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int CategoryId { get; set; }
        public string Condition { get; set; } = string.Empty;
        public string ConditionAr { get; set; } = string.Empty;
        public int? YearOfManufacture { get; set; }
        public string? Specifications { get; set; }
        public bool IsNegotiable { get; set; }
        public string TransactionMethod { get; set; } = string.Empty;
        public string? Governorate { get; set; }
        public string? District { get; set; }
        public string Status { get; set; } = string.Empty;
        public string StatusAr { get; set; } = string.Empty;
        public string? VideoUrl { get; set; }
        public List<MarketplaceListingPhotoDto> Photos { get; set; } = new();
        public ListingSellerDto Seller { get; set; } = new();
        public bool CanBuyNow { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ListingManagementInfoDto? ManagementInfo { get; set; }

    }
}
