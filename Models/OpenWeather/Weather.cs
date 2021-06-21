namespace WeatherOperator.Models.OpenWeather
{
    public record Weather
    {
        public string Main { get; init; } = string.Empty;

        public string Description { get; init; } = string.Empty;
    }
}
