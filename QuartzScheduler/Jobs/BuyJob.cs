using Quartz;
using TradingDataLibrary.Logging;
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

        readonly string interval = "1h";

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
            var positions = new Positions();
            try
            {
                positions = await _positionsApiClient.GetAllPositions();
            }
            catch (Exception ex)
            {
                StaticLogger.LogMessage($"BuyJob take positions {ex.Message}");
                throw;
            }

            var allPositions = positions.PositionsList.Select(x => x.symbol).ToList();
            var longPositions = positions.PositionsList.Where(x => x.IsLong()).ToList();
            string text = null;
            string openPositionText = null;

            foreach (var instrument in GlobalValues.Instruments)
            {
                try
                {
                    var signal = await _tradeCandlesService.GetRSISignal(instrument.Symbol, interval, positions.PositionsList.Where(x => x.symbol == instrument.Symbol).ToList());
                    if (signal != null)
                    {
                        if (text != null)
                            text += "\n";

                        text += $"{instrument.Symbol} {signal.Text}";

                        if (openPositionText != null && signal.IsNotify)
                            openPositionText += "\n";

                        if (signal.IsNotify && !positions.IsInMemory)
                        {
                            if (signal.IsLong)
                            {
                                var result = await _positionsApiClient.OpenPosition(instrument, true);
                                if (result)
                                    openPositionText += $"{instrument.Symbol} открыта long";
                                else
                                    openPositionText += $"{instrument.Symbol} пора открывать long";
                            }
                            else
                            {
                                var result = await _positionsApiClient.OpenPosition(instrument, false);
                                if (result)
                                    openPositionText += $"{instrument.Symbol} открыта short";
                                else
                                    openPositionText += $"{instrument.Symbol} пора открывать short";
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    StaticLogger.LogMessage($"BuyJob {instrument.Symbol} {ex.Message}");
                }
            }

            if (openPositionText != null)
            {
                try
                {
                    var responseMessage = await _telegramApiClient.SendMessage(TelegramApiBot.BuyBot, openPositionText);
                    if(!responseMessage.IsSuccessStatusCode)
                        StaticLogger.LogMessage($"BuyJob BuyBot request {responseMessage.Content}");
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
                    var responseMessage = await _telegramApiClient.SendMessage(TelegramApiBot.InfoBot, text);
                    if (!responseMessage.IsSuccessStatusCode)
                        StaticLogger.LogMessage($"BuyJob InfoBot request {responseMessage.Content}");
                }
                catch (Exception ex)
                {
                    StaticLogger.LogMessage($"BuyJob InfoBot request {ex.Message}");
                }
            }
        }
    }
}
