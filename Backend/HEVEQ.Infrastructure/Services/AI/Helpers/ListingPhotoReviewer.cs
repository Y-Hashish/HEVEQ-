using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.Text.Json;

namespace HEVEQ.Infrastructure.Services.AI.Helpers
{
    public class ListingPhotoReviewer(Kernel kernel, IHttpClientFactory httpClientFactory)
    {
        private const int MaxPhotosToReview = 5; // cost control — review first 5 only

        public record PhotoReviewResult(
            string PhotoUrl,
            bool IsFlagged,
            string? Flag,
            string? Reason);

        public async Task<List<PhotoReviewResult>> ReviewAsync(
            IEnumerable<string> photoUrls, CancellationToken ct)
        {
            var results = new List<PhotoReviewResult>();
            var urls = photoUrls.Take(MaxPhotosToReview).ToList();

            foreach (var url in urls)
            {
                try
                {
                    var result = await ReviewSinglePhotoAsync(url, ct);
                    results.Add(result);
                }
                catch (Exception)
                {
                    // Don't fail the whole job if one photo review fails
                    results.Add(new PhotoReviewResult(url, false, null, null));
                }
            }

            return results;
        }

        private async Task<PhotoReviewResult> ReviewSinglePhotoAsync(string url, CancellationToken ct)
        {
            // Fetch photo bytes
            var client = httpClientFactory.CreateClient();
            var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct);
            response.EnsureSuccessStatusCode();

            var mimeType = response.Content.Headers.ContentType?.MediaType ?? "image/jpeg";
            var bytes = await response.Content.ReadAsByteArrayAsync(ct);

            var chat = kernel.GetRequiredService<IChatCompletionService>();
            var history = new ChatHistory();

            history.AddSystemMessage(
                "You are a photo content moderation AI for an equipment rental marketplace in Egypt. " +
                "Your job is to flag photos that violate platform rules. " +
                "Respond ONLY with valid JSON — no markdown, no explanation.");

            var userMessage = new ChatMessageContentItemCollection
            {
                new TextContent("""
                    Review this listing photo and return this exact JSON:
                    {
                      "isFlagged": true | false,
                      "flag": "FLAG_CODE or null",
                      "reason": "one sentence explanation or null"
                    }

                    Flag the photo if it contains ANY of the following:
                    - Phone numbers, WhatsApp numbers, or any contact information
                    - Email addresses
                    - Watermarks with external platform names (Facebook, Instagram, WhatsApp, OLX, etc.)
                    - Text overlays advertising external websites or social media
                    - Inappropriate or offensive content
                    - Completely unrelated content (not equipment or parts)
                    - Screenshot of another website or app
                    - Heavily edited or fake/AI-generated equipment photos
                    - QR codes or barcodes that link externally

                    Use one of these flag codes:
                    CONTACT_INFO — phone, email, or WhatsApp visible
                    EXTERNAL_PLATFORM — competitor or social media watermark
                    INAPPROPRIATE_CONTENT — offensive or adult content
                    UNRELATED_CONTENT — not equipment or parts
                    EXTERNAL_LINK — QR code or URL visible
                    FAKE_PHOTO — AI-generated or heavily manipulated
                    SCREENSHOT — screenshot of another app or website

                    If the photo is clean equipment photography: isFlagged=false, flag=null, reason=null
                    """),
                new ImageContent(new ReadOnlyMemory<byte>(bytes), mimeType)
            };

            history.Add(new ChatMessageContent(AuthorRole.User, userMessage));

            var settings = new OpenAIPromptExecutionSettings
            {
                ResponseFormat = "json_object"
            };

            var aiResponse = await chat.GetChatMessageContentAsync(history, settings, kernel, ct);
            var content = aiResponse.Content ?? "{}";

            using var doc = JsonDocument.Parse(content);
            var root = doc.RootElement;

            var isFlagged = root.TryGetProperty("isFlagged", out var fEl) && fEl.GetBoolean();
            var flag = root.TryGetProperty("flag", out var flEl) && flEl.ValueKind != JsonValueKind.Null
                ? flEl.GetString() : null;
            var reason = root.TryGetProperty("reason", out var rEl) && rEl.ValueKind != JsonValueKind.Null
                ? rEl.GetString() : null;

            return new PhotoReviewResult(url, isFlagged, flag, reason);
        }
    }
}