using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace WebhookServer.Receiver
{
    [RoutePrefix("api/webhook")]
    public class WebhookController : ApiController
    {
        private async Task LogRequest(WebhookContentEventRequestDTO data)
        {
            var headers = Request.Headers.ToDictionary(k => k.Key, v => v.Value.FirstOrDefault().ToString());
            Console.WriteLine($"Received Header: {JsonConvert.SerializeObject(headers)}");

            Console.WriteLine($"Received request: {JsonConvert.SerializeObject(data)}");
        }

        [HttpPost]
        [Route("200")]
        public async Task<IHttpActionResult> ReturnOk([FromBody] WebhookContentEventRequestDTO data)
        {
            Console.WriteLine("------------------------------------------");

            Console.WriteLine("Starting Ok");
            await Task.Delay(1000);
            await LogRequest(data);

            Console.WriteLine("------------------------------------------");
            return Ok(new { success = true });
        }

        [HttpPost]
        [Route("400")]
        public async Task<IHttpActionResult> ReturnBadRequest([FromBody] WebhookContentEventRequestDTO data)
        {
            Console.WriteLine("------------------------------------------");

            Console.WriteLine("Starting BadRequest");
            await Task.Delay(1000);
            await LogRequest(data);

            Console.WriteLine("------------------------------------------");
            return BadRequest("Bad request");
        }

        [HttpPost]
        [Route("401")]
        public async Task<IHttpActionResult> ReturnUnauthorized([FromBody] WebhookContentEventRequestDTO data)
        {
            Console.WriteLine("------------------------------------------");

            Console.WriteLine("Starting Unauthorized");
            await Task.Delay(1000);
            await LogRequest(data);

            Console.WriteLine("------------------------------------------");
            throw new HttpResponseException(HttpStatusCode.Unauthorized);
        }

        [HttpPost]
        [Route("408")]
        public async Task<IHttpActionResult> ReturnRequestTimeout([FromBody] WebhookContentEventRequestDTO data)
        {
            Console.WriteLine("------------------------------------------");

            Console.WriteLine("Starting RequestTimeout");
            await Task.Delay(1000);
            await LogRequest(data);
            
            await Task.Delay(60* 60 * 1000);
            Console.WriteLine("------------------------------------------");
            throw new HttpResponseException(HttpStatusCode.RequestTimeout);
        }

        [HttpPost]
        [Route("500")]
        public async Task<IHttpActionResult> ReturnInternalServerError([FromBody] WebhookContentEventRequestDTO data)
        {
            Console.WriteLine("------------------------------------------");

            Console.WriteLine("Starting InternalServerError");
            await Task.Delay(1000);
            await LogRequest(data);

            Console.WriteLine("------------------------------------------");
            throw new HttpResponseException(HttpStatusCode.InternalServerError);
        }
    }
}
