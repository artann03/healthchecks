using Cassandra;

namespace Bisko.Healthchecks.Core.Services.Cassandra.Connections
{
    public interface IDatabaseConnection
    {
        ISession Session { get; }
    }
}
