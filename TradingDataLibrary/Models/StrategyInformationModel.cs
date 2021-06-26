using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TradingDataLibrary.Models
{
    public class StrategyInformationModel
    {
        public Dictionary<string, List<(decimal DealResult, DateTimeOffset OpenDate, DateTimeOffset CloseDate)>> DealResults { get; set; }
    }
}
