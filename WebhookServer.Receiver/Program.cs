using System.Web.Http;
using Microsoft.Owin;
using Microsoft.Owin.Hosting;
using Owin;
using Swashbuckle.Application;
using System;

[assembly: OwinStartup(typeof(WebhookServer.Receiver.Startup))]
namespace WebhookServer.Receiver
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string baseAddress = "http://localhost:5000/";

            using (WebApp.Start<Startup>(baseAddress))
            {
                Console.WriteLine($"API started at {baseAddress}");
                Console.ReadLine(); // Giữ console mở
            }
        }
    }

    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            HttpConfiguration config = new HttpConfiguration();

            // Cấu hình route theo thuộc tính
            config.MapHttpAttributeRoutes();

            // Cấu hình route thông thường
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            // Thêm Swagger
            config.EnableSwagger(c =>
            {
                c.SingleApiVersion("v1", "Console API Webhook");
            }).EnableSwaggerUi();

            // Bật Web API middleware
            app.UseWebApi(config);
        }
    }
}
