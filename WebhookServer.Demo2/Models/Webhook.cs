using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebhookServer.Demo2.Common;

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
        public int QueueNumber { get; set; }
        public WebhookStatus WebhookStatus { get; set; } = WebhookStatus.NotRun;
        public int TotalQueue { get; set; }
        public int Delay { get; set; }
        public int Speed { get; set; }
        public Stopwatch Stopwatch { get; set; }
        public string JobQueue { get; set; }

        public Webhook()
        {
            ID = ++_lastId;
            Name = $"Webhook-{ID}";
            IsEnabled = true;
            RateLimit = 100;
            //TotalQueue = 50 * ID;
            TotalQueue = 500;

            //RateUnit = GetRandomEnumValue<RateUnit>(_random);
            RateUnit = ID >= 9 ? RateUnit.Days : (ID >= 6 ? RateUnit.Hours : RateUnit.Minutes);
            switch (RateUnit)
            {
                case RateUnit.Minutes:
                    Delay = 100;
                    JobQueue = "minute-webhook";
                    break;
                case RateUnit.Hours:
                    //Delay = 300;
                    Delay = 100;
                    JobQueue = "minute-webhook";
                    break;
                case RateUnit.Days:
                    //Delay = 1000;
                    Delay = 100;
                    JobQueue = "daily-webhook";
                    break;
            }
            Speed = 30 * 1000 / Delay;
            Stopwatch = new Stopwatch();
        }

        public override string ToString()
        {
            return $"{Name,-10} " +
                $"- Speed/[{RateUnit,7}] [{Speed,4}] " +
                $"- Status [{WebhookStatus,-8}] " +
                $"- Processed/Total [{TotalQueue - QueueNumber,4}/{TotalQueue,4}] " +
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
            //this.WebhookStatus = WebhookStatus.Running;
            //Stopwatch.Start();
        }

        public async Task StopAsync()
        {
            //this.WebhookStatus = WebhookStatus.Stoped;
            //Stopwatch.Stop();
            //var cache = CacheManager.WebhookQueues;
            //if (cache.TryGetValue(this.ID, out var queue))
            //{
            //    if (queue.IsEmpty)
            //        this.WebhookStatus = WebhookStatus.Done;
            //}
        }
    }

}
