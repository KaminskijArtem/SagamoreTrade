using System;
using System.Collections.Generic;
using System.Text;

namespace TradingDataLibrary.Models
{
    public class RSISignalModel
    {
        public bool ShouldOpenPosition { get; set;}
        public bool IsPositionOpened { get; set;}
        public string Text { get; set;}
    }
}
