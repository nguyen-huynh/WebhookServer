using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebhookServer.Demo2.Models;

namespace WebhookServer.Demo2.Common
{
    public class CacheManager
    {
        public readonly object _consoleLock = new object();
        private readonly object _webhooksLock = new object();
        private readonly object _mainQueuesLock = new object();
        private readonly object _failedQueuesLock = new object();
        private readonly object _databaseQueuesLock = new object();
        private readonly AppConfiguration appConfiguration;

        public CacheManager(AppConfiguration appConfiguration)
        {
            this.appConfiguration = appConfiguration;
        }

        private Dictionary<int, Webhook> _webhooks;
        private ConcurrentQueue<WebhookQueue> _mainQueues;
        private ConcurrentQueue<WebhookQueue> _failedQueues;
        private List<WebhookQueue> _databaseQueues;
        public DateTimeOffset? NextSchedule { get; set; } = null;

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

        public ConcurrentQueue<WebhookQueue> MainQueues
        {
            get
            {
                if (_mainQueues == null)
                {
                    lock (_mainQueuesLock)
                        if (_mainQueues == null)
                        {
                            _mainQueues = new ConcurrentQueue<WebhookQueue>();
                        }
                }
                return _mainQueues;
            }
        }

        public ConcurrentQueue<WebhookQueue> FailedQueues
        {
            get
            {
                if (_failedQueues == null)
                {
                    lock (_failedQueuesLock)
                        if (_failedQueues == null)
                        {
                            _failedQueues = new ConcurrentQueue<WebhookQueue>();
                        }
                }
                return _failedQueues;
            }
        }

        public List<WebhookQueue> DatabaseQueues
        {
            get
            {
                if (_databaseQueues == null)
                {
                    lock (_databaseQueuesLock)
                        if (_databaseQueues == null)
                        {
                            _databaseQueues = new List<WebhookQueue>();
                        }
                }
                return _databaseQueues;
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

        public async Task EnqueueRequest(WebhookQueue queue)
        {
            MainQueues.Enqueue(queue);
        }

        public async Task RetryFailedRequest()
        {
            while (!FailedQueues.IsEmpty && FailedQueues.TryDequeue(out var queue))
            {
                MainQueues.Enqueue(queue);
            }
        }

        public async Task EnqueueFailedRequest(WebhookQueue queue)
        {
            FailedQueues.Enqueue(queue);
        }
    }
}