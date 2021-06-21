using k8s.Models;
using KubeOps.Operator.Webhooks;
using WeatherOperator.Entities;

namespace WeatherOperator.Validators
{
    public class DataValidator : IValidationWebhook<V1WeatherData>
    {
        public AdmissionOperations Operations => AdmissionOperations.Create | AdmissionOperations.Update;

        public ValidationResult Create(V1WeatherData newEntity, bool dryRun)
            => Validate(newEntity);

        public ValidationResult Update(V1WeatherData oldEntity, V1WeatherData newEntity, bool dryRun)
            => Validate(newEntity);

        private static ValidationResult Validate(V1WeatherData data)
        {
            if (data.GetLabel("weather-operator.dev/location") == null)
            {
                return ValidationResult.Fail(400, "Location Label missing.");
            }

            if (data.OwnerReferences() == null || data.OwnerReferences().Count <= 0)
            {
                return ValidationResult.Fail(400, "No Location Owner defined.");
            }

            return ValidationResult.Success();
        }
    }
}
