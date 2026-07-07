//using HEVEQ.Application.Common.AI.Interfaces;
//using HEVEQ.Application.Common.AI.Interfaces.PreBooking;
//using HEVEQ.Application.Common.Persistence.Interfaces;
//using HEVEQ.Infrastructure.AI.Agents;
//using HEVEQ.Infrastructure.AI.Plugins;
//using HEVEQ.Infrastructure.AI.Services;
//using HEVEQ.Infrastructure.Persistence.Repositories;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.SemanticKernel;

//namespace HEVEQ.Infrastructure.AI.DependencyInjection;

//public static class PreBookingServiceCollectionExtensions
//{
//    public static IServiceCollection AddPreBookingAgents(this IServiceCollection services, IConfiguration configuration)
//    {
//        var modelId = configuration["PreBookingAgents:OpenAi:Model"] ?? "gpt-5-mini";
//        var apiKey = configuration["PreBookingAgents:OpenAi:ApiKey"]
//            ?? throw new InvalidOperationException("PreBookingAgents:OpenAi:ApiKey is not configured.");

      
//        services.AddScoped(_ =>
//        {
//            var builder = Kernel.CreateBuilder();
//            builder.AddOpenAIChatCompletion(modelId: modelId, apiKey: apiKey);
//            return builder.Build();
//        });

//        services.AddScoped<ConciergeGeoAvailabilityPlugin>();
//        services.AddScoped<ModeratorTrustIncidentPlugin>();

//        services.AddScoped<IPreBookingFeasibilityEvaluator, ConciergeFeasibilityEvaluator>();
//        services.AddScoped<IPreBookingTrustEvaluator, ModeratorTrustEvaluator>();

//        services.AddScoped<IBookingReadRepository, BookingReadRepository>();
//        services.AddScoped<IPreBookingOrchestrator, PreBookingOrchestrationService>();

//        return services;
//    }
//}