using Hangfire;
using Hangfire.Client;
using Hangfire.Common;
using Hangfire.Server;
using Hangfire.Storage.Monitoring;
using System;
using System.Collections.Generic;
using System.Linq;
using WebhookServer.Demo2.Models;

namespace WebhookServer.Demo2.Helpers
{

    public class PreventDuplicateRecurringJobFilter : JobFilterAttribute, IClientFilter
    {
        public void OnCreating(CreatingContext context)
        {
            if (context.Job != null && context.Connection != null)
            {
                if (context.Job.Type != typeof(WebhookJob))
                    return;

                var webhookName = context.Job.Args[0]?.ToString();

                if (!string.IsNullOrEmpty(webhookName))
                {
                    var monitorAPI =  JobStorage.Current.GetMonitoringApi();
                    var jobs = monitorAPI.EnqueuedJobs("daily-webhook", 0, int.MaxValue);
                    var jobs2 = monitorAPI.EnqueuedJobs("hour-webhook", 0, int.MaxValue);
                    var jobs3 = monitorAPI.EnqueuedJobs("minute-webhook", 0, int.MaxValue);

                    if (jobs.Any(job => Compare(job, webhookName)) || jobs2.Any(job => Compare(job, webhookName)) || jobs3.Any(job => Compare(job, webhookName)))
                    {
                        //Console.WriteLine($"🔴 Job {webhookName} is canceled");
                        context.Canceled = true;
                    }
                }
            }
        }

        private bool Compare(KeyValuePair<string, EnqueuedJobDto> job, string webhookName)
        {
            if (job.Value.Job.Type != typeof(WebhookJob)) return false;
            var jobName = job.Value.Job.Args[0]?.ToString();
            return jobName == webhookName;
        }

        public void OnCreated(CreatedContext context)
        {
        }

        public void OnPerforming(PerformingContext context)
        {
            throw new NotImplementedException();
        }

        public void OnPerformed(PerformedContext context)
        {
            throw new NotImplementedException();
        }
    }
}