using Newtonsoft.Json;

namespace DataGEMS.Gateway.App.Service.DatasetLinking.Model
{
	public class RefineLinkingResponse
	{
		[JsonProperty("job_id")]
		public Guid JobId { get; set; }
		[JsonProperty("status")]
		public string Status { get; set; }
	}
}
