using HEVEQ.Application.Common.AI;
using HEVEQ.Application.Common.AI.Interfaces.ReEngagement;
using HEVEQ.Application.Common.AI.Interfaces.Search;
using HEVEQ.Application.Common.Persistence.Interfaces;
using HEVEQ.Application.Features.Bookings.Services.Implementation;
using HEVEQ.Infrastructure.AI.Plugins;
using HEVEQ.Infrastructure.AI.Services;
using HEVEQ.Infrastructure.AI.Services.PostRejectionReengagement;
using HEVEQ.Infrastructure.AI.Services.Search;
using HEVEQ.Infrastructure.Persistence.Qdrant;
using HEVEQ.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Embeddings;
using Qdrant.Client;

namespace HEVEQ.Infrastructure.AI.DependencyInjection;

public static class SearchAndReengagementServiceExtensions
{
    public static IServiceCollection AddSearchAndReengagementAI(
        this IServiceCollection services,
        SearchAndReengagementOptions options,
        IConfiguration configuration)
    {
        // ══════════════════════════════════════════════════════════════════════
        // INFRASTRUCTURE — Qdrant
        // ══════════════════════════════════════════════════════════════════════

        services.AddSingleton(_ =>
        {
            var host = configuration["Qdrant:Host"] ?? "localhost";
            var port = int.Parse(configuration["Qdrant:Port"] ?? "6334");
            var apiKey = configuration["Qdrant:ApiKey"];
            return string.IsNullOrWhiteSpace(apiKey)
                ? new QdrantClient(host, port)
                : new QdrantClient(host, port, apiKey: apiKey);
        });

        // Singleton — QdrantClient is thread-safe; QdrantListingIndex is stateless
        services.AddSingleton<IQdrantListingIndex, QdrantListingIndex>();

        // ══════════════════════════════════════════════════════════════════════
        // INFRASTRUCTURE — Semantic Kernel + OpenAI
        // ══════════════════════════════════════════════════════════════════════

        services.AddSingleton(_ =>
            Kernel.CreateBuilder()
                .AddOpenAIChatCompletion(
                    modelId: "gpt-4o",
                    apiKey: options.OpenAiApiKey)
                .AddOpenAITextEmbeddingGeneration(
                    modelId: "text-embedding-3-small",
                    apiKey: options.OpenAiApiKey)
                .Build());

        services.AddSingleton(sp =>
            sp.GetRequiredService<Kernel>()
              .GetRequiredService<Microsoft.SemanticKernel.ChatCompletion.IChatCompletionService>());

        services.AddSingleton(sp =>
            sp.GetRequiredService<Kernel>()
              .GetRequiredService<Microsoft.SemanticKernel.Embeddings.ITextEmbeddingGenerationService>());

        // SK plugin — singleton (stateless, thread-safe)
        services.AddSingleton<SearchServicesPlugin>();

        // ══════════════════════════════════════════════════════════════════════
        // TASK 1 — Semantic Search + RAG Pipeline
        // ══════════════════════════════════════════════════════════════════════

        // Geofence validator is a pure static-list lookup — singleton is safe
        services.AddSingleton<IGeofenceValidator, EgyptianGeofenceValidator>();

        // All three search-pipeline adapters are Scoped (share the request's DI scope)
        services.AddScoped<ISearchIntentExtractor, SemanticKernelSearchIntentExtractor>();
        services.AddScoped<IServiceVectorSearchService, QdrantServiceVectorSearchService>();
        services.AddScoped<IClarificationService, SemanticKernelClarificationService>();

        // Repository — Scoped (shares EF Core DbContext with the rest of the request)
        services.AddScoped<IServiceListingReadRepository, ServiceListingReadRepository>();

        // Analytics log writer — Scoped; failures are swallowed inside the impl
        services.AddScoped<ISearchQueryLogWriter, SearchQueryLogWriter>();

        // Reindex service — Scoped (needs DbContext for EmbeddingStatus write-back)
        services.AddScoped<QdrantSyncService>();

        // ══════════════════════════════════════════════════════════════════════
        // TASK 3 — Post-Rejection Re-Engagement Pipeline
        // ══════════════════════════════════════════════════════════════════════

        // Booking read repo — Scoped
        services.AddScoped<IBookingReadRepository, BookingReadRepository>();

        // Notification persistence — Scoped (writes to Notifications table)
        services.AddScoped<INotificationWriter, NotificationWriter>();

        // Core re-engagement service — Scoped
        services.AddScoped<IAlternativeListingFinder, PostRejectionAlternativesService>();

        // Re-engagement background scheduler:
        //   - ReengagementJobChannel   → Singleton (owns the Channel<Guid>)
        //   - IReengagementJobScheduler → Singleton (writes to the channel)
        //   - ReengagementBackgroundWorker → IHostedService (reads channel, fires jobs)
        //   - ReengagementJobExecutor  → Scoped (resolved per-job from a fresh scope)
        services.AddSingleton<ReengagementJobChannel>();
        services.AddSingleton<IReengagementJobScheduler, ChannelReengagementScheduler>();
        services.AddHostedService<ReengagementBackgroundWorker>();
        services.AddScoped<ReengagementJobExecutor>();

        return services;
    }
}