using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;
using ReqresIntegration.Core.Interfaces;
using ReqresIntegration.Infrastructure.ApiClients;
using ReqresIntegration.Infrastructure.Configuration;
using ReqresIntegration.Infrastructure.Interfaces;
using ReqresIntegration.Infrastructure.Services;

namespace ReqresIntegration.ConsoleDemo
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            using IHost host = CreateHostBuilder(args).Build();

            var userService = host.Services.GetRequiredService<IExternalUserService>();

            Console.WriteLine("Fetching all users from reqres.in...");

            var users = await userService.GetAllUsersAsync();

            foreach (var user in users)
            {
                Console.WriteLine($"ID: {user.Id}, Name: {user.First_Name} {user.Last_Name}, Email: {user.Email}");
            }

            // calling again within 5 mins then it will return the data from cache.
            var users1 = await userService.GetAllUsersAsync();

            Console.WriteLine("Done.");
        }

        static IHostBuilder CreateHostBuilder(string[] args) =>
                Host.CreateDefaultBuilder(args)
                    .ConfigureAppConfiguration((context, config) =>
                    {
                        config.SetBasePath(Directory.GetCurrentDirectory());
                        config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                    })
                    .ConfigureServices((context, services) =>
                    {
                        var config = context.Configuration;

                        services.Configure<ReqresApiOptions>(config.GetSection("ReqresApi"));

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
                        //services.AddScoped<IExternalUserService, ExternalUserService>();

                        // Optional: Add caching version
                        services.AddMemoryCache();
                        services.AddScoped<ExternalUserService>();
                        services.AddScoped<IExternalUserService>(sp =>
                        {
                            var inner = sp.GetRequiredService<ExternalUserService>();
                            var cache = sp.GetRequiredService<IMemoryCache>();
                            return new CachedExternalUserService(inner, cache);
                        });
                    })
                    .ConfigureLogging(logging =>
                    {
                        logging.ClearProviders();
                        logging.AddConsole();
                    });
    }

}