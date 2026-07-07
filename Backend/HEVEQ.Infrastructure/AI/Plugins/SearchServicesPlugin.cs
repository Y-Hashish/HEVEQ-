using HEVEQ.Application.Common.AI;
using HEVEQ.Application.Common.AI.Models;
using HEVEQ.Application.Common.AI.Models.Search;
using HEVEQ.Infrastructure.Persistence.Qdrant;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Embeddings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace HEVEQ.Infrastructure.AI.Plugins;

public sealed record SearchIntentResponseDto(
    [property: JsonPropertyName("search_target")] string SearchTarget,
    [property: JsonPropertyName("equipment_type")] string? EquipmentType,
    [property: JsonPropertyName("location")] string? Location,
    [property: JsonPropertyName("task_description")] string TaskDescription,
    [property: JsonPropertyName("language")] string Language);

public sealed record ExplanationEntryDto(
    [property: JsonPropertyName("listing_id")] string ListingId,
    [property: JsonPropertyName("explanation")] string Explanation);

internal sealed record BatchExplanationResponseDto(
    [property: JsonPropertyName("explanations")] IReadOnlyList<ExplanationEntryDto> Explanations);

public sealed record ClarificationResponseDto(
    [property: JsonPropertyName("message")] string Message,
    [property: JsonPropertyName("language")] string Language);

public sealed class SearchServicesPlugin
{
    private readonly IChatCompletionService _chat;
    private readonly ITextEmbeddingGenerationService _embedding;
    private readonly IQdrantListingIndex _qdrant;

    public SearchServicesPlugin(
        IChatCompletionService chat,
        ITextEmbeddingGenerationService embedding,
        IQdrantListingIndex qdrant)
    {
        _chat = chat;
        _embedding = embedding;
        _qdrant = qdrant;
    }

    [KernelFunction("extract_search_intent")]
    [Description("Parses a customer's Arabic/English query to extract heavy equipment type, location, and task description.")]
    public async Task<SearchIntentResponseDto> ExtractIntentAsync(
            string rawQuery,
            IReadOnlyList<ConversationTurn>? history = null,
            CancellationToken ct = default)
    {
        // 1️⃣ هنا الـ Prompt الصح الخاص باستخراج النية وتحديد الـ Target
        var systemPrompt = @"You are an expert AI Search Assistant for HEVEQ, a heavy equipment platform in Egypt.
Your job is to parse the user's raw query and strictly extract:
1. search_target: Determine if the user wants to rent/hire ('ServiceListing') or buy/purchase ('Marketplace'). If the user just mentions an equipment (e.g., 'عايز حفار' or 'عايز ونش') without any explicit keywords for buying or renting, you MUST return 'Ambiguous'.
2. equipment_type: The specific type of machine (e.g., 'حفار', 'كرك', 'ونش', 'خلاطة'). If not explicitly clear or missing, return null.
3. location: The Egyptian city or governorate mentioned (e.g., 'طنطا', 'القاهرة', 'الغربية'). Normalize it to standard text or return null if missing.
4. task_description: A clean summary of the work needed or item sought.
5. language: Detect if the query is in 'EgyptianArabic' or 'English'.

Respond ONLY with a valid JSON object matching this schema:
{
  ""search_target"": string,
  ""equipment_type"": string or null,
  ""location"": string or null,
  ""task_description"": string,
  ""language"": string
}";

        var chat = new ChatHistory(systemPrompt);

        if (history != null)
        {
            foreach (var turn in history)
            {
                chat.AddMessage(turn.Role.Equals("user", StringComparison.OrdinalIgnoreCase) ? AuthorRole.User : AuthorRole.Assistant, turn.Content);
            }
        }

        chat.AddUserMessage(rawQuery);

        var settings = Utilities.StructuredSettings.JsonMode();
        var response = await _chat.GetChatMessageContentAsync(chat, executionSettings: settings, cancellationToken: ct);
        var jsonText = response.ToString();

        var fallback = new SearchIntentResponseDto("Ambiguous", null, null, rawQuery, "EgyptianArabic");
        return Utilities.JsonSafe.DeserializeOrFallback(jsonText, fallback);
    }

    [KernelFunction("search_similar_listings")]
    public async Task<IReadOnlyList<VectorSearchHit>> SearchSimilarListingsAsync(
        string intentText,
        int topK = 15,
        CancellationToken ct = default)
    {
        var vector = await _embedding.GenerateEmbeddingAsync(intentText, cancellationToken: ct);
        return await _qdrant.QuerySimilarAsync(vector, topK, ct);
    }

    [KernelFunction("explain_all_matches")]
    public async Task<IReadOnlyDictionary<Guid, string>> ExplainAllMatchesAsync(
        SearchIntentResponseDto intent,
        IReadOnlyList<ServiceListingSnapshot> listings,
        CancellationToken ct = default)
    {
        var result = new Dictionary<Guid, string>();
        return result;
    }

    [KernelFunction("explain_marketplace_matches")]
    public async Task<IReadOnlyDictionary<Guid, string>> ExplainMarketplaceMatchesAsync(
        SearchIntentResponseDto intent,
        IReadOnlyList<MarketplaceListingSnapshot> listings,
        CancellationToken ct = default)
    {
        var result = new Dictionary<Guid, string>();
        return result;
    }

    [KernelFunction("generate_clarification")]
    [Description("Generates a friendly response asking for missing details based strictly on the missing parameters list.")]
    public async Task<ClarificationResponseDto> GenerateClarificationAsync(
        SearchIntentResponseDto partialIntent,
        IReadOnlyList<string> missingParams,
        CancellationToken ct = default)
    {
        var missingParamsText = string.Join(", ", missingParams);

  
        var systemPrompt = $@"You are a friendly AI customer service agent for HEVEQ heavy equipment platform in Egypt.
The user is looking for heavy machinery, but some parameters are missing to fulfill their request.
The missing parameters list is: [{missingParamsText}].

Based STRICTLY on what is missing in that specific list, write a short, professional, yet friendly response in natural Egyptian Arabic (اللهجة العامية المصرية):
1. If 'SearchTarget' is in the list: Ask them if they want to RENT the equipment (تأجير / إيجار) or BUY it (شراء / للبيع).
2. If 'Location' is in the list: Ask where they need the machine or where they are located.
3. If 'EquipmentType' is in the list: Ask what specific type of machine they need (do NOT ask this if it's already known).

Combine these specific questions into one warm, brief, cohesive sentence. Do not use MSA (فصحى).

Respond ONLY with a valid JSON object matching this schema:
{{
  ""message"": ""your friendly dynamic question here"",
  ""language"": ""EgyptianArabic""
}}";

        var chat = new ChatHistory(systemPrompt);
        var settings = Utilities.StructuredSettings.JsonMode();

        var response = await _chat.GetChatMessageContentAsync(chat, executionSettings: settings, cancellationToken: ct);

        var fallback = new ClarificationResponseDto("يا غالي حابب تأجر الونش ده لشغلانة ولا بتدور على ونش تشريه؟ وعرفنا حابب يكون فين بالظبط؟", "EgyptianArabic");
        return Utilities.JsonSafe.DeserializeOrFallback(response.ToString(), fallback);
    }
}
