using System;

namespace WebhookServer.Demo2.Models
{
    public class WebhookQueue
    {
        private static int _counter = 0;
        public int Guid { get; set; } = _counter++;
        public int WebhookID { get; set; }
        public DateTime? RateLimiterRetryAfter { get; set; } = null;
    }
}