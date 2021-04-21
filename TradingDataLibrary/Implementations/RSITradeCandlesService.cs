using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradingDataLibrary.ApiClient;
using TradingDataLibrary.Interfaces;
using TradingDataLibrary.Models;

namespace TradingDataLibrary.Implementations
{
    public class RSITradeCandlesService : IRSITradeCandlesService
    {
        private readonly ICandlesApiClient _candlesApiClient;
        private readonly int N = 14;
        public RSITradeCandlesService(ICandlesApiClient candlesApiClient)
        {
            _candlesApiClient = candlesApiClient;
        }
        public async Task<RSISignalModel> GetRSISignal(string symbol, string interval, int positionsCount)
        {
            var candles = await _candlesApiClient.GetCandles(symbol, interval);
            var rsiList = CalculateRSI(candles);
            var rsi = decimal.Round(rsiList.Last().Value, 2);
            var signal = new RSISignalModel();

            if (rsi < 30 || positionsCount > 0)
            {
                if (positionsCount > 0)
                    signal.Text += $"- (позиций:{positionsCount}) ";

                var rsiPeaksCount = CalculateRSIPeaksCount(rsiList);
                if (rsiPeaksCount > 0)
                    signal.Text += $"(пиков rsi:{rsiPeaksCount}) ";

                var rsiPeaksHistory = GetRSIPeaksHistory(rsiList);
                signal.Text += $"(история rsi:{rsiPeaksHistory}) ";

                if (positionsCount == 0 && rsiPeaksCount > 1 && rsi < 30)
                {
                    signal.Text += "%E2%9D%A4";
                    signal.IsNotify = true;
                }

                var rsiPrev = decimal.Round(rsiList.Take(rsiList.Count() - 1).Last().Value, 2);
                var rsiPrevPrev = decimal.Round(rsiList.Take(rsiList.Count() - 2).Last().Value, 2);
                signal.Text += $"{rsi}% ({rsiPrev}% {rsiPrevPrev}%)%";
                return signal;
            }

            return null;
        }

        private string GetRSIPeaksHistory(List<decimal?> rsiList)
        {
            var result = "";
            var rsiUp = 0;
            var rsiDown = 0;

            for (var i = rsiList.Count - 1; rsiList[i] != null; i--)
            {
                if (rsiList[i] < 30 && rsiList.Take(i).TakeLast(5).All(x => x > 30))
                {
                    if (rsiUp != 0)
                        result = result.Insert(0, $"{rsiUp}↑");
                    rsiDown++;
                    rsiUp = 0;
                }
                if (rsiList[i] > 70 && rsiList.Take(i).TakeLast(5).All(x => x < 70))
                {
                    if (rsiDown != 0)
                        result = result.Insert(0, $"{rsiDown}↓");
                    rsiUp++;
                    rsiDown = 0;
                }
            }

            return result;
        }

        public async Task<InPositionRSISignalModel> GetInPositionRSISignal(string symbol, string interval, Position position)
        {
            var candles = await _candlesApiClient.GetCandles(symbol, interval);
            var rsiList = CalculateRSI(candles);
            var rsi = rsiList.Last().Value;

            if (rsi > 70 && candles.Last().Close > position.openPrice)
            {
                var outputModel = new InPositionRSISignalModel
                {
                    Text = $"{decimal.Round(rsi, 2)}%",
                    ShouldClosePosition = true
                };
                return outputModel;
            }
            else if (rsi > 70)
            {
                var outputModel = new InPositionRSISignalModel
                {
                    Text = $"{decimal.Round(rsi, 2)}% может стоит закрыть?",
                    ShouldClosePosition = false
                };
                return outputModel;
            }

            return null;
        }
        private List<decimal?> CalculateRSI(List<Candle> candles)
        {
            // Add null values for first item, iteration will start from second item of OhlcList
            var RS = new List<decimal?>();
            var RSI = new List<decimal?>();

            RS.Add(null);
            RSI.Add(null);

            decimal gainSum = 0;
            decimal lossSum = 0;
            for (int i = 1; i < N; i++)
            {
                decimal thisChange = candles[i].Close - candles[i - 1].Close;
                if (thisChange > 0)
                {
                    gainSum += thisChange;
                }
                else
                {
                    lossSum += (-1) * thisChange;
                }
                RS.Add(null);
                RSI.Add(null);
            }

            var averageGain = gainSum / N;
            var averageLoss = lossSum / N;
            var rs = averageGain / averageLoss;
            RS.Add(rs);
            var rsi = 100 - (100 / (1 + rs));
            RSI.Add(rsi);

            for (int i = N + 1; i < candles.Count; i++)
            {
                decimal thisChange = candles[i].Close - candles[i - 1].Close;
                if (thisChange > 0)
                {
                    averageGain = (averageGain * (N - 1) + thisChange) / N;
                    averageLoss = (averageLoss * (N - 1)) / N;
                }
                else
                {
                    averageGain = (averageGain * (N - 1)) / N;
                    averageLoss = (averageLoss * (N - 1) + (-1) * thisChange) / N;
                }
                rs = averageGain / averageLoss;
                RS.Add(rs);
                rsi = 100 - (100 / (1 + rs));
                RSI.Add(rsi);
            }

            return RSI;
        }
        private int CalculateRSIPeaksCount(List<decimal?> rsiList)
        {
            var result = 0;
            if (rsiList.Last() < 30)
            {
                for (var i = rsiList.Count - 1; rsiList[i] < 70; i--)
                {
                    if (rsiList[i] < 30 && rsiList.Take(i).TakeLast(5).All(x => x > 30))
                        result++;
                }
            }
            return result;
        }
    }
}
