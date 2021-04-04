using Microsoft.Extensions.Configuration;
using Quartz;
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
    public class SellJob : IJob
    {
        private readonly IRSITradeCandlesService _tradeCandlesService;
        private readonly IConfiguration _configuration;
        private readonly IPositionsApiClient _positionsApiClient;

        readonly string interval = "30m";
        private string bot2Token;
        private string chatId;

        public SellJob(IRSITradeCandlesService tradeCandlesService,
            IConfiguration configuration,
            IPositionsApiClient positionsApiClient)
        {
            _tradeCandlesService = tradeCandlesService;
            _configuration = configuration;
            _positionsApiClient = positionsApiClient;

            bot2Token = _configuration["TelegramConfiguration:Bot2Token"];
            chatId = _configuration["TelegramConfiguration:ChatId"];
        }
        public async Task Execute(IJobExecutionContext context)
        {
            var positions = await _positionsApiClient.GetAllPositions();
            var longPositions = positions.Where(x => x.IsLong()).ToList();

            string text = null;
            foreach (var position in positions)
            {
                try
                {
                    var signal = await _tradeCandlesService.GetInPositionRSISignal(position.symbol, interval, position);
                    if (signal != null)
                    {
                        if (!string.IsNullOrEmpty(text))
                            text += "\n";

                        text += $"{position.symbol} {signal.Text}";

                        if (signal.ShouldClosePosition)
                        {
                            var result = await _positionsApiClient.ClosePosition(position.id);
                            if (result)
                                text += " закрыта";
                        }
                    }
                }
                catch (Exception ex)
                {
                    StaticLogger.LogMessage($"SellJob {position.symbol} {ex.Message}");
                }
            }
            if (text != null)
            {
                string baseUrl = $"https://api.telegram.org/bot{bot2Token}/sendMessage?chat_id={chatId}&text={StaticCounter.counter2}) {text}";
                StaticCounter.counter2++;
                var client = new HttpClient();
                try
                {
                    await client.GetAsync(baseUrl);
                }
                catch (Exception ex)
                {
                    StaticLogger.LogMessage($"SellJob bot2 request {ex.Message}");
                }
            }
        }
    }
}
