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

            var adx = new ADX(14);
            List<Ohlc> ohlcList = candles.Select(x => 
            new Ohlc() {
                Date = x.OpenTime.UtcDateTime,
                Open = (double)x.Open,
                Close = (double)x.Close,
                High = (double)x.High,
                Low = (double)x.Low,
                Volume = x.Volume
                }).ToList();
            // fill ohlcList
            adx.Load(ohlcList);
            var serie = adx.Calculate();
            var adxVal = Math.Round(serie.ADX.Last().Value,2);
            var diffVal = Math.Round((serie.DIPositive.Last()-serie.DINegative.Last()).Value,2);

            var rsi = Calculate(candles);
            if (rsi < 35 || rsi > 65 || isInposition)
                return $"{decimal.Round(rsi, 2)}% ADX:{adxVal} diff:{diffVal}";

            return null;
        }

        public async Task<string> GetInPositionRSISignal(string symbol, string interval)
        {
            var candles = await _candlesApiClient.GetCandles(symbol, interval);
            var rsi = Calculate(candles);
            if (rsi > 70)
                return $"{decimal.Round(rsi, 2)}%";

            return null;
        }

        public decimal Calculate(List<Candle> candles)
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

            return RSI.Last().Value;
        }


    }
}
