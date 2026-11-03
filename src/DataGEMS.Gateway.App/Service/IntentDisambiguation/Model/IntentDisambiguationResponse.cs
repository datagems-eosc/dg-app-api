using DataGEMS.Gateway.App.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace DataGEMS.Gateway.App.Service.IntentDisambiguation.Model
{
	public class IntentDisambiguationResponse
	{
		[JsonProperty("intent")]
		[JsonConverter(typeof(StringEnumConverter))]
		public IntentDisambiguationIntentType Intent { get; set; }

		[JsonProperty("confidence")]
		public decimal Confidence { get; set; }

		[JsonProperty("search_query")]
		public string SearchQuery { get; set; }

		[JsonProperty("degraded")]
		public bool Degraded { get; set; }

		[JsonProperty("model")]
		public string Model { get; set; }

		[JsonProperty("prompt_version")]
		public string PromptVersion { get; set; }
	}
}
