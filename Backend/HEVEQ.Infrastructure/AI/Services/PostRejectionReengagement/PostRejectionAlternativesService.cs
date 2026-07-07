using HEVEQ.Application.Common.AI;
using HEVEQ.Application.Common.AI.Interfaces.ReEngagement;
using HEVEQ.Application.Common.AI.Models.ReEngagement;
using HEVEQ.Application.Common.AI.Models.Search;
using HEVEQ.Application.Common.Persistence;
using HEVEQ.Application.Common.Persistence.Interfaces;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HEVEQ.Infrastructure.AI.Services.PostRejectionReengagement;

public sealed class PostRejectionAlternativesService : IAlternativeListingFinder
{
    private readonly IChatCompletionService _chat;
    private readonly IBookingReadRepository _bookingRepository;
    private readonly IServiceListingReadRepository _listingRepository;

    public PostRejectionAlternativesService(
        IChatCompletionService chat,
        IBookingReadRepository bookingRepository,
        IServiceListingReadRepository listingRepository)
    {
        _chat = chat;
        _bookingRepository = bookingRepository;
        _listingRepository = listingRepository;
    }

    public async Task<ReengagementNotificationResult> BuildReengagementAsync(
        Guid rejectedBookingId,
        int maxResults = 3,
        CancellationToken ct = default)
    {
        var booking = await _bookingRepository.GetBookingContextAsync(rejectedBookingId, ct)
            ?? throw new InvalidOperationException($"Booking {rejectedBookingId} not found or has been deleted.");

        var alternatives = await _listingRepository.FindAlternativesAsync(
            categoryId: booking.CategoryId,
            maxHourlyRate: booking.HourlyRateSnapshot * 1.20m,
            originLatitude: booking.SiteLatitude,
            originLongitude: booking.SiteLongitude,
            excludeProviderProfileId: booking.ProviderProfileId,
            maxResults: maxResults,
            ct: ct);

        if (alternatives.Count == 0)
            return new ReengagementNotificationResult(rejectedBookingId, alternatives, string.Empty);

        var listingsSummary = BuildListingsSummary(alternatives);
        var notificationMessage = await GenerateMessageAsync(booking.JobTitle, booking.Governorate, listingsSummary, ct);

        return new ReengagementNotificationResult(rejectedBookingId, alternatives, notificationMessage);
    }

    private static string BuildListingsSummary(IReadOnlyList<ServiceListingSnapshot> listings)
    {
        var sb = new StringBuilder();
        foreach (var (l, i) in listings.Select((l, i) => (l, i + 1)))
        {
            sb.AppendLine(
                $"{i}. {l.ProviderCompanyName} — {l.Title}" +
                $" | {l.HourlyRate:F0} EGP/hr" +
                $" | تقييم {l.ProviderAverageRating:F1}/5" +
                (l.DistanceKm.HasValue ? $" | {l.DistanceKm.Value:F1} km away" : ""));
        }
        return sb.ToString().TrimEnd();
    }

    private async Task<string> GenerateMessageAsync(
        string jobTitle,
        string governorate,
        string listingsSummary,
        CancellationToken ct)
    {
        var chatHistory = new ChatHistory(systemMessage: """
            You are a helpful, friendly assistant for HEVEQ , Egypt's industrial
            heavy equipment rental platform. A customer's booking was just rejected by
            a provider. Your task: write a SHORT (2-3 sentences), warm notification
            message in Egyptian Arabic that:
              1. Briefly acknowledges the rejection without dwelling on it.
              2. Presents the alternative providers by name in a natural, reassuring way.
              3. Ends with a gentle call to action (e.g. "اختار اللي يناسبك دلوقتي").
            No bullet points, no headers, no formal tone. Plain conversational Arabic.
            """);

        chatHistory.AddUserMessage(
            $"Job: {jobTitle}\n" +
            $"Governorate: {governorate}\n\n" +
            $"Available alternatives:\n{listingsSummary}");

        var response = await _chat.GetChatMessageContentAsync(
            chatHistory,
            executionSettings: new OpenAIPromptExecutionSettings { MaxTokens = 200 },
            cancellationToken: ct);

        return (response.Content ?? string.Empty).Trim();
    }
}