using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;

namespace HealthcheckDatabase.Serives
{
    public class TestHealthCheckWithArgsService : IHealthCheck
    {
        public TestHealthCheckWithArgsService()
        {

        }


        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                using (var ping = new Ping())
                {
                    var reply = ping.Send("asp.net-hacker.rocks");
                    if (reply.Status != IPStatus.Success)
                    {
                        return await Task.FromResult(HealthCheckResult.Unhealthy("Ping is unhealthy"));

                    }

                    if (reply.RoundtripTime > 100)
                    {
                        return await Task.FromResult(HealthCheckResult.Degraded("Ping is degraded"));
                    }

                    return await Task.FromResult(HealthCheckResult.Healthy("Ping is healthy"));
                }
            }
            catch
            {
                return await Task.FromResult(HealthCheckResult.Unhealthy("Ping is unhealthy"));
            }
        }
    }
}
