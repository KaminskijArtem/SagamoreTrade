using System.Collections.Generic;

namespace TradingDataLibrary.Models
{
    public class GlobalValues
    {
        public static List<Instrument> instruments = new List<Instrument>
        {
            new Instrument
            {
                Symbol = "BTC/USD",
                Margin = 5,
                PositionSize = 0.002
            },
            new Instrument
            {
                Symbol = "ETH/USD",
                Margin = 5,
                PositionSize = 0.03
            },
            new Instrument
            {
                Symbol = "LTC/USD",
                Margin = 5,
                PositionSize = 0.35
            },
            new Instrument
            {
                Symbol = "BCH/USD",
                Margin = 5,
                PositionSize = 0.09
            },
            new Instrument
            {
                Symbol = "ETH/BTC",
                Margin = 5,
                PositionSize = 0.03
            },
            new Instrument
            {
                Symbol = "US500",
                Margin = 20,
                PositionSize = 0.1
            },
            new Instrument
            {
                Symbol = "US30",
                Margin = 20,
                PositionSize = 0.01
            },
            new Instrument
            {
                Symbol = "US100",
                Margin = 20,
                PositionSize = 0.03
            },
            new Instrument
            {
                Symbol = "Gold",
                Margin = 25,
                PositionSize = 0.3
            }
        };
    }

    public class Instrument
    {
        public string Symbol { get; set; }
        public int Margin { get; set; }
        public double PositionSize { get; internal set; }
    }
}
