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

    //[KernelFunction("extract_search_intent")]
    //[Description("Parses a customer's Arabic/English query...")]
    //public async Task<SearchIntentResponseDto> ExtractIntentAsync(
    //    string rawQuery,
    //    IReadOnlyList<ConversationTurn>? history = null,
    //    CancellationToken ct = default)
    //{
    //    var chat = new ChatHistory("...");
    //    var response = await _chat.GetChatMessageContentAsync(chat, cancellationToken: ct);

    //    return new SearchIntentResponseDto(null, null, rawQuery, "English");
    //}


    [KernelFunction("extract_search_intent")]
    [Description("Parses a customer's Arabic/English query to extract heavy equipment type, location, and task description.")]
    public async Task<SearchIntentResponseDto> ExtractIntentAsync(
            string rawQuery,
            IReadOnlyList<ConversationTurn>? history = null,
            CancellationToken ct = default)
    {
        var systemPrompt = @"You are an expert AI Search Assistant for HEVEQ, a heavy equipment marketplace in Egypt.
Your job is to parse the user's raw query and extract:
1. equipment_type: The specific type of machine (e.g., 'حفار', 'كرك', 'ونش', 'خلاطة'). If not explicitly clear or missing, return null.
2. location: The Egyptian city or governorate mentioned (e.g., 'طنطا', 'القاهرة', 'الغربية'). Normalize it to standard text or return null if missing.
3. task_description: A clean summary of the work needed.
4. language: Detect if the query is in 'EgyptianArabic' or 'English'.

Respond ONLY with a valid JSON object matching this schema:
{
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

        // استخدام الـ StructuredSettings عشان نضمن الـ JSON Mode من الـ AI
        var settings = Utilities.StructuredSettings.JsonMode();

        var response = await _chat.GetChatMessageContentAsync(chat, executionSettings: settings, cancellationToken: ct);
        var jsonText = response.ToString();

        // الـ Fallback هنا كائن فارغ لكن ديناميكي، والـ Utilities هتنظف الـ Fences لو ظهرت
        var fallback = new SearchIntentResponseDto(null, null, rawQuery, "EgyptianArabic");
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

        if (listings.Count == 0)
            return result;

        return result;
    }

    //[KernelFunction("generate_clarification")]
    //public async Task<ClarificationResponseDto> GenerateClarificationAsync(
    //    SearchIntentResponseDto partialIntent,
    //    IReadOnlyList<string> missingParams,
    //    CancellationToken ct = default)
    //{
    //    return new ClarificationResponseDto("...", "English");
    //}


    [KernelFunction("generate_clarification")]
    public async Task<ClarificationResponseDto> GenerateClarificationAsync(
        SearchIntentResponseDto partialIntent,
        IReadOnlyList<string> missingParams,
        CancellationToken ct = default)
    {
        var missingParamsText = string.Join(", ", missingParams);
        var systemPrompt = $@"You are a friendly AI customer service agent for HEVEQ heavy equipment marketplace. 
The user wants to search for equipment, but these parameters are missing: [{missingParamsText}].
Write a short, professional, yet friendly response in natural Egyptian Arabic (اللهجة العامية المصرية) asking the user to clarify the missing information (e.g., asking where they need the machine or what specific type of equipment they need). 
Do not use MSA (فصحى). Keep it warm and brief.

Respond ONLY with a valid JSON object matching this schema:
{{
  ""message"": ""your friendly question here"",
  ""language"": ""EgyptianArabic""
}}";

        var chat = new ChatHistory(systemPrompt);

        // تطبيق الـ JSON Mode هنا كمان
        var settings = Utilities.StructuredSettings.JsonMode();

        var response = await _chat.GetChatMessageContentAsync(chat, executionSettings: settings, cancellationToken: ct);

        // لو الـ Parsing فشل لأي سبب، الجملة دي هتكون مجرد خط دفاع أخير، لكن الطبيعي الـ AI هيرد ديناميكياً
        var fallback = new ClarificationResponseDto("يا غالي محتاجين نعرف تفاصيل أكتر عن نوع المعدة والمكان فين بالظبط؟", "EgyptianArabic");

        return Utilities.JsonSafe.DeserializeOrFallback(response.ToString(), fallback);
    }
}