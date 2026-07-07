using HEVEQ.Application.Common.AI.Interfaces;
using HEVEQ.Application.Common.Interfaces;
using HEVEQ.Application.Common.Persistence.Interfaces;
using HEVEQ.Application.Common.Jobs;
using HEVEQ.Application.Common.Options;
using HEVEQ.Application.Common.Payments;
using HEVEQ.Domain.Identity;
using HEVEQ.Infrastructure.Payments.Stripe;
using HEVEQ.Infrastructure.Persistence;
using HEVEQ.Infrastructure.Persistence.Qdrant;
using HEVEQ.Infrastructure.Persistence.Repositories;
using HEVEQ.Infrastructure.Services;
using HEVEQ.Infrastructure.Services.AI;
using HEVEQ.Infrastructure.Services.AI.BackgroundJobs;
using HEVEQ.Infrastructure.Services.AI.Helpers;
using HEVEQ.Infrastructure.Services.AI.ModerationAgent;
using HEVEQ.Infrastructure.Services.BackgroundJobs;
using HEVEQ.Infrastructure.Services.Email;
using HEVEQ.Infrastructure.Services.Storage;
using HEVEQ.Infrastructure.Services.AI.ComplainSummarizing;
using HEVEQ.Infrastructure.Services.AI.ReviewFiltering;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;

namespace HEVEQ.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sqlOptions => sqlOptions.UseNetTopologySuite());
        });

        services.AddIdentityCore<ApplicationUser>(options =>
        {
            options.User.RequireUniqueEmail = true;

            options.Password.RequiredLength = 6;
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;
        })
        .AddRoles<IdentityRole<Guid>>()
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<ApplicationDbContext>());

        services.Configure<JwtHelper>(configuration.GetSection("JWT"));
        services.AddScoped<IJwtService, JwtService>();

        services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
        services.AddScoped<IEmailService, EmailService>();

        services.Configure<CloudinarySettings>(configuration.GetSection("Cloudinary"));
        services.AddScoped<IImageStorageService, ImageStorageService>();

        services.Configure<StripePaymentOptions>(configuration.GetSection("StripePayment"));
        services.Configure<PaymentPlatformOptions>(configuration.GetSection("PaymentPlatform"));
        services.AddScoped<IPaymentCheckoutService, StripePaymentCheckoutService>();

        services.Configure<BackgroundJobOptions>(configuration.GetSection("BackgroundJobs"));

        services.AddScoped<ProviderResponseSlaJob>();
        services.AddScoped<CustomerCompletionAutoConfirmJob>();
        services.AddScoped<EscrowReleaseAfterCompletionJob>();
        services.AddScoped<MarketplaceAutoConfirmJob>();
        services.AddScoped<MarketplaceEscrowReleaseJob>();

        services.AddScoped<IBackgroundJobService, HangfireBackgroundJobService>();

        services.AddHttpClient();

        var openAiApiKey = configuration["AiSettings:OpenAi:ApiKey"];
        var openAiModel = configuration["AiSettings:OpenAi:Model"] ?? "gpt-5-mini";

        if (!string.IsNullOrWhiteSpace(openAiApiKey))
        {
            services.AddOpenAIChatCompletion(
                modelId: openAiModel,
                apiKey: openAiApiKey);

            services.AddKernel();

            services.AddScoped<IReviewFilterService, SemanticKernelReviewFilterService>();
            services.AddScoped<IComplaintAnalysisService, SemanticKernelComplaintAnalysisService>();
            services.AddScoped<IDocumentVisionExtractor, GptVisionDocumentExtractor>();

            services.AddScoped<MarketplaceModerationJob>();
            services.AddScoped<ServiceListingModerationJob>();
            services.AddScoped<DocumentOcrJob>();

            services.AddScoped<ServiceListingRiskAnalyzer>();
            services.AddScoped<MarketplaceListingRiskAnalyzer>();
        }

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        services.AddScoped<IServiceListingReadRepository, ServiceListingReadRepository>();
        services.AddScoped<IBookingReadRepository, BookingReadRepository>();
        services.AddScoped<ISearchQueryLogWriter, SearchQueryLogWriter>();
        services.AddScoped<INotificationWriter, NotificationWriter>();

        // Pre-Booking Multi-Agent orchestration (Concierge + Moderator)
        // Register orchestrator, evaluators and their plugins. Scoped to share DbContext per request.
        services.AddScoped<HEVEQ.Application.Common.AI.Interfaces.PreBooking.IPreBookingOrchestrator, HEVEQ.Infrastructure.AI.Services.PreBookingOrchestrationService>();
        services.AddScoped<HEVEQ.Application.Common.AI.Interfaces.PreBooking.IPreBookingFeasibilityEvaluator, HEVEQ.Infrastructure.AI.Agents.ConciergeFeasibilityEvaluator>();
        services.AddScoped<HEVEQ.Application.Common.AI.Interfaces.PreBooking.IPreBookingTrustEvaluator, HEVEQ.Infrastructure.AI.Agents.ModeratorTrustEvaluator>();

        // Plugins used by agents - they depend on IApplicationDbContext (scoped)
        services.AddScoped<HEVEQ.Infrastructure.AI.Plugins.ConciergeGeoAvailabilityPlugin>();
        services.AddScoped<HEVEQ.Infrastructure.AI.Plugins.ModeratorTrustIncidentPlugin>();

        services.AddSingleton<IQdrantListingIndex, QdrantListingIndex>();

        return services;
    }
}