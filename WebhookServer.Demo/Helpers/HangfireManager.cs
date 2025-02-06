using Hangfire;
using Hangfire.MemoryStorage;
using Hangfire.SqlServer;
using Microsoft.Owin.Hosting;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using WebhookServer.Demo.Models;
using WebhookServer.Demo.Servers;

namespace WebhookServer.Demo.Helpers
{
    public static class HangfireManager
    {
        private static BackgroundJobServer _server;
        private static IDisposable _webApp;

        public static void Configure()
        {
            //GlobalConfiguration.Configuration
            //    .UseSqlServerStorage("Server=.;Database=webhook.cf.com;Trusted_Connection=True;");
            GlobalConfiguration.Configuration
            .UseMemoryStorage();

            _webApp = WebApp.Start<OwinStartup>("http://localhost:5000");
            Console.WriteLine("Hangfire Dashboard đang chạy tại: http://localhost:5000/hangfire");

            var serverOptions = new BackgroundJobServerOptions
            {
                WorkerCount = 3,
            };

            // Khởi động Hangfire Server
            _server = new BackgroundJobServer(serverOptions);
            Console.WriteLine("Hangfire Server started...");

            BackgroundJob.Enqueue(() => AnotherJob1());
            //BackgroundJob.Enqueue(() => AnotherJob2());

        }

        public static void AnotherJob1()
        {
            Thread.Sleep(3600000);
        }
        public static void AnotherJob2()
        {
            Thread.Sleep(3600000);
        }

        
        public static void EnqueueTestJob()
        {
            BackgroundJob.Enqueue(() => Console.WriteLine($"Job chạy lúc: {DateTime.Now}"));
        }

        public static void AddWebhookRecurringJobs()
        {
            foreach (var webhook in CacheManager.Webhooks.Values)
            {
                RecurringJob.AddOrUpdate(
                    webhook.Name, 
                    () => new WebhookJob().RunAsync(webhook.Name, webhook.ID),
                    "*/30 * * * * *"
                    );
                RecurringJob.TriggerJob(webhook.Name);
            }
        }

        public static void Stop()
        {
            Console.WriteLine("Dừng Hangfire...");
            _webApp?.Dispose();   // Dừng OWIN self-host
            _server?.Dispose();   // Dừng server
            Console.WriteLine("Hangfire đã được dừng.");
        }

    }

}
