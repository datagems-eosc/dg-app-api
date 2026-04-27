using Newtonsoft.Json;

namespace DataGEMS.Gateway.App.Service.DatasetFileManagement.Model
{
	public class Profile
	{
		public List<ProfileEdge> Edges { get; set; }
		public List<ProfileNode> Nodes { get; set; }

		public class ProfileNode
		{
			[JsonProperty("id")]
			public Guid Id { get; set; }
			[JsonProperty("labels")]
			public List<string> Labels { get; set; }
			[JsonProperty("properties")]
			public Dictionary<string, object> Properties { get; set; }
		}

		public class ProfileEdge
		{
			[JsonProperty("from_")]
			public Guid From { get; set; }

			[JsonProperty("to")]
			public Guid To { get; set; }

			[JsonProperty("labels")]
			public List<string> Labels { get; set; }
		}
	}
}
