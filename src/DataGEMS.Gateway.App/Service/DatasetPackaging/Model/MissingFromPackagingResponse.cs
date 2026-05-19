using Newtonsoft.Json;

namespace DataGEMS.Gateway.App.Service.DatasetPackaging.Model
{
	public class MissingFromPackagingResponse
	{
		[JsonProperty("status")]
		public string Status { get; set; }

		[JsonProperty("missing_ids")]
		public List<Guid> MissingIds { get; set; }
	}
}
