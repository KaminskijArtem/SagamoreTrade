using Quartz;
using System;
using System.Threading.Tasks;
using TelegramApiLibrary;
using TelegramApiLibrary.Interfaces;
using TradingDataLibrary.ApiClient;
using TradingDataLibrary.Interfaces;
using TradingDataLibrary.Logging;
using TradingDataLibrary.Models;
using System.Linq;

namespace QuartzScheduler.Jobs
{
    public class SellJob : IJob
    {
        private readonly IRSITradeCandlesService _tradeCandlesService;
        private readonly ITelegramApiClient _telegramApiClient;
        private readonly IPositionsApiClient _positionsApiClient;

        readonly string interval = "1h";

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
            Positions positions;
            try
            {
                positions = await _positionsApiClient.GetAllPositions();
            }
            catch (Exception ex)
            {
                StaticLogger.LogMessage($"SellJob take positions {ex.Message}");
                throw;
            }

            string text = null;
            foreach (var position in positions.PositionsList)
            {
                try
                {
                    var signal = await _tradeCandlesService
                        .GetInPositionRSISignal(position.symbol, interval, position, positions.PositionsList.Where(x => x.symbol == position.symbol));
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
                        else
                            text += " может стоит закрыть?";
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
                    await _telegramApiClient.SendMessage(TelegramApiBots.SellBot, text);
                }
                catch (Exception ex)
                {
                    StaticLogger.LogMessage($"SellJob SellBot request {ex.Message}");
                }
            }
        }
    }
}
