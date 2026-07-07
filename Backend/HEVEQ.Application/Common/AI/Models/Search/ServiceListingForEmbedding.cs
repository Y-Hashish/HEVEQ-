
using System.Text;

namespace HEVEQ.Application.Common.AI.Models.Search;

/// <summary>
/// Flat projection of an active ServiceListing joined with ProviderProfile and Category,
/// carrying every field needed to:
///   (a) build the rich text string that gets embedded into a 1536-dim vector,
///   (b) populate the Qdrant point payload (used for filtering + score boosting),
///   (c) write the post-sync status back to ServiceListings.EmbeddingStatus.
/// </summary>
public sealed record ServiceListingForEmbedding(
    Guid Id,
    string Title,
    string Description,
    string? Tags,
    string? EquipmentModel,
    string? EquipmentCapacity,
    int CategoryId,
    string CategoryName,
    decimal HourlyRate,
    string ProviderCompanyName,
    Guid ProviderProfileId,
    double ProviderBaseLatitude,
    double ProviderBaseLongitude)
{
    /// <summary>
    /// Builds the rich text string that is sent to text-embedding-3-small.
    /// Combines category, title, description, equipment specs, and tags so the
    /// resulting vector captures both semantic meaning and technical specifics.
    /// The provider name is appended last so workshop reputation can influence
    /// query proximity when customers search by brand.
    /// </summary>
    public string BuildEmbeddingText()
    {
        var sb = new StringBuilder(512);

        // Category context first — helps the model anchor the domain
        sb.Append(CategoryName).Append(" service. ");
        sb.Append(Title).Append(". ");
        sb.Append(Description.TrimEnd('.', ' ')).Append(". ");

        if (!string.IsNullOrWhiteSpace(EquipmentModel))
            sb.Append("Equipment model: ").Append(EquipmentModel).Append(". ");

        if (!string.IsNullOrWhiteSpace(EquipmentCapacity))
            sb.Append("Capacity: ").Append(EquipmentCapacity).Append(". ");

        if (!string.IsNullOrWhiteSpace(Tags))
            sb.Append("Keywords: ").Append(Tags).Append(". ");

        sb.Append("Provider: ").Append(ProviderCompanyName).Append('.');

        return sb.ToString();
    }

    /// <summary>
    /// Builds the Qdrant payload dictionary stored alongside the vector.
    /// <list type="bullet">
    ///   <item><c>is_active</c> — MUST be present; the ActiveFilter in
    ///     QdrantListingIndex.QuerySimilarAsync uses it to exclude suspended points.</item>
    ///   <item><c>category_id</c> / <c>hourly_rate</c> — available for future
    ///     payload filtering (e.g. price-range search).</item>
    ///   <item>Remaining fields are stored for observability and debugging.</item>
    /// </list>
    /// </summary>
    public IReadOnlyDictionary<string, object> BuildQdrantPayload() =>
        new Dictionary<string, object>
        {
            ["is_active"] = true,
            ["category_id"] = CategoryId,
            ["category_name"] = CategoryName,
            ["hourly_rate"] = (double)HourlyRate,
            ["provider_profile_id"] = ProviderProfileId.ToString(),
            ["provider_name"] = ProviderCompanyName,
            ["title"] = Title
        };
}