using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebhookServer.Demo2.Common;

namespace WebhookServer.Demo2.Windows
{
    internal class StatusWindow : BaseWindow, IWindow
    {
        private readonly IWindowManager windowManager;
        private readonly HangfireManager hangfireManager;
        private readonly CacheManager cache;
        private object _lock = new object();
        private bool isStoping = true;

        public StatusWindow(IServiceProvider serviceProvider,
                            IWindowManager windowManager,
                            HangfireManager hangfireManager,
                            CacheManager cache) : base(serviceProvider)
        {
            this.windowManager = windowManager;
            this.hangfireManager = hangfireManager;
            this.cache = cache;
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
                { ConsoleKey.A, AddRequest },
            };
        }

        public override async Task Show()
        {
            Console.Clear();
            Console.WriteLine("Select an option:");
            Console.WriteLine("S. Press 'S' to start webhook");
            Console.WriteLine("A. During job runing, press 'A' to enqueue 100 requests to all webhook");
            Console.WriteLine("\nEsc: Exit");
        }

        public override async Task<bool> Update()
        {
            lock (_lock)
            {
                if (isStoping) return true;
                Console.SetCursorPosition(0, 0);
                foreach (var webhook in cache.Webhooks.Values)
                {
                    Console.WriteLine(webhook.ToString());
                }
            }
            return true;
        }

        private async Task<bool> StartWebhook()
        {
            Console.Clear();
            this.hangfireManager.Configure();
            _ = this.cache.Webhooks;
            _ = this.cache.WebhookQueues;
            isStoping = false;


            return true;
        }

        private async Task<bool> AddRequest()
        {

            return true;
        }
    }
}