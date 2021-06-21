using System.Collections.Generic;
using System.Threading.Tasks;
using TradingDataLibrary.Models;

namespace TradingDataLibrary.Interfaces
{
    public interface IRSITradeCandlesService
    {
        Task<RSISignalModel> GetRSISignal(string symbol, string interval, List<Position> positions);
        Task<InPositionRSISignalModel> GetInPositionRSISignal(string symbol, string interval, Position position, IEnumerable<Position> allSymbolPositions);
        Task<StrategyInformationModel> GetStrategyInformation();
    }
}
