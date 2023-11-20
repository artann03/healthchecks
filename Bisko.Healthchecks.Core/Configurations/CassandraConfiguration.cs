namespace Bisko.Healthchecks.Core.Configurations
{
    public class CassandraConfiguration
    {
        public string KeySpace { get; set; }
        public string Table { get; set; }
        public string Nodes { get; set; }
        public int Port { get; set; }
        public string User { get; set; }
        public string Password { get; set; }

        /// <summary>
        /// Ttl in seconds for insert/update retention policy
        /// </summary>
        public int TtlPolicy { get; set; }
        public int HtmlContentTtlPolicy { get; set; }

        public void Deconstruct(out string user, out string password)
        {
            user = User;
            password = Password;
        }
    }
}
