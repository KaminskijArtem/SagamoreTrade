﻿using Quartz;
using QuartzScheduler.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using TelegramApiLibrary;
using TelegramApiLibrary.Interfaces;
using TradingDataLibrary.ApiClient;
using TradingDataLibrary.Interfaces;

namespace QuartzScheduler.Jobs
{
    public class SellJob : IJob
    {
        private readonly IRSITradeCandlesService _tradeCandlesService;
        private readonly ITelegramApiClient _telegramApiClient;
        private readonly IPositionsApiClient _positionsApiClient;

        readonly string interval = "30m";

        public SellJob(IRSITradeCandlesService tradeCandlesService,
            ITelegramApiClient telegramApiClient,
            IPositionsApiClient positionsApiClient)
        {
            _tradeCandlesService = tradeCandlesService;
            _telegramApiClient = telegramApiClient;
            _positionsApiClient = positionsApiClient;
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
                try
                {
                    await _telegramApiClient.SendMessage(TelegramApiBots.NotifyBot, text);
                }
                catch (Exception ex)
                {
                    StaticLogger.LogMessage($"SellJob bot2 request {ex.Message}");
                }
            }
        }
    }
}