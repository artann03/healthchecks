using Cassandra;
using System.Collections.Generic;

namespace Bisko.Healthchecks.Core.Configurations
{
    public static class BiskoHealthcheckConfiguration
    {
        public static PulsarProducerOptions PulsarProducerOptions { get; set; }
        public static string KafkaBroker { get; set; }
        public static string SqlServerConnectionString { get; set; }
        public static string PostgresqlConnectionString { get; set; }
        public static string RedisConnectionString { get; set; }
        public static string HealthcheckEndpoint { get; set; } = "https://localhost:80/ready";
        public static string HealthcheckWebhookEndpoint { get; set; } = "https://chat.googleapis.com/v1/spaces/AAAAVGM5YWk/messages?key=AIzaSyDdI0hCZtE6vySjMm-WEfRq3CPzqKqqsHI&token=NkmCQtAI9r0TjVzQcIxjP14dFJaGl8a6Fm7u7UiNoig";
        public static string ServiceName { get; set; }
        public static string Environment { get; set; }
        public static List<string> UrlsToPing { get; set; }
        public static ClickhouseHealthcheckSettings ClickhouseHealthcheckSettings { get; set; }
        public static string BigQueryProjectId { get; set; }
        public static CassandraConfiguration CassandraHealthcheckConfiguration { get; set; }
    }

    public class BiskoHealthcheckSettings
    {
        public PulsarProducerOptions PulsarProducerOptions { get; set; }
        public string KafkaBroker { get; set; }
        public string SqlServerConnectionString { get; set; }
        public string PostgresqlConnectionString { get; set; }
        public string RedisConnectionString { get; set; }
        public string HealthcheckEndpoint { get; set; } = "https://localhost:80/ready";
        public string HealthcheckWebhookEndpoint { get; set; } = "https://chat.googleapis.com/v1/spaces/AAAAVGM5YWk/messages?key=AIzaSyDdI0hCZtE6vySjMm-WEfRq3CPzqKqqsHI&token=NkmCQtAI9r0TjVzQcIxjP14dFJaGl8a6Fm7u7UiNoig";
        public string ServiceName { get; set; }
        public string Environment { get; set; }
        public List<string> UrlsToPing { get; set; } = new();
        public ClickhouseHealthcheckSettings ClickhouseHealthcheckSettings { get; set; }
        public string BigQueryProjectId { get; set; }
        public CassandraConfiguration CassandraHealthcheckConfiguration { get; set; }
    }
}
