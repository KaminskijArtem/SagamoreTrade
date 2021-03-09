using System;
using System.Collections.Generic;
using System.Text;

namespace QuartzScheduler.Base
{
    public class GlobalValues
    {
        public static List<string> symbols = new List<string>
        {   "BTC/USD", "BTC/EUR", "ETH/USD", "ETH/EUR", "LTC/USD", "BCH/USD", "ETH/BTC",
            "US500", "US30", "US100",
            "Gold", "Silver", "Platinum"
        };

        public static List<string> inLongPositionSymbols = new List<string>
        {
            "ETH/BTC", "BCH/USD"
        };

        public static List<string> inShortPositionSymbols = new List<string>
        {
            "LTC/USD"
        };
    }
}
