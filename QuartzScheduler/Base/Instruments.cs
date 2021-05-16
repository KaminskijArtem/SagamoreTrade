using System.Collections.Generic;

namespace QuartzScheduler.Base
{
    public class GlobalValues
    {
        public static List<Instrument> instruments = new List<Instrument>
        {
            new Instrument
            {
                Symbol = "BTC/USD"
            },
            new Instrument
            {
                Symbol = "ETH/USD"
            },
            new Instrument
            {
                Symbol = "LTC/USD"
            },
            new Instrument
            {
                Symbol = "BCH/USD"
            },
            new Instrument
            {
                Symbol = "ETH/BTC"
            },
            new Instrument
            {
                Symbol = "US500"
            },
            new Instrument
            {
                Symbol = "US30"
            },
            new Instrument
            {
                Symbol = "US100"
            },
            new Instrument
            {
                Symbol = "Gold"
            }
        };
    }

    public class Instrument
    {
        public string Symbol { get; set; }
    }
}
