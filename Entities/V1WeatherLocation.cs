using System;
using k8s.Models;
using KubeOps.Operator.Entities;

namespace WeatherOperator.Entities
{
    public class V1WeatherLocationSpec
    {
        public double Latitude { get; set; }

        public double Longitude { get; set; }
    }

    public class V1WeatherLocationStatus
    {
        public DateTime? LastCheck { get; set; }

        public string? Error { get; set; }
    }

    [KubernetesEntity(
        ApiVersion = "v1",
        Group = "kubernetes.dev",
        Kind = "WeatherLocation")]
    public class V1WeatherLocation : CustomKubernetesEntity
        <V1WeatherLocationSpec, V1WeatherLocationStatus>
    {
    }
}
