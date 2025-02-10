using Hangfire;
using Hangfire.Common;
using Hangfire.MemoryStorage;
using Hangfire.SqlServer;
using Microsoft.Extensions.Logging;
using Microsoft.Owin.Hosting;
using System;
using System.Data.SqlClient;
using System.Runtime.InteropServices;
using System.Threading;
using WebhookServer.Demo2.Helpers;
using WebhookServer.Demo2.Models;
using WebhookServer.Demo2.Servers;

namespace WebhookServer.Demo2.Common
{
    public class HangfireManager : IDisposable
    {
        private readonly ILogger<HangfireManager> _logger;
        private readonly CacheManager _cache;
        private BackgroundJobServer _server;
        private BackgroundJobServer _webhookServer;
        private IDisposable _webApp;
        private bool _isConfigured = false;

        public HangfireManager(ILogger<HangfireManager> logger, CacheManager cache)
        {
            this._logger = logger;
            this._cache = cache;
        }

        public void Configure()
        {
            if (_isConfigured) return;

            _isConfigured = true;
            _logger.LogInformation("Configuring Hangfire Manager");

            GlobalConfiguration.Configuration.UseMemoryStorage();
            GlobalJobFilters.Filters.Add(new PreventDuplicateRecurringJobFilter());

            _logger.LogInformation("Init Owin Webserver");
            _webApp = WebApp.Start<OwinStartup>("http://localhost:5000");
            Console.WriteLine("Hangfire Dashboard run at: http://localhost:5000/hangfire");

            var defaultServerOptions = new BackgroundJobServerOptions
            {
                WorkerCount = 1,
            };

            // Start Hangfire Server
            _server = new BackgroundJobServer(defaultServerOptions);
            _webhookServer = new BackgroundJobServer(new BackgroundJobServerOptions
            {
                WorkerCount = 3,
                Queues = new[] { "daily-webhook", "hour-webhook", "minute-webhook" },
            });
            _logger.LogInformation("Hangfire Server started...");

            BackgroundJob.Enqueue(() => AnotherJob1()); 
        }

        public static void AnotherJob1()
        {
            Thread.Sleep(3600000);
        }

        public void Dispose()
        {
            try
            {
                _logger.LogInformation("Stopping Hangfire...");
                _webApp?.Dispose();   // Stop OWIN self-host
                _server?.Dispose();   // Stop server
                _webhookServer?.Dispose();
                _logger.LogInformation("Hangfire Stoped.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }
    }
}