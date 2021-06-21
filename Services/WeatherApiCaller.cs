using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Cronos;
using DotnetKubernetesClient;
using DotnetKubernetesClient.LabelSelectors;
using k8s.Models;
using KubeOps.Operator.Entities.Extensions;
using KubeOps.Operator.Rbac;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using WeatherOperator.Entities;
using WeatherOperator.Models.OpenWeather;
using Timer = System.Timers.Timer;

namespace WeatherOperator.Services
{
    [EntityRbac(typeof(V1WeatherData), Verbs = RbacVerb.All)]
    [EntityRbac(typeof(V1WeatherLocation), Verbs = RbacVerb.Get | RbacVerb.List)]
    public class WeatherApiCaller : IHostedService, IDisposable
    {
        private static readonly string ApiKey = Environment.GetEnvironmentVariable("API_KEY") ?? string.Empty;
        private readonly ILogger<WeatherApiCaller> _logger;
        private readonly IKubernetesClient _kubernetesClient;
        private readonly HttpClient _httpClient = new();

#if DEBUG
        private readonly CronExpression _cron = CronExpression.Parse("* * * * *");
#else
        private readonly CronExpression _cron = CronExpression.Parse("0 * * * *");
#endif

        private Timer? _timer;

        public WeatherApiCaller(ILogger<WeatherApiCaller> logger, IKubernetesClient kubernetesClient)
        {
            _logger = logger;
            _kubernetesClient = kubernetesClient;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
            => await ScheduleJob(cancellationToken);

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Stop();
            _timer?.Dispose();
            _timer = null;

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }

        private async Task ScheduleJob(CancellationToken cancellationToken)
        {
            var next = _cron.GetNextOccurrence(DateTime.UtcNow);
            if (!next.HasValue)
            {
                return;
            }

            var delay = next.Value - DateTimeOffset.Now;
            if (delay.TotalMilliseconds <= 0)
            {
                await ScheduleJob(cancellationToken);
            }

            _timer = new(delay.TotalMilliseconds);
            _timer.Elapsed += async (_, _) =>
            {
                _timer.Dispose();
                _timer = null;

                if (!cancellationToken.IsCancellationRequested)
                {
                    await CheckWeatherApi();
                }

                if (!cancellationToken.IsCancellationRequested)
                {
                    await ScheduleJob(cancellationToken);
                }
            };
            _timer.Start();
        }

        private async Task CheckWeatherApi()
        {
            _logger.LogInformation($"Start weather check @ {DateTime.UtcNow:yyyy.MM.dd-HH:mm:ss}");
            var locations =
                await _kubernetesClient.List<V1WeatherLocation>();

            foreach (var location in locations)
            {
                try
                {
                    _logger.LogInformation($"Fetch information about location {location.Name()}.");
                    var response = await _httpClient.GetStringAsync(CreateUrl(location));
                    var data = JsonConvert.DeserializeObject<OneCall>(response);
                    location.Status.LastCheck = DateTime.UtcNow;

                    var weatherData = await _kubernetesClient.List<V1WeatherData>(
                        null,
                        new EqualsSelector("weather-operator.dev/location", location.Uid()));

                    _logger.LogInformation("Delete all weather data older than 12 hours.");
                    await _kubernetesClient.Delete(
                        weatherData
                            .Where(d => d.CreationTimestamp() != null)
                            .Where(d => d.CreationTimestamp() < DateTime.UtcNow.AddHours(-12)));

                    var weather = data.Current.Weather.FirstOrDefault();

                    if (weather != null)
                    {
                        _logger.LogInformation("Create new weather data.");
                        var newData = new V1WeatherData
                        {
                            Metadata =
                            {
                                Name = $"weather-{location.Name()}-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}",
                            },
                            Spec =
                            {
                                Sunrise = DateTimeOffset.FromUnixTimeSeconds(data.Current.Sunrise).LocalDateTime,
                                Sunset = DateTimeOffset.FromUnixTimeSeconds(data.Current.Sunset).LocalDateTime,
                                Temperature = Math.Round(data.Current.Temperature, 2),
                                MainWeather = weather.Main,
                                Description = weather.Description,
                            },
                        };

                        newData.SetLabel("weather-operator.dev/location", location.Uid());
                        newData.AddOwnerReference(location.MakeOwnerReference());
                        await _kubernetesClient.Create(newData);
                    }
                    else
                    {
                        _logger.LogWarning("No weather data received.");
                    }

                    location.Status.Error = null;
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Error during API fetch of location {location.Name()}.");
                    location.Status.Error = e.Message;
                }
                finally
                {
                    await _kubernetesClient.UpdateStatus(location);
                }
            }
        }

        private static string CreateUrl(V1WeatherLocation location)
            => "https://api.openweathermap.org/data/2.5/onecall?" +
               $"lat={Math.Round(location.Spec.Latitude, 6)}&lon={Math.Round(location.Spec.Longitude, 6)}" +
               $"&exclude=minutely,hourly,daily,alerts&units=metric&appid={ApiKey}";
    }
}
