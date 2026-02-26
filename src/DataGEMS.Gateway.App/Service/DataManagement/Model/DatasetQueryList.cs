using Newtonsoft.Json;

namespace DataGEMS.Gateway.App.Service.DataManagement.Model
{
	public class DatasetQueryList
	{
		[JsonProperty("code")]
		public int Code { get; set; }
		[JsonProperty("message")]
		public string Message { get; set; }
		[JsonProperty("datasets")]
		public List<Dataset> Datasets { get; set; }
		[JsonProperty("offset")]
		public int Offset { get; set; }
		[JsonProperty("count")]
		public int Count { get; set; }
		[JsonProperty("total")]
		public int Total { get; set; }

		public class Dataset
		{
			[JsonProperty("nodes")]
			public List<Dictionary<string, object>> Nodes { get; set; }

			[JsonProperty("edges")]
			public List<Dictionary<string, object>> Edges { get; set; }
		}
	}
}
