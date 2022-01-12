using BlazorOIDC.Shared;
using System.Net.Http.Json;

namespace BlazorOIDC.Client.Services
{
    public class WeatherForecastService
    {
        private readonly HttpClient Http;

        public WeatherForecastService(HttpClient Http) {
            this.Http = Http;
        }

        public async Task<IEnumerable<WeatherForecast>> GetWeatherForecast()
        {
            Console.WriteLine(Http.BaseAddress + "/WeatherForecast");
            return await Http.GetFromJsonAsync<WeatherForecast[]>("WeatherForecast");
        }
    }
}
