using Hangfire;
using Microsoft.Owin;
using Owin;

namespace WebhookServer.Demo2.Servers
{
    public class OwinStartup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseHangfireDashboard();
        }
    }
}
