﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TradingDataLibrary.Models;

namespace TradingDataLibrary.ApiClient
{
    public interface IPositionsApiClient
    {
        Task<List<Position>> GetAllPositions();
        Task<bool> ClosePosition(Guid id);
        Task<bool> OpenPosition(Instrument instrument, bool isLong);
    }
}
