using Hangfire;
using Hangfire.Storage;
using Polly;
using Polly.RateLimiting;
using Polly.Timeout;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.RateLimiting;
using System.Threading.Tasks;
using WebhookServer.Demo2.Helpers;
using WebhookServer.Demo2.Common;

namespace WebhookServer.Demo2.Models
{
    internal class WebhookJob
    {
        public static Dictionary<string, bool> JobRunningStatus = new Dictionary<string, bool>();
        private ResiliencePipeline _timeoutPipeline;
        private ResiliencePipeline _rateLimiterPipeline;

        public ConcurrentQueue<int> MainQueue = new ConcurrentQueue<int>();  // Main Queue

        /// <summary>
        /// Init pipeline per webhook, should keep this pipeline instead of disposing after job run.
        /// </summary>
        private void InitPipelines(int rateLimit, RateUnit rateUnit)
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(30);
            //switch (rateUnit)
            //{
            //    case RateUnit.Hours:
            //        timeSpan = TimeSpan.FromHours(1);
            //        break;
            //    case RateUnit.Days:
            //        timeSpan = TimeSpan.FromDays(1);
            //        break;
            //}

            // Confire RateLimit (EX: 100 request every 30 seconds)
            var rateLimitOptions = new SlidingWindowRateLimiterOptions
            {
                PermitLimit = rateLimit,  // Ratelimit
                AutoReplenishment = true,
                SegmentsPerWindow = 4,
                Window = timeSpan,
            };

            // Configure timeout for Job, stop the job for {interval}
            var timeoutOptions = new TimeoutStrategyOptions
            {
                Timeout = TimeSpan.FromSeconds(30), // For test only, should based on rateUnit
                
            };

            // Pipeline timeout for the whole job ( only apply Timeout, no RateLimit)
            _timeoutPipeline = new ResiliencePipelineBuilder()
                .AddTimeout(timeoutOptions)
                .Build();

            // Pipeline Rate Limiting for webhook send request (SendRequestAsync)
            _rateLimiterPipeline = new ResiliencePipelineBuilder()
                .AddRateLimiter(new SlidingWindowRateLimiter(rateLimitOptions))
                .Build();
        }

        public async Task RunAsync(string jobId, int webhookID)
        {
            //if (JobRunningStatus.ContainsKey(jobId) && JobRunningStatus[jobId])
            //{
            //    return; // If there is a same webhook runing, don't run again
            //}

            //try
            //{
            //    JobRunningStatus[jobId] = true;

            //    var webhooks = CacheManager.Webhooks;
            //    var webhookQueues = CacheManager.WebhookQueues;

            //    if (webhooks.TryGetValue(webhookID, out var webhook))
            //    {
            //        InitPipelines(webhook.RateLimit, webhook.RateUnit);
            //        webhook.WebhookStatus = WebhookStatus.Running;
            //        await webhook.StartAsync();

            //        try
            //        {
            //            // Apply timeout for the whole `RunAsync`
            //            await _timeoutPipeline.ExecuteAsync(async (token) =>
            //            {
            //                if (webhookQueues.TryGetValue(webhookID, out var queue))
            //                {
            //                    int count = 0;
            //                    // Check if CancellationToken request cancel this run
            //                    while (!token.IsCancellationRequested && queue.TryDequeue(out int requestId))
            //                    {
            //                        // Aplly rate limiting per SendRequestAsync
            //                        // This means if exceed the Rate limit, throw exception and break the loop.
            //                        try
            //                        {
            //                            await _rateLimiterPipeline.ExecuteAsync(
            //                                async token2 => await SendRequestAsync(requestId, webhook, queue)
            //                            );
            //                            count++;
            //                        }
            //                        catch (RateLimiterRejectedException)
            //                        {
            //                            Console.WriteLine($"Rate limit exceeded, requeue request {requestId} into failed queue");
            //                            break;
            //                        }
            //                    }
            //                }
            //            });
            //        }
            //        catch (TimeoutRejectedException)
            //        {
            //            Console.WriteLine($"Timeout exceeded, Job {jobId} failed");
            //        }
            //        catch (Exception ex)
            //        {
            //            Console.WriteLine($"Failed to run job {jobId}: {ex.Message}");
            //        }
            //        finally
            //        {
            //            await webhook.StopAsync();
            //        }
            //    }
            //}
            //finally
            //{
            //    JobRunningStatus[jobId] = false;
            //}
        }

        [Queue("minute-webhook"), PreventDuplicateRecurringJobFilter]
        public async Task ShortRunAsync(string jobId, int webhookID)
            => await RunAsync(jobId, webhookID);

        [Queue("daily-webhook"), PreventDuplicateRecurringJobFilter]
        public async Task LongRunAsync(string jobId, int webhookID)
            => await RunAsync(jobId, webhookID);

        public async Task SendRequestAsync(int queueID, Webhook webhook, ConcurrentQueue<int> queue)
        {
            await Task.Delay(webhook.Delay);
            webhook.QueueNumber = queue.Count;
            //BackgroundStatusWindow.UpdateStatus(null, null);
        }
    }
}
