using Quartz;
using QuartzScheduler.Base;
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
    public class NoLeverageSellJob : IJob
    {
        private readonly IRSITradeCandlesService _tradeCandlesService;
        private readonly ITelegramApiClient _telegramApiClient;

        readonly string interval = "1h";

        public NoLeverageSellJob(IRSITradeCandlesService tradeCandlesService,
            ITelegramApiClient telegramApiClient)
        {
            _tradeCandlesService = tradeCandlesService;
            _telegramApiClient = telegramApiClient;
        }
        public async Task Execute(IJobExecutionContext context)
        {
            string text = null;
            foreach (var symbol in NoLeverageGlobalValues.inPositionSymbols)
            {
                try
                {
                    var signal = await _tradeCandlesService.GetInPositionRSISignal(symbol, interval, null);
                    if (signal != null)
                    {
                        if (!string.IsNullOrEmpty(text))
                            text += "\n";

                        text += $"{symbol} {signal.Text}";

                        if (signal.ShouldClosePosition)
                        {
                            text += " нужно закрывать";
                        }
                    }
                }
                catch (Exception ex)
                {
                    StaticLogger.LogMessage($"NoLeverageSellJob {symbol} {ex.Message}");
                }
            }
            if (text != null)
            {
                try
                {
                    await _telegramApiClient.SendMessage(TelegramApiBots.NoLeverageSellBot, text);
                }
                catch (Exception ex)
                {
                    StaticLogger.LogMessage($"NoLeverageSellJob SellBot request {ex.Message}");
                }
            }
        }
    }
}
