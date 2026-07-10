using DataGEMS.Gateway.App.Common;
using DataGEMS.Gateway.App.Model;
using Newtonsoft.Json;

namespace DataGEMS.Gateway.App.Service.TaskOrchestrator.Model
{
	public class QueryDisambiguationResponse
	{
		[JsonProperty("ap")]
		public AnalyticalPattern AnalyticalPattern { get; set; }
		[JsonProperty("metadata")]
		public QueryDisambiguationMetadata Metadata { get; set; }
	}
}
