using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TradingDataLibrary.Models;

namespace TradingDataLibrary.Interfaces
{
    public interface IRSITradeCandlesService
    {
        Task<string> GetRSISignal(string symbol, string interval, bool isInposition);
        Task<InPositionRSISignalModel> GetInPositionRSISignal(string symbol, string interval, bool isLong);
    }
}
