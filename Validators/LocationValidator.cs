using System;
using KubeOps.Operator.Webhooks;
using WeatherOperator.Entities;

namespace WeatherOperator.Validators
{
    public class LocationValidator : IValidationWebhook<V1WeatherLocation>
    {
        public AdmissionOperations Operations => AdmissionOperations.Create | AdmissionOperations.Update;

        public ValidationResult Create(V1WeatherLocation newEntity, bool dryRun)
            => CheckLocation(newEntity);

        public ValidationResult Update(V1WeatherLocation oldEntity, V1WeatherLocation newEntity, bool dryRun)
            => CheckLocation(newEntity);

        private static ValidationResult CheckLocation(V1WeatherLocation location)
            => Math.Abs(location.Spec.Latitude) <= 90 &&
               Math.Abs(location.Spec.Longitude) <= 180
                ? ValidationResult.Success()
                : ValidationResult.Fail(
                    400,
                    $"Latitude ({location.Spec.Latitude}) or Longitude ({location.Spec.Longitude}) invalid");
    }
}
