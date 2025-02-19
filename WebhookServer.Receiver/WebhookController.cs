using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace WebhookServer.Receiver
{
    public class WebhookController : ApiController
    {
        [HttpPost]
        [Route("api/webhook/receive")]
        public async Task<IHttpActionResult> Receive()
        {
            Console.WriteLine("------------------------------------------");
            var headers = Request.Headers.ToDictionary(k => k.Key, v => v.Value.FirstOrDefault().ToString());
            Console.WriteLine($"Received request: {JsonConvert.SerializeObject(headers)}");

            string content = await Request.Content.ReadAsStringAsync();
            Console.WriteLine($"Received request: {content}");
            Console.WriteLine("------------------------------------------");

            return Ok(new { success = true });
        }
    }
}
