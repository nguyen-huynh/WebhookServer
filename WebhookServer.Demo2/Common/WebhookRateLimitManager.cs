using Polly;
using Polly.RateLimiting;
using Polly.Timeout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.RateLimiting;
using System.Threading.Tasks;
using WebhookServer.Demo2.Helpers;
using WebhookServer.Demo2.Models;

namespace WebhookServer.Demo2.Common
{
    public class WebhookRateLimitManager
    {
        private readonly object _timeoutLockObject = new object();
        private readonly object _rateLimitLockObject = new object();
        private readonly CacheManager cache;
        private readonly AppConfiguration appConfiguration;

        public WebhookRateLimitManager(CacheManager cache,
            AppConfiguration appConfiguration)
        {
            this.cache = cache;
            this.appConfiguration = appConfiguration;
        }

        private ResiliencePipeline _timeoutPipeline;
        private Dictionary<int, ResiliencePipeline> _rateLimitPipelines;

        public ResiliencePipeline TimeoutPipeline
        {
            get
            {
                if (_timeoutPipeline == null)
                {
                    lock (_timeoutLockObject)
                    {
                        if (_timeoutPipeline == null) // Double-check locking
                        {
                            _timeoutPipeline = new ResiliencePipelineBuilder()
                                .AddTimeout(new TimeoutStrategyOptions
                                {
                                    Timeout = TimeSpan.FromSeconds(appConfiguration.WebhookRequestTimeout),
                                })
                                .Build();
                        }
                    }
                }
                return _timeoutPipeline;
            }
        }

        public Dictionary<int, ResiliencePipeline> RateLimitPipelines
        {
            get
            {
                if (_rateLimitPipelines == null)
                {
                    lock (_rateLimitLockObject)
                    {
                        if (_rateLimitPipelines == null) // Double-check locking
                        {
                            _rateLimitPipelines = new Dictionary<int, ResiliencePipeline>();
                            foreach (var webhook in cache.Webhooks.Values)
                            {
                                var pipeline = BuildRateLimitFromWebhook(webhook);
                                _rateLimitPipelines.Add(webhook.ID, pipeline);
                            }
                        }
                    }
                }
                return _rateLimitPipelines;
            }
        }

        public ResiliencePipeline BuildRateLimitFromWebhook(Webhook webhook)
        {
            var timeSpan = Utilities.RateUnitToInterval(webhook.RateUnit);

            var rateLimitOptions = new SlidingWindowRateLimiterOptions
            {
                PermitLimit = webhook.RateLimit,  // Ratelimit
                AutoReplenishment = true,
                SegmentsPerWindow = 4,
                Window = timeSpan,
            };
            var rateLimiter = new SlidingWindowRateLimiter(rateLimitOptions);
            var pipeline = new ResiliencePipelineBuilder()
                    .AddRateLimiter(new SlidingWindowRateLimiter(rateLimitOptions))
                    .Build();
            return pipeline;
        }
    }
}