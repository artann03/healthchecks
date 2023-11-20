using Bisko.Healthchecks.Core.Configurations;
using Bisko.Healthchecks.Core.Services.BigQuery;
using Bisko.Healthchecks.Core.Services.Cassandra;
using Bisko.Healthchecks.Core.Services.Cassandra.Connections;
using Bisko.Healthchecks.Core.Services.Clickhouse;
using Bisko.Healthchecks.Core.Services.Pulsar.Consumers;
using Bisko.Healthchecks.Core.Services.Pulsar.Producers;
using Bisko.Messaging.Core.Model.Logger;
using Cassandra;
using Confluent.Kafka;
using HealthChecks.UI.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bisko.Healthchecks.Core.Extensions.Healthchecks
{
    public static class HealthcheckServiceCollectionExtensions
    {
        public static void AddBiskoHealthChecks(this IServiceCollection services, BiskoHealthcheckSettings configuration)
        {
            configuration.InjectConfigurationPropertiesInStaticClass();

            services.AddHealthChecks().AddPossibleHealthchecks(services);

            services.AddHealthChecksUI(setup =>
            {
                setup.AddHealthCheckEndpoint(BiskoHealthcheckConfiguration.ServiceName, BiskoHealthcheckConfiguration.HealthcheckEndpoint);

                setup.AddWebhookNotification("Google Chat",
                    BiskoHealthcheckConfiguration.HealthcheckWebhookEndpoint,
                    payload: $"{{ text: \"*ERROR* \n*[{BiskoHealthcheckConfiguration.Environment.ToUpper()}] [[LIVENESS]]* \n[[DESCRIPTIONS]]\" }}",
                    restorePayload: "{ text: \"*SUCCESS*  \n*[[LIVENESS]]* -- *Healthchecks restored*\" }",
                    shouldNotifyFunc: x => x?.Status == UIHealthStatus.Unhealthy,
                    customDescriptionFunc: y =>
                    {
                        if (y == null) return "";
                        var entries = y.Entries?.Where(x => x.Value.Status == UIHealthStatus.Unhealthy)?.Take(3);
                        var values = entries?.Select(x =>
                        {
                            var data = x.Value?.Data?.Take(2);
                            var dataResponse = string.Join(", ", data?.Select(d => $"\n{d.Key} - {d.Value}") ?? new List<string>());
                            dataResponse = x.Value?.Data?.Count > 2 ? dataResponse + "\n..." : dataResponse;
                            return $" *{x.Key} - {x.Value?.Description}* {dataResponse}";
                        })?.ToList();
                        return string.Join(";\n", values) + $"\n\nFor more information, please visit: {BiskoHealthcheckConfiguration.HealthcheckEndpoint}";
                    });
            }).AddInMemoryStorage();
        }

        public static IApplicationBuilder UseRegisteredHealthChecks(this IApplicationBuilder app)
        {
            // build the app, register other middleware
            app.UseHealthChecksUI(config => config.UIPath = "/healthchecks");
            app.UseHealthChecks("/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
            {
                ResponseWriter = WriteHealthCheckResponse
            });
            return app;
        }

        private static Task WriteHealthCheckResponse(HttpContext httpContext, HealthReport result)
        {
            httpContext.Response.ContentType = "application/json";

            return httpContext.Response.WriteAsync(
                JsonConvert.SerializeObject(
                    new HealthReport(
                        result.Entries.ToDictionary(x => x.Key,
                        y =>
                        {
                            var description = y.Value.Exception != null ? y.Value.Description + $" - Exception: {y.Value.Exception?.Message}" : y.Value.Description;
                            return new HealthReportEntry(y.Value.Status, description, y.Value.Duration, null, y.Value.Data);
                        }),
                        result.Status,
                        result.TotalDuration
                    )
                )
            );
        }

        private static void InjectConfigurationPropertiesInStaticClass(this BiskoHealthcheckSettings configuration)
        {
            Type configType = typeof(BiskoHealthcheckConfiguration);
            Type settingsType = typeof(BiskoHealthcheckSettings);

            foreach (var property in settingsType.GetProperties())
            {
                var configProperty = configType.GetProperty(property.Name);
                if (configProperty != null && configProperty.CanWrite)
                {
                    var value = property.GetValue(configuration, null);
                    configProperty.SetValue(null, value);
                }
            }
        }

        private static void AddPossibleHealthchecks(this IHealthChecksBuilder builder, IServiceCollection services)
        {
            if (BiskoHealthcheckConfiguration.KafkaBroker != null)
                builder.AddKafka(new ProducerConfig(new ClientConfig { BootstrapServers = BiskoHealthcheckConfiguration.KafkaBroker }), new LoggingEvent().TopicName, "Kafka", timeout: TimeSpan.FromSeconds(99));

            if (BiskoHealthcheckConfiguration.SqlServerConnectionString != null)
                builder.AddSqlServer(connectionString: BiskoHealthcheckConfiguration.SqlServerConnectionString);

            if (BiskoHealthcheckConfiguration.RedisConnectionString != null)
            {
                builder.AddRedis(BiskoHealthcheckConfiguration.RedisConnectionString, "Redis");
            }

            if (BiskoHealthcheckConfiguration.PostgresqlConnectionString != null)
                builder.AddNpgSql(npgsqlConnectionString: BiskoHealthcheckConfiguration.PostgresqlConnectionString);

            if (BiskoHealthcheckConfiguration.PulsarProducerOptions != null)
            {
                builder.AddCheck<PulsarProducerHealthcheck>(nameof(PulsarProducerHealthcheck));

                builder.AddCheck<PulsarConsumerHealthcheck>(nameof(PulsarConsumerHealthcheck));
            }

            if (BiskoHealthcheckConfiguration.CassandraHealthcheckConfiguration != null)
            {
                services.AddCassandraServices();

                builder.AddCheck<CassandraDatabaseHealthCheck>(nameof(CassandraDatabaseHealthCheck));
            }

            if (BiskoHealthcheckConfiguration.ClickhouseHealthcheckSettings != null)
                builder.AddCheck<ClickhouseHealthcheck>(nameof(ClickhouseHealthcheck));

            if (BiskoHealthcheckConfiguration.BigQueryProjectId != null)
                builder.AddCheck<BigQueryHealthcheck>(nameof(BigQueryHealthcheck));

            foreach (var urlToPing in BiskoHealthcheckConfiguration.UrlsToPing)
                builder.AddPingHealthCheck(setup => setup.AddHost(new Uri(urlToPing).Host, 1000), $"Ping: {urlToPing}");
        }

        public static void AddCassandraServices(this IServiceCollection services)
        {
            services.AddSingleton<IDatabaseConnection, CassandraDatabaseConnection>();
        }
    }
}
