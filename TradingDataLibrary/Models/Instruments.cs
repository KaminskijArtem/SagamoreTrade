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
                OpenPositionSymbol = "BTC%2FUSD_LEVERAGE",
                Margin = 2,
                PositionSize = 0.001
            },
            new Instrument
            {
                Symbol = "ETH/USD",
                OpenPositionSymbol = "ETH%2FUSD_LEVERAGE",
                Margin = 2,
                PositionSize = 0.02
            },
            new Instrument
            {
                Symbol = "US500",
                OpenPositionSymbol = "US500.",
                Margin = 20,
                PositionSize = 0.1
            },
            new Instrument
            {
                Symbol = "US30",
                OpenPositionSymbol = "US30.",
                Margin = 20,
                PositionSize = 0.01
            },
            new Instrument
            {
                Symbol = "US100",
                OpenPositionSymbol = "US100.",
                Margin = 20,
                PositionSize = 0.03
            },
            new Instrument
            {
                Symbol = "Gold",
                OpenPositionSymbol = "Gold.",
                Margin = 50,
                PositionSize = 0.6
            }
        };
    }

    public class Instrument
    {
        public string Symbol { get; set; }
        public int Margin { get; set; }
        public double PositionSize { get; set; }
        public string OpenPositionSymbol { get; set; }
    }
}
