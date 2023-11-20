namespace Bisko.Healthchecks.Core.Configurations
{
    public class ClickhouseHealthcheckSettings
    {
        public string[] ClickhouseInstances { get; set; }
        public string ClusterName { get; set; }
        public bool IsOctonicaClient { get; set; } = false;
        public bool IsClickhouseAdo { get; set; } = false;
    }
}
