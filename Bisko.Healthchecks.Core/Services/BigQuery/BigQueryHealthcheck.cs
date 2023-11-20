using Google.Cloud.BigQuery.V2;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Threading;
using System;
using Bisko.Healthchecks.Core.Configurations;

namespace Bisko.Healthchecks.Core.Services.BigQuery
{
    public class BigQueryHealthcheck : IHealthCheck
    {
        private readonly BigQueryClient _bigQueryClient;

        public BigQueryHealthcheck()
        {
            _bigQueryClient = InitializeBigQueryClient();
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var queryJob = await _bigQueryClient.ExecuteQueryAsync("SELECT 1", parameters: null, cancellationToken: cancellationToken);

                if (queryJob.TotalRows == 1)
                    return await Task.FromResult(new HealthCheckResult(status: HealthStatus.Healthy, description: $"BigQuery is healthy"));

                return await Task.FromResult(new HealthCheckResult(status: HealthStatus.Unhealthy, description: $"BigQuery is unhealthy"));
            }
            catch (Exception ex)
            {
                return await Task.FromResult(new HealthCheckResult(status: HealthStatus.Unhealthy, description: $"BigQuery is unhealthy - ex:{ex.Message}"));
            }
        }

        private BigQueryClient InitializeBigQueryClient()
        {
            string credentialsPath = FindCredentialsPath();

            if (credentialsPath != null)
            {
                Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credentialsPath);
                return BigQueryClient.Create(BiskoHealthcheckConfiguration.BigQueryProjectId ?? throw new Exception());
            }

            throw new Exception("Credentials not found. Unable to initialize BigQueryClient.");
        }

        private string FindCredentialsPath()
        {
            try
            {
                var applicationPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                var credentialsPath = Path.Combine(applicationPath, "credentials.json");

                if (!File.Exists(credentialsPath))
                {
                    var binPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin");
                    credentialsPath = Path.Combine(binPath, "credentials.json");
                }

                return credentialsPath;
            }
            catch (Exception)
            {
                return null;
            }
        }


    }
}
