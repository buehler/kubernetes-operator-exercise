using Newtonsoft.Json;

namespace WeatherOperator.Models.OpenWeather
{
    public record OneCall
    {
        [JsonProperty("lat")]
        public double Latitude { get; init; }

        [JsonProperty("lon")]
        public double Longitude { get; init; }

        public WeatherData Current { get; set; } = new();
    }
}
