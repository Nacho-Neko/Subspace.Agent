namespace Subspace.Agent.Core.Model
{

    public class ConfigModel
    {
#nullable disable
        public string listen { get; set; }
        public List<NodeInfo> node { get; set; }
#nullable enable
    }

    public class NodeInfo
    {
#nullable disable
        public string url { get; set; }
        public string name { get; set; }
#nullable enable
    }
}
