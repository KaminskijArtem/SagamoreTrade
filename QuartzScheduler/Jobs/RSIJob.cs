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
using TradingDataLibrary.ApiClient;
using System.Linq;
using TradingDataLibrary.Models;

namespace QuartzScheduler.Jobs
{
    public class RSIJob : IJob
    {
        private readonly IRSITradeCandlesService _tradeCandlesService;
        private readonly IConfiguration _configuration;
        private readonly IPositionsApiClient _positionsApiClient;

        readonly string interval = "30m";
        private string bot1Token;
        private string bot2Token;
        private string chatId;

        public RSIJob(IRSITradeCandlesService tradeCandlesService,
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
            var shortPositions = positions.Where(x => !x.IsLong()).ToList();

            string text = null;
            foreach (var symbol in GlobalValues.symbols)
            {
                try
                {
                    var signal = await _tradeCandlesService.GetRSISignal(symbol, interval, allPositions.Contains(symbol));
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
                    StaticLogger.LogMessage($"bot request {ex.Message}");
                }
            }

            SendInPositionSignal(longPositions, true);
            SendInPositionSignal(shortPositions, false);


        }

        private async void SendInPositionSignal(List<Position> positions, bool isLong)
        {
            string text = null;
            foreach (var position in positions)
            {
                try
                {
                    var signal = await _tradeCandlesService.GetInPositionRSISignal(position.symbol, interval, isLong);
                    if (signal != null)
                    {
                        if (text != null)
                            text += "\n";

                        text += $"{position.symbol} {signal.Text}";

                        if(signal.ShouldClosePosition)
                        { 
                            var result = await _positionsApiClient.ClosePosition(position.id);
                            if(result)
                                 text += " закрыта";
                        }
                    }
                }
                catch (Exception ex)
                {
                    StaticLogger.LogMessage($"{position} inPositionRequest {ex.Message}");
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
                    StaticLogger.LogMessage($"bot2 request {ex.Message}");
                }
            }
        }
    }
}
