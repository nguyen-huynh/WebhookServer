using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebhookServer.Demo2.Models;

namespace WebhookServer.Demo2.Helpers
{
    public static class Utilities
    {
        public static TimeSpan RateUnitToInterval(RateUnit rateUnit)
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(10);
            switch (rateUnit)
            {
                case RateUnit.Hours:
                    timeSpan = TimeSpan.FromMinutes(1);
                    break;

                case RateUnit.Days:
                    timeSpan = TimeSpan.FromMinutes(2);
                    break;
            }
            return timeSpan;
        }
    }
}
