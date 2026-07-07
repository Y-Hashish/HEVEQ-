using System.Text.Json;
using System.Text.Json.Serialization;
using HEVEQ.Application.Common.AI.Interfaces.ReEngagement;
using HEVEQ.Application.Common.Persistence.Interfaces;

namespace HEVEQ.Application.Features.Bookings.Services.Implementation;

// ─── Notification body DTO serialised into Notifications.Body ─────────────────

public sealed class ReengagementNotificationBody
{
    [JsonPropertyName("message")]
    public string Message { get; init; } = string.Empty;

    [JsonPropertyName("alternatives")]
    public List<AlternativeListingDto> Alternatives { get; init; } = [];
}

public sealed class AlternativeListingDto
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; init; } = string.Empty;

    [JsonPropertyName("providerCompanyName")]
    public string ProviderCompanyName { get; init; } = string.Empty;

    [JsonPropertyName("hourlyRate")]
    public decimal HourlyRate { get; init; }

    [JsonPropertyName("providerAverageRating")]
    public decimal ProviderAverageRating { get; init; }

    [JsonPropertyName("distanceKm")]
    public double? DistanceKm { get; init; }
}

// ─── Executor ─────────────────────────────────────────────────────────────────

public sealed class ReengagementJobExecutor
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly IAlternativeListingFinder _alternativeFinder;
    private readonly IBookingReadRepository _bookingRepository;
    private readonly INotificationWriter _notificationWriter;

    public ReengagementJobExecutor(
        IAlternativeListingFinder alternativeFinder,
        IBookingReadRepository bookingRepository,
        INotificationWriter notificationWriter)
    {
        _alternativeFinder = alternativeFinder;
        _bookingRepository = bookingRepository;
        _notificationWriter = notificationWriter;
    }

    public async Task ExecuteAsync(Guid bookingId, CancellationToken ct)
    {
        var booking = await _bookingRepository.GetBookingContextAsync(bookingId, ct);
        if (booking is null) return;

        var result = await _alternativeFinder.BuildReengagementAsync(bookingId, maxResults: 3, ct);

        if (result.SuggestedAlternatives.Count == 0)
        {
            // No alternatives — store plain text body
            var noAltBody = JsonSerializer.Serialize(new ReengagementNotificationBody
            {
                Message = "المزوّد مش قادر ياخد الشغلة دي دلوقتي. " +
                               "بنبحّث عن بدائل قريبة وهنعلّمك أول ما نلاقي.",
                Alternatives = []
            }, JsonOpts);

            await _notificationWriter.SendAsync(
                booking.CustomerId,
                "BookingRejectedNoAlternatives",
                "طلبك اتأجّل",
                noAltBody,
                bookingId.ToString(), "Booking", ct);
            return;
        }

        // Map ServiceListingSnapshot → AlternativeListingDto
        var alternativeDtos = result.SuggestedAlternatives
            .Select(l => new AlternativeListingDto
            {
                Id = l.Id.ToString(),
                Title = l.Title,
                ProviderCompanyName = l.ProviderCompanyName,
                HourlyRate = l.HourlyRate,
                ProviderAverageRating = l.ProviderAverageRating,
                DistanceKm = l.DistanceKm
            })
            .ToList();

        var body = JsonSerializer.Serialize(new ReengagementNotificationBody
        {
            Message = result.NotificationMessage,
            Alternatives = alternativeDtos
        }, JsonOpts);

        await _notificationWriter.SendAsync(
            booking.CustomerId,
            "BookingRejectedWithAlternatives",
            "لقينالك بدائل قريبة! 🚜",
            body,
            bookingId.ToString(), "Booking", ct);
    }
}