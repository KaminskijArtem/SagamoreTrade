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
                cronExpression: "0 0/5 * * * ?")); // run every 5 minutes

            services.AddSingleton<SellJob>();
            services.AddSingleton(new JobSchedule(
                jobType: typeof(SellJob),
                cronExpression: "30 0/5 * * * ?")); // run every 5 minutes

            services.AddHostedService<QuartzHostedService>();
        }
    }
}
