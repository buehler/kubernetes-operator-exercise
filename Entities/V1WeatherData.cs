using System;
using DotnetKubernetesClient.Entities;
using k8s.Models;
using KubeOps.Operator.Entities;
using KubeOps.Operator.Entities.Annotations;

namespace WeatherOperator.Entities
{
    public class V1WeatherDataSpec
    {
        [AdditionalPrinterColumn]
        public string MainWeather { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [AdditionalPrinterColumn]
        public double Temperature { get; set; }

        public DateTime Sunrise { get; set; }

        public DateTime Sunset { get; set; }
    }

    [KubernetesEntity(
        ApiVersion = "v1",
        Group = "kubernetes.dev",
        Kind = "WeatherData")]
    [EntityScope(EntityScope.Cluster)]
    public class V1WeatherData : CustomKubernetesEntity<V1WeatherDataSpec>
    {
        public V1WeatherData()
        {
            Kind = "WeatherData";
            ApiVersion = "kubernetes.dev/v1";
        }
    }
}
