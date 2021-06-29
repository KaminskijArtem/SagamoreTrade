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
            var lastCandle = candles.Last();

            var rsiSignal = GetRSISignalByCandles(positions, candles);

            if (rsiSignal != null && rsiSignal.IsNotify)
            {
                var ema = await GetEMA200(symbol, "1d");

                bool isWithGlobalTrend;
                if (rsiSignal.IsLong)
                    isWithGlobalTrend = lastCandle.Close > ema;
                else
                    isWithGlobalTrend = lastCandle.Close < ema;

                if (!isWithGlobalTrend)
                    rsiSignal.IsNotify = false;

            }

            return rsiSignal;
        }

        private RSISignalModel GetRSISignalByCandles(List<Position> positions, List<Candle> candles)
        {
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

        public async Task<InPositionRSISignalModel> GetInPositionRSISignal(string symbol, string interval, Position position, IEnumerable<Position> allSymbolPositions)
        {
            var candles = await _candlesApiClient.GetCandles(symbol, interval);
            var instrument = GlobalValues.Instruments.Where(x => x.Symbol == symbol).First();
            return GetInPositionRSISignalByCandles(position, allSymbolPositions, candles, instrument.Strategy);
        }

        private InPositionRSISignalModel GetInPositionRSISignalByCandles(Position position, IEnumerable<Position> allSymbolPositions, List<Candle> candles, Strategy strategy)
        {
            var rsiList = CalculateRSI(candles);
            var rsi = rsiList.Last().Value;
            var currentPrice = candles.Last().Close;
            var isShouldClose = false;
            var isNotify = false;

            switch (strategy)
            {
                case Strategy.Agressive:
                    {
                        isShouldClose = (rsi > 50 && position.IsLong()) || (rsi < 50 && !position.IsLong());
                        break;
                    }
                case Strategy.Peaceful:
                    {
                        isShouldClose = (rsi > 50 && position.IsLong() && allSymbolPositions.All(x => x.openPrice < currentPrice))
                        || (rsi < 50 && !position.IsLong() && allSymbolPositions.All(x => x.openPrice > currentPrice));

                        isNotify = (rsi > 70 && position.IsLong()) || (rsi < 30 && !position.IsLong());

                        break;
                    }
                default: break;
            }

            if (isShouldClose || isNotify)
            {
                var outputModel = new InPositionRSISignalModel
                {
                    Text = $"{Math.Round(rsi, 2)}%",
                    ShouldClosePosition = isShouldClose
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

        public async Task<StrategyInformationModel> GetStrategyInformation()
        {
            var result = new StrategyInformationModel
            {
                DealResults = new Dictionary<string, List<(decimal DealResult, DateTimeOffset OpenDate, DateTimeOffset CloseDate)>>()
            };
            foreach (var instrument in GlobalValues.Instruments)
                await AddResultsBySymbol(result, instrument.Symbol, 12, Strategy.Agressive);

            var btcSum = result.DealResults["BTC/USD"].Sum(x => x.DealResult);
            var ethSum = result.DealResults["ETH/USD"].Sum(x => x.DealResult);
            var us30Sum = result.DealResults["US30"].Sum(x => x.DealResult);
            var us100Sum = result.DealResults["US100"].Sum(x => x.DealResult);
            var us500Sum = result.DealResults["US500"].Sum(x => x.DealResult);
            var GoldSum = result.DealResults["Gold"].Sum(x => x.DealResult);

            var btcLast = result.DealResults["BTC/USD"].Last();
            var ethLast = result.DealResults["ETH/USD"].Last();
            var us30Last = result.DealResults["US30"].Last();
            var us100Lst = result.DealResults["US100"].Last();
            var us500Last = result.DealResults["US500"].Last();
            var GoldLast = result.DealResults["Gold"].Last();

            return result;
        }

        private async Task AddResultsBySymbol(StrategyInformationModel result, string symbol, int monthCount, Strategy strategy)
        {
            Console.WriteLine(symbol);
            result.DealResults[symbol] = new List<(decimal DealResult, DateTimeOffset OpenDate, DateTimeOffset CloseDate)>();

            var interval = "1h";

            var candles = await GetHistoricalData(symbol, interval, monthCount);

            var openPositions = new List<Position>();

            for (int i = 0; i < candles.Count; i++)
            {
                var item = candles[i];
                if ((DateTimeOffset.UtcNow - item.OpenTime) < TimeSpan.FromDays(365))
                {
                    var last20Candles = candles.Skip(i - 19).Take(20).ToList();

                    var rsiSignal = GetRSISignalByCandles(openPositions, last20Candles);
                    if (rsiSignal != null && rsiSignal.IsNotify)
                    {
                        var position = new Position
                        {
                            symbol = symbol,
                            openPrice = item.Close,
                            openTimestamp = item.OpenTime.ToUnixTimeMilliseconds()
                        };

                        if (rsiSignal.IsLong)
                            position.openQuantity = 1;
                        else
                            position.openQuantity = -1;

                        var ema = await GetEMA200(symbol, "1d", position.openTimestamp);

                        bool isWithGlobalTrend;
                        if (rsiSignal.IsLong)
                            isWithGlobalTrend = item.Close > ema;
                        else
                            isWithGlobalTrend = item.Close < ema;

                        if (isWithGlobalTrend)
                            openPositions.Add(position);
                    }

                    var isClearPositions = false;
                    foreach (var position in openPositions)
                    {
                        var inPositionSygnal = GetInPositionRSISignalByCandles(position, openPositions, last20Candles, strategy);
                        if (inPositionSygnal != null && inPositionSygnal.ShouldClosePosition)
                        {
                            isClearPositions = true;
                            var resultPercent = Math.Round(position.openQuantity * (item.Close - position.openPrice) / position.openPrice * 100, 2);
                            var dealResult = (resultPercent, position.GetOpenTimestamp(), item.OpenTime);
                            result.DealResults[symbol].Add(dealResult);
                        }
                    }
                    if (isClearPositions)
                        openPositions.Clear();
                }
            }
        }

        private async Task<decimal> GetEMA200(string symbol, string timeFrame, long? openTimestamp = null)
        {
            var candles = await _candlesApiClient.GetCandles(symbol, timeFrame, null, openTimestamp);

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

            var ema = new EMA(200, true);
            ema.Load(ohlcList);
            var emaSerie = ema.Calculate();
            return (decimal)emaSerie.Values.Last().Value;
        }

        private async Task<List<Candle>> GetHistoricalData(string symbol, string interval, int monthCount)
        {
            var candles = new List<Candle>();

            for (int i = 0; i < monthCount + 1; i++)
            {
                var now = DateTime.UtcNow;

                if (i == 0)
                {
                    var startDate = new DateTimeOffset(new DateTime(now.Year, now.Month, 1)).ToUnixTimeMilliseconds();
                    var endDate = new DateTimeOffset(now).ToUnixTimeMilliseconds();
                    var monthCandles = await _candlesApiClient.GetCandles(symbol, interval, startDate, endDate);
                    candles = candles.Concat(monthCandles).ToList();
                }
                else
                {
                    var startDate = new DateTimeOffset(new DateTime(now.Year, now.Month, 1).AddMonths(-i)).ToUnixTimeMilliseconds();
                    var endDate = new DateTimeOffset(new DateTime(now.Year, now.Month, 1).AddMonths(-i + 1).AddMinutes(-5)).ToUnixTimeMilliseconds();
                    var monthCandles = await _candlesApiClient.GetCandles(symbol, interval, startDate, endDate);
                    candles = candles.Concat(monthCandles).ToList();
                }
            }

            var orderedCandles = candles.OrderBy(x => x.OpenTime).ToList();
            return orderedCandles;
        }

    }
}
