using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using ReqresIntegration.Core.Interfaces;
using ReqresIntegration.Infrastructure.ApiClients;
using ReqresIntegration.Infrastructure.Configuration;
using ReqresIntegration.Infrastructure.Interfaces;
using ReqresIntegration.Infrastructure.Services;

namespace ReqresIntegration.Infrastructure.ServiceExtension
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddReqresClient(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<ReqresApiOptions>(configuration.GetSection("ReqresApi"));

            services.AddHttpClient("ReqresClient", client =>
            {
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            })
            .AddPolicyHandler(HttpPolicyExtensions
                .HandleTransientHttpError() // Handles 5xx and network failures and timeout error
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: attempt => TimeSpan.FromSeconds(3),
                    onRetry: (outcome, timespan, retryAttempt, context) =>
                    {
                        Console.WriteLine($"⚠️ Retry {retryAttempt} after {timespan.TotalSeconds}s due to {outcome.Exception?.Message ?? outcome.Result.StatusCode.ToString()}");
                    })
            );

            services.AddScoped<IReqresApiClient, ReqresApiClient>();
            return services;
        }

        public static IServiceCollection UseExternalUserService(this IServiceCollection services)
        {
            services.AddScoped<IExternalUserService, ExternalUserService>();
            return services;
        }

        public static IServiceCollection UseCachedExternalUserService(this IServiceCollection services)
        {
            services.AddMemoryCache();
            services.AddScoped<ExternalUserService>();
            services.AddScoped<IExternalUserService>(sp =>
            {
                var inner = sp.GetRequiredService<ExternalUserService>();
                var cache = sp.GetRequiredService<IMemoryCache>();
                return new CachedExternalUserService(inner, cache);
            });
            return services;
        }

    }
}
