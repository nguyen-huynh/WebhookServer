using Hangfire;
using Microsoft.Owin;
using Owin;

namespace WebhookServer.Demo.Servers
{
    internal class OwinStartup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseHangfireDashboard(); // Mặc định có sẵn giao diện quản lý job
        }

    }
}
