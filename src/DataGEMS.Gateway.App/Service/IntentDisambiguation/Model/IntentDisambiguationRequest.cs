using Newtonsoft.Json;

namespace DataGEMS.Gateway.App.Service.IntentDisambiguation.Model
{
	public class IntentDisambiguationRequest
	{
		[JsonProperty("query")]
		public string Query { get; set; }
		[JsonProperty("context")]
		public IntentDisambiguationRequestContext Context { get; set; }

		public class IntentDisambiguationRequestContext
		{
			[JsonProperty("domain")]
			public string Domain { get; set; }
			[JsonProperty("dataset_ids")]
			public List<Guid> DatasetIds { get; set; }
			[JsonProperty("history")]
			public List<string> History { get; set; }
		}
	}
}
