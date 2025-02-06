using Hangfire;
using Hangfire.Storage;
using Polly;
using Polly.RateLimit;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebhookServer.Demo.Helpers;

namespace WebhookServer.Demo.Models
{
    internal class WebhookJob
    {
        public static Dictionary<string, bool> JobRunningStatus = new Dictionary<string, bool>();
        private AsyncRateLimitPolicy _rateLimiter;

        public ConcurrentQueue<int> MainQueue = new ConcurrentQueue<int>();  // Main Queue

        private void InitRateLimiter(int rateLimit, RateUnit rateUnit)
        {
            if (rateUnit == RateUnit.Hours)
            {
                _rateLimiter = Policy.RateLimitAsync(rateLimit, TimeSpan.FromHours(1));
            }
            else if (rateUnit == RateUnit.Days)
            {
                _rateLimiter = Policy.RateLimitAsync(rateLimit, TimeSpan.FromDays(1));
            }
            else
            {
                _rateLimiter = Policy.RateLimitAsync(rateLimit, TimeSpan.FromSeconds(30));
            }
        }

        public async Task RunAsync(string jobId, int webhookID)
        {
            // Kiểm tra trạng thái của job trước khi tiếp tục
            if (JobRunningStatus.ContainsKey(jobId) && JobRunningStatus[jobId])
            {
                return; // Nếu job đã chạy, bỏ qua việc enqueue thêm job
            }

            try
            {
                JobRunningStatus[jobId] = true;

                #region MyRegion
                var webhooks = CacheManager.Webhooks;
                var webhookQueues = CacheManager.WebhookQueues;
                if (webhooks.TryGetValue(webhookID, out var webhook))
                {
                    InitRateLimiter(webhook.RateLimit, webhook.RateUnit);
                    webhook.WebhookStatus = WebhookStatus.Running;

                    if (webhookQueues.TryGetValue(webhookID, out var queue))
                    {
                        int count = 0;

                        while (count < webhook.RateLimit && queue.TryDequeue(out int requestId))
                        {
                            try
                            {
                                count++;
                                await _rateLimiter.ExecuteAsync(async () =>
                                {
                                    await Task.Delay(200); // Giả lập request mất 1000ms

                                    webhook.QueueNumber = queue.Count();
                                    if (queue.IsEmpty)
                                        webhook.WebhookStatus = WebhookStatus.Done;
                                });
                            }
                            catch (RateLimitRejectedException)
                            {
                                Console.WriteLine($"Rate limit exceeded, requeue request {requestId} into failed queue");
                                // Nếu bị rate limit, có thể đưa lại vào queue failed
                                //queue.Enqueue(requestId);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Failed to run {ex.Message}");
                            }
                            finally
                            {
                                // Đảm bảo giải phóng semaphore khi công việc hoàn thành
                                webhook.QueueNumber = queue.Count();
                                if (queue.IsEmpty)
                                    webhook.WebhookStatus = WebhookStatus.Done;
                            }
                        }
                    }
                    else
                    {
                        if (webhook.QueueNumber == 0)
                            webhook.WebhookStatus = WebhookStatus.Done;
                    }
                }
                #endregion
            }
            finally
            {
                JobRunningStatus[jobId] = false;
            }

            
            
        }
    }
}
