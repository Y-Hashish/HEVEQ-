using HEVEQ.Application.Common.AI.Interfaces;
using HEVEQ.Application.Common.AI.Models;
using HEVEQ.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.Text.Json;

namespace HEVEQ.Infrastructure.Services.AI;

public class MarketplaceListingRiskAnalyzer(
    IApplicationDbContext context,
    Kernel kernel,
    IHttpClientFactory httpClientFactory,
    ILogger<MarketplaceListingRiskAnalyzer> logger)
    : IListingRiskAnalyzer
{
    public Task<ListingRiskReport> AnalyzeServiceListingAsync(
        Guid serviceListingId, CancellationToken ct = default)
        => throw new NotSupportedException("Use ServiceListingRiskAnalyzer for service listings.");

    public async Task<ListingRiskReport> AnalyzeMarketplaceListingAsync(
        Guid marketplaceListingId, CancellationToken ct = default)
    {
        var listing = await context.MarketplaceListings
            .AsNoTracking()
            .Include(l => l.Category)
            .Include(l => l.Photos)
            .FirstOrDefaultAsync(l => l.Id == marketplaceListingId, ct)
            ?? throw new InvalidOperationException(
                $"MarketplaceListing {marketplaceListingId} not found.");

        // --- Build listing context for AI ---
        var listingContext = $"""
            Title: {listing.Title}
            Category: {listing.Category?.Name ?? "Unknown"}
            Condition: {listing.Condition}
            Year of Manufacture: {listing.YearOfManufacture?.ToString() ?? "Not specified"}
            Price: {listing.Price:N0} EGP
            Negotiable: {(listing.IsNegotiable ? "Yes" : "No")}
            Transaction Method: {listing.TransactionMethod}
            Location: {listing.Governorate}, {listing.District}
            Description: {listing.Description}
            Specifications: {listing.Specifications ?? "None"}
            """;

        // --- Fetch photo bytes ---
        var photoBytes = await FetchPhotoBytesAsync(
            listing.Photos.Select(p => p.PhotoUrl).ToList(), ct);

        // --- Single AI call reviews everything ---
        var result = await RunFullReviewAsync(listingContext, photoBytes, ct);

        var riskLevel = ToRiskLevel(result.RiskScore);

        logger.LogInformation(
            "Marketplace moderation complete for {Id}. Score={Score}, Risk={Level}",
            marketplaceListingId, result.RiskScore, riskLevel);

        return new ListingRiskReport(
            ListingId: marketplaceListingId,
            RiskScore: result.RiskScore,
            RiskLevel: riskLevel,
            Recommendation: ToRecommendation(result.RiskScore),
            SpamDetected: result.SpamDetected,
            PriceAnomalous: result.PriceAnomalous,
            PriceAnomalyReason: result.PriceAnomalyReason,
            ContactLeakDetected: result.ContactLeakDetected,
            Flags: result.Flags,
            PhotoFlags: result.PhotoFlags,
            AdminNote: result.AdminNote,
            AnalyzedAtUtc: DateTime.UtcNow);
    }

    private async Task<AiReviewResult> RunFullReviewAsync(
        string listingContext,
        List<(byte[] Bytes, string MimeType)> photos,
        CancellationToken ct)
    {
        var chat = kernel.GetRequiredService<IChatCompletionService>();
        var history = new ChatHistory();

        history.AddSystemMessage("""
    You are a content moderation AI for a heavy-equipment marketplace platform in Egypt.
    You review listing text AND photos together in one comprehensive review.
    You help human admins make decisions — you never approve or reject yourself.
    Respond ONLY with valid JSON. No markdown, no explanation.
    Analyze both Arabic and English content.
    ALL text values in your response (flags, reasons, notes) MUST be written in Arabic.
    Flag codes must remain in English (e.g. SPAM_DETECTED, PRICE_ANOMALY) but all
    explanations, reasons, and notes must be in Arabic.
    """);

        // Build multimodal message — text + all photos
        var userContent = new ChatMessageContentItemCollection();

        userContent.Add(new TextContent($$"""
            Review this marketplace equipment listing comprehensively.

            === LISTING DETAILS ===
            {{listingContext}}

            === PHOTOS ===
            {{(photos.Count > 0 ? $"{photos.Count} photo(s) attached. Review each one." : "No photos uploaded yet.")}}

            Return ONLY this exact JSON:
            {
              "riskScore": 0-100,
              "spamDetected": true | false,
              "priceAnomalous": true | false,
              "priceAnomalyReason": "explanation or null",
              "contactLeakDetected": true | false,
              "flags": [],
              "photoFlags": [],
              "adminNote": "one paragraph summary for the admin reviewer or null"
            }

            === TEXT REVIEW RULES ===

            SPAM — set spamDetected=true and riskScore 70-100 if ANY:
            - Random Arabic/English characters (keyboard mashing)
            - Repeated words excessively: "حفار حفار حفار" or "mixer mixer mixer"
            - Gibberish or meaningless sequences
            - Copy-pasted filler with no real information
            - Keyword stuffing for search manipulation

            CONTACT LEAK — contactLeakDetected=true, add CONTACT_LEAK flag if ANY:
            - أرقام التليفون (Egyptian format or international)
            - Email addresses
            - WhatsApp, Telegram, Facebook, Instagram, OLX references
            - External URLs

            PRICE — evaluate using your knowledge of Egyptian heavy equipment market:
            - Is this price realistic for the category, condition, and year stated?
            - Suspiciously low price may indicate scam or bait-and-switch
            - Suspiciously high price may indicate price gouging
            - If anomalous: priceAnomalous=true, explain in priceAnomalyReason, add PRICE_ANOMALY flag
            - priceAnomalyReason: اكتب السبب بالعربي

            QUALITY (not spam, just low quality) — riskScore 30-60:
            - Add flags: LOW_QUALITY_DESCRIPTION, TOO_SHORT, MISSING_SPECS, VAGUE_CONDITION

            CLEAN listing — riskScore 0-30, all booleans false, empty flags

            === PHOTO REVIEW RULES ===
            Review each attached photo and add to photoFlags array:
            Format: "PHOTO_N:FLAG_CODE:reason" where N is photo number (1, 2, 3...)
            - reason field in photoFlags MUST be in Arabic
            Format: "PHOTO_N:FLAG_CODE:السبب بالعربي"
             

            Flag codes:
            - CONTACT_INFO — phone number, email, or WhatsApp visible in photo
            - EXTERNAL_PLATFORM — competitor or social media watermark (OLX, Facebook, etc.)
            - UNRELATED_CONTENT — not equipment or parts
            - INAPPROPRIATE_CONTENT — offensive material
            - EXTERNAL_LINK — QR code or URL visible
            - FAKE_PHOTO — AI-generated or heavily manipulated
            - SCREENSHOT — screenshot of another app or website
            - LOW_QUALITY — too blurry or dark to verify equipment condition

            If all photos are clean equipment photography: photoFlags=[]

            === RISK SCORE GUIDE ===
            0-30: Clean listing, approve
            31-60: Minor issues, review recommended
            61-79: Significant issues, approval requires admin attention
            80-100: High risk, strongly recommend rejection

            Add each applicable flag to the flags array:
            SPAM_DETECTED, CONTACT_LEAK, PRICE_ANOMALY,
            LOW_QUALITY_DESCRIPTION, TOO_SHORT, MISSING_SPECS, VAGUE_CONDITION

            adminNote: اكتب ملخص واضح للأدمن باللغة العربية يوضح المشاكل والإجراء المقترح. null لو الـ listing نظيف.

            Return ONLY JSON.
            """));

        // Attach photos to the same message
        foreach (var (bytes, mimeType) in photos)
            userContent.Add(new ImageContent(new ReadOnlyMemory<byte>(bytes), mimeType));

        history.Add(new ChatMessageContent(AuthorRole.User, userContent));

        var settings = new OpenAIPromptExecutionSettings
        {
            ResponseFormat = "json_object"
        };

        var response = await chat.GetChatMessageContentAsync(history, settings, kernel, ct);
        var content = response.Content ?? "{}";

        logger.LogInformation("AI Review Response: {Response}", content);

        using var doc = JsonDocument.Parse(content);
        var root = doc.RootElement;

        return new AiReviewResult(
            RiskScore: root.TryGetProperty("riskScore", out var rsEl) ? rsEl.GetInt32() : 0,
            SpamDetected: root.TryGetProperty("spamDetected", out var spEl) && spEl.GetBoolean(),
            PriceAnomalous: root.TryGetProperty("priceAnomalous", out var paEl) && paEl.GetBoolean(),
            PriceAnomalyReason: root.TryGetProperty("priceAnomalyReason", out var prEl) && prEl.ValueKind != JsonValueKind.Null
                ? prEl.GetString() : null,
            ContactLeakDetected: root.TryGetProperty("contactLeakDetected", out var clEl) && clEl.GetBoolean(),
            Flags: root.TryGetProperty("flags", out var fEl)
                ? fEl.EnumerateArray()
                    .Select(f => f.GetString() ?? "")
                    .Where(f => f.Length > 0)
                    .ToList()
                : new List<string>(),
            PhotoFlags: root.TryGetProperty("photoFlags", out var pfEl)
                ? pfEl.EnumerateArray()
                    .Select(f => f.GetString() ?? "")
                    .Where(f => f.Length > 0)
                    .ToList()
                : new List<string>(),
            AdminNote: root.TryGetProperty("adminNote", out var anEl) && anEl.ValueKind != JsonValueKind.Null
                ? anEl.GetString() : null);
    }

    private async Task<List<(byte[] Bytes, string MimeType)>> FetchPhotoBytesAsync(
        List<string> urls, CancellationToken ct)
    {
        const int MaxPhotos = 5; // cost control
        var results = new List<(byte[], string)>();
        var client = httpClientFactory.CreateClient();

        foreach (var url in urls.Take(MaxPhotos))
        {
            try
            {
                var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct);
                response.EnsureSuccessStatusCode();
                var mimeType = response.Content.Headers.ContentType?.MediaType ?? "image/jpeg";
                var bytes = await response.Content.ReadAsByteArrayAsync(ct);
                results.Add((bytes, mimeType));
            }
            catch
            {
                // Skip failed photo fetch — don't fail the whole job
            }
        }

        return results;
    }

    private static RiskLevel ToRiskLevel(int score) => score switch
    {
        >= 70 => RiskLevel.High,
        >= 40 => RiskLevel.Medium,
        _ => RiskLevel.Low
    };

    private static ModerationRecommendation ToRecommendation(int score) => score switch
    {
        >= 70 => ModerationRecommendation.Reject,
        >= 40 => ModerationRecommendation.ApproveWithConditions,
        _ => ModerationRecommendation.Approve
    };

    private record AiReviewResult(
        int RiskScore,
        bool SpamDetected,
        bool PriceAnomalous,
        string? PriceAnomalyReason,
        bool ContactLeakDetected,
        IReadOnlyList<string> Flags,
        IReadOnlyList<string> PhotoFlags,
        string? AdminNote);
}