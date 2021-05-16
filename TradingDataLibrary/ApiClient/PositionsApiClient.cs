using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
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
        public async Task<List<Position>> GetAllPositions()
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
                throw new Exception($"{response.StatusCode}");

            var contents = await response.Content.ReadAsStreamAsync();
            var objPositions = await JsonSerializer.DeserializeAsync<TradingPositionsResult>(contents);
            if(objPositions.positions != null)
                objPositions.positions.ForEach(x => x.symbol = x.symbol.Replace("_LEVERAGE", "").Replace(".", ""));

            return objPositions.positions;
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
            return false;
        }
    }
}
