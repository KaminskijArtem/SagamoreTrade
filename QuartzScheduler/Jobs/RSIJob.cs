using Quartz;
using System;
using System.Collections.Generic;
using TradingDataLibrary.Interfaces;
using System.Threading.Tasks;
using QuartzScheduler.Base;
using System.Net.Http;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using QuartzScheduler.Logging;

namespace QuartzScheduler.Jobs
{
    public class RSIJob : IJob
    {
        readonly string interval = "1h";
        private readonly IRSITradeCandlesService _tradeCandlesService;
        private string bot1Token;
        private string bot2Token;
        private string chatId;

        private readonly IConfiguration Configuration;

        public RSIJob(IRSITradeCandlesService tradeCandlesService, IConfiguration configuration)
        {
            _tradeCandlesService = tradeCandlesService;
            Configuration = configuration;
            bot1Token = Configuration["TelegramConfiguration:Bot1Token"];
            bot2Token = Configuration["TelegramConfiguration:Bot2Token"];
            chatId = Configuration["TelegramConfiguration:ChatId"];
        }
        public async Task Execute(IJobExecutionContext context)
        {

            string text = null;
            foreach (var symbol in GlobalValues.symbols)
            {
                try
                {
                    var signal = await _tradeCandlesService.GetRSISignal(symbol, interval, GlobalValues.inPositionSymbols.Contains(symbol));
                    if (signal != null)
                    {
                        if (text != null)
                            text += "\n";

                        text += $"{symbol} {signal}";
                    }
                }
                catch (Exception ex)
                {
                    StaticLogger.LogMessage($"{symbol} simpleRequest {ex.Message}");
                }

            }
            if (text != null)
            {
                string baseUrl = $"https://api.telegram.org/bot{bot1Token}/sendMessage?chat_id={chatId}&text={StaticCounter.counter}) {text}";
                StaticCounter.counter++;
                var client = new HttpClient();
                try
                {
                    await client.GetAsync(baseUrl);
                }
                catch (Exception ex)
                {
                    StaticLogger.LogMessage($"bot1 request {ex.Message}");
                }
            }

            string text2 = null;
            foreach (var symbol in GlobalValues.inPositionSymbols)
            {
                try
                {
                    var signal = await _tradeCandlesService.GetInPositionRSISignal(symbol, interval);
                    if (signal != null)
                    {
                        if (text2 != null)
                            text2 += "\n";

                        text2 += $"{symbol} {signal}";
                    }
                }
                catch (Exception ex)
                {
                    StaticLogger.LogMessage($"{symbol} inPositionRequest {ex.Message}");
                }
            }
            if (text2 != null)
            {
                string baseUrl = $"https://api.telegram.org/bot{bot2Token}/sendMessage?chat_id={chatId}&text={StaticCounter.counter2}) Пора продавать {text2}";
                StaticCounter.counter2++;
                var client = new HttpClient();
                try
                {
                    await client.GetAsync(baseUrl);
                }
                catch (Exception ex)
                {
                    StaticLogger.LogMessage($"bot2 request {ex.Message}");
                }
            }

        }
    }
}
