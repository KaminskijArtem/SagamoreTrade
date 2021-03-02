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
                cronExpression: "0 0,5,10,15,20,25,30,35,40,45,50,55 * * * ?")); // run every 5 minutes
                //cronExpression: "0,30 * * * * ?")); // run every minute at 0,30s

            services.AddHostedService<QuartzHostedService>();
        }
    }
}
