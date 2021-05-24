using NetTrader.Indicator;
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
        public async Task<RSISignalModel> GetRSISignal(string symbol, string interval, Position position)
        {
            var candles = await _candlesApiClient.GetCandles(symbol, interval);

            var rsiList = CalculateRSI(candles);
            var rsi = decimal.Round(rsiList.Last().Value, 2);
            var prevRsi = rsiList[^2].Value;

            var signal = new RSISignalModel();

            if (position == null && prevRsi > 30 && prevRsi < 70)
                return null;

            if (position != null && position.IsLong())
                signal.Text += $"long ";

            if (position != null && !position.IsLong())
                signal.Text += $"short ";

            if (position == null && (prevRsi < 30 || prevRsi > 70))
            {
                var isShouldOpen = (prevRsi < 30 && rsi > 30.5m) || (prevRsi > 70 && rsi < 69.5m);

                if (isShouldOpen)
                {
                    signal.Text += "%E2%9D%A4";
                    signal.IsNotify = true;
                    signal.IsLong = prevRsi < 30;
                }
            }

            signal.Text += $"{rsi}%";
            return signal;
        }

        private List<int> CalculateRSIPeaks(List<decimal?> rsiList)
        {
            var result = new List<int>();
            var topRsiCount = 0;
            var lowRsiCount = 0;

            for (var i = 0; i < rsiList.Count(); i++)
            {
                if (rsiList[i] != null && rsiList[i] > 48 && rsiList[i] < 52)
                {
                    var topRsiIndex = rsiList.GetRange(i, rsiList.Count() - i).FindIndex(x => x > 70);
                    var lowRsiIndex = rsiList.GetRange(i, rsiList.Count() - i).FindIndex(x => x < 30);

                    if (lowRsiIndex == -1 && topRsiIndex == -1)
                    {
                        if (lowRsiCount != 0)
                        {
                            result.Add(lowRsiCount);
                            lowRsiCount = 0;
                        }
                        if (topRsiCount != 0)
                        {
                            result.Add(topRsiCount);
                            topRsiCount = 0;
                        }
                        return result;
                    }

                    if ((topRsiIndex != -1 && topRsiIndex < lowRsiIndex) || (lowRsiIndex == -1 && topRsiIndex > 0))
                    {
                        topRsiCount++;
                        if (lowRsiCount != 0)
                        {
                            result.Add(lowRsiCount);
                            lowRsiCount = 0;
                        }
                        i += topRsiIndex;
                    }
                    if ((lowRsiIndex != -1 && lowRsiIndex < topRsiIndex) || (topRsiIndex == -1 && lowRsiIndex > 0))
                    {
                        lowRsiCount++;
                        if (topRsiCount != 0)
                        {
                            result.Add(topRsiCount);
                            topRsiCount = 0;
                        }
                        i += lowRsiIndex;
                    }
                }
            }

            if (lowRsiCount != 0)
            {
                result.Add(lowRsiCount);
                lowRsiCount = 0;
            }
            if (topRsiCount != 0)
            {
                result.Add(topRsiCount);
                topRsiCount = 0;
            }
            return result;
        }

        public async Task<InPositionRSISignalModel> GetInPositionRSISignal(string symbol, string interval, Position position)
        {
            var candles = await _candlesApiClient.GetCandles(symbol, interval);
            var rsiList = CalculateRSI(candles);
            var rsi = rsiList.Last().Value;

            if ((rsi > 50 && position.IsLong()) || (rsi < 50 && !position.IsLong()))
            {
                var outputModel = new InPositionRSISignalModel
                {
                    Text = $"{decimal.Round(rsi, 2)}%",
                    ShouldClosePosition = true
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
    }
}
