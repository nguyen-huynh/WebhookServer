using Hangfire;
using Hangfire.Client;
using Hangfire.Common;
using Hangfire.Server;
using Hangfire.States;
using Hangfire.Storage.Monitoring;
using System;
using System.Collections.Generic;
using System.Linq;
using WebhookServer.Demo2.Common;
using WebhookServer.Demo2.Models;

namespace WebhookServer.Demo2.Helpers
{
    public class PreventDuplicateRecurringJobFilter : JobFilterAttribute, IClientFilter
    {
        private readonly List<string> states;

        public PreventDuplicateRecurringJobFilter(string states)
        {
            this.states = states?.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>();
        }

        public void OnCreating(CreatingContext context)
        {
            if (context.Job != null && context.Connection != null)
            {
                if (context.Job.Type != typeof(WebhookRepository))
                    return;
                
                var methodName = context.Job.Method.Name;
                if (!states.Contains(context.InitialState.Name)) return;

                if (!string.IsNullOrEmpty(methodName))
                {
                    var monitorAPI = JobStorage.Current.GetMonitoringApi();
                    var jobs = monitorAPI.EnqueuedJobs("webhook-job", 0, int.MaxValue);

                    if (jobs.Any(job => Compare(job, methodName)))
                    {
                        context.Canceled = true;
                        return;
                    }
                    var scheduledJobs = monitorAPI.ScheduledJobs(0, int.MaxValue);
                    if (scheduledJobs.Any(job => Compare(job, methodName)))
                    {
                        var onOgingJob = scheduledJobs.FirstOrDefault(job => Compare(job, methodName));
                        if (onOgingJob.Value.EnqueueAt < DateTime.Now)
                        {
                            // Remove ongoingjob here
                            BackgroundJob.Delete(onOgingJob.Key);
                        }
                        context.Canceled = false;
                        return;
                    }
                    var processingJobs = monitorAPI.ProcessingJobs(0, int.MaxValue);
                    if (processingJobs.Any(job => Compare(job, methodName)))
                    {
                        context.Canceled = true;
                        return;
                    }
                }
            }
        }

        private bool Compare(KeyValuePair<string, EnqueuedJobDto> job, string value)
        {
            if (job.Value.Job.Type != typeof(WebhookRepository)) return false;
            var jobMethodName = job.Value.Job.Method.Name;
            return jobMethodName == value;
        }

        private bool Compare(KeyValuePair<string, ScheduledJobDto> job, string value)
        {
            if (job.Value.Job.Type != typeof(WebhookRepository)) return false;
            var jobMethodName = job.Value.Job.Method.Name;
            return jobMethodName == value;
        }

        private bool Compare(KeyValuePair<string, ProcessingJobDto> job, string value)
        {
            if (job.Value.Job.Type != typeof(WebhookRepository)) return false;
            var jobMethodName = job.Value.Job.Method.Name;
            return jobMethodName == value;
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