using Bisko.Healthchecks.Core.Configurations;
using Bisko.Messaging.Core.Abstraction;
using Bisko.Messaging.Core.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Bisko.Healthchecks.Core.Services.Pulsar.Producers
{
    public class PulsarProducerHealthcheck : IHealthCheck
    {
        private readonly IServiceProvider _serviceProvider;
        public PulsarProducerHealthcheck(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var producerTypes = BiskoHealthcheckConfiguration.PulsarProducerOptions.PulsarProducerType.Assembly.GetTypes()
                    .Where(t => t.GetInterfaces().Any(gi => gi.IsGenericType && gi.GetGenericTypeDefinition() == typeof(IProducer<>)))
                    .Where(x => !x.IsAbstract)
                    .ToList();

                Dictionary<string, object> notConnectedProducers = new();
                foreach (var producerType in producerTypes)
                {
                    var producerInterface = producerType.GetInterfaces().FirstOrDefault(x => x.Name.Contains("IProducer"));
                    object pulsarProducer;
                    try
                    {
                        pulsarProducer = _serviceProvider.GetRequiredService(producerInterface);
                    }
                    catch
                    {
                        pulsarProducer = null;
                    }

                    if (pulsarProducer == null)
                        continue;

                    var getProducerStateMethod = pulsarProducer.GetType().GetMethod("GetProducerState");
                    string getProducerStateResult = (string)getProducerStateMethod.Invoke(pulsarProducer, new object[] { });

                    if (!getProducerStateResult.Equals(ConsumerProducerStatuses.Connected.ToString()))
                        notConnectedProducers.Add(pulsarProducer.GetType().Name, getProducerStateResult);
                }

                if (notConnectedProducers.Count > 0)
                    return await Task.FromResult(new HealthCheckResult(status: HealthStatus.Unhealthy, description: "Not all producers are connected", data: notConnectedProducers));

                return await Task.FromResult(new HealthCheckResult(status: HealthStatus.Healthy, description: $"The state of all existing producers is {ConsumerProducerStatuses.Connected}"));
            }
            catch (Exception ex)
            {
                return await Task.FromResult(new HealthCheckResult(status: HealthStatus.Unhealthy, description: $"Failed to send message to Producer: {ex.Message}"));
            }
        }
    }
}
