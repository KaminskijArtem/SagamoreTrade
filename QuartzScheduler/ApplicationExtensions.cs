using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using QuartzScheduler.Base;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using QuartzScheduler.Jobs;

namespace QuartzScheduler
{
    public static class ApplicationExtensions
    {
        public static void AddSchedulerServices(this IServiceCollection services)
        {
            // Add Quartz services
            services.AddSingleton<IJobFactory, SingletonJobFactory>();
            services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>();

            services.AddSingleton<RSIJob>();
            services.AddSingleton(new JobSchedule(
                jobType: typeof(RSIJob),
                cronExpression: "30 2,7,12,17,22,27,32,37,42,47,52,57 * * * ?")); // run every 5 minutes at 30s
                //cronExpression: "0,30 * * * * ?")); // run every minute at 0,30s

            services.AddHostedService<QuartzHostedService>();
        }
    }
}
