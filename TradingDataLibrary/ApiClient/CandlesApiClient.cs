using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TradingDataLibrary.Models;

namespace TradingDataLibrary.ApiClient
{
    public class CandlesApiClient : ICandlesApiClient
    {
        private readonly HttpClient client = new HttpClient();

        public async Task<List<Candle>> GetCandles(string symbol, string interval, long? startTime = null, long? endTime = null)
        {
            //try
            //{
            //    var url = $"https://api-adapter.backend.currency.com/api/v1/klines?symbol={symbol}&interval={interval}&limit=1000";

            //    if (startTime != null)
            //        url += $"&startTime={startTime}";
            //    if (endTime != null)
            //        url += $"&endTime={endTime}";

            //    var stream = await client.GetStreamAsync(url);
            //    var output = new List<Candle>();

            //    var objCandles = await JsonSerializer.DeserializeAsync<List<List<JsonElement>>>(stream);

            //    foreach (var objCandle in objCandles)
            //    {
            //        output.Add(new Candle
            //        {
            //            OpenTime = DateTimeOffset.FromUnixTimeMilliseconds(objCandle[0].GetInt64()),
            //            Open = decimal.Parse(objCandle[1].GetString()),
            //            High = decimal.Parse(objCandle[2].GetString()),
            //            Low = decimal.Parse(objCandle[3].GetString()),
            //            Close = decimal.Parse(objCandle[4].GetString()),
            //            Volume = objCandle[5].GetInt64()
            //        });
            //    }
            //    return output;
            //}
            //catch (Exception ex)
            //{
            //    Thread.Sleep(500);
            //    return await GetCandles(symbol, interval, startTime, endTime);
            //}

            var url = $"https://api-adapter.backend.currency.com/api/v1/klines?symbol={symbol}&interval={interval}&limit=1000";

            if (startTime != null)
                url += $"&startTime={startTime}";
            if (endTime != null)
                url += $"&endTime={endTime}";

            var stream = await client.GetStreamAsync(url);
            var output = new List<Candle>();

            var objCandles = await JsonSerializer.DeserializeAsync<List<List<JsonElement>>>(stream);

            foreach (var objCandle in objCandles)
            {
                output.Add(new Candle
                {
                    OpenTime = DateTimeOffset.FromUnixTimeMilliseconds(objCandle[0].GetInt64()),
                    Open = decimal.Parse(objCandle[1].GetString()),
                    High = decimal.Parse(objCandle[2].GetString()),
                    Low = decimal.Parse(objCandle[3].GetString()),
                    Close = decimal.Parse(objCandle[4].GetString()),
                    Volume = objCandle[5].GetInt64()
                });
            }
            return output;
        }
    }
}
