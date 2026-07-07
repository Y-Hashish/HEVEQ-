using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using HEVEQ.Application.Common.AI.Interfaces.PreBooking;
using HEVEQ.Infrastructure.AI.Agents;
using HEVEQ.Infrastructure.AI.Plugins;
using HEVEQ.Application.Common.Persistence.Interfaces;
using Microsoft.Extensions.Configuration;

namespace HEVEQ.Api.DiValidation;

public static class PreBookingDiValidator
{
    public static void ValidatePreBookingDi(IServiceCollection services, IConfiguration configuration)
    {
        // Quick static checks for presence/duplicates
        var required = new Type[]
 {
    typeof(IPreBookingOrchestrator),
    typeof(IPreBookingFeasibilityEvaluator),
    typeof(IPreBookingTrustEvaluator),
    typeof(ConciergeGeoAvailabilityPlugin),
    typeof(ModeratorTrustIncidentPlugin),
    typeof(IBookingReadRepository),
    typeof(Kernel),
    typeof(IChatCompletionService)
 };

        foreach (var t in required)
        {
            if (!services.Any(sd => sd.ServiceType == t))
            {
                throw new InvalidOperationException($"Missing DI registration for {t.FullName}");
            }
        }

        //var kernelCount = services.Count(sd => sd.ServiceType == typeof(Kernel));
        //if (kernelCount > 1)
        //    throw new InvalidOperationException($"Multiple Kernel registrations detected: {kernelCount}");

        //var chatCount = services.Count(sd => sd.ServiceType == typeof(IChatCompletionService));
        //if (chatCount > 1)
        //    throw new InvalidOperationException($"Multiple IChatCompletionService registrations detected: {chatCount}");

        // Check that no Scoped service is registered as implementation type for a Singleton
        var singletons = services.Where(sd => sd.Lifetime == ServiceLifetime.Singleton).ToList();
        foreach (var singleton in singletons)
        {
            var implType = singleton.ImplementationType;
            if (implType == null) continue;
            var ctor = implType.GetConstructors().OrderByDescending(c => c.GetParameters().Length).FirstOrDefault();
            if (ctor == null) continue;
            foreach (var p in ctor.GetParameters())
            {
                var paramType = p.ParameterType;
                var reg = services.FirstOrDefault(sd => sd.ServiceType == paramType);
                if (reg != null && reg.Lifetime == ServiceLifetime.Scoped)
                {
                    throw new InvalidOperationException($"Singleton {implType.FullName} depends on scoped service {paramType.FullName}");
                }
            }
        }

        // Attempt to build a provider and resolve required services to catch resolution-time issues.
        // BuildProvider with ValidateScopes = true so resolving scoped services outside of scope will throw.
        using var sp = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });

        try
        {
            // Resolve Kernel and chat service from root provider (they are singletons)
            var kernel = sp.GetService<Kernel>() ?? throw new InvalidOperationException("Kernel resolution failed");
            var chat = sp.GetService<IChatCompletionService>() ?? throw new InvalidOperationException("IChatCompletionService resolution failed");

            // Resolve scoped services inside a scope
            using var scope = sp.CreateScope();
            var scopedProvider = scope.ServiceProvider;

            scopedProvider.GetRequiredService<IPreBookingOrchestrator>();
            scopedProvider.GetRequiredService<IPreBookingFeasibilityEvaluator>();
            scopedProvider.GetRequiredService<IPreBookingTrustEvaluator>();
            scopedProvider.GetRequiredService<ConciergeGeoAvailabilityPlugin>();
            scopedProvider.GetRequiredService<ModeratorTrustIncidentPlugin>();
            scopedProvider.GetRequiredService<IBookingReadRepository>();

        }
        catch (Exception ex)
        {
            // Bubble a clear message during startup
            throw new InvalidOperationException("Pre-Booking DI validation failed: " + ex.Message, ex);
        }
    }
}
