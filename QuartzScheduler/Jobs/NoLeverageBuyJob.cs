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
    public class NoLeverageBuyJob : IJob
    {
        private readonly IRSITradeCandlesService _tradeCandlesService;
        private readonly ITelegramApiClient _telegramApiClient;

        readonly string interval = "1h";

        public NoLeverageBuyJob(IRSITradeCandlesService tradeCandlesService,
            ITelegramApiClient telegramApiClient)
        {
            _tradeCandlesService = tradeCandlesService;
            _telegramApiClient = telegramApiClient;
        }
        public async Task Execute(IJobExecutionContext context)
        {
            string text = null;
            string openPositionText = null;

            foreach (var symbol in NoLeverageGlobalValues.symbols)
            {
                try
                {
                    var signal = await _tradeCandlesService.GetRSISignal(symbol, interval, 0);
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
                    await _telegramApiClient.SendMessage(TelegramApiBots.NoLeverageBuyBot, openPositionText);
                }
                catch (Exception ex)
                {
                    StaticLogger.LogMessage($"BuyJob NoLeverageBuyBot request {ex.Message}");
                }
            }

            if (text != null)
            {
                try
                {
                    await _telegramApiClient.SendMessage(TelegramApiBots.NoLeverageInfoBot, text);
                }
                catch (Exception ex)
                {
                    StaticLogger.LogMessage($"BuyJob NoLeverageInfoBot request {ex.Message}");
                }
            }
        }
    }
}
