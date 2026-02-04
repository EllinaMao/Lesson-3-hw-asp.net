using Lesson_3.Models.Interfases;
namespace Lesson_3.Models
{
    public class NbuCurrencyService : ICurrencyService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        public NbuCurrencyService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _config = config;
        }

        public async Task<List<IDto>> GetRatesAsync()
        {
            string? url = _config["APIService:NBU"];

            if (string.IsNullOrEmpty(url))
                throw new Exception("NBU URL is missing in config");

            var rates = await _httpClient.GetFromJsonAsync<List<NbuRate>>(url);

            if (rates == null) return new List<IDto>();

            return rates.Cast<IDto>().ToList();
        }
    }
}
