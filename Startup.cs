using KubeOps.Operator;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using WeatherOperator.Services;

namespace WeatherOperator
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddKubernetesOperator();
            services.AddHostedService<WeatherApiCaller>();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseKubernetesOperator();
        }
    }
}
