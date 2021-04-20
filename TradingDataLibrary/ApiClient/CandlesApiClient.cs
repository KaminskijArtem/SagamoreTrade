using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using TradingDataLibrary.Models;

namespace TradingDataLibrary.ApiClient
{
    public class CandlesApiClient : ICandlesApiClient
    {
        private readonly HttpClient client = new HttpClient();

        public async Task<List<Candle>> GetCandles(string symbol, string interval)
        {
            var streamTask = client.GetStreamAsync(
                $"https://api-adapter.backend.currency.com/api/v1/klines?symbol={symbol}&interval={interval}&limit=1000");
            var output = new List<Candle>();

            var objCandles = await JsonSerializer.DeserializeAsync<List<List<JsonElement>>>(await streamTask);
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
