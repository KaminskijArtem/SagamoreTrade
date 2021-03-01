using System;
using System.Collections.Generic;
using System.Text;

namespace QuartzScheduler.Base
{
    public class GlobalValues
    {
        public static List<string> symbols = new List<string>
        {   "BTC/USD" ,"BTC/EUR" ,"ETH/USD" , "ETH/EUR", "LTC/USD",  "BCH/USD", "ETH/BTC",
            "LTC/EUR", "BCH/EUR",
            "US500", "US30", "US100", 
            "Gold", "Copper"
        };

        public static List<string> inPositionSymbols = new List<string>
        {
            "Gold"
        };
    }
}
