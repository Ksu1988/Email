using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.EventLog;
using Microsoft.Extensions.Configuration;

namespace WorkerHrEmail
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseWindowsService()
                .ConfigureLogging(logging =>
                {
                    //logging.AddConsole();
                    logging.SetMinimumLevel(LogLevel.Debug);

                    IConfiguration config = new ConfigurationBuilder()
                        .AddJsonFile("appsettings.json")
                        .Build();
                    string name = config.GetSection("Name").Value;

                    logging.ClearProviders();
                    logging.AddEventLog(new EventLogSettings
                    {
                        SourceName = name,
                        Filter = (source, level) => level >= LogLevel.Debug
                    });
                })
                .ConfigureServices((hostContext, services) =>
                {
                    IConfiguration configuration = hostContext.Configuration;
                    services.AddSingleton(configuration);

                    services.AddHostedService<Worker>();
                });
    }
}
