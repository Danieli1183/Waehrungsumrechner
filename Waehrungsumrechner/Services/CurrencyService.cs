using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Waehrungsumrechner.Models;

namespace Waehrungsumrechner.Services
{
    public interface ICurrencyService
    {
        Task<List<Currency>> GetCurrenciesAsync();
        Task<Dictionary<string, decimal>> GetRatesAsync(string baseCurrencyCode);
    }

    public class CurrencyService : ICurrencyService
    {
        private readonly HttpClient _http = new HttpClient();

        public async Task<List<Currency>> GetCurrenciesAsync()
        {
            const string url = "https://cdn.jsdelivr.net/npm/@fawazahmed0/currency-api@latest/v1/currencies.json";
            var json = await _http.GetStringAsync(url);
            var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

            return dict
                .Select(kv => new Currency
                {
                    Code = kv.Key.ToUpperInvariant(),
                    Name = kv.Value
                })
                .OrderBy(c => c.Code)
                .ToList();
        }

        public async Task<Dictionary<string, decimal>> GetRatesAsync(string baseCurrencyCode)
        {
            string url = $"https://cdn.jsdelivr.net/npm/@fawazahmed0/currency-api@latest/v1/currencies/{baseCurrencyCode.ToLower()}.json";
            var json = await _http.GetStringAsync(url);

            var root = JObject.Parse(json);
            var ratesObj = (JObject)root[baseCurrencyCode.ToLower()];

            return ratesObj
                .Properties()
                .ToDictionary(
                    p => p.Name.ToUpper(),
                    p => p.Value.Value<decimal>()
                );
        }
    }
}
