using System;
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
        public int InitQueuePerWebhook { get; set; } = 100;
    }
}
