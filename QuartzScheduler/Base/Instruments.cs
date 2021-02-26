using System;
using System.Collections.Generic;
using System.Text;

namespace QuartzScheduler.Base
{
    public class GlobalValues
    {
        public static List<string> symbols = new List<string>
        { "BTC/USD" ,"ETH/USD" ,"XRP/USD" ,"LTC/USD", "DOGE/USD", "BCH/USD", "BTC/EUR", "ETH/EUR", "UNI/USD", "ETH/BTC",
            "XRP/EUR", "COMP/USD", "LINK/USD", "XRP/BTC", "BCH/BTC", "BTC/RUB", "LTC/BTC", "BCH/EUR", "ETH/RUB", "LTC/EUR",
            "XRP/RUB", "XRP/BYN", "BTC/BYN", "ETH/BYN", "LTC/BYN", "LTC/RUB",
            "US500", "VIX", "US30", "DE30", "US100",
            "Gold", "Silver", "Oil - Brent", "Oil - Crude", "Natural Gas", "Copper", "Platinum", "Aluminum",
            "AAPL"
        };

        public static List<string> inPositionSymbols = new List<string>
        { "ETH/BTC" };
    }
}
