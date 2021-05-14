using Quartz;
using QuartzScheduler.Base;
using QuartzScheduler.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TelegramApiLibrary;
using TelegramApiLibrary.Interfaces;
using TradingDataLibrary.ApiClient;
using TradingDataLibrary.Interfaces;
using TradingDataLibrary.Models;

namespace QuartzScheduler.Jobs
{
    public class BuyJob : IJob
    {
        private readonly IRSITradeCandlesService _tradeCandlesService;
        private readonly ITelegramApiClient _telegramApiClient;
        private readonly IPositionsApiClient _positionsApiClient;

        readonly string interval = "30m";

        public BuyJob(IRSITradeCandlesService tradeCandlesService,
            ITelegramApiClient telegramApiClient,
            IPositionsApiClient positionsApiClient)
        {
            _tradeCandlesService = tradeCandlesService;
            _telegramApiClient = telegramApiClient;
            _positionsApiClient = positionsApiClient;
        }
        public async Task Execute(IJobExecutionContext context)
        {
            var positions = new List<Position>();
            try
            {
                positions = await _positionsApiClient.GetAllPositions();
            }
            catch (Exception ex)
            {
                StaticLogger.LogMessage($"BuyJob take positions {ex.Message}");
                throw;
            }

            var allPositions = positions.Select(x => x.symbol).ToList();
            var longPositions = positions.Where(x => x.IsLong()).ToList();
            string text = null;
            string openPositionText = null;

            foreach (var symbol in GlobalValues.symbols)
            {
                try
                {
                    var signal = await _tradeCandlesService.GetRSISignal(symbol, interval, allPositions.Any(x => x == symbol));
                    if (signal != null)
                    {
                        if (text != null)
                            text += "\n";

                        text += $"{symbol} {signal.Text}";

                        if (openPositionText != null && signal.IsNotify)
                            openPositionText += "\n";

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
                try
                {
                    await _telegramApiClient.SendMessage(TelegramApiBots.BuyBot, openPositionText);
                }
                catch (Exception ex)
                {
                    StaticLogger.LogMessage($"BuyJob BuyBot request {ex.Message}");
                }
            }

            if (text != null)
            {
                try
                {
                    await _telegramApiClient.SendMessage(TelegramApiBots.InfoBot, text);
                }
                catch (Exception ex)
                {
                    StaticLogger.LogMessage($"BuyJob InfoBot request {ex.Message}");
                }
            }
        }
    }
}
