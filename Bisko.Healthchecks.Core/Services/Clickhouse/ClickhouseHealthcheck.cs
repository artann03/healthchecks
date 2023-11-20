using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using System.Threading;
using System;
using Bisko.Healthchecks.Core.Configurations;
using ClickHouseOctonicaConnection = Octonica.ClickHouseClient.ClickHouseConnection;
using ClickHouseAdoConnection = ClickHouse.Ado.ClickHouseConnection;

namespace Bisko.Healthchecks.Core.Services.Clickhouse
{
    public class ClickhouseHealthcheck : IHealthCheck
    {
        private ClickHouseOctonicaConnection octonicaConnection;
        private ClickHouseAdoConnection adoConnection;
        public bool IsAdoConnection = BiskoHealthcheckConfiguration.ClickhouseHealthcheckSettings.IsClickhouseAdo;
        private bool IsOctonicaConnection = BiskoHealthcheckConfiguration.ClickhouseHealthcheckSettings.IsOctonicaClient;
        private readonly Random _random;
        private readonly string[] _clickhouseInstances = BiskoHealthcheckConfiguration.ClickhouseHealthcheckSettings.ClickhouseInstances ?? throw new Exception();

        public ClickhouseHealthcheck()
        {

            _random = new Random();
            InitializeNewConnection();
        }

        private void InitializeNewConnection()
        {
            if (IsOctonicaConnection)
            {
                int randomIndex = _random.Next(_clickhouseInstances.Length);
                octonicaConnection = new ClickHouseOctonicaConnection(_clickhouseInstances[randomIndex]);
                octonicaConnection.Open();
            }
            else if (IsAdoConnection)
            {
                int randomIndex = _random.Next(_clickhouseInstances.Length);
                adoConnection = new ClickHouseAdoConnection(_clickhouseInstances[randomIndex]);
                adoConnection.Open();
            }
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var healthCheckDescriptions = new List<string>();

                foreach (var clickhouseInstance in _clickhouseInstances)
                {
                    InitializeNewConnection();

                    var currentState = ConnectionState.Open;

                    if (IsAdoConnection)
                        currentState = CurrentState(adoConnection);
                    else if (IsOctonicaConnection)
                        currentState = CurrentState(default, octonicaConnection);

                    if (currentState == ConnectionState.Closed)
                        healthCheckDescriptions.Add($"ClickHouse connection state for the instance \"{clickhouseInstance}\" in the cluster: \"{BiskoHealthcheckConfiguration.ClickhouseHealthcheckSettings.ClusterName ?? throw new Exception()}\" is \"{currentState}\"");
                }

                if (healthCheckDescriptions.Count > 0)
                {
                    var description = string.Join("\n", healthCheckDescriptions);
                    return new HealthCheckResult(context.Registration.FailureStatus, description);
                }

                return await Task.FromResult(new HealthCheckResult(status: HealthStatus.Healthy, description: $"ClickHouse is healthy"));
            }
            catch (Exception exception)
            {
                return await Task.FromResult(new HealthCheckResult(status: HealthStatus.Unhealthy, description: $"ClickHouse is unhealthy - ex:{exception.Message}"));
            }
        }
        public static ConnectionState CurrentState(ClickHouseAdoConnection currentAdoConnection = default, ClickHouseOctonicaConnection currentOctonicaConnection = default)
        {
            try
            {
                if (currentAdoConnection != default)
                {
                    var queryCanBeExecuted = currentAdoConnection.CreateCommand($"SELECT 1").ExecuteScalar();
                    if ((byte)queryCanBeExecuted == 1)
                        return ConnectionState.Open;
                }
                else if (currentOctonicaConnection != default)
                {
                    var queryCanBeExecuted = currentOctonicaConnection.CreateCommand($"SELECT 1").ExecuteScalar();
                    if ((byte)queryCanBeExecuted == 1)
                        return ConnectionState.Open;
                }
            }
            catch (Exception) { }

            return ConnectionState.Closed;
        }
    }
}
