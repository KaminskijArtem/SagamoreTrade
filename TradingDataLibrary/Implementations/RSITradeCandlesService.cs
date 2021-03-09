using NetTrader.Indicator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        public async Task<string> GetRSISignal(string symbol, string interval, bool isInposition)
        {
            var candles = await _candlesApiClient.GetCandles(symbol, interval);

            var emaSerie = GetEmaSerie(candles);
            var emaVal = emaSerie.Values.Last().Value;
            var emaDiff = Math.Round(Math.Abs(emaVal - (double)candles.Last().Close) * 100 / (double)candles.Last().Close, 2);

            var rsiList = Calculate(candles);
            var rsi = decimal.Round(rsiList.Last().Value, 2);
            var rsiPrev = decimal.Round(rsiList.Take(rsiList.Count() - 1).Last().Value, 2);
            var rsiPrevPrev = decimal.Round(rsiList.Take(rsiList.Count() - 2).Last().Value, 2);

            var topEmaDiffs = GetTopEmaDiffs(emaSerie, candles);

            if (rsi < 32 || rsi > 68 || isInposition)
                return $"{rsi}% ({rsiPrev}% {rsiPrevPrev}%) emaDiff:{emaDiff}% ({topEmaDiffs})";

            return null;
        }

        private SingleDoubleSerie GetEmaSerie(List<Candle> candles)
        {
            var ohlcList = candles.Select(x =>
            new Ohlc()
            {
                Date = x.OpenTime.UtcDateTime,
                Open = (double)x.Open,
                Close = (double)x.Close,
                High = (double)x.High,
                Low = (double)x.Low,
                Volume = x.Volume
            }).ToList();

            var ema = new EMA(200, false);
            ema.Load(ohlcList);
            return ema.Calculate();
        }

        private string GetTopEmaDiffs(SingleDoubleSerie emaSerie, List<Candle> candles)
        {
            var emaDiffs = new List<decimal>();
            for (var i = candles.Count() - 1; emaSerie.Values[i].HasValue; i--)
            {
                emaDiffs.Add(Math.Round(Math.Abs((decimal)emaSerie.Values[i].Value - candles[i].Close) * 100 / candles[i].Close, 1));
            }
            emaDiffs.Sort();
            emaDiffs.Reverse();
            var top1 = emaDiffs.First();
            var top76 = emaDiffs.Skip(75).First();
            var top151 = emaDiffs.Skip(150).First();

            return $"{top1}|{top76}|{top151}";
        }

        public async Task<InPositionRSISignalModel> GetInPositionRSISignal(string symbol, string interval, bool isLong)
        {
            var candles = await _candlesApiClient.GetCandles(symbol, interval);
            var rsiList = Calculate(candles);
            var rsi = rsiList.Last().Value;
            var outputModel = new InPositionRSISignalModel { Text = "", ShouldClosePosition = false };

            StaticRsiStorage.Storage.TryGetValue(symbol, out decimal rsiPrev);

            if (rsiPrev != 0)
            {
                if ((isLong && (rsi < rsiPrev)) || (!isLong && rsi > rsiPrev))
                    outputModel.ShouldClosePosition = true;
            }
            StaticRsiStorage.Storage[symbol] = rsi;

            if ((rsi < 50 && !isLong) || (rsi > 50 && isLong))
            { 
                outputModel.Text = $"{decimal.Round(rsi, 2)}%";
                return outputModel;
            }

            return null;
        }

        public List<decimal?> Calculate(List<Candle> candles)
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
