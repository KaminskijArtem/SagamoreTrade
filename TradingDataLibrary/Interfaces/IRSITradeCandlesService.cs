using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TradingDataLibrary.Models;

namespace TradingDataLibrary.Interfaces
{
    public interface IRSITradeCandlesService
    {
        Task<RSISignalModel> GetRSISignal(string symbol, string interval, int positionsCount);
        Task<InPositionRSISignalModel> GetInPositionRSISignal(string symbol, string interval, bool isLong);
    }
}
