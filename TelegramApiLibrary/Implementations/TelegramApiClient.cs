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
        public async Task<HttpResponseMessage> SendMessage(TelegramApiBot bot, string text)
        {
            string botToken = string.Empty;
            var counter = 0;

            switch (bot)
            {
                case TelegramApiBot.InfoBot:
                    {
                        botToken = _configuration["TelegramConfiguration:Bot1Token"];
                        counter = StaticCounter.counter++;
                        break;
                    }
                case TelegramApiBot.SellBot:
                    {
                        botToken = _configuration["TelegramConfiguration:Bot2Token"];
                        counter = StaticCounter.counter2++;
                        break;
                    }
                case TelegramApiBot.BuyBot:
                    {
                        botToken = _configuration["TelegramConfiguration:Bot3Token"];
                        counter = StaticCounter.counter3++;
                        break;
                    }
                case TelegramApiBot.NoLeverageInfoBot:
                    {
                        botToken = _configuration["TelegramConfiguration:Bot4Token"];
                        counter = StaticCounter.counter4++;
                        break;
                    }
                case TelegramApiBot.NoLeverageBuyBot:
                    {
                        botToken = _configuration["TelegramConfiguration:Bot5Token"];
                        counter = StaticCounter.counter5++;
                        break;
                    }
                case TelegramApiBot.NoLeverageSellBot:
                    {
                        botToken = _configuration["TelegramConfiguration:Bot6Token"];
                        counter = StaticCounter.counter6++;
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
