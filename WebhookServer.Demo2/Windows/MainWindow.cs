using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;

namespace WebhookServer.Demo2.Windows
{
    internal class MainWindow : BaseWindow, IWindow
    {
        private readonly IWindowManager windowManager;

        public MainWindow(IServiceProvider serviceProvider, IWindowManager windowManager) : base(serviceProvider)
        {
            this.windowManager = windowManager;

            _inputHandler = new Dictionary<ConsoleKey, Func<Task<bool>>>
            {
                { ConsoleKey.Escape, async () => {
                    windowManager.SetNextWindow(null);
                    return false;
                }},
                { ConsoleKey.D1, async () =>
                    {
                        windowManager.SetNextWindow<StatusWindow>();
                        return false;
                    }
                },
            };
        }

        public override async Task Show()
        {
            Console.Clear();
            Console.WriteLine("Select an option:");
            Console.WriteLine("1. Init and start 10 webhooks with 150-100-50 request" +
                "\n\tAfter starting, press 'A' key to add 100 requests to random webhook " +
                "\n\tor '1-9' to add 50 request to selected webhook");
            Console.WriteLine("\nEsc: Exit");
        }
    }
}
