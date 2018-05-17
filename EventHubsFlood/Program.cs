using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.EventHubs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace EventHubsFlood
{
    class Program
    {
        public static IConfiguration Configuration { get; private set; }
        public static ILoggerFactory LoggerFactory { get; private set; }

        static void Main(string[] args)
        {
            Configuration = BuildConfiguration();
            LoggerFactory = BuildLogger();

            var cts = new CancellationTokenSource();

            Console.CancelKeyPress += (s, e) => { e.Cancel = true; cts.Cancel(); };

            MainAsync(cts.Token).GetAwaiter().GetResult();
        }

        private static ILoggerFactory BuildLogger()
        {
            return new LoggerFactory().AddConsole(Configuration.GetSection("Logging"));
        }

        private static IConfiguration BuildConfiguration()
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(AppContext.BaseDirectory))
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            return configBuilder.Build();
        }

        private static async Task MainAsync(CancellationToken token)
        {
            var logger = LoggerFactory.CreateLogger("Program");
            logger.LogInformation("Opening the flood gates...");

            var sender = EventHubClient.CreateFromConnectionString(Configuration.GetConnectionString("EventHub"));

            var count = 0;
            var random = new Random();
            while (!token.IsCancellationRequested) {
                var events = new List<EventData>();
                for (var i = 0; i < random.Next(1, 100); i++) {
                    count++;
                    var data = Encoding.UTF8.GetBytes(count.ToString());
                    events.Add(new EventData(data));
                }
                var partitionKey = (count % 100).ToString();
                await sender.SendAsync(events, partitionKey);
                logger.LogDebug($"Sent message {count}");
            }

            logger.LogInformation("Flood gates closed.");
        }
    }
}
