using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebhookServer.Demo2.Common;
using WebhookServer.Demo2.Models;

namespace WebhookServer.Demo2.Windows
{
    public class StatusWindow : BaseWindow, IWindow
    {
        private readonly IWindowManager windowManager;
        private readonly ILogger<StatusWindow> logger;
        private readonly HangfireManager hangfireManager;
        private readonly CacheManager cache;
        private readonly WebhookRepository webhookRepository;
        private object _lock = new object();
        private bool isStoping = true;
        private bool isStarted = false;

        public StatusWindow(IServiceProvider serviceProvider,
                            IWindowManager windowManager,
                            ILogger<StatusWindow> logger,
                            HangfireManager hangfireManager,
                            CacheManager cache,
                            WebhookRepository webhookRepository) : base(serviceProvider)
        {
            this.windowManager = windowManager;
            this.logger = logger;
            this.hangfireManager = hangfireManager;
            this.cache = cache;
            this.webhookRepository = webhookRepository;
            this.AddEvents();
        }

        private void AddEvents()
        {
            _inputHandler = new Dictionary<ConsoleKey, Func<Task<bool>>>
            {
                { ConsoleKey.Escape, async () => {
                    windowManager.SetNextWindow<MainWindow>();
                    isStoping = true;
                    return false;
                }},
                { ConsoleKey.S, StartWebhook },
                { ConsoleKey.D1, async () => await AddRequest(1) },
                { ConsoleKey.D2, async () => await AddRequest(2) },
                { ConsoleKey.D3, async () => await AddRequest(3) },
                { ConsoleKey.D4, async () => await AddRequest(4) },
                { ConsoleKey.D5, async () => await AddRequest(5) },
                { ConsoleKey.D6, async () => await AddRequest(6) },
                { ConsoleKey.D7, async () => await AddRequest(7) },
                { ConsoleKey.D8, async () => await AddRequest(8) },
                { ConsoleKey.D9, async () => await AddRequest(9) },
                { ConsoleKey.D0, async () => await AddRequest(10) },
                { ConsoleKey.A, async() => await AddRequest(null) },
            };
        }

        public override async Task Show()
        {
            Console.Clear();
            Console.WriteLine("Select an option:");
            Console.WriteLine("S. Press 'S' to start webhook");
            Console.WriteLine("A. During job runing, press 'A' to enqueue 100 requests to random webhook" +
                "\n'1-9' to add 100 request to selected webhook");
            Console.WriteLine("\nEsc: Exit");
        }

        public override async Task<bool> Update()
        {
            lock (_lock)
            {
                if (isStoping) return true;
                Console.SetCursorPosition(0, 0);
                Console.WriteLine($"{DateTime.UtcNow.ToString("HH:mm:ss"),-20} - Next Schdule: {cache.NextSchedule?.DateTime.ToString("HH:mm:ss"),-20} - MainQueues: {cache.MainQueues.Count,5} - FailedQueues: {cache.FailedQueues.Count,5}");
                foreach (var webhook in cache.Webhooks.Values)
                {
                    Console.WriteLine(webhook.ToString());
                }
            }
            return true;
        }

        private async Task<bool> StartWebhook()
        {
            if (isStarted) return true;

            Console.Clear();
            this.hangfireManager.Configure();
            _ = this.cache.Webhooks;
            _ = this.cache.MainQueues;
            _ = this.cache.FailedQueues;
            isStoping = false;

            foreach (var webhook in this.cache.Webhooks.Values)
            {
                var initNoQueue = webhook.TotalQueue;
                for (var i = 0; i < initNoQueue; i++)
                {
                    var queue = new WebhookQueue
                    {
                        WebhookID = webhook.ID
                    };
                    cache.DatabaseQueues.Add(queue);
                }
            }

            webhookRepository.MainJobRunInBackground();
            return true;
        }

        private static Random random = new Random();
        private async Task<bool> AddRequest(int? webhookID)
        {
            webhookID = webhookID ?? (random.Next(10) + 1);
            if (cache.Webhooks.TryGetValue(webhookID.Value, out var webhook))
            {
                for (int i = 0; i < 100; i++)
                {
                    var queue = new WebhookQueue { WebhookID = webhookID.Value };
                    cache.DatabaseQueues.Add(queue);
                    webhook.TotalQueue++;
                };
                logger.LogInformation($"Added 100 requests to {webhook.Name}");
            }
            
            webhookRepository.MainJobRunInBackground();
            return true;
        }
    }
}