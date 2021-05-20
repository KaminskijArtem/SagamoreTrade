﻿using NetTrader.Indicator;
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

            var signal = new RSISignalModel();

            if (position == null && !(rsi > 45 && rsi < 55))
                return null;

            if (position != null && position.IsLong())
                signal.Text += $"long ";

            if (position != null && !position.IsLong())
                signal.Text += $"short ";

            if (position == null && rsi > 45 && rsi < 55)
            {
                var ohlcList = candles.Select(x =>
                new Ohlc
                {
                    Open = (double)x.Open,
                    Close = (double)x.Close,
                    High = (double)x.High,
                    Low = (double)x.Low,
                    Volume = x.Volume,
                    Date = x.OpenTime.UtcDateTime

                }).ToList();

                var ADX = new ADX(14);
                ADX.Load(ohlcList);
                var adxSerie = ADX.Calculate();
                var lastADX = adxSerie.ADX.Last();

                var MACD = new MACD(12, 26, 9);
                MACD.Load(ohlcList);
                var macdSerie = MACD.Calculate();
                var lastMACDSignal = macdSerie.MACDHistogram.Last();

                if (lastADX > 20)
                {
                    if (rsi > 50 && lastMACDSignal > 0)
                    {
                        signal.Text += "%E2%9D%A4";
                        signal.IsNotify = true;
                        signal.IsLong = true;
                    }
                    if (rsi < 50 && lastMACDSignal < 0)
                    {
                        signal.Text += "%E2%9D%A4";
                        signal.IsNotify = true;
                        signal.IsLong = false;
                    }
                }
            }

            signal.Text += $"{rsi}%";
            return signal;
        }

        public async Task<InPositionRSISignalModel> GetInPositionRSISignal(string symbol, string interval, Position position)
        {
            var candles = await _candlesApiClient.GetCandles(symbol, interval);
            var rsiList = CalculateRSI(candles);
            var rsi = rsiList.Last().Value;

            if ((rsi > 55 || rsi < 40) && !position.IsLong() || (rsi < 45 || rsi > 60) && position.IsLong())
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
