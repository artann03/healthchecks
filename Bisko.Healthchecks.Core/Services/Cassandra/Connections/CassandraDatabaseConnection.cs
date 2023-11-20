using Bisko.Healthchecks.Core.Configurations;
using Cassandra;

namespace Bisko.Healthchecks.Core.Services.Cassandra.Connections
{
    public class CassandraDatabaseConnection : IDatabaseConnection
    {
        private readonly CassandraConfiguration _cassandraConfiguration;
        private Cluster _cluster;
        private ISession _session;

        public CassandraDatabaseConnection(CassandraConfiguration cassandraConfiguration)
        {
            _cassandraConfiguration = cassandraConfiguration;
            SetSession();
        }

        private void SetSession()
        {
            _cluster = Connect();
            _session = _cluster.Connect();
        }

        public ISession Session => _session ?? _cluster.Connect();

        private Cluster Connect()
        {
            var user = _cassandraConfiguration.User;
            var password = _cassandraConfiguration.Password;
            string[] nodes = _cassandraConfiguration.Nodes.Split(",");

            QueryOptions queryOptions = new QueryOptions()
                .SetConsistencyLevel(ConsistencyLevel.One);

            Cluster cluster = Cluster.Builder()
                .AddContactPoints(nodes)
                .WithCredentials(user, password)
                .WithDefaultKeyspace(_cassandraConfiguration.KeySpace)
                .WithQueryOptions(queryOptions)
                .Build();

            return cluster;
        }
    }
}
