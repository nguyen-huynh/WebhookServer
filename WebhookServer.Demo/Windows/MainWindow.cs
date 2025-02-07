using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using WebhookServer.Demo.Helpers;

namespace WebhookServer.Demo.Windows
{
    internal class MainWindow
    {
        private static Timer timer;
        internal static void Listen()
        {
            while (true)
            {
                Console.WriteLine("Press key to select (1: Init Webhooks, Esc: Exit):");
                var key = Console.ReadKey(true).Key;

                if (key == ConsoleKey.D1) // Nhấn "1" để thực hiện job
                {
                    Clear();
                    timer.Stop();
                    Console.WriteLine("BackgroundStatusWindow...");
                    BackgroundStatusWindow.Init();
                    BackgroundStatusWindow.Show();
                    break;
                }
                else if (key == ConsoleKey.Escape) // Nhấn "Esc" để thoát
                {
                    break;
                }
                else
                {
                    //Console.WriteLine("Phím không hợp lệ. Vui lòng thử lại.");
                }
            }
            Console.WriteLine("Exit...");
        }

        private static void InitClock()
        {
            Console.CursorVisible = false;
            timer = new Timer();
            timer.Elapsed += UpdateClock;
            timer.Start();
        }

        private static void UpdateClock(object sender, ElapsedEventArgs e)
        {
            lock (CacheManager._consoleLock)
            {
                var left = Console.CursorLeft;
                var top = Console.CursorTop;

                Console.SetCursorPosition(Console.WindowWidth - 20, 0); // Đặt con trỏ về đầu màn hình
                Console.WriteLine(DateTime.Now.ToString("HH:mm:ss"));
                Console.SetCursorPosition(left, top);
            }
        }

        internal static void Show()
        {
            InitClock();
            Console.WriteLine("Select an option:");
            Console.WriteLine("1. Init and start 10 webhooks" +
                "\n\tAdd 500 request per webhook, webhook allow send 100 requests per 30 seconds");
            Console.WriteLine("\nEsc: Exit");
        }

        internal static void Clear()
        {
            Console.Clear();
            UpdateClock(null, null);
        }
    }
}
