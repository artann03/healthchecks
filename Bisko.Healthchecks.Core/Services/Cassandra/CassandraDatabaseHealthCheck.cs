using Cassandra;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.Linq;
using Bisko.Healthchecks.Core.Services.Cassandra.Connections;

namespace Bisko.Healthchecks.Core.Services.Cassandra
{
    public class CassandraDatabaseHealthCheck : IHealthCheck
    {
        private readonly IDatabaseConnection _databaseConnection;

        public CassandraDatabaseHealthCheck(IDatabaseConnection databaseConnection)
        {
            _databaseConnection = databaseConnection;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _databaseConnection.Session.ExecuteAsync(new SimpleStatement("SELECT release_version FROM system.local"));

                if (result == null || !result.Any())
                    return await Task.FromResult(new HealthCheckResult(status: HealthStatus.Unhealthy, description: $"Database is unhealthy"));

                return await Task.FromResult(new HealthCheckResult(status: HealthStatus.Healthy, description: $"Database is healthy"));
            }
            catch (Exception ex)
            {
                return await Task.FromResult(new HealthCheckResult(status: HealthStatus.Unhealthy, description: $"Database is unhealthy - ex:{ex.Message}"));
            }
        }
    }
}
