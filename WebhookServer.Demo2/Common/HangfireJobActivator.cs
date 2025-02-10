using Hangfire;
using System;

namespace WebhookServer.Demo2.Common
{
    public class HangfireJobActivator : JobActivator
    {
        private readonly IServiceProvider _serviceProvider;

        public HangfireJobActivator(IServiceProvider serviceProvider)
        {
            this._serviceProvider = serviceProvider;
        }

        public override object ActivateJob(Type jobType)
        {
            var service = _serviceProvider.GetService(jobType);
            if (service == null)
            {
                return base.ActivateJob(jobType);
            }
            return service;
        }
    }
}