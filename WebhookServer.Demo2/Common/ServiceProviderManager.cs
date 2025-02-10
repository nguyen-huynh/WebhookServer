using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NReco.Logging.File;
using System;
using System.Net.Http;
using WebhookServer.Demo2.Models;
using WebhookServer.Demo2.Windows;
using Hangfire;
using Hangfire.MemoryStorage;

namespace WebhookServer.Demo2.Common
{
    internal class ServiceProviderManager
    {
        internal static IConfiguration GetConfiguration()
        {
            return new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", false, true)
                .Build();
        }

        internal static ServiceProvider GetServiceProvider()
        {
            var configuration = GetConfiguration();

            var serviceCollection = new ServiceCollection()
                .AddLogging(builder =>
                {
                    var loggingSection = configuration.GetSection("Logging");
                    builder
                        .AddConfiguration(configuration)
                        .AddFile(loggingSection, configureOpts =>
                        {
                            configureOpts.FormatLogFileName = fileName => String.Format(fileName, DateTime.UtcNow.ToString("yyyyMMdd"));
                        });
                })
                .AddSingleton<IConfiguration>(configuration)
                .AddSingleton<AppConfiguration>(service =>
                {
                    var config = service.GetService<IConfiguration>();
                    var instance = config?.GetSection(nameof(AppConfiguration))
                        .Get<AppConfiguration>() ?? new AppConfiguration();
                    return instance;
                })
                .AddSingleton<CacheManager>()
                .AddSingleton<HangfireManager>()
                .AddSingleton<WebhookRateLimitManager>()
                .AddSingleton<WebhookRepository>()
                .AddSingleton<IWindowManager, WindowManager>()
                .AddSingleton<MainWindow>()
                .AddSingleton<StatusWindow>()
                .AddSingleton<HttpClient>(services =>
                {
                    var appConfig = services.GetService<AppConfiguration>();
                    var client = new HttpClient();
                    //if (string.IsNullOrEmpty(appConfig.CommunifireSite)) throw new ArgumentException("Invalid configuration", nameof(AppConfiguration.CommunifireSite));

                    //client.BaseAddress = new Uri(appConfig.CommunifireSite);
                    //client.DefaultRequestHeaders.Accept.Clear();
                    //client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    return client;
                });

            var serviceProvider = serviceCollection.BuildServiceProvider();
            return serviceProvider;
        }
    }
}