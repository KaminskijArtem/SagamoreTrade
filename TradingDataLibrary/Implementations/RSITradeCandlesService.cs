using NetTrader.Indicator;
using System;
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
        public async Task<RSISignalModel> GetRSISignal(string symbol, string interval, List<Position> positions)
        {
            var candles = await _candlesApiClient.GetCandles(symbol, interval);

            var rsiList = CalculateRSI(candles);
            var rsi = Math.Round(rsiList.Last().Value, 2);
            var prevRsi = rsiList[^2].Value;
            var prevPrevRsi = rsiList[^3].Value;

            var isWasOverBuy = prevRsi > 70 || prevPrevRsi > 70;
            var isWasOverSold = prevRsi < 30 || prevPrevRsi < 30;

            var signal = new RSISignalModel();
            
            if (positions.Count == 1 && (rsi < 20 || rsi > 80))
            {
                signal.Text += "%E2%9D%A4%E2%9D%A4";
                signal.IsNotify = true;
                signal.IsLong = rsi < 20;
                signal.Text += $"{rsi}%";
                return signal;
            }
            if (positions.Count == 2 && (rsi < 10 || rsi > 90))
            {
                signal.Text += "%E2%9D%A4%E2%9D%A4%E2%9D%A4";
                signal.IsNotify = true;
                signal.IsLong = rsi < 10;
                signal.Text += $"{rsi}%";
                return signal;
            }

            var position = positions.FirstOrDefault();

            if (position == null && !isWasOverBuy && !isWasOverSold)
                return null;

            if (position != null && position.IsLong())
                signal.Text += $"long ";

            if (position != null && !position.IsLong())
                signal.Text += $"short ";

            if (position == null && (isWasOverSold || isWasOverBuy))
            {
                var isShouldOpen = (isWasOverSold && rsi > 30 && rsi < 35) || (isWasOverBuy && rsi < 70 && rsi > 65);

                if (isShouldOpen)
                {
                    signal.Text += "%E2%9D%A4";
                    signal.IsNotify = true;
                    signal.IsLong = isWasOverSold;
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
                    Text = $"{Math.Round(rsi, 2)}%",
                    ShouldClosePosition = true
                };
                return outputModel;
            }

            return null;
        }
        private List<double?> CalculateRSI(List<Candle> candles)
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

            var RSI = new RSI(14);
            RSI.Load(ohlcList);
            var rsiSerie = RSI.Calculate();
            return rsiSerie.RSI;

        }
    }
}
