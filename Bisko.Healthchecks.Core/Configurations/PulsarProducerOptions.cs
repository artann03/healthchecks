
using System;

namespace Bisko.Healthchecks.Core.Configurations
{
    public class PulsarProducerOptions
    {
        public Type PulsarProducerType { get; set; }

        public PulsarProducerOptions(Type pulsarProducerType)
        {
            PulsarProducerType = pulsarProducerType;
        }
    }
}
