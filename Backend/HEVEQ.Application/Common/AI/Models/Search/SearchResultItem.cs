
namespace HEVEQ.Application.Common.AI.Models.Search;

/// <summary>One result card for the Angular search UI: the hydrated listing, its
/// Qdrant similarity score, and the single-sentence RAG match explanation.</summary>
public sealed record SearchResultItem(
    ServiceListingSnapshot? ServiceListing,
    MarketplaceListingSnapshot? MarketplaceListing,
    float SimilarityScore,
    string MatchExplanation);