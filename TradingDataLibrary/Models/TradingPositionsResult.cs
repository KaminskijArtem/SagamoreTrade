using System;
using System.Collections.Generic;
using System.Text;

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
        public double openQuantity { get; set; }
        public decimal openPrice { get; set; }
        public bool IsLong()
        {
            return openQuantity > 0;
        }
    }
}
