using DataGEMS.Gateway.App.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace DataGEMS.Gateway.App.Service.DatasetLinking.Model
{
	public class DatasetLinkingJobStatus
	{
		[JsonProperty("status")]
		[JsonConverter(
		typeof(StringEnumConverter),
		typeof(SnakeCaseNamingStrategy))]
		public DatasetLinkingStatus Status { get; set; }
	}
}
