using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebhookServer.Demo.Models
{
    public enum RateUnit
    {
        Minutes = 0,
        Hours = 1,
        Days = 2
    }

    public enum WebhookStatus
    {
        NotRun = 0,
        Running = 1,
        Done = 2,
    }

    public class Webhook
    {
        private static int _lastId = 0; // ID tự tăng
        private static readonly Random _random = new Random();

        public int ID { get; private set; }
        public string Name { get; private set; }
        public bool IsEnabled { get; set; }
        public int RateLimit { get; set; }
        public RateUnit RateUnit { get; set; }
        public int QueueNumber { get; set; }
        public WebhookStatus WebhookStatus { get; set; } = WebhookStatus.NotRun;
        public int TotalQueue { get; set; }

        public Webhook()
        {
            ID = ++_lastId;
            Name = $"Webhook-{ID}";
            IsEnabled = true;
            RateLimit = 100;
            RateUnit = RateUnit.Minutes;
            TotalQueue = 50 * ID;

        }

        public override string ToString()
        {
            return string.Format("{0,-15} - Status [{1,-8}] - Total [{2,5}] - Remaining [{3,5}]",
                Name,
                WebhookStatus,
                TotalQueue,
                QueueNumber);
        }

        public async Task<bool> ProcessAsync()
        {
            await Task.Delay(1000); // Delay 1 giây

            bool result = _random.Next(100) < 95; // 95% true, 5% false
            return result;
        }
    }

}
