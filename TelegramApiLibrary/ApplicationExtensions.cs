using Microsoft.Extensions.DependencyInjection;
using TelegramApiLibrary.Implementations;
using TelegramApiLibrary.Interfaces;

namespace TelegramApiLibrary
{
    public static class ApplicationExtensions
    {
        public static void AddTelegramApiServices(this IServiceCollection services)
        {
            services.AddTransient<ITelegramApiClient, TelegramApiClient>();
        }
    }
}
