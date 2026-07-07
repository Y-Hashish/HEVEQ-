using Hangfire;
using HEVEQ.Api.Middleware;
using HEVEQ.Application;
using HEVEQ.Application.Common.Jobs;
using HEVEQ.Application.Common.Mappings;
using HEVEQ.Infrastructure;
using HEVEQ.Infrastructure.Persistence;
using HEVEQ.Infrastructure.AI.DependencyInjection;
using HEVEQ.Infrastructure.Identity;
using HEVEQ.Infrastructure.Services.BackgroundJobs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json.Serialization;
using DotNetEnv;
using HEVEQ.Api.Realtime.Hubs;
using HEVEQ.Api.Filters;
using HEVEQ.Api.Realtime.Services;
using HEVEQ.Application.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Linq;
using System.Reflection;

namespace HEVEQ.Api
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var envPath = Path.Combine(builder.Environment.ContentRootPath, ".env");

            if (File.Exists(envPath))
            {
                Env.Load(envPath);
            }

            builder.Configuration.AddEnvironmentVariables();

            // Application Layer Dependencies
            builder.Services.AddApplication(builder.Configuration);

            // Infrastructure Layer Dependencies
            builder.Services.AddInfrastructure(builder.Configuration);
            builder.Services.AddControllers(options =>
            {
                options.Filters.Add<ArabicErrorResponseFilter>();
            })
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(
                    new JsonStringEnumConverter(allowIntegerValues: true));
            });
            builder.Services.AddSignalR();
            builder.Services.AddSingleton<IRealtimeEventPublisher, SignalRRealtimeEventPublisher>();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AngularClient", policy =>
                {
                    policy
                        .WithOrigins(
                            "http://localhost:4200",
                            "http://127.0.0.1:4200"
                        )
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });
            builder.Services.Configure<Microsoft.AspNetCore.Mvc.ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();



            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(o =>
            {
                o.SaveToken = false;
                o.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["JWT:Issuer"],
                    ValidAudience = builder.Configuration["JWT:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"])),
                    ClockSkew = TimeSpan.Zero
                };

                o.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) &&
                            (path.StartsWithSegments("/hubs/realtime")))
                        {
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };
            });

            builder.Services.AddHangfire(config => {
                config.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection"));
            });
            builder.Services.AddHangfireServer();


            var openAiApiKey = builder.Configuration["AiSettings:OpenAi:ApiKey"];

            if (!string.IsNullOrWhiteSpace(openAiApiKey))
            {
                builder.Services.AddSearchAndReengagementAI(
                    new SearchAndReengagementOptions
                    {
                        OpenAiApiKey = openAiApiKey
                    },
                    builder.Configuration);
            }

            // Validate DI registrations for Pre-Booking orchestration before building the app.
            HEVEQ.Api.DiValidation.PreBookingDiValidator.ValidatePreBookingDi(builder.Services, builder.Configuration);

            var app = builder.Build();
            app.UseStaticFiles();
            app.UseHangfireDashboard("/hangfire");
            using (var scope = app.Services.CreateScope())
            {
                var recurringJobs = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();

                var backgroundJobOptions = scope.ServiceProvider
                    .GetRequiredService<IOptions<BackgroundJobOptions>>()
                    .Value;

                recurringJobs.AddOrUpdate<ProviderResponseSlaJob>(
                    "booking-provider-response-sla",
                    job => job.RunAsync(),
                    backgroundJobOptions.ProviderResponseSlaCron
                );

                recurringJobs.AddOrUpdate<CustomerCompletionAutoConfirmJob>(
                    "booking-customer-completion-auto-confirm",
                    job => job.RunAsync(),
                    backgroundJobOptions.CustomerCompletionAutoConfirmCron
                );

                recurringJobs.AddOrUpdate<EscrowReleaseAfterCompletionJob>(
                    "booking-escrow-release-after-completion",
                    job => job.RunAsync(),
                    backgroundJobOptions.EscrowReleaseAfterCompletionCron
                );

                recurringJobs.AddOrUpdate<MarketplaceAutoConfirmJob>(
                    "marketplace-auto-confirm",
                    job => job.RunAsync(),
                    backgroundJobOptions.MarketplaceAutoConfirmCron
                );

                recurringJobs.AddOrUpdate<MarketplaceEscrowReleaseJob>(
                    "marketplace-escrow-release",
                    job => job.RunAsync(),
                    backgroundJobOptions.MarketplaceEscrowReleaseCron
                );
            }

            using (var scope = app.Services.CreateScope())
            {
                await IdentitySeeder.SeedAsync(scope.ServiceProvider);
            }

            

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseCors("AngularClient");
            app.UseMiddleware<ExceptionHandlingMiddleware>();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapHub<RealtimeHub>("/hubs/realtime");

            app.MapControllers();

            app.Run();
        }
    }
}
