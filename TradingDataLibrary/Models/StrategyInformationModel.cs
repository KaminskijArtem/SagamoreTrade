using System;
using System.Collections.Generic;
using System.Text;

namespace TradingDataLibrary.Models
{
    public class StrategyInformationModel
    {
        public List<(string Symbol, List<(decimal DealResult, long OpenDate, long CloseDate)>)> DealResults { get; set; }
    }
}
