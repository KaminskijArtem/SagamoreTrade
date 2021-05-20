using System;
using System.Collections.Generic;
using System.Text;

namespace TradingDataLibrary.Models
{
    public class Positions
    {
        public List<Position> PositionsList { get; set; }
        public bool IsInMemory { get; set; }
    }
}
