using System;
using System.Collections.Generic;

namespace TradingDataLibrary.Models
{
    public class TradingPositionsResult
    {
        public List<Position> positions { get; set; }
    }

    public class Position
    {
        public Guid id { get; set; }
        public string symbol { get; set; }
        public decimal openQuantity { get; set; }
        public decimal openPrice { get; set; }
        public long openTimestamp { get; set; }
        public bool IsLong()
        {
            return openQuantity > 0;
        }
        public DateTimeOffset GetOpenTimestamp()
        {
            return DateTimeOffset.FromUnixTimeMilliseconds(openTimestamp);
        }
    }
}
