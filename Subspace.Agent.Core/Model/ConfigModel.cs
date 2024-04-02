namespace Subspace.Agent.Core.Model
{

	public class ConfigModel
    {
		public ConfigModel()
		{
		}

		public ConfigModel(string listen)
		{
			this.Listen = listen;
			Nodes = new List<NodeInfo>();
		}
#nullable disable
		public string Listen { get; set; }
		public List<NodeInfo>? Nodes { get; set; }
#nullable enable
    }

    public class NodeInfo
    {
		public NodeInfo()
		{
		}

		public NodeInfo(string url, string name)
		{
			this.Url = url;
			this.Name = name;
		}
#nullable disable
		public string Url { get; set; }
        public string Name { get; set; }
#nullable enable
		public List<NodePool>? Pools { get; set; }
	}
	public enum NodePool
	{
		Piece,
		Submit,
	}
}
