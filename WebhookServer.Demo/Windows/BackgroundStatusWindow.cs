using System;
using System.Timers;
using System.Linq;
using WebhookServer.Demo.Helpers;

namespace WebhookServer.Demo.Windows
{


    internal class BackgroundStatusWindow
    {
        private static Timer clockTimer;
        private static Timer statusTimer;

        internal static void Init()
        {
            HangfireManager.Configure();
        }
        public static void Show()
        {
            Console.Clear();
            _ = CacheManager.Webhooks;
            _ = CacheManager.WebhookQueues;
            CacheManager.EnqueueWebhookItems();
            InitClock();
            InitStatusUpdater();

            //Console.WriteLine("Nhấn Esc để quay về MainWindow");

            HangfireManager.AddWebhookRecurringJobs();
            while (true)
            {
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true).Key;
                    if (key == ConsoleKey.Escape)
                    {
                        Stop();
                        return;
                    }
                }
            }
        }

        private static void InitClock()
        {
            //clockTimer = new Timer(1000); // 1 giây
            //clockTimer.Elapsed += UpdateClock;
            //clockTimer.Start();
        }

        
        private static void InitStatusUpdater()
        {
            statusTimer = new Timer(500); // Cập nhật mỗi 0.5s
            statusTimer.Elapsed += UpdateStatus;
            statusTimer.Start();
        }

        private static object _lock = new object();
        private static void UpdateClock(object sender, ElapsedEventArgs e)
        {
            lock (_lock)
            {
                Console.SetCursorPosition(Console.WindowWidth - 20, 0);
                Console.WriteLine(DateTime.Now.ToString("HH:mm:ss"));
            }
        }


        private static void UpdateStatus(object sender, ElapsedEventArgs e)
        {
            lock (_lock)
            {
                //Console.SetCursorPosition(0, 1);
                //Console.WriteLine(new string(' ', Console.WindowWidth));
                Console.Clear();
                foreach (var webhook in CacheManager.Webhooks.Values)
                {
                    Console.WriteLine(webhook.ToString());
                }
            }
        }

        private static void Stop()
        {
            clockTimer?.Stop();
            statusTimer?.Stop();
            Console.Clear();
        }

        
    }
}
