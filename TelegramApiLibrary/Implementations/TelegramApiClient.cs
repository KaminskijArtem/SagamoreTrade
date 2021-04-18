using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
using TelegramApiLibrary.Interfaces;

namespace TelegramApiLibrary.Implementations
{
    public class TelegramApiClient : ITelegramApiClient
    {
        private readonly IConfiguration _configuration;
        private string chatId;

        public TelegramApiClient(IConfiguration configuration)
        {
            _configuration = configuration;
            chatId = _configuration["TelegramConfiguration:ChatId"];
        }
        public async Task<HttpResponseMessage> SendMessage(TelegramApiBots bot, string text)
        {
            string botToken = string.Empty;
            var counter = 0;

            switch (bot)
            {
                case TelegramApiBots.SilentBot:
                    {
                        botToken = _configuration["TelegramConfiguration:Bot1Token"];
                        counter = StaticCounter.counter++;
                        break;
                    }
                case TelegramApiBots.NotifyBot:
                    {
                        botToken = _configuration["TelegramConfiguration:Bot2Token"];
                        counter = StaticCounter.counter2++;
                        break;
                    }
                default: break;
            }

            var baseUrl = $"https://api.telegram.org/bot{botToken}/sendMessage?chat_id={chatId}&text={counter}) {text}";
            var client = new HttpClient();
            return await client.GetAsync(baseUrl);
        }
    }
}
