using System.Collections.Generic;
using Newtonsoft.Json;

namespace WeatherOperator.Models.OpenWeather
{
    public record WeatherData
    {
        public int Sunrise { get; init; }

        public int Sunset { get; init; }

        [JsonProperty("temp")]
        public double Temperature { get; init; }

        public IEnumerable<Weather> Weather { get; init; } = new List<Weather>();
    }
}
