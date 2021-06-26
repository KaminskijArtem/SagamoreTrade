using System.Collections.Generic;
using System.Threading.Tasks;
using TradingDataLibrary.Models;

namespace TradingDataLibrary.ApiClient
{
    public interface ICandlesApiClient
    {
        Task<List<Candle>> GetCandles(string symbol, string interval, long? startTime = null, long? endTime = null);

    }
}
