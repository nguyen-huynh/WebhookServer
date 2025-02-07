using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebhookServer.Demo.Models;

namespace WebhookServer.Demo.Helpers
{
    internal class CacheManager
    {
        public static readonly object _consoleLock = new object();
        private static readonly object _webhooksLock = new object();
        private static readonly object _queuesLock = new object();
        private static Dictionary<int, Webhook> _webhooks;
        private static Dictionary<int, ConcurrentQueue<int>> _webhookQueues;


        public static Dictionary<int, Webhook> Webhooks
        {
            get
            {
                if (_webhooks == null)
                {
                    lock (_webhooksLock)
                    {
                        if (_webhooks == null) // Double-check locking
                        {
                            _webhooks = new Dictionary<int, Webhook>();
                            InitializeWebhooks(10);
                        }
                    }
                }
                return _webhooks;
            }
        }

        public static Dictionary<int, ConcurrentQueue<int>> WebhookQueues
        {
            get
            {
                if (_webhookQueues == null)
                {
                    lock (_queuesLock)
                    {
                        if (_webhookQueues == null) // Double-check locking
                        {
                            _webhookQueues = new Dictionary<int, ConcurrentQueue<int>>();
                            InitializeQueues();
                        }
                    }
                }
                return _webhookQueues;
            }
        }

        private static void InitializeWebhooks(int numberOfWebhook)
        {
            for (int i = 0; i < numberOfWebhook; i++)
            {
                var webhook = new Webhook();
                _webhooks[webhook.ID] = webhook;
            }
        }

        private static void InitializeQueues()
        {
            foreach (var webhook in Webhooks.Values)
            {
                _webhookQueues[webhook.ID] = new ConcurrentQueue<int>();
            }
        }

        public static void EnqueueWebhookItems()
        {
            lock (_queuesLock)
            {
                foreach (var webhook in Webhooks.Values)
                {
                    if (WebhookQueues.TryGetValue(webhook.ID, out var queue))
                    {
                        for (int i = 0; i < webhook.TotalQueue; i++)
                        {
                            queue.Enqueue(i);
                        }
                        webhook.QueueNumber = queue.ToArray().Length;
                    }
                }
            }
        }
    }
}