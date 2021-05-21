using Microsoft.Extensions.DependencyInjection;
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

            services.AddSingleton<BuyJob>();
            services.AddSingleton(new JobSchedule(
                jobType: typeof(BuyJob),
                //cronExpression: "10 4,9,14,19,24,29,34,39,44,49,54,59 * * * ?")); // run every 5 minutes
                cronExpression: "0 * * * * ?")); // run every minute at 0s

            services.AddSingleton<SellJob>();
            services.AddSingleton(new JobSchedule(
                jobType: typeof(SellJob),
                //cronExpression: "0 4,9,14,19,24,29,34,39,44,49,54,59 * * * ?")); // run every 5 minutes
                cronExpression: "30 * * * * ?")); // run every minute at 30s

            services.AddHostedService<QuartzHostedService>();
        }
    }
}
