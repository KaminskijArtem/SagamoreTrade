using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using TradingDataLibrary.ApiClient;
using TradingDataLibrary.Implementations;
using TradingDataLibrary.Interfaces;

namespace TradingDataLibrary
{
    public static class ApplicationExtensions
    {
        public static void AddTradingDataServices(this IServiceCollection services)
        {
            services.AddTransient<IRSITradeCandlesService, RSITradeCandlesService>();
            services.AddTransient<ICandlesApiClient, CandlesApiClient>();
        }
    }
}
