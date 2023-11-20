using Bisko.Messaging.Core.Extensions;
using Bisko.Messaging.Core.Model;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Org.BouncyCastle.Asn1.Mozilla;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Bisko.Healthchecks.Core.Services.Pulsar.Consumers
{
    public class PulsarConsumerHealthcheck : IHealthCheck
    {
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                Dictionary<string, object> notConnectedConsumers = new();

                var consumerStatuses = PulsarConsumerExtensions.GetConsumerStatuses();

                foreach (var consumer in consumerStatuses)
                {
                    if (!consumer.ConsumerStatus.Equals(nameof(ConsumerProducerStatuses.Connected)))
                        notConnectedConsumers.Add(
                            consumer.ConsumerName,
                            consumer.ConsumerStatus
                            );
                }

                if (notConnectedConsumers.Count > 0)
                {
                    return await Task.FromResult(
                        new HealthCheckResult(
                            status: HealthStatus.Unhealthy,
                            description: "Not all consumers are connected",
                            data: notConnectedConsumers
                            ));
                }

                return await Task.FromResult(
                            new HealthCheckResult(
                                status: HealthStatus.Healthy,
                                description: $"The state of all existing consumers is {ConsumerProducerStatuses.Connected}"
                            ));
            }
            catch (Exception ex)
            {
                return await Task.FromResult(
                    new HealthCheckResult(
                        status: HealthStatus.Unhealthy,
                        description: $"{ex.Message}"
                        ));
            }
        }
    }
}
