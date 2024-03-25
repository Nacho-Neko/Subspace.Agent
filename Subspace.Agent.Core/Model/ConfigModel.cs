namespace Subspace.Agent.Core.Model
{

    public class ConfigModel
    {
		public ConfigModel()
		{
		}

		public ConfigModel(string listen)
		{
			this.listen = listen;
			node = new List<NodeInfo>();
		}
#nullable disable
		public string listen { get; set; }
        public List<NodeInfo> node { get; set; }
#nullable enable
    }

    public class NodeInfo
    {
		public NodeInfo()
		{
		}

		public NodeInfo(string url, string name)
		{
			this.url = url;
			this.name = name;
		}
#nullable disable
		public string url { get; set; }
        public string name { get; set; }
#nullable enable
    }
}
