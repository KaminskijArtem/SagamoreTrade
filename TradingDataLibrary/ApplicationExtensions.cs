using Microsoft.Extensions.DependencyInjection;
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
            services.AddTransient<IPositionsApiClient, PositionsApiClient>();
        }
    }
}
