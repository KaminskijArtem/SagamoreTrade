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

            var bb = new BollingerBand(20, 2);
            List<Ohlc> ohlcList = candles.Select(x =>
            new Ohlc()
            {
                Date = x.OpenTime.UtcDateTime,
                Open = (double)x.Open,
                Close = (double)x.Close,
                High = (double)x.High,
                Low = (double)x.Low,
                Volume = x.Volume
            }).ToList();
            // fill ohlcList
            bb.Load(ohlcList);
            var serie = bb.Calculate();
            var outOfBBSignal100 = CalculateCountOutOfBB(candles, serie, 100);
            var outOfBBSignal200 = CalculateCountOutOfBB(candles, serie, 200);
            var outOfBBSignal300 = CalculateCountOutOfBB(candles, serie, 300);

            var adx = new ADX(14);
            adx.Load(ohlcList);
            var adxSerie = adx.Calculate();
            var adxVal = Math.Round(adxSerie.ADX.Last().Value, 2);

            var rsiList = Calculate(candles);
            var rsi = decimal.Round(rsiList.Last().Value, 2);
            var rsiPrev = decimal.Round(rsiList.Take(rsiList.Count() - 1).Last().Value, 2);
            var rsiPrevPrev = decimal.Round(rsiList.Take(rsiList.Count() - 2).Last().Value, 2);

            if ((rsi < 32 && rsi > rsiPrev && rsiPrev > rsiPrevPrev) || (rsi > 68 && rsi < rsiPrev && rsiPrev < rsiPrevPrev) || isInposition)
                return $"{rsi}% ({rsiPrev}% {rsiPrevPrev}%) 100:{outOfBBSignal100} 200:{outOfBBSignal200} 300:{outOfBBSignal300} adx:{adxVal}";

            return null;
        }

        private string CalculateCountOutOfBB(List<Candle> candles, BollingerBandSerie serie, int count)
        {
            var upCount = 0;
            var downCount = 0;
            for (var i = candles.Count() - 1; i > candles.Count() - count; i--)
            {
                if ((double)candles[i].Close > serie.UpperBand[i].Value)
                    upCount++;
                if ((double)candles[i].Close < serie.LowerBand[i].Value)
                    downCount++;
            }
            return $"{upCount}↑{downCount}↓";
        }

        public async Task<string> GetInPositionRSISignal(string symbol, string interval)
        {
            var candles = await _candlesApiClient.GetCandles(symbol, interval);
            var rsiList = Calculate(candles);
            var rsi = rsiList.Last().Value;
            if (rsi > 45 && rsi < 55)
                return $"{decimal.Round(rsi, 2)}%";

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
