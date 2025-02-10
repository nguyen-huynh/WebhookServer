using Hangfire;
using Microsoft.Owin;
using Owin;

namespace WebhookServer.Demo2.Servers
{
    internal class OwinStartup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseHangfireDashboard();
        }

    }
}
