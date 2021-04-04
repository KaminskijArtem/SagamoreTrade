using Microsoft.Extensions.Configuration;
using Quartz;
using QuartzScheduler.Base;
using QuartzScheduler.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TradingDataLibrary.ApiClient;
using TradingDataLibrary.Interfaces;

namespace QuartzScheduler.Jobs
{
    public class BuyJob : IJob
    {
        private readonly IRSITradeCandlesService _tradeCandlesService;
        private readonly IConfiguration _configuration;
        private readonly IPositionsApiClient _positionsApiClient;

        readonly string interval = "30m";
        private string bot1Token;
        private string bot2Token;
        private string chatId;

        public BuyJob(IRSITradeCandlesService tradeCandlesService,
            IConfiguration configuration,
            IPositionsApiClient positionsApiClient)
        {
            _tradeCandlesService = tradeCandlesService;
            _configuration = configuration;
            _positionsApiClient = positionsApiClient;

            bot1Token = _configuration["TelegramConfiguration:Bot1Token"];
            bot2Token = _configuration["TelegramConfiguration:Bot2Token"];
            chatId = _configuration["TelegramConfiguration:ChatId"];
        }
        public async Task Execute(IJobExecutionContext context)
        {
            var positions = await _positionsApiClient.GetAllPositions();
            var allPositions = positions.Select(x => x.symbol).ToList();
            var longPositions = positions.Where(x => x.IsLong()).ToList();

            string text = null;
            string openPositionText = null;

            foreach (var symbol in GlobalValues.symbols)
            {
                try
                {
                    var signal = await _tradeCandlesService.GetRSISignal(symbol, interval, allPositions.Count(x => x == symbol));
                    if (signal != null)
                    {
                        if (text != null)
                            text += "\n";

                        if (openPositionText != null)
                            openPositionText += "\n";

                        text += $"{symbol} {signal.Text}";

                        if (signal.IsNotify)
                            openPositionText += $"{symbol} пора открывать";
                    }
                }
                catch (Exception ex)
                {
                    StaticLogger.LogMessage($"BuyJob {symbol} {ex.Message}");
                }

            }

            if (openPositionText != null)
            {
                string baseUrl = $"https://api.telegram.org/bot{bot2Token}/sendMessage?chat_id={chatId}&text={StaticCounter.counter2}) {openPositionText}";
                StaticCounter.counter2++;
                var client = new HttpClient();
                try
                {
                    await client.GetAsync(baseUrl);
                }
                catch (Exception ex)
                {
                    StaticLogger.LogMessage($"BuyJob bot2 request {ex.Message}");
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
                    StaticLogger.LogMessage($"BuyJob bot1 request {ex.Message}");
                }
            }
        }
    }
}
