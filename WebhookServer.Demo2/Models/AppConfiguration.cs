﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebhookServer.Demo2.Models
{
    public class AppConfiguration
    {
        public int TargetFPS { get; set; } = 30;
        public int TargetFrameRate { get { return 1000 / TargetFPS; } }
        public int NoOfWebhooks { get; set; } = 10;
        public int InitMinuteQueue { get; set; } = 300;
        public int InitHourQueue { get; set; } = 100;
        public int InitDayQueue { get; set; } = 50;
        public int RateLimitMinuteQueue { get; set; } = 50;
        public int RateLimitHourQueue { get; set; } = 100;
        public int RateLimitDayQueue { get; set; } = 1000;
        public int WebhookRequestTimeout { get; set; } = 30;
    }
}
