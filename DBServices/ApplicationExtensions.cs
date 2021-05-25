using Microsoft.Extensions.DependencyInjection;
using System;
using Microsoft.Extensions.Configuration;
using System.Configuration;
using System.IO;
using Microsoft.EntityFrameworkCore;

namespace DBServices
{
    public static class ApplicationExtensions
    {
        public static void AddDBServices(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(connectionString));
        }
    }
}
