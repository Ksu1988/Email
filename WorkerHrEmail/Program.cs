using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Core;
using WorkerHrEmail.Services;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace WorkerHrEmail
{
    public class Program
    {
        private static Logger _serilogLogger;

        private static IConfiguration Configuration { get; } = new ConfigurationBuilder()
            .SetBasePath(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location))
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production"}.json", optional: true)
            .Build();

        public static void Main(string[] args)
        {
            // CreateHostBuilder(args).Build().Run();
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .ReadFrom.Configuration(Configuration)
                .CreateLogger();

            //_serilogLogger.Information("Starting HR email service");
            //_serilogLogger.Information(Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT"));
            //CreateHostBuilder(args).Build().Run();

            try
            {
                Log.Logger.Information("Starting HR email service");
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                Log.Logger.Fatal(ex, "HR email service terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseWindowsService()
                //.ConfigureLogging(logger =>
                //{
                //    var _serilogLogger = new LoggerConfiguration()
                //        .ReadFrom.Configuration(Configuration)
                //        .CreateLogger();

                //    logger.AddSerilog(_serilogLogger);
                //})
                // .ConfigureLogging(lb => { lb.AddSerilog(_serilogLogger); })
                .ConfigureLogging(logger => logger.AddSerilog(_serilogLogger))
                .UseSerilog()
                .ConfigureServices((hostContext, services) =>
                {
                    IConfiguration configuration = hostContext.Configuration;
                    services.AddSingleton(configuration);
                    //services.AddLogging(lb =>
                    //{
                    //    var _serilogLogger = new LoggerConfiguration()
                    //    .Enrich.FromLogContext()
                    //    .ReadFrom.Configuration(configuration)
                    //    .CreateLogger();

                    //    lb.AddSerilog(_serilogLogger, dispose: true);

                    //});
                    //services.AddSingleton<ILogger>(x =>
                    //{
                    //    return new LoggerConfiguration()
                    //    .Enrich.FromLogContext()
                    //    .ReadFrom.Configuration(Configuration)
                    //    .CreateLogger();
                    //});
                    services.AddSingleton<EmailService>();
                    services.AddHostedService<Worker>();
                });
    }
}
