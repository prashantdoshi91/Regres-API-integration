using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ReqresIntegration.Core.Interfaces;
using ReqresIntegration.Infrastructure.ServiceExtension;

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

                        services.AddReqresClient(config);

                        // use this for without caching
                        //services.UseExternalUserService();

                        // Optional: Add caching version
                        services.UseCachedExternalUserService();
                    })
                    .ConfigureLogging(logging =>
                    {
                        logging.ClearProviders();
                        logging.AddConsole();
                    });
    }

}