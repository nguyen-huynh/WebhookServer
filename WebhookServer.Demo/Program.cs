using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using WebhookServer.Demo.Helpers;
using WebhookServer.Demo.Windows;

namespace WebhookServer.Demo
{
    internal class Program
    {

        static async Task Main(string[] args)
        {
            try
            {
                MainWindow.Show();
                MainWindow.Listen();
            }
            finally
            {
                HangfireManager.Stop();
            }
        }
    }
}
