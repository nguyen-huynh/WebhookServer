using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using WebhookServer.Demo2.Models;

namespace WebhookServer.Demo2.Common
{
    public class CacheManager
    {
        public readonly object _consoleLock = new object();
        private readonly object _webhooksLock = new object();
        private readonly object _queuesLock = new object();
        private readonly AppConfiguration appConfiguration;

        public CacheManager (AppConfiguration appConfiguration)
        {
            this.appConfiguration = appConfiguration;
        }
        
        private Dictionary<int, Webhook> _webhooks;
        private Dictionary<int, ConcurrentQueue<int>> _webhookQueues;
        public Dictionary<int, Webhook> Webhooks
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
                            InitializeWebhooks(appConfiguration.NoOfWebhooks);
                        }
                    }
                }
                return _webhooks;
            }
        }

        public Dictionary<int, ConcurrentQueue<int>> WebhookQueues
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

        private void InitializeWebhooks(int numberOfWebhook)
        {
            for (int i = 0; i < numberOfWebhook; i++)
            {
                var webhook = new Webhook();
                webhook.TotalQueue = appConfiguration.InitQueuePerWebhook;
                _webhooks[webhook.ID] = webhook;
            }
        }

        private void InitializeQueues()
        {
            foreach (var webhook in Webhooks.Values)
            {
                _webhookQueues[webhook.ID] = new ConcurrentQueue<int>();
            }
        }

        public void EnqueueWebhookItems()
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