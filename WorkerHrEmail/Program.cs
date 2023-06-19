using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Core;
using WorkerHrEmail.Services;

namespace WorkerHrEmail
{
    public class Program
    {
        private static Logger _serilogLogger;

        private static IConfiguration Configuration { get; } = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
            .Build();

        public static void Main(string[] args)
        {
            _serilogLogger = new LoggerConfiguration()
                .Enrich
                .FromLogContext()
                .ReadFrom
                .Configuration(Configuration)
                .CreateLogger();
            try
            {
                _serilogLogger.Information("Starting HR email service");
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                _serilogLogger.Fatal(ex, "HR email service terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseWindowsService()
                .ConfigureLogging(logger => logger.AddSerilog(_serilogLogger))
                .UseSerilog()
                .ConfigureServices((hostContext, services) =>
                {
                    IConfiguration configuration = hostContext.Configuration;
                    services.AddSingleton<EmailService>();
                    services.AddHostedService<Worker>();
                });
    }
}
