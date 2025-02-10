using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using WebhookServer.Demo2.Common;
using WebhookServer.Demo2.Windows;

namespace WebhookServer.Demo2
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            using (var serviceProvider = ServiceProviderManager.GetServiceProvider())
            {
                var logger = serviceProvider.GetService<ILogger<Program>>();
                var configuration = serviceProvider.GetService<IConfiguration>();
                try
                {
                    var manager = serviceProvider.GetService<IWindowManager>();
                    manager.SetNextWindow<MainWindow>();
                    await manager.RunAsync();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "[Error] {Message}", ex.Message);
                }
            }
        }
    }
}