using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebhookServer.Demo2.Common;
using WebhookServer.Demo2.Helpers;

namespace WebhookServer.Demo2.Models
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
        Stoped = 2,
        Done = 3,
        Waiting = 4,
    }

    public class Webhook
    {
        private static int _lastId = 0;
        private static readonly Random _random = new Random();

        public int ID { get; private set; }
        public string Name { get; private set; }
        public bool IsEnabled { get; set; }
        public int RateLimit { get; set; }
        public RateUnit RateUnit { get; set; }
        public int SuccessQueueNumber = 0;
        public WebhookStatus WebhookStatus { get; set; } = WebhookStatus.NotRun;
        public int TotalQueue { get; set; }
        public int Delay { get; set; }
        public int Speed { get; set; }
        public Stopwatch Stopwatch { get; set; }
        public string JobQueue { get; set; }
        public int Interval { get; set; }

        public Webhook()
        {
            ID = ++_lastId;
            Name = $"Webhook-{ID}";
            IsEnabled = true;
            RateLimit = 100;
            //TotalQueue = 50 * ID;
            TotalQueue = 300;

            //RateUnit = GetRandomEnumValue<RateUnit>(_random);
            RateUnit = ID >= 9 ? RateUnit.Days : (ID >= 6 ? RateUnit.Hours : RateUnit.Minutes);
            Delay = 25;
            Speed = 1 * 1000 / Delay;
            Stopwatch = new Stopwatch();
        }

        public override string ToString()
        {
            return $"{Name,-10} " +
                $"- Speed [{Speed,4}/s] " +
                $"- Rate [{RateLimit, 4} per {Interval,4} second]" +
                $"- Status [{WebhookStatus,-8}] " +
                $"- Processed/Total [{SuccessQueueNumber,4}/{TotalQueue,4}] " +
                //$"- Remaining [{QueueNumber,4}] [] " +
                $"- RunTime [{Stopwatch.ElapsedMilliseconds / 100, 4}]";
        }
        public static T GetRandomEnumValue<T>(Random random) where T : Enum
        {
            Array values = Enum.GetValues(typeof(T));
            return (T)values.GetValue(random.Next(values.Length));
        }

        public async Task StartAsync()
        {
            this.WebhookStatus = WebhookStatus.Running;
            Stopwatch.Start();
        }

        public async Task StopAsync()
        {
            this.WebhookStatus = WebhookStatus.Stoped;
            if (this.SuccessQueueNumber == this.TotalQueue)
                this.WebhookStatus = WebhookStatus.Done;
            Stopwatch.Stop();
        }
    }

}
