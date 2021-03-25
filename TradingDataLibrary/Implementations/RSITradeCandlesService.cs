﻿using NetTrader.Indicator;
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
        public async Task<RSISignalModel> GetRSISignal(string symbol, string interval, int positionsCount)
        {
            var candles = await _candlesApiClient.GetCandles(symbol, interval);
            var rsiList = Calculate(candles);
            var rsi = decimal.Round(rsiList.Last().Value, 2);
            var signal = new RSISignalModel();

            if (rsi < 30 || rsi > 70 || positionsCount > 0)
            {
                signal.ShouldOpenPosition = true;

                if (positionsCount > 0)
                    signal.Text += $"- (позиций:{positionsCount}) | ";

                var rsiCount = CalculateRSICount(rsiList);
                if (rsiCount > 0)
                    signal.Text += $"(пиков rsi:{rsiCount}) | ";

                var rsiPrev = decimal.Round(rsiList.Take(rsiList.Count() - 1).Last().Value, 2);
                var rsiPrevPrev = decimal.Round(rsiList.Take(rsiList.Count() - 2).Last().Value, 2);
                signal.Text += $"{rsi}% ({rsiPrev}% {rsiPrevPrev}%)%";
                return signal;
            }

            return null;
        }

        private int CalculateRSICount(List<decimal?> rsiList)
        {
            var result = 0;
            if (rsiList.Last() > 70)
            {
                for (var i = rsiList.Count - 1; rsiList[i] > 50; i--)
                {
                    if(rsiList[i] > 70 && rsiList[i-1] < 70)
                        result++;
                }
            }
            else if (rsiList.Last() < 30)
            {
                for (var i = rsiList.Count - 1; rsiList[i] < 50; i--)
                {
                    if(rsiList[i] < 30 && rsiList[i-1] > 30)
                        result++;
                }
            }
            return result;
        }

        public async Task<InPositionRSISignalModel> GetInPositionRSISignal(string symbol, string interval, bool isLong)
        {
            var candles = await _candlesApiClient.GetCandles(symbol, interval);
            var rsiList = Calculate(candles);
            var rsi = rsiList.Last().Value;
            var outputModel = new InPositionRSISignalModel { Text = "", ShouldClosePosition = false };

            if ((rsi < 50 && !isLong) || (rsi > 50 && isLong))
            {
                outputModel.Text = $"{decimal.Round(rsi, 2)}%";
                outputModel.ShouldClosePosition = true;
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
