using DataGEMS.Gateway.App.Common;
using Newtonsoft.Json;

namespace DataGEMS.Gateway.App.Service.TaskOrchestrator.Model
{
	public class AdHocQueryTaskOrchestratorResponse
	{
		[JsonProperty("code")]
		public int Code { get; set; }
		[JsonProperty("message")]
		public string Message { get; set; }
		[JsonProperty("ap")]
		public AnalyticalPattern AnalyticalPattern { get; set; }
	}
}
