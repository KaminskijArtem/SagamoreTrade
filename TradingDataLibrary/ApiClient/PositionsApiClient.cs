﻿using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TradingDataLibrary.Logging;
using TradingDataLibrary.Models;

namespace TradingDataLibrary.ApiClient
{
    public class PositionsApiClient : IPositionsApiClient
    {
        private readonly IConfiguration _configuration;

        private string key;
        private string secretKey;

        public PositionsApiClient(IConfiguration configuration)
        {
            _configuration = configuration;
            key = _configuration["ApiConfiguration:Key"];
            secretKey = _configuration["ApiConfiguration:SecretKey"];
        }
        public async Task<Positions> GetAllPositions()
        {
            var client = new HttpClient();
            var time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            var sign = CreateSignature($"timestamp={time}", secretKey);
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri($"https://api-adapter.backend.currency.com/api/v1/tradingPositions?timestamp={time}&signature={sign}"),
                Method = HttpMethod.Get,
            };
            request.Headers.Add("X-MBX-APIKEY", key);

            var response = await client.SendAsync(request);

            if(!response.IsSuccessStatusCode)
            {
                if(InMemoryPositions.Positions == null)
                    throw new Exception($"{response.StatusCode} {await response.Content.ReadAsStringAsync()}");
                else
                {
                    StaticLogger.LogMessage($"GetAllPositions {response.StatusCode} {await response.Content.ReadAsStringAsync()}");
                    return new Positions { PositionsList = InMemoryPositions.Positions, IsInMemory = true };
                }
            }

            var contents = await response.Content.ReadAsStreamAsync();
            var objPositions = await JsonSerializer.DeserializeAsync<TradingPositionsResult>(contents);
            if(objPositions.positions != null)
                objPositions.positions.ForEach(x => x.symbol = x.symbol.Replace("_LEVERAGE", "").Replace(".", ""));

            InMemoryPositions.Positions = objPositions.positions;

            return new Positions { PositionsList = objPositions.positions, IsInMemory = false };
        }

        private string CreateSignature(string queryString, string secret)
        {

            var keyBytes = Encoding.UTF8.GetBytes(secret);
            var queryStringBytes = Encoding.UTF8.GetBytes(queryString);
            var hmacsha256 = new HMACSHA256(keyBytes);

            var bytes = hmacsha256.ComputeHash(queryStringBytes);

            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }

        public async Task<bool> ClosePosition(Guid id)
        {
            var client = new HttpClient();
            var time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            var sign = CreateSignature($"timestamp={time}&positionId={id}", secretKey);
            var request = new HttpRequestMessage()
            {
                RequestUri = 
                new Uri($"https://api-adapter.backend.currency.com/api/v1/closeTradingPosition?timestamp={time}&positionId={id}&signature={sign}"),
                Method = HttpMethod.Post,
            };
            request.Headers.Add("X-MBX-APIKEY", key);

            var response = await client.SendAsync(request);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> OpenPosition(Instrument instrument, bool isLong)
        {
            var client = new HttpClient();
            var time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var side = isLong ? "BUY" : "SELL";

            var queryString = $"timestamp={time}&symbol={instrument.OpenPositionSymbol}&side={side}&type=MARKET&quantity={instrument.PositionSize}&leverage={instrument.Margin}&accountId=5229702511416516";
            var sign = CreateSignature(queryString, secretKey);
            var request = new HttpRequestMessage()
            {
                RequestUri =
                new Uri($"https://api-adapter.backend.currency.com/api/v1/order?{queryString}&signature={sign}"),
                Method = HttpMethod.Post,
            };
            request.Headers.Add("X-MBX-APIKEY", key);

            var response = await client.SendAsync(request);

            return response.IsSuccessStatusCode;
        }
    }
}
