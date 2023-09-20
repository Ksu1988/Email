using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Serilog;
using WorkerHrEmail.Services;
using System.Reflection;

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
                .ConfigureAppConfiguration((ctx, builder) =>
                {
                    builder.SetBasePath(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location))
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production"}.json", optional: true, reloadOnChange: true)
                    .Build();
                })
                .UseWindowsService()
                .UseSerilog((ctx, builder) =>
                {
                    builder.Enrich.FromLogContext()
                    .ReadFrom.Configuration(ctx.Configuration);
                })
                .ConfigureServices((ctx, services) =>
                {
                    IConfiguration configuration = ctx.Configuration;
                    services.AddSingleton(configuration);
                    services.AddSingleton<EmailService>();
                    services.AddHostedService<Worker>();
                });
    }
}
