using Hangfire;
using Hangfire.Common;
using Hangfire.MemoryStorage;
using Hangfire.SqlServer;
using Microsoft.Owin.Hosting;
using System;
using System.Data.SqlClient;
using System.Runtime.InteropServices;
using System.Threading;
using WebhookServer.Demo.Models;
using WebhookServer.Demo.Servers;

namespace WebhookServer.Demo.Helpers
{
    public static class HangfireManager
    {
        private static BackgroundJobServer _server;
        private static BackgroundJobServer _WebhookServer;
        private static IDisposable _webApp;

        public static void Configure()
        {
            //var connectionString = "Server=.;Database=webhook.cf.com;Trusted_Connection=True;";
            //try
            //{
            //    using (var connection = new SqlConnection(connectionString))
            //    {
            //        connection.Open();
            //        using (var command = new SqlCommand(@"
            //DELETE FROM [HangFire].[Job];
            //DELETE FROM [HangFire].[JobQueue];
            //DELETE FROM [HangFire].[State];
            //DELETE FROM [HangFire].[Server];
            //DELETE FROM [HangFire].[List];
            //DELETE FROM [HangFire].[Set];
            //DELETE FROM [HangFire].[Hash];
            //DELETE FROM [HangFire].[Counter];", connection))
            //        {
            //            command.ExecuteNonQuery();
            //        }
            //    }
            //}
            //catch { }

            //GlobalConfiguration.Configuration
            //    .UseSqlServerStorage(connectionString);
            GlobalConfiguration.Configuration
            .UseMemoryStorage();
            GlobalJobFilters.Filters.Add(new PreventDuplicateRecurringJobFilter());

            _webApp = WebApp.Start<OwinStartup>("http://localhost:5000");
            Console.WriteLine("Hangfire Dashboard run at: http://localhost:5000/hangfire");

            var defaultServerOptions = new BackgroundJobServerOptions
            {
                WorkerCount = 1,
            };

            // Start Hangfire Server
            _server = new BackgroundJobServer(defaultServerOptions);
            _WebhookServer = new BackgroundJobServer(new BackgroundJobServerOptions
            {
                WorkerCount = 3,
                // prevent duplicate job created from recurring job
                // seems not work yet
                //FilterProvider = new JobFilterCollection { new PreventDuplicateRecurringJobFilter() },  
                Queues = new[] { "daily-webhook", "hour-webhook" , "minute-webhook" },  // create queues with priority, daily has higher priority than others
            });
            Console.WriteLine("Hangfire Server started...");

            BackgroundJob.Enqueue(() => AnotherJob1()); // Long job like Rebuild index take worker from main hangfire server
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


        public static void AddWebhookRecurringJobs()
        {
            foreach (var webhook in CacheManager.Webhooks.Values)
            {
                if (webhook.JobQueue == "daily-webhook")
                {
                    RecurringJob.AddOrUpdate(
                    webhook.Name,
                    () => new WebhookJob().LongRunAsync(webhook.Name, webhook.ID),
                    "*/5 * * * * *"    // for test only, 
                    );
                }
                else
                {
                    RecurringJob.AddOrUpdate(
                    webhook.Name,
                    () => new WebhookJob().ShortRunAsync(webhook.Name, webhook.ID),
                    "*/5 * * * * *"  // for test only, 
                    );
                }

                
                RecurringJob.TriggerJob(webhook.Name);
            }
        }

        public static void Stop()
        {
            Console.WriteLine("Stopping Hangfire...");
            _webApp?.Dispose();   // Stop OWIN self-host
            _server?.Dispose();   // Stop server
            _WebhookServer?.Dispose();
            Console.WriteLine("Hangfire Stoped.");
        }

    }

}
