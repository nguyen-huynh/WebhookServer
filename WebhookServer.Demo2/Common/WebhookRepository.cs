using Hangfire;
using Hangfire.Logging;
using Hangfire.States;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.RateLimiting;
using Polly.Timeout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebhookServer.Demo2.Helpers;
using WebhookServer.Demo2.Models;
using WebhookServer.Demo2.Windows;

namespace WebhookServer.Demo2.Common
{

    public class WebhookRepository
    {
        private readonly ILogger<WebhookRepository> logger;
        private readonly CacheManager cache;
        private readonly WebhookRateLimitManager webhookRateLimitManager;

        public WebhookRepository(
            ILogger<WebhookRepository> logger,
            CacheManager cache,
            WebhookRateLimitManager webhookRateLimitManager)
        {
            this.logger = logger;
            this.cache = cache;
            this.webhookRateLimitManager = webhookRateLimitManager;
        }

        public void MainJobRunInBackground()
        {
            BackgroundJob.Enqueue<WebhookRepository>(job => job.MainJobRunAsync());
        }

        [Queue("webhook-job"), PreventDuplicateRecurringJobFilter("Enqueued")]
        [AutomaticRetry(Attempts = 0, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        public async Task MainJobRunAsync()
        {
            var webhooks = cache.Webhooks;
            var mainQueues = cache.MainQueues;
            var failedQueues = cache.FailedQueues;
            var databaseQueues = cache.DatabaseQueues;

            if (mainQueues.IsEmpty && failedQueues.IsEmpty && databaseQueues.Count == 0) return;

            await MoveFailedQueuesToMainQueues();
            await DatabaseToMainQueue();
            var nextSchedule = await ProcessMainQueues();

            // Handle all queue until empty
            if (nextSchedule.HasValue)
            {
                BackgroundJob.Schedule<WebhookRepository>(job => job.MainJobRunAsync(), nextSchedule.Value);
            }
            else
            {
                MainJobRunInBackground();
            }
        }

        private async Task DatabaseToMainQueue()
        {
            var databaseQueues = cache.DatabaseQueues;
            if (databaseQueues.Any())
            {
                var now = DateTime.UtcNow;
                var toEnqueue = databaseQueues.Where(x =>
                    !x.RateLimiterRetryAfter.HasValue
                    || x.RateLimiterRetryAfter.Value < now).ToList();
                toEnqueue.ForEach(cache.MainQueues.Enqueue);
                toEnqueue.ForEach(queue => { cache.DatabaseQueues.Remove(queue); });
            }
        }

        private async Task MoveFailedQueuesToMainQueues()
        {
            await cache.RetryFailedRequest();
        }

        public async Task<DateTimeOffset?> ProcessMainQueues()
        {
            DateTimeOffset? nextSchedule = null;
            var mainQueues = cache.MainQueues;
            while (!mainQueues.IsEmpty && mainQueues.TryDequeue(out var queue))
            {
                if (cache.Webhooks.TryGetValue(queue.WebhookID, out var webhook))
                {
                    await webhook.StartAsync();
                    if (!webhookRateLimitManager.RateLimitPipelines.TryGetValue(webhook.ID, out var rateLimiter))
                    {
                        rateLimiter = webhookRateLimitManager.BuildRateLimitFromWebhook(webhook);
                        webhookRateLimitManager.RateLimitPipelines.Add(webhook.ID, rateLimiter);
                    }
                    try
                    {
                        await rateLimiter.ExecuteAsync(async token =>
                        {
                            await ProcessQueue(queue, webhook);
                        });
                    }
                    catch (TimeoutRejectedException)
                    {
                        logger.LogInformation($"Timeout limit, requeue request {queue.Guid} into failed queue");
                        //cache.FailedQueues.Enqueue(queue);
                    }
                    catch (RateLimiterRejectedException ex)
                    {
                        var retryAfter = ex.RetryAfter;
                        if (!retryAfter.HasValue)
                        {
                            retryAfter = Utilities.RateUnitToInterval(webhook.RateUnit);
                        }

                        var retryTime = DateTimeOffset.UtcNow + retryAfter.Value;
                        if (nextSchedule == null)
                            nextSchedule = retryTime;
                        else
                            nextSchedule = nextSchedule > retryTime ? retryTime : nextSchedule;

                        cache.NextSchedule = nextSchedule;
                        //logger.LogInformation($"Rate limit exceeded, requeue request {queue.Guid} into failed queue");
                        await cache.EnqueueFailedRequest(queue);
                    }
                    await webhook.StopAsync();
                }
                else
                {
                    // Return, Webhook deleted. No enqueue to FailedQueue
                    continue;
                }
            }
            return nextSchedule;
        }

        public async Task ProcessQueue(WebhookQueue queue, Webhook webhook)
        {
            await webhookRateLimitManager.TimeoutPipeline.ExecuteAsync(async token =>
            {
                await Task.Delay(webhook.Delay, token);
                webhook.SuccessQueueNumber++;
            });
        }
    }
}
