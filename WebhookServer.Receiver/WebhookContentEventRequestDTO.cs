using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebhookServer.Receiver
{
    public class WebhookContentEventRequestDTO
    {
        public string EventType { get; set; }
        public long EventID { get; set; }
        public string Timestamp { get; set; }
        public int WebhookID { get; set; }
        public Dictionary<string, object> Content { get; set; }
    }
}
