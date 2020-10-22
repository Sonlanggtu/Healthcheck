using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Threading;
using System.Threading.Tasks;

namespace HealthcheckDatabase
{
    public class ExampleHealthCheckService : IHealthCheckService
    {

        //public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
        //                                                CancellationToken cancellationToken = default(CancellationToken))
        //{
        //    var healthCheckResultHealthy = true;

        //    if (healthCheckResultHealthy)
        //    {
        //        return Task.FromResult(
        //            HealthCheckResult.Healthy("A healthy result."));
        //    }

        //    return Task.FromResult(
        //        HealthCheckResult.Unhealthy("An unhealthy result."));
        //}
    }
}